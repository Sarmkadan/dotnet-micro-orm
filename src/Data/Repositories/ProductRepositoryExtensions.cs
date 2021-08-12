#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using System.Linq;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Extension methods for ProductRepository providing additional query capabilities
/// </summary>
public static class ProductRepositoryExtensions
{
    /// <summary>
    /// Gets products by their SKUs (case-insensitive)
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="skus">List of SKUs to retrieve</param>
    /// <returns>List of matching products</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="skus"/> is <see langword="null"/></exception>
    public static async Task<List<Product>> GetBySkusAsync(this ProductRepository repository, IEnumerable<string> skus)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(skus);

        if (!skus.Any())
            return [];

        var products = await repository.GetAllAsync();
        var skuSet = new HashSet<string>(skus, StringComparer.OrdinalIgnoreCase);
        return products.Where(p => skuSet.Contains(p.Sku)).ToList();
    }

    /// <summary>
    /// Gets products by their IDs
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="ids">List of product IDs to retrieve</param>
    /// <returns>List of matching products</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="ids"/> is <see langword="null"/></exception>
    public static async Task<List<Product>> GetByIdsAsync(this ProductRepository repository, IEnumerable<int> ids)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(ids);

        if (!ids.Any())
            return [];

        var products = await repository.GetAllAsync();
        var idSet = new HashSet<int>(ids);
        return products.Where(p => idSet.Contains(p.Id)).ToList();
    }

    /// <summary>
    /// Gets products within a price range with optional sorting and filtering
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="minPrice">Minimum price (inclusive)</param>
    /// <param name="maxPrice">Maximum price (inclusive)</param>
    /// <param name="sortBy">Sorting option: PriceAsc, PriceDesc, Name, ProfitDesc</param>
    /// <param name="includeInactive">Whether to include inactive products</param>
    /// <returns>List of products matching criteria</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minPrice"/> is negative</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxPrice"/> is less than <paramref name="minPrice"/></exception>
    public static async Task<List<Product>> GetByPriceRangeAsync(
        this ProductRepository repository,
        decimal minPrice,
        decimal maxPrice,
        string sortBy = "PriceAsc",
        bool includeInactive = false)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice), "Minimum price cannot be negative");

        if (maxPrice <= minPrice)
            return [];

        var products = includeInactive
            ? await repository.GetAllAsync()
            : await repository.GetActiveProductsAsync();

        var result = products
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToList();

        return sortBy.ToLowerInvariant() switch
        {
            "priceasc" => result.OrderBy(p => p.Price).ToList(),
            "pricedesc" => result.OrderByDescending(p => p.Price).ToList(),
            "name" => result.OrderBy(p => p.Name).ToList(),
            "profitdesc" => result
                .Where(p => p.CostPrice.HasValue && p.CostPrice > 0)
                .OrderByDescending(p => p.GetProfit())
                .ToList(),
            _ => result.OrderBy(p => p.Price).ToList()
        };
    }

    /// <summary>
    /// Gets products by category with pagination support
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="categoryId">Category ID to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="sortBy">Sorting option: Name, PriceAsc, PriceDesc, StockAsc, StockDesc</param>
    /// <returns>Paginated list of products with total count</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="categoryId"/> is not positive</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> is less than 1</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than 1 or greater than 100</exception>
    public static async Task<(List<Product> Items, int TotalCount)> GetByCategoryPagedAsync(
        this ProductRepository repository,
        int categoryId,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Name")
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (categoryId <= 0)
            throw new ArgumentOutOfRangeException(nameof(categoryId), "Category ID must be positive");

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be 1 or greater");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100");

        var allProducts = await repository.GetActiveProductsAsync();
        var categoryProducts = allProducts.Where(p => p.CategoryId == categoryId).ToList();

        var totalCount = categoryProducts.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (pageNumber > totalPages && totalPages > 0)
            pageNumber = totalPages;

        var query = categoryProducts.AsQueryable();

        var orderedQuery = sortBy.ToLowerInvariant() switch
        {
            "priceasc" => query.OrderBy(p => p.Price),
            "pricedesc" => query.OrderByDescending(p => p.Price),
            "stockasc" => query.OrderBy(p => p.StockQuantity),
            "stockdesc" => query.OrderByDescending(p => p.StockQuantity),
            _ => query.OrderBy(p => p.Name)
        };

        var pagedProducts = orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedProducts, totalCount);
    }

    /// <summary>
    /// Gets products that are below a specific stock threshold
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <param name="includeOutOfStock">Whether to include products with zero stock</param>
    /// <returns>List of low stock products</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="threshold"/> is negative</exception>
    public static async Task<List<Product>> GetLowStockProductsAsync(
        this ProductRepository repository,
        int threshold = 10,
        bool includeOutOfStock = true)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (threshold < 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative");

        var activeProducts = await repository.GetActiveProductsAsync();
        var result = activeProducts
            .Where(p => p.StockQuantity <= threshold)
            .ToList();

        if (!includeOutOfStock)
            result = result.Where(p => p.StockQuantity > 0).ToList();

        return result.OrderBy(p => p.StockQuantity).ToList();
    }

    /// <summary>
    /// Gets products with the highest profit margin
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="topCount">Number of top products to return</param>
    /// <param name="minProfitMargin">Minimum profit margin percentage (0-100)</param>
    /// <returns>List of most profitable products</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="topCount"/> is less than 1</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minProfitMargin"/> is less than 0 or greater than 100</exception>
    public static async Task<List<Product>> GetMostProfitableAsync(
        this ProductRepository repository,
        int topCount = 10,
        decimal minProfitMargin = 0)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (topCount < 1)
            throw new ArgumentOutOfRangeException(nameof(topCount), "Top count must be 1 or greater");

        if (minProfitMargin < 0 || minProfitMargin > 100)
            throw new ArgumentOutOfRangeException(nameof(minProfitMargin), "Profit margin must be between 0 and 100");

        var activeProducts = await repository.GetActiveProductsAsync();

        var profitableProducts = activeProducts
            .Where(p => p.CostPrice.HasValue && p.CostPrice > 0)
            .Where(p => p.Price > 0 && (p.Price - p.CostPrice.Value) / p.Price * 100 >= minProfitMargin)
            .OrderByDescending(p => p.GetProfit())
            .Take(topCount)
            .ToList();

        return profitableProducts;
    }

    /// <summary>
    /// Gets products by multiple category IDs
    /// </summary>
    /// <param name="repository">The product repository</param>
    /// <param name="categoryIds">List of category IDs to filter by</param>
    /// <returns>List of products in specified categories</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="categoryIds"/> is <see langword="null"/></exception>
    public static async Task<List<Product>> GetByCategoryIdsAsync(this ProductRepository repository, IEnumerable<int> categoryIds)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(categoryIds);

        if (!categoryIds.Any())
            return [];

        var activeProducts = await repository.GetActiveProductsAsync();
        var categoryIdSet = new HashSet<int>(categoryIds);
        return activeProducts.Where(p => categoryIdSet.Contains(p.CategoryId)).ToList();
    }
}