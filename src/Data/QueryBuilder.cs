#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Fluent query builder that composes LINQ expressions for filtering, ordering,
/// and paginating <typeparamref name="T"/> entities. Implements <see cref="IQueryBuilder{T}"/>.
/// </summary>
/// <typeparam name="T">The entity type being queried. Must derive from <see cref="BaseEntity"/>.</typeparam>
/// <remarks>
/// The builder is stateful - each method call modifies internal state and returns <c>this</c>.
/// Terminal methods (<see cref="ToListAsync"/>, <see cref="FirstOrDefaultAsync"/>,
/// <see cref="CountAsync"/>) materialize the query against the underlying repository.
/// A single <see cref="QueryBuilder{T}"/> instance should not be reused across multiple queries.
/// </remarks>
public sealed class QueryBuilder<T> : IQueryBuilder<T> where T : BaseEntity
{
    private readonly IRepository<T> _repository;
    private Expression<Func<T, bool>>? _whereClause;
    private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;
    private int? _take;
    private int? _skip;

    public QueryBuilder(IRepository<T> repository)
    {
        _repository = repository;
    }

    public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _whereClause = predicate;
        return this;
    }

    public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var query = _repository.Query();
        _orderBy = q => q.OrderBy(keySelector);
        return this;
    }

    public IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        var query = _repository.Query();
        _orderBy = q => q.OrderByDescending(keySelector);
        return this;
    }

    public IQueryBuilder<T> Take(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Take count must be greater than zero");
        _take = count;
        return this;
    }

    public IQueryBuilder<T> Skip(int count)
    {
        if (count < 0)
            throw new ArgumentException("Skip count cannot be negative");
        _skip = count;
        return this;
    }

    public IQueryBuilder<T> Include(Expression<Func<T, object>> navigationProperty)
    {
        // Placeholder for lazy loading support in future versions
        return this;
    }

    public async Task<List<T>> ToListAsync()
    {
        var query = _repository.Query();

        if (_whereClause is not null)
            query = query.Where(_whereClause);

        if (_orderBy is not null)
            query = _orderBy(query);

        if (_skip.HasValue)
            query = query.Skip(_skip.Value);

        if (_take.HasValue)
            query = query.Take(_take.Value);

        return await Task.FromResult(query.ToList());
    }

    public async Task<T?> FirstOrDefaultAsync()
    {
        var query = _repository.Query();

        if (_whereClause is not null)
            query = query.Where(_whereClause);

        if (_orderBy is not null)
            query = _orderBy(query);

        return await Task.FromResult(query.FirstOrDefault());
    }

    public async Task<int> CountAsync()
    {
        var query = _repository.Query();

        if (_whereClause is not null)
            query = query.Where(_whereClause);

        return await Task.FromResult(query.Count());
    }
}
