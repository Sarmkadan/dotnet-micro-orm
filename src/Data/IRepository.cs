// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entities);
    Task<int> DeleteRangeAsync(List<T> entities);
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);
    Task<(List<T> Items, int TotalCount)> GetPagedWithCountAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);
    IQueryable<T> Query();
}

/// <summary>
/// Unit of work interface for transaction management
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<bool> BeginTransactionAsync(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.ReadCommitted);
    Task<bool> CommitAsync();
    Task<bool> RollbackAsync();
    Task<int> SaveChangesAsync();
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
/// Query builder interface for complex queries
/// </summary>
public interface IQueryBuilder<T> where T : BaseEntity
{
    IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);
    IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IQueryBuilder<T> Take(int count);
    IQueryBuilder<T> Skip(int count);
    IQueryBuilder<T> Include(Expression<Func<T, object>> navigationProperty);
    Task<List<T>> ToListAsync();
    Task<T?> FirstOrDefaultAsync();
    Task<int> CountAsync();
}

/// <summary>
/// Batch operation interface
/// </summary>
public interface IBatchOperation<T> where T : BaseEntity
{
    Task<int> InsertBatchAsync(List<T> entities, int batchSize = 1000);
    Task<int> UpdateBatchAsync(List<T> entities, int batchSize = 1000);
    Task<int> DeleteBatchAsync(List<int> ids, int batchSize = 1000);
    Task<int> DeleteBatchAsync(Expression<Func<T, bool>> predicate, int batchSize = 1000);
}
