#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Fluent query builder for complex queries
/// </summary>
public sealed class QueryBuilder<T> : IQueryBuilder<T> where T : BaseEntity
{
    private readonly IRepository<T> _repository;
    private Expression<Func<T, bool>>? _whereClause;
    private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;
    private int? _take;
    private int? _skip;
    private readonly HashSet<string> _includes = [];

    public QueryBuilder(IRepository<T> repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    /// <summary>
    /// Adds a filter to the query. Multiple calls are combined with a logical AND.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null</exception>
    public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _whereClause = _whereClause is null ? predicate : Combine(_whereClause, predicate);
        return this;
    }

    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keySelector"/> is null</exception>
    public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        _orderBy = q => q.OrderBy(keySelector);
        return this;
    }

    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keySelector"/> is null</exception>
    public IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        _orderBy = q => q.OrderByDescending(keySelector);
        return this;
    }

    private static Expression<Func<T, bool>> Combine(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var body = Expression.AndAlso(
            new ParameterRebinder(left.Parameters[0], parameter).Visit(left.Body),
            new ParameterRebinder(right.Parameters[0], parameter).Visit(right.Body));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ParameterRebinder(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
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

    /// <summary>
    /// Records a navigation property to be materialized with the results.
    /// The included member names are exposed through <see cref="IncludedProperties"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="navigationProperty"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the expression does not point at a member of <typeparamref name="T"/></exception>
    public IQueryBuilder<T> Include(Expression<Func<T, object>> navigationProperty)
    {
        ArgumentNullException.ThrowIfNull(navigationProperty);

        var body = navigationProperty.Body is UnaryExpression unary ? unary.Operand : navigationProperty.Body;

        if (body is not MemberExpression member)
            throw new ArgumentException("Navigation property must be a member access expression", nameof(navigationProperty));

        _includes.Add(member.Member.Name);
        return this;
    }

    /// <summary>
    /// Gets the navigation properties registered through <see cref="Include"/>.
    /// </summary>
    public IReadOnlyCollection<string> IncludedProperties => _includes;

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
