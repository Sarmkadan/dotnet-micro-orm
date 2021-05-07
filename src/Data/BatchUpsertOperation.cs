#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using System.Reflection;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Batch upsert implementation that generates a SQL MERGE statement for SQL Server
/// (and equivalent UPSERT for other providers).  Each batch is executed in a single
/// round-trip to minimize network overhead.
/// </summary>
public sealed class BatchUpsertOperation<T> : IBatchUpsertOperation<T>
    where T : BaseEntity, new()
{
    private readonly IDatabaseContext _context;
    private readonly string _tableName;
    private readonly string _schema;

    /// <param name="context">Active database context.</param>
    public BatchUpsertOperation(IDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tableName = GetTableName();
        _schema = GetTableSchema();
    }

    /// <inheritdoc/>
    public async Task<UpsertResult<T>> UpsertAsync(T entity, Expression<Func<T, object>> keySelector)
    {
        var results = await UpsertRangeAsync([entity], keySelector, 1);
        return results[0];
    }

    /// <inheritdoc/>
    public async Task<List<UpsertResult<T>>> UpsertRangeAsync(
        List<T> entities,
        Expression<Func<T, object>> keySelector,
        int batchSize = OrmConstants.DefaultBatchSize)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));
        if (batchSize <= 0 || batchSize > OrmConstants.MaxBatchSize)
            throw new ArgumentOutOfRangeException(nameof(batchSize),
                string.Format(OrmConstants.Messages.InvalidBatchSize, OrmConstants.MaxBatchSize));

        if (entities.Count == 0)
            return [];

        foreach (var entity in entities)
        {
            entity.Validate(out var errors);
            if (errors.Count > 0)
                throw new EntityValidationException("Entity validation failed", errors);
            entity.PreSave();
        }

        var keyProperties = ExtractKeyProperties(keySelector);
        var allColumns = GetMappedColumns();
        var nonKeyColumns = allColumns.Where(c => !c.IsPrimaryKey).ToList();

        var results = new List<UpsertResult<T>>(entities.Count);

        foreach (var batch in entities.Chunk(batchSize))
        {
            var batchResults = await ExecuteBatchUpsertAsync(batch.ToList(), keyProperties, allColumns, nonKeyColumns);
            results.AddRange(batchResults);
        }

        return results;
    }

    // Executes one MERGE statement for a batch and returns per-row outcomes.
    private async Task<List<UpsertResult<T>>> ExecuteBatchUpsertAsync(
        List<T> batch,
        List<string> keyProperties,
        List<ColumnMapping> allColumns,
        List<ColumnMapping> nonKeyColumns)
    {
        var provider = _context.GetDatabaseProvider();
        var (sql, parameters) = provider == DatabaseProvider.SqlServer
            ? BuildSqlServerMerge(batch, keyProperties, allColumns, nonKeyColumns)
            : BuildGenericUpsert(batch, keyProperties, allColumns, nonKeyColumns);

        var actionColumn = "__action";
        var rows = await _context.ExecuteQueryAsync(sql, parameters);

        // Map the $action output column back to each entity by position
        var results = new List<UpsertResult<T>>(batch.Count);
        for (int i = 0; i < batch.Count; i++)
        {
            bool wasInserted = true;
            if (i < rows.Count && rows[i].TryGetValue(actionColumn, out var action))
                wasInserted = action?.ToString()?.Equals("INSERT", StringComparison.OrdinalIgnoreCase) == true;

            results.Add(new UpsertResult<T> { Entity = batch[i], WasInserted = wasInserted });
        }

        return results;
    }

    // Builds a SQL Server MERGE statement with OUTPUT $action for each row in the batch.
    private (string Sql, Dictionary<string, object> Parameters) BuildSqlServerMerge(
        List<T> batch,
        List<string> keyProperties,
        List<ColumnMapping> allColumns,
        List<ColumnMapping> nonKeyColumns)
    {
        var parameters = new Dictionary<string, object>();
        var sourceRows = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var entity = batch[i];
            var rowValues = new List<string>();
            foreach (var col in allColumns.Where(c => !c.IsPrimaryKey))
            {
                var paramName = $"p{i}_{col.PropertyName}";
                var value = typeof(T).GetProperty(col.PropertyName)?.GetValue(entity);
                parameters[paramName] = value ?? DBNull.Value;
                rowValues.Add($"@{paramName} AS [{col.ColumnName}]");
            }
            sourceRows.Add($"SELECT {string.Join(", ", rowValues)}");
        }

        var sourceAlias = "src";
        var targetAlias = "tgt";

        var joinCondition = string.Join(" AND ",
            keyProperties.Select(p =>
            {
                var col = allColumns.First(c => c.PropertyName == p);
                return $"[{targetAlias}].[{col.ColumnName}] = [{sourceAlias}].[{col.ColumnName}]";
            }));

        var keyColumnSet = keyProperties
            .Select(p => allColumns.First(c => c.PropertyName == p).ColumnName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var updateSet = string.Join(", ",
            nonKeyColumns
                .Where(c => !keyColumnSet.Contains(c.ColumnName))
                .Select(c => $"[{targetAlias}].[{c.ColumnName}] = [{sourceAlias}].[{c.ColumnName}]"));

        var insertColumns = string.Join(", ", nonKeyColumns.Select(c => $"[{c.ColumnName}]"));
        var insertValues = string.Join(", ", nonKeyColumns.Select(c => $"[{sourceAlias}].[{c.ColumnName}]"));

        var sql = $"""
            MERGE [{_schema}].[{_tableName}] AS [{targetAlias}]
            USING (
                {string.Join(" UNION ALL ", sourceRows)}
            ) AS [{sourceAlias}]
            ON {joinCondition}
            WHEN MATCHED THEN
                UPDATE SET {updateSet}
            WHEN NOT MATCHED THEN
                INSERT ({insertColumns})
                VALUES ({insertValues})
            OUTPUT $action AS [__action];
            """;

        return (sql, parameters);
    }

    // Fallback for providers that do not support MERGE: issues individual upserts.
    private (string Sql, Dictionary<string, object> Parameters) BuildGenericUpsert(
        List<T> batch,
        List<string> keyProperties,
        List<ColumnMapping> allColumns,
        List<ColumnMapping> nonKeyColumns)
    {
        // Build a SELECT that returns a constant "__action" value so the caller
        // can still parse the result set uniformly.
        var parameters = new Dictionary<string, object>();
        var statements = new List<string>();

        for (int i = 0; i < batch.Count; i++)
        {
            var entity = batch[i];
            foreach (var col in allColumns.Where(c => !c.IsPrimaryKey))
            {
                var paramName = $"p{i}_{col.PropertyName}";
                var value = typeof(T).GetProperty(col.PropertyName)?.GetValue(entity);
                parameters[paramName] = value ?? DBNull.Value;
            }

            var keyColConditions = string.Join(" AND ",
                keyProperties.Select(p =>
                {
                    var col = allColumns.First(c => c.PropertyName == p);
                    return $"[{col.ColumnName}] = @p{i}_{col.PropertyName}";
                }));

            var updateSet = string.Join(", ",
                nonKeyColumns
                    .Where(c => !keyProperties.Select(p => allColumns.First(x => x.PropertyName == p).ColumnName)
                                              .Contains(c.ColumnName, StringComparer.OrdinalIgnoreCase))
                    .Select(c => $"[{c.ColumnName}] = @p{i}_{c.PropertyName}"));

            var insertColumns = string.Join(", ", nonKeyColumns.Select(c => $"[{c.ColumnName}]"));
            var insertValues  = string.Join(", ", nonKeyColumns.Select(c => $"@p{i}_{c.PropertyName}"));

            statements.Add($"""
                IF EXISTS (SELECT 1 FROM [{_schema}].[{_tableName}] WHERE {keyColConditions})
                BEGIN
                    UPDATE [{_schema}].[{_tableName}] SET {updateSet} WHERE {keyColConditions};
                    SELECT 'UPDATE' AS [__action];
                END
                ELSE
                BEGIN
                    INSERT INTO [{_schema}].[{_tableName}] ({insertColumns}) VALUES ({insertValues});
                    SELECT 'INSERT' AS [__action];
                END
                """);
        }

        return (string.Join("\n", statements), parameters);
    }

    // Resolves property names from the key selector expression.
    private static List<string> ExtractKeyProperties(Expression<Func<T, object>> keySelector)
    {
        var names = new List<string>();

        // Handle single-property lambda: x => x.Sku
        if (keySelector.Body is MemberExpression memberExpr)
        {
            names.Add(memberExpr.Member.Name);
            return names;
        }

        // Handle anonymous-type projection: x => new { x.Sku, x.CategoryId }
        if (keySelector.Body is NewExpression newExpr)
        {
            names.AddRange(newExpr.Members?.Select(m => m.Name) ?? []);
            return names;
        }

        // Handle boxing via UnaryExpression: x => (object)x.Id
        if (keySelector.Body is UnaryExpression unary && unary.Operand is MemberExpression boxed)
        {
            names.Add(boxed.Member.Name);
            return names;
        }

        throw new OrmException("Unsupported key selector expression. Use a single property or an anonymous-type projection.");
    }

    /// <summary>Resolves the table name for <typeparamref name="T"/> from its mapping attributes.</summary>
    public string GetTableName()
    {
        var attr = typeof(T).GetCustomAttribute<Domain.Models.TableAttribute>();
        return attr?.Name ?? typeof(T).Name + "s";
    }

    /// <summary>Resolves the table schema for <typeparamref name="T"/> from its mapping attributes.</summary>
    public string GetTableSchema()
    {
        var attr = typeof(T).GetCustomAttribute<Domain.Models.TableAttribute>();
        return attr?.Schema ?? OrmConstants.DefaultSchema;
    }

    private List<ColumnMapping> GetMappedColumns()
    {
        var columns = new List<ColumnMapping>();
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.GetCustomAttribute<Domain.Models.NotMappedAttribute>() is not null)
                continue;

            var columnAttr = prop.GetCustomAttribute<Domain.Models.ColumnAttribute>();
            if (columnAttr is null) continue;

            columns.Add(new ColumnMapping
            {
                PropertyName = prop.Name,
                ColumnName   = columnAttr.Name,
                IsPrimaryKey = columnAttr.IsPrimaryKey,
                PropertyType = prop.PropertyType
            });
        }
        return columns;
    }

    private sealed class ColumnMapping
    {
        public string PropertyName { get; set; } = "";
        public string ColumnName   { get; set; } = "";
        public bool   IsPrimaryKey { get; set; }
        public Type   PropertyType { get; set; } = typeof(object);
    }
}
