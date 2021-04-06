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
public class sealed ProductRepository : Repository<Product>
{
    public ProductRepository(IDatabaseContext context) : base(context) { }

    // Gets product by SKU
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var products = await GetAllAsync();
        return products.FirstOrDefault(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
    }

    // Gets products by category
    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        if (categoryId <= 0)
            return [];

        var products = await GetAllAsync();
        return products.Where(p => p.CategoryId == categoryId && p.IsActive).ToList();
    }

    // Gets active products
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        var products = await GetAllAsync();
        return products.Where(p => p.IsActive).ToList();
    }

    // Gets low stock products
    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.StockQuantity <= threshold).ToList();
    }

    // Gets out of stock products
    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.StockQuantity == 0).ToList();
    }

    // Gets products by price range
    public async Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
    }

    // Gets expensive products
    public async Task<List<Product>> GetExpensiveProductsAsync(decimal priceThreshold = 100m)
    {
        var products = await GetActiveProductsAsync();
        return products.Where(p => p.Price > priceThreshold).OrderByDescending(p => p.Price).ToList();
    }

    // Searches products by name
    public async Task<List<Product>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        var products = await GetActiveProductsAsync();
        var term = searchTerm.ToLowerInvariant();
        return products.Where(p => p.Name.ToLowerInvariant().Contains(term)).ToList();
    }

    // Gets most profitable products
    public async Task<List<Product>> GetMostProfitableAsync(int topCount = 10)
    {
        var products = await GetActiveProductsAsync();
        return products
            .Where(p => p.CostPrice.HasValue && p.CostPrice > 0)
            .OrderByDescending(p => p.GetProfit())
            .Take(topCount)
            .ToList();
    }

    // Gets total inventory value
    public async Task<decimal> GetInventoryValueAsync()
    {
        var products = await GetActiveProductsAsync();
        return products.Sum(p => p.Price * p.StockQuantity);
    }
}
