#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Base specification pattern for encapsulating query logic and filters.
/// Provides a clean way to compose complex queries with reusable specifications.
/// </summary>
public abstract class Specification<T> where T : class
{
    /// <summary>Predicate to filter results</summary>
    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    /// <summary>Properties to include (eager loading)</summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    /// <summary>Raw SQL includes for eager loading</summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>Order by expression</summary>
    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    /// <summary>Order by descending expression</summary>
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    /// <summary>Page number for pagination</summary>
    public int? PageNumber { get; protected set; }

    /// <summary>Page size for pagination</summary>
    public int? PageSize { get; protected set; }

    /// <summary>Whether to take all items without pagination</summary>
    public bool IsPagingEnabled { get; protected set; }

    /// <summary>Adds an include for eager loading</summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>Adds a string-based include</summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>Applies pagination</summary>
    protected virtual void ApplyPaging(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        IsPagingEnabled = true;
    }

    /// <summary>Applies ordering</summary>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>Applies descending ordering</summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
}

/// <summary>
/// User specification for common user queries
/// </summary>
public sealed class ActiveUsersSpecification : Specification<User>
{
    public ActiveUsersSpecification()
    {
        Criteria = u => u.IsActive;
        ApplyOrderBy(u => u.CreatedDate);
    }
}

public sealed class UserByIdSpecification : Specification<User>
{
    public UserByIdSpecification(int userId)
    {
        Criteria = u => u.Id == userId;
        AddInclude("Orders");
    }
}

public sealed class UsersByEmailSpecification : Specification<User>
{
    public UsersByEmailSpecification(string email)
    {
        Criteria = u => u.Email == email;
    }
}

/// <summary>
/// Product specifications
/// </summary>
public sealed class ActiveProductsSpecification : Specification<Product>
{
    public ActiveProductsSpecification()
    {
        Criteria = p => p.IsActive;
        ApplyOrderBy(p => p.Name);
    }
}

public sealed class ProductsByPriceRangeSpecification : Specification<Product>
{
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        Criteria = p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive;
        ApplyOrderBy(p => p.Price);
    }
}

public sealed class LowStockProductsSpecification : Specification<Product>
{
    public LowStockProductsSpecification(int lowStockThreshold)
    {
        Criteria = p => p.StockQuantity < lowStockThreshold && p.IsActive;
        ApplyOrderBy(p => p.StockQuantity);
    }
}

/// <summary>
/// Order specifications
/// </summary>
public sealed class UserOrdersSpecification : Specification<Order>
{
    public UserOrdersSpecification(int userId)
    {
        Criteria = o => o.UserId == userId;
        AddInclude("Items");
        ApplyOrderByDescending(o => o.CreatedDate);
    }
}

public sealed class PendingOrdersSpecification : Specification<Order>
{
    public PendingOrdersSpecification()
    {
        Criteria = o => o.Status == "Pending" || o.Status == "Confirmed";
        AddInclude("Items");
        AddInclude("User");
        ApplyOrderBy(o => o.CreatedDate);
    }
}

public sealed class RecentOrdersSpecification : Specification<Order>
{
    public RecentOrdersSpecification(int daysBefore = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBefore);
        Criteria = o => o.CreatedDate >= cutoffDate;
        ApplyOrderByDescending(o => o.CreatedDate);
        ApplyPaging(1, 50);
    }
}

