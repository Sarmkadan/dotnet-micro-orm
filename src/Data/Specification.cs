#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;

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
public class sealed ActiveUsersSpecification : Specification<User>
{
    public ActiveUsersSpecification()
    {
        Criteria = u => u.IsActive;
        ApplyOrderBy(u => u.CreatedAt);
    }
}

public class sealed UserByIdSpecification : Specification<User>
{
    public UserByIdSpecification(int userId)
    {
        Criteria = u => u.Id == userId;
        AddInclude("Orders");
    }
}

public class sealed UsersByEmailSpecification : Specification<User>
{
    public UsersByEmailSpecification(string email)
    {
        Criteria = u => u.Email == email;
    }
}

/// <summary>
/// Product specifications
/// </summary>
public class sealed ActiveProductsSpecification : Specification<Product>
{
    public ActiveProductsSpecification()
    {
        Criteria = p => p.IsActive;
        ApplyOrderBy(p => p.Name);
    }
}

public class sealed ProductsByPriceRangeSpecification : Specification<Product>
{
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        Criteria = p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive;
        ApplyOrderBy(p => p.Price);
    }
}

public class sealed LowStockProductsSpecification : Specification<Product>
{
    public LowStockProductsSpecification(int lowStockThreshold)
    {
        Criteria = p => p.Inventory.Quantity < lowStockThreshold && p.IsActive;
        AddInclude("Inventory");
        ApplyOrderBy(p => p.Inventory.Quantity);
    }
}

/// <summary>
/// Order specifications
/// </summary>
public class sealed UserOrdersSpecification : Specification<Order>
{
    public UserOrdersSpecification(int userId)
    {
        Criteria = o => o.UserId == userId;
        AddInclude("Items");
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}

public class sealed PendingOrdersSpecification : Specification<Order>
{
    public PendingOrdersSpecification()
    {
        Criteria = o => o.Status == "Pending" || o.Status == "Confirmed";
        AddInclude("Items");
        AddInclude("User");
        ApplyOrderBy(o => o.CreatedAt);
    }
}

public class sealed RecentOrdersSpecification : Specification<Order>
{
    public RecentOrdersSpecification(int daysBefore = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBefore);
        Criteria = o => o.CreatedAt >= cutoffDate;
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging(1, 50);
    }
}

/// <summary>
/// Placeholder domain model interfaces for specification implementation
/// </summary>
public interface User
{
    int Id { get; }
    string Email { get; }
    bool IsActive { get; }
    DateTime CreatedAt { get; }
}

public interface Product
{
    int Id { get; }
    string Name { get; }
    decimal Price { get; }
    bool IsActive { get; }
    Inventory Inventory { get; }
}

public interface Inventory
{
    int Quantity { get; }
}

public interface Order
{
    int Id { get; }
    int UserId { get; }
    string Status { get; }
    DateTime CreatedAt { get; }
    List<OrderItem> Items { get; }
}

public interface OrderItem
{
    int Id { get; }
}
