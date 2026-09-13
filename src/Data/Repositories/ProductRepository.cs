#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Product-specific repository with inventory and catalog operations
/// </summary>
public sealed class ProductRepository : Repository<Product>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used for data access.</param>
    public ProductRepository(IDatabaseContext context) : base(context) { }

    /// <summary>
    /// Returns the product matching the given SKU, or <c>null</c> if none is found.
    /// </summary>
    /// <param name="sku">The SKU to search for. Matching is case-insensitive.</param>
    /// <returns>The matching product, or <c>null</c> if not found or <paramref name="sku"/> is blank.</returns>
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        ArgumentNullException.ThrowIfNull(sku);

        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var products = await GetAllAsync();
        return products.FirstOrDefault(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the active products belonging to the given category.
    /// </summary>
    /// <param name="categoryId">The category identifier to filter by.</param>
    /// <returns>The matching active products, or an empty list if <paramref name="categoryId"/> is not positive.</returns>
    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        if (categoryId <= 0)
            return [];

        var products = await GetAllAsync();
        return products.Where(p => p.CategoryId == categoryId && p.IsActive).ToList();
    }

    /// <summary>
    /// Returns all active products.
    /// </summary>
    /// <returns>A list of all active products.</returns>
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        var products = await GetAllAsync();
        return products.Where(p => p.IsActive).ToList();
    }

    /// <summary>
    /// Returns the active products whose stock quantity is at or below the given threshold.
    /// </summary>
    /// <param name="threshold">The maximum stock quantity to include. Defaults to <c>10</c>.</param>
    /// <returns>A list of active products with low stock.</returns>
    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.StockQuantity <= threshold).ToList();
    }

    /// <summary>
    /// Returns the active products that are currently out of stock.
    /// </summary>
    /// <returns>A list of active products with zero stock.</returns>
    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.StockQuantity == 0).ToList();
    }

    /// <summary>
    /// Returns the active products whose price falls within the given inclusive range.
    /// </summary>
    /// <param name="minPrice">The minimum price (inclusive).</param>
    /// <param name="maxPrice">The maximum price (inclusive).</param>
    /// <returns>A list of active products within the price range.</returns>
    public async Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
    }

    /// <summary>
    /// Returns the active products whose price is above the given threshold, ordered by price descending.
    /// </summary>
    /// <param name="priceThreshold">The minimum price to qualify as expensive. Defaults to <c>100</c>.</param>
    /// <returns>A list of expensive active products, sorted by price (highest first).</returns>
    public async Task<List<Product>> GetExpensiveProductsAsync(decimal priceThreshold = 100m)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.Price > priceThreshold).OrderByDescending(p => p.Price).ToList();
    }

    /// <summary>
    /// Returns the active products whose name contains the given search term (case-insensitive).
    /// </summary>
    /// <param name="searchTerm">The term to search for within product names.</param>
    /// <returns>A list of active products matching the search term, or an empty list if <paramref name="searchTerm"/> is blank.</returns>
    public async Task<List<Product>> SearchByNameAsync(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);

        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        var products = await GetActiveProductsAsync();
        var term = searchTerm.ToLowerInvariant();
        return products.Where(p => p.Name.ToLowerInvariant().Contains(term)).ToList();
    }

    /// <summary>
    /// Returns the most profitable active products, ordered by profit descending.
    /// </summary>
    /// <param name="topCount">The maximum number of products to return. Defaults to <c>10</c>.</param>
    /// <returns>A list of the most profitable active products.</returns>
    public async Task<List<Product>> GetMostProfitableAsync(int topCount = 10)
    {
        var products = await GetActiveProductsAsync();
        return products
            .Where(p => p.CostPrice.HasValue && p.CostPrice > 0)
            .OrderByDescending(p => p.GetProfit())
            .Take(topCount)
            .ToList();
    }

    /// <summary>
    /// Calculates the total inventory value of all active products.
    /// </summary>
    /// <returns>The sum of (price × stock quantity) for all active products.</returns>
    public async Task<decimal> GetInventoryValueAsync()
    {
        var products = await GetActiveProductsAsync();
        return products.Sum(p => p.Price * p.StockQuantity);
    }
}
