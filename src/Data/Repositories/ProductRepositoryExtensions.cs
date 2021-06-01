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
    public static async Task<List<Product>> GetBySkusAsync(this ProductRepository repository, IEnumerable<string> skus)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (skus is null || !skus.Any())
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
    public static async Task<List<Product>> GetByIdsAsync(this ProductRepository repository, IEnumerable<int> ids)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (ids is null || !ids.Any())
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
    public static async Task<List<Product>> GetByPriceRangeAsync(
        this ProductRepository repository,
        decimal minPrice,
        decimal maxPrice,
        string sortBy = "PriceAsc",
        bool includeInactive = false)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (minPrice < 0)
            minPrice = 0;

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
    public static async Task<(List<Product> Items, int TotalCount)> GetByCategoryPagedAsync(
        this ProductRepository repository,
        int categoryId,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Name")
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (categoryId <= 0)
            return ([], 0);

        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1 || pageSize > 100)
            pageSize = 20;

        var allProducts = await repository.GetActiveProductsAsync();
        var categoryProducts = allProducts.Where(p => p.CategoryId == categoryId).ToList();

        var totalCount = categoryProducts.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (pageNumber > totalPages && totalPages > 0)
            pageNumber = totalPages;

        IOrderedQueryable<Product>? orderedQuery = null;

        var query = categoryProducts.AsQueryable();

        switch (sortBy.ToLowerInvariant())
        {
            case "priceasc":
                orderedQuery = query.OrderBy(p => p.Price);
                break;
            case "pricedesc":
                orderedQuery = query.OrderByDescending(p => p.Price);
                break;
            case "stockasc":
                orderedQuery = query.OrderBy(p => p.StockQuantity);
                break;
            case "stockdesc":
                orderedQuery = query.OrderByDescending(p => p.StockQuantity);
                break;
            default:
                orderedQuery = query.OrderBy(p => p.Name);
                break;
        }

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
    public static async Task<List<Product>> GetLowStockProductsAsync(
        this ProductRepository repository,
        int threshold = 10,
        bool includeOutOfStock = true)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (threshold < 0)
            threshold = 0;

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
    public static async Task<List<Product>> GetMostProfitableAsync(
        this ProductRepository repository,
        int topCount = 10,
        decimal minProfitMargin = 0)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (topCount < 1)
            topCount = 10;

        if (minProfitMargin < 0)
            minProfitMargin = 0;

        if (minProfitMargin > 100)
            minProfitMargin = 100;

        var activeProducts = await repository.GetActiveProductsAsync();

        var profitableProducts = activeProducts
            .Where(p => p.CostPrice.HasValue && p.CostPrice > 0)
            .Where(p => (p.Price - p.CostPrice.Value) / p.Price * 100 >= minProfitMargin)
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
    public static async Task<List<Product>> GetByCategoryIdsAsync(this ProductRepository repository, IEnumerable<int> categoryIds)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (categoryIds is null || !categoryIds.Any())
            return [];

        var activeProducts = await repository.GetActiveProductsAsync();
        var categoryIdSet = new HashSet<int>(categoryIds);
        return activeProducts.Where(p => categoryIdSet.Contains(p.CategoryId)).ToList();
    }
}