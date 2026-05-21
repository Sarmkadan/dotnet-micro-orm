#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Generic repository interface providing CRUD, query, pagination, and bulk operations
/// for entities derived from <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">The entity type. Must inherit from <see cref="BaseEntity"/>.</typeparam>
/// <remarks>
/// Implementations should be registered as scoped services in the DI container.
/// Use <see cref="IUnitOfWork"/> to coordinate transactional changes across multiple repositories.
/// For complex queries, prefer <see cref="IQueryBuilder{T}"/> obtained via <see cref="Query"/>.
/// </remarks>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Retrieves an entity by its primary key, or <c>null</c> if not found.</summary>
    /// <param name="id">The primary key value.</param>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Returns the first entity matching <paramref name="predicate"/>, or <c>null</c>.</summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Returns all entities of type <typeparamref name="T"/>. Use with caution on large tables.</summary>
    Task<List<T>> GetAllAsync();

    /// <summary>Returns all entities matching the given predicate.</summary>
    Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Counts entities, optionally filtered by <paramref name="predicate"/>.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>Returns <c>true</c> if at least one entity matches <paramref name="predicate"/>.</summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Inserts a new entity and returns it with the assigned primary key.</summary>
    Task<T> AddAsync(T entity);

    /// <summary>Updates an existing entity and returns the updated instance.</summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>Deletes an entity by primary key. Returns <c>true</c> if the entity existed.</summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>Deletes the given entity. Returns <c>true</c> if successful.</summary>
    Task<bool> DeleteAsync(T entity);

    /// <summary>Inserts multiple entities in a single operation.</summary>
    Task<List<T>> AddRangeAsync(List<T> entities);

    /// <summary>Deletes multiple entities. Returns the number of entities removed.</summary>
    Task<int> DeleteRangeAsync(List<T> entities);

    /// <summary>Returns a page of entities with 1-based page numbering.</summary>
    /// <param name="pageNumber">1-based page index.</param>
    /// <param name="pageSize">Maximum number of items per page.</param>
    /// <param name="predicate">Optional filter applied before paging.</param>
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);

    /// <summary>Returns a page of entities along with the total count (useful for pagination UIs).</summary>
    Task<(List<T> Items, int TotalCount)> GetPagedWithCountAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);

    /// <summary>Returns an <see cref="IQueryable{T}"/> for building complex LINQ queries.</summary>
    IQueryable<T> Query();
}

/// <summary>
/// Unit of work interface for coordinating transactional changes across multiple repositories.
/// Implements <see cref="IAsyncDisposable"/> - always use within a <c>await using</c> block
/// to ensure connections are properly released.
/// </summary>
/// <example>
/// <code>
/// await using var uow = serviceProvider.GetRequiredService&lt;IUnitOfWork&gt;();
/// await uow.BeginTransactionAsync();
/// var userRepo = uow.Repository&lt;User&gt;();
/// await userRepo.AddAsync(new User { Name = "Alice" });
/// await uow.CommitAsync();
/// </code>
/// </example>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Gets a repository instance for the specified entity type within this unit of work.</summary>
    IRepository<T> Repository<T>() where T : BaseEntity;

    /// <summary>Begins a database transaction with the specified isolation level.</summary>
    Task<bool> BeginTransactionAsync(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.ReadCommitted);

    /// <summary>Commits the current transaction, persisting all changes.</summary>
    Task<bool> CommitAsync();

    /// <summary>Rolls back the current transaction, discarding all changes.</summary>
    Task<bool> RollbackAsync();

    /// <summary>Flushes pending changes to the database without committing the transaction.</summary>
    Task<int> SaveChangesAsync();

    /// <summary>Returns <c>true</c> if there are uncommitted changes tracked by this unit of work.</summary>
    bool HasChanges();
}

/// <summary>
/// Database context interface
/// </summary>
public interface IDatabaseContext : IAsyncDisposable
{
    Task<bool> OpenAsync();
    Task<bool> CloseAsync();
    Task<bool> TestConnectionAsync();
    Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    DatabaseProvider GetDatabaseProvider();
    string GetConnectionString();
}

/// <summary>
/// Fluent query builder for composing complex queries with filtering, ordering, and paging.
/// All builder methods return <c>this</c> for method chaining. Terminal operations
/// (<see cref="ToListAsync"/>, <see cref="FirstOrDefaultAsync"/>, <see cref="CountAsync"/>)
/// execute the query.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
/// <example>
/// <code>
/// var results = await queryBuilder
///     .Where(u => u.IsActive)
///     .OrderBy(u => u.Name)
///     .Skip(20)
///     .Take(10)
///     .ToListAsync();
/// </code>
/// </example>
public interface IQueryBuilder<T> where T : BaseEntity
{
    /// <summary>Adds a filter predicate. Only one Where clause is supported; subsequent calls overwrite.</summary>
    IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);

    /// <summary>Orders results by the specified key in ascending order.</summary>
    IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

    /// <summary>Orders results by the specified key in descending order.</summary>
    IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

    /// <summary>Limits the result set to <paramref name="count"/> items.</summary>
    IQueryBuilder<T> Take(int count);

    /// <summary>Skips the first <paramref name="count"/> items (for offset-based pagination).</summary>
    IQueryBuilder<T> Skip(int count);

    /// <summary>Eagerly loads a navigation property (reserved for future lazy-loading support).</summary>
    IQueryBuilder<T> Include(Expression<Func<T, object>> navigationProperty);

    /// <summary>Executes the query and returns all matching entities as a list.</summary>
    Task<List<T>> ToListAsync();

    /// <summary>Executes the query and returns the first matching entity, or <c>null</c>.</summary>
    Task<T?> FirstOrDefaultAsync();

    /// <summary>Executes the query and returns the count of matching entities.</summary>
    Task<int> CountAsync();
}

/// <summary>
/// Interface for high-throughput bulk insert, update, and delete operations.
/// Operations are split into chunks of <paramref name="batchSize"/> to limit
/// memory consumption and database lock duration.
/// </summary>
/// <typeparam name="T">The entity type for bulk operations.</typeparam>
public interface IBatchOperation<T> where T : BaseEntity
{
    /// <summary>Bulk-inserts entities in batches. Returns the total number of rows inserted.</summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="batchSize">Maximum entities per database round-trip (default: 1000).</param>
    Task<int> InsertBatchAsync(List<T> entities, int batchSize = 1000);

    /// <summary>Bulk-updates entities in batches. Returns the total number of rows affected.</summary>
    Task<int> UpdateBatchAsync(List<T> entities, int batchSize = 1000);

    /// <summary>Bulk-deletes entities by primary key in batches.</summary>
    Task<int> DeleteBatchAsync(List<int> ids, int batchSize = 1000);

    /// <summary>Bulk-deletes entities matching a predicate in batches.</summary>
    Task<int> DeleteBatchAsync(Expression<Func<T, bool>> predicate, int batchSize = 1000);
}
