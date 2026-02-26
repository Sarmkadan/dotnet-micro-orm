#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

/// <summary>
/// Represents a paginated result set with metadata for navigation.
/// Used for list endpoints to provide efficient data retrieval and pagination.
/// </summary>
public class sealed PagedResult<T>
{
    /// <summary>The items in the current page</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Total number of items across all pages</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number (1-based)</summary>
    public int PageNumber { get; set; }

    /// <summary>Number of items per page</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages</summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>Whether there are more pages after this one</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Whether there are previous pages</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Order by field name</summary>
    public string? OrderBy { get; set; }

    /// <summary>Sort direction (asc or desc)</summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>Number of items skipped</summary>
    public int SkippedItems => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Creates a paged result from a complete list
    /// </summary>
    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount,
        string? orderBy = null,
        string sortDirection = "asc")
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

        return new PagedResult<T>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = orderBy,
            SortDirection = sortDirection
        };
    }

    /// <summary>
    /// Creates a paged result from a queryable
    /// </summary>
    public static PagedResult<T> FromQueryable(
        IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var total = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Create(items, pageNumber, pageSize, total);
    }

    /// <summary>
    /// Maps items to a different type while preserving pagination metadata
    /// </summary>
    public PagedResult<TNew> Map<TNew>(Func<T, TNew> selector)
    {
        return new PagedResult<TNew>
        {
            Items = Items.Select(selector).ToList(),
            TotalCount = TotalCount,
            PageNumber = PageNumber,
            PageSize = PageSize,
            OrderBy = OrderBy,
            SortDirection = SortDirection
        };
    }

    /// <summary>
    /// Gets the next page configuration
    /// </summary>
    public PaginationInfo GetNextPageInfo()
    {
        if (!HasNextPage)
            throw new InvalidOperationException("No next page available");

        return new PaginationInfo
        {
            PageNumber = PageNumber + 1,
            PageSize = PageSize
        };
    }

    /// <summary>
    /// Gets the previous page configuration
    /// </summary>
    public PaginationInfo GetPreviousPageInfo()
    {
        if (!HasPreviousPage)
            throw new InvalidOperationException("No previous page available");

        return new PaginationInfo
        {
            PageNumber = PageNumber - 1,
            PageSize = PageSize
        };
    }
}

/// <summary>
/// Pagination information for requesting specific pages
/// </summary>
public class sealed PaginationInfo
{
    /// <summary>Page number (1-based)</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Number of items per page</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Field to order by</summary>
    public string? OrderBy { get; set; }

    /// <summary>Sort direction (asc or desc)</summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>Validates pagination parameters</summary>
    public bool IsValid => PageNumber >= 1 && PageSize >= 1 && PageSize <= 1000;

    /// <summary>Gets number of items to skip</summary>
    public int GetSkip() => (PageNumber - 1) * PageSize;

    /// <summary>Gets number of items to take</summary>
    public int GetTake() => PageSize;

    /// <summary>Creates a pagination info with default values</summary>
    public static PaginationInfo Default => new();

    /// <summary>Creates a pagination info with specific page and size</summary>
    public static PaginationInfo Create(int pageNumber, int pageSize) =>
        new() { PageNumber = pageNumber, PageSize = pageSize };
}

/// <summary>
/// Pagination request from API
/// </summary>
public class sealed PaginationRequest
{
    /// <summary>Page number (1-based), defaults to 1</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page, defaults to 10</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Field to sort by</summary>
    public string? Sort { get; set; }

    /// <summary>Sort direction (asc/desc), defaults to asc</summary>
    public string Order { get; set; } = "asc";

    /// <summary>Search query/filter</summary>
    public string? Search { get; set; }

    /// <summary>Converts to pagination info</summary>
    public PaginationInfo ToPaginationInfo() => new()
    {
        PageNumber = Math.Max(1, Page),
        PageSize = Math.Min(Math.Max(1, PageSize), 1000), // Clamp to 1-1000
        OrderBy = Sort,
        SortDirection = Order?.ToLowerInvariant() == "desc" ? "desc" : "asc"
    };
}
