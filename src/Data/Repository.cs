#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using System.Reflection;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Generic base repository with CRUD and query operations
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    private readonly IDatabaseContext _context;
    private readonly List<T> _changeTracking = [];
    private readonly string _tableName;
    private readonly string _schema;

    public Repository(IDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tableName = GetTableName();
        _schema = GetTableSchema();
    }

    // Retrieves entity by primary key
    public async Task<T?> GetByIdAsync(int id)
    {
        var idProperty = typeof(T).GetProperty("Id") ?? throw new OrmException("Entity must have Id property");
        return await FirstOrDefaultAsync(e => (int)idProperty.GetValue(e)! == id);
    }

    // Retrieves first entity matching predicate
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var list = await GetAsync(predicate);
        return list.FirstOrDefault();
    }

    // Retrieves all entities
    public async Task<List<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM [{_schema}].[{_tableName}]";
        var results = await _context.ExecuteQueryAsync(query);
        return results.Select(MapToEntity).ToList();
    }

    // Retrieves entities matching predicate
    public async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var all = await GetAllAsync();
        return all.AsQueryable().Where(predicate).ToList();
    }

    // Counts entities
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = $"SELECT COUNT(*) FROM [{_schema}].[{_tableName}]";
        var count = await _context.ExecuteScalarAsync(query);
        return count is not null ? (int)(long)count : 0;
    }

    // Checks if entity exists
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var count = await CountAsync(predicate);
        return count > 0;
    }

    // Adds entity
    public async Task<T> AddAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Validate(out var errors);
        if (errors.Count > 0)
            throw new EntityValidationException("Entity validation failed", errors);

        entity.PreSave();
        _changeTracking.Add(entity);

        var columns = GetMappedColumns();
        var columnNames = string.Join(", ", columns.Select(c => $"[{c.ColumnName}]"));
        var parameterNames = string.Join(", ", columns.Select(c => $"@{c.PropertyName}"));

        var query = $"INSERT INTO [{_schema}].[{_tableName}] ({columnNames}) VALUES ({parameterNames})";

        var parameters = BuildParameters(entity, columns);
        await _context.ExecuteNonQueryAsync(query, parameters);

        return entity;
    }

    // Updates entity
    public async Task<T> UpdateAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        entity.Validate(out var errors);
        if (errors.Count > 0)
            throw new EntityValidationException("Entity validation failed", errors);

        entity.PreSave();
        var columns = GetMappedColumns().Where(c => !c.IsPrimaryKey).ToList();
        var setClause = string.Join(", ", columns.Select(c => $"[{c.ColumnName}] = @{c.PropertyName}"));
        var idProperty = typeof(T).GetProperty("Id") ?? throw new OrmException("Entity must have Id property for Update operations");
        var idValue = idProperty.GetValue(entity);

        var query = $"UPDATE [{_schema}].[{_tableName}] SET {setClause} WHERE [Id] = @Id";

        var parameters = BuildParameters(entity, columns);
        parameters["Id"] = idValue!;

        // Concurrency token handling
        var concurrencyTokenInfo = GetConcurrencyTokenColumn();
        if (concurrencyTokenInfo.PropertyName is not null)
        {
            var concurrencyProperty = typeof(T).GetProperty(concurrencyTokenInfo.PropertyName);
            if (concurrencyProperty is null)
                throw new OrmException($"Concurrency token property '{concurrencyTokenInfo.PropertyName}' not found on entity.");

            var originalConcurrencyValue = concurrencyProperty.GetValue(entity);
            if (originalConcurrencyValue is null)
                throw new OrmException($"Concurrency token '{concurrencyTokenInfo.PropertyName}' cannot be null for entity with Id {idValue}.");

            query += $" AND [{concurrencyTokenInfo.ColumnName}] = @OriginalConcurrencyValue";
            parameters["OriginalConcurrencyValue"] = originalConcurrencyValue;
        }

        var rowsAffected = await _context.ExecuteNonQueryAsync(query, parameters);

        if (rowsAffected == 0)
        {
            if (concurrencyTokenInfo.PropertyName is not null)
            {
                throw new ConcurrencyException($"Concurrency conflict detected for entity {typeof(T).Name} with Id {idValue}. The entity may have been modified or deleted by another user.")
                    .WithContext("EntityType", typeof(T).Name)
                    .WithContext("EntityId", idValue!);
            }
            throw new OrmException($"Entity {typeof(T).Name} with Id {idValue} not found for update.");
        }

        // Update the concurrency token value on the entity after successful update
        if (concurrencyTokenInfo.PropertyName is not null && concurrencyTokenInfo.PropertyType != typeof(byte[]))
        {
            var newConcurrencyValue = await GetConcurrencyTokenValueAsync(idValue!, concurrencyTokenInfo.ColumnName!);
            typeof(T).GetProperty(concurrencyTokenInfo.PropertyName!)?.SetValue(entity, newConcurrencyValue);
        }

        return entity;
    }

    // Deletes entity by id
    public async Task<bool> DeleteAsync(int id)
    {
        var entityToDelete = await GetByIdAsync(id);
        if (entityToDelete is null)
            return false;
        return await DeleteAsync(entityToDelete);
    }

    // Deletes entity
    public async Task<bool> DeleteAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var idProperty = typeof(T).GetProperty("Id") ?? throw new OrmException("Entity must have Id property for Delete operations");
        var idValue = idProperty.GetValue(entity);
        var query = $"DELETE FROM [{_schema}].[{_tableName}] WHERE [Id] = @Id";

        var parameters = new Dictionary<string, object> { { "Id", idValue! } };

        // Concurrency token handling
        var concurrencyTokenInfo = GetConcurrencyTokenColumn();
        if (concurrencyTokenInfo.PropertyName is not null)
        {
            var concurrencyProperty = typeof(T).GetProperty(concurrencyTokenInfo.PropertyName);
            if (concurrencyProperty is null)
                throw new OrmException($"Concurrency token property '{concurrencyTokenInfo.PropertyName}' not found on entity.");

            var originalConcurrencyValue = concurrencyProperty.GetValue(entity);
            if (originalConcurrencyValue is null)
                throw new OrmException($"Concurrency token '{concurrencyTokenInfo.PropertyName}' cannot be null for entity with Id {idValue}.");

            query += $" AND [{concurrencyTokenInfo.ColumnName}] = @OriginalConcurrencyValue";
            parameters["OriginalConcurrencyValue"] = originalConcurrencyValue;
        }

        var rowsAffected = await _context.ExecuteNonQueryAsync(query, parameters);

        if (rowsAffected == 0)
        {
            if (concurrencyTokenInfo.PropertyName is not null)
            {
                throw new ConcurrencyException($"Concurrency conflict detected for entity {typeof(T).Name} with Id {idValue}. The entity may have been modified or deleted by another user.")
                    .WithContext("EntityType", typeof(T).Name)
                    .WithContext("EntityId", idValue!);
            }
            return false;
        }

        _changeTracking.Remove(entity);
        return true;
    }

    // Bulk insert
    public async Task<List<T>> AddRangeAsync(List<T> entities)
    {
        const int MAX_SQL_PARAMS_PER_BATCH = 2000; // SQL Server limit is 2100, leaving some buffer

        if (entities is null)
            throw new ArgumentNullException(nameof(entities));
        if (!entities.Any())
            return new List<T>();

        // Validate all entities upfront
        foreach (var entity in entities)
        {
            entity.Validate(out var errors);
            if (errors.Count > 0)
                throw new EntityValidationException("Entity validation failed", errors);
            entity.PreSave();
        }

        var mappedColumns = GetMappedColumns().Where(c => !c.IsPrimaryKey).ToList(); // Exclude primary key for insert
        var parametersPerEntity = mappedColumns.Count;

        if (parametersPerEntity == 0)
            throw new OrmException("No columns mapped for entity, cannot perform batch insert.");

        var maxEntitiesPerBatch = MAX_SQL_PARAMS_PER_BATCH / parametersPerEntity;
        if (maxEntitiesPerBatch == 0)
            throw new OrmException($"Single entity has {parametersPerEntity} parameters, exceeding MAX_SQL_PARAMS_PER_BATCH ({MAX_SQL_PARAMS_PER_BATCH}). Adjust MAX_SQL_PARAMS_PER_BATCH or mapped columns.");

        var allResults = new List<T>();
        var columnNames = string.Join(", ", mappedColumns.Select(c => $"[{c.ColumnName}]"));

        for (int i = 0; i < entities.Count; i += maxEntitiesPerBatch)
        {
            var batch = entities.Skip(i).Take(maxEntitiesPerBatch).ToList();
            if (!batch.Any()) continue;

            var (valuesClause, batchParameters) = BuildBatchInsertParameters(batch, mappedColumns);
            var query = $"INSERT INTO [{_schema}].[{_tableName}] ({columnNames}) VALUES {valuesClause}";

            await _context.ExecuteNonQueryAsync(query, batchParameters);
            allResults.AddRange(batch);
            _changeTracking.AddRange(batch);
        }

        return allResults;
    }

    // Bulk update
    public async Task<List<T>> UpdateRangeAsync(List<T> entities)
    {
        if (entities is null)
            throw new ArgumentNullException(nameof(entities));

        var updated = new List<T>(entities.Count);
        foreach (var entity in entities)
            updated.Add(await UpdateAsync(entity));
        return updated;
    }

    // Bulk delete
    public async Task<int> DeleteRangeAsync(List<T> entities)
    {
        if (entities is null)
            throw new ArgumentNullException(nameof(entities));

        var deletedCount = 0;
        foreach (var entity in entities)
            if (await DeleteAsync(entity))
                deletedCount++;
        return deletedCount;
    }

    // Paged query
    public async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
    {
        var (items, _) = await GetPagedWithCountAsync(pageNumber, pageSize, predicate);
        return items;
    }

    // Paged query with total count
    public async Task<(List<T> Items, int TotalCount)> GetPagedWithCountAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
    {
        var totalCount = await CountAsync(predicate);
        var offset = (pageNumber - 1) * pageSize;

        var query = $"SELECT * FROM [{_schema}].[{_tableName}] ORDER BY [Id] OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
        var results = await _context.ExecuteQueryAsync(query);
        var entities = results.Select(MapToEntity).ToList();

        if (predicate is not null)
            entities = entities.AsQueryable().Where(predicate).ToList();

        return (entities, totalCount);
    }

    /// <summary>
    /// Materializes the whole table and exposes it as an in-memory queryable source.
    /// Intended as the backing source for <see cref="QueryBuilder{T}"/>; prefer the
    /// async members for large tables.
    /// </summary>
    public IQueryable<T> Query() => GetAllAsync().GetAwaiter().GetResult().AsQueryable();

    public async IAsyncEnumerable<T> QueryStreamAsync(string query, Dictionary<string, object>? parameters = null)
    {
        var results = _context.ExecuteStreamAsync(query, parameters);
        await foreach (var row in results)
        {
            yield return MapToEntity(row);
        }
    }

    private string GetTableName()
    {
        var attr = typeof(T).GetCustomAttribute<TableAttribute>();
        return attr?.Name ?? typeof(T).Name + "s";
    }

    private string GetTableSchema()
    {
        var attr = typeof(T).GetCustomAttribute<TableAttribute>();
        return attr?.Schema ?? Constants.OrmConstants.DefaultSchema;
    }

    private List<ColumnMapping> GetMappedColumns()
    {
        var columns = new List<ColumnMapping>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttribute<NotMappedAttribute>() is not null)
                continue;

            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr is null)
                continue;

            columns.Add(new ColumnMapping
            {
                PropertyName = prop.Name,
                ColumnName = columnAttr.Name,
                IsPrimaryKey = columnAttr.IsPrimaryKey,
                PropertyType = prop.PropertyType
            });
        }

        return columns;
    }

    private Dictionary<string, object> BuildParameters(T entity, List<ColumnMapping> columns, string? parameterPrefix = null)
    {
        var parameters = new Dictionary<string, object>();
        foreach (var column in columns)
        {
            var paramName = parameterPrefix is null ? column.PropertyName : $"{parameterPrefix}_{column.PropertyName}";
            var value = typeof(T).GetProperty(column.PropertyName)?.GetValue(entity);
            parameters[paramName] = value ?? DBNull.Value;
        }
        return parameters;
    }

    private (string ValuesClause, Dictionary<string, object> Parameters) BuildBatchInsertParameters(List<T> entities, List<ColumnMapping> columns)
    {
        var allParameters = new Dictionary<string, object>();
        var valueSets = new List<string>();

        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            var parameterPrefix = $"p{i}"; // Unique prefix for each entity in the batch
            var entityParameters = BuildParameters(entity, columns, parameterPrefix);

            foreach (var param in entityParameters)
            {
                allParameters.Add(param.Key, param.Value);
            }

            var parameterNamesForEntity = string.Join(", ", columns.Select(c => $"@{parameterPrefix}_{c.PropertyName}"));
            valueSets.Add($"({parameterNamesForEntity})");
        }

        return (string.Join(", ", valueSets), allParameters);
    }

    private T MapToEntity(Dictionary<string, object> row)
    {
        var entity = new T();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr is null) continue;

            if (row.TryGetValue(columnAttr.Name, out var value) && value != DBNull.Value)
                prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
        }

        entity.PostLoad();
        return entity;
    }

    private ConcurrencyTokenInfo GetConcurrencyTokenColumn()
    {
        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            var concurrencyAttr = prop.GetCustomAttribute<ConcurrencyTokenAttribute>();
            if (concurrencyAttr is not null)
            {
                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr is null)
                    throw new OrmException($"ConcurrencyToken property '{prop.Name}' must also have a ColumnAttribute.");

                return new ConcurrencyTokenInfo
                {
                    PropertyName = prop.Name,
                    ColumnName = columnAttr.Name,
                    PropertyType = prop.PropertyType
                };
            }
        }
        return new ConcurrencyTokenInfo(); // No concurrency token found
    }

    private async Task<object?> GetConcurrencyTokenValueAsync(object entityId, string concurrencyColumnName)
    {
        var idProperty = typeof(T).GetProperty("Id") ?? throw new OrmException("Entity must have Id property.");
        var query = $"SELECT [{concurrencyColumnName}] FROM [{_schema}].[{_tableName}] WHERE [Id] = @Id";
        var parameters = new Dictionary<string, object> { { "Id", entityId } };
        return await _context.ExecuteScalarAsync(query, parameters);
    }

    private class ConcurrencyTokenInfo
    {
        public string? PropertyName { get; set; }
        public string? ColumnName { get; set; }
        public Type? PropertyType { get; set; }
    }

    private class ColumnMapping
    {
        public string PropertyName { get; set; } = "";
        public string ColumnName { get; set; } = "";
        public bool IsPrimaryKey { get; set; }
        public Type PropertyType { get; set; } = typeof(object);
    }
}
