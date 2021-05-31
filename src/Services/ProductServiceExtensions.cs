#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetMicroOrm.Services;

using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Extension methods for ProductService providing enhanced functionality
/// </summary>
public static class ProductServiceExtensions
{
    /// <summary>
    /// Creates a new product with validation and returns the created product
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="sku">Product SKU (must be unique)</param>
    /// <param name="name">Product name</param>
    /// <param name="price">Product price (must be positive)</param>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="description">Optional product description</param>
    /// <returns>The created Product instance</returns>
    public static async Task<Product> CreateProductAsync(
        this ProductService service,
        string sku,
        string name,
        decimal price,
        int categoryId,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Length < 3)
            throw new ArgumentException("SKU must be at least 3 characters");

        if (price <= 0)
            throw new ArgumentException("Price must be positive");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required");

        return await service.CreateProductAsync(sku, name, price, categoryId, description);
    }

    /// <summary>
    /// Bulk creates multiple products with validation
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="products">Collection of product data to create</param>
    /// <returns>List of created Product instances</returns>
    public static async Task<List<Product>> CreateProductsBulkAsync(
        this ProductService service,
        IEnumerable<(string Sku, string Name, decimal Price, int CategoryId, string? Description)> products
    )
    {
        var createdProducts = new List<Product>();

        foreach (var product in products)
        {
            var created = await service.CreateProductAsync(
                product.Sku,
                product.Name,
                product.Price,
                product.CategoryId,
                product.Description
            );
            createdProducts.Add(created);
        }

        return createdProducts;
    }

    /// <summary>
    /// Gets product by SKU for quick lookup
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="sku">Product SKU to find</param>
    /// <returns>Product if found, null otherwise</returns>
    public static async Task<Product?> GetProductBySkuAsync(this ProductService service, string sku)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Length < 3)
            throw new ArgumentException("SKU must be at least 3 characters");

        var allProducts = await service.GetActiveProductsAsync();
        return allProducts.FirstOrDefault(p => string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Updates product price with validation
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="productId">Product identifier</param>
    /// <param name="newPrice">New price (must be positive)</param>
    /// <returns>The updated Product instance</returns>
    public static async Task<Product> UpdateProductPriceAsync(
        this ProductService service,
        int productId,
        decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive");

        return await service.UpdateProductAsync(productId, price: newPrice);
    }

    /// <summary>
    /// Updates product name
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="productId">Product identifier</param>
    /// <param name="newName">New product name</param>
    /// <returns>The updated Product instance</returns>
    public static async Task<Product> UpdateProductNameAsync(
        this ProductService service,
        int productId,
        string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Product name is required");

        return await service.UpdateProductAsync(productId, name: newName);
    }

    /// <summary>
    /// Adds stock to multiple products in a single operation
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="stockUpdates">Collection of product stock updates</param>
    /// <returns>List of updated Product instances</returns>
    public static async Task<List<Product>> IncreaseStockBulkAsync(
        this ProductService service,
        IEnumerable<(int ProductId, int Quantity)> stockUpdates)
    {
        var updatedProducts = new List<Product>();

        foreach (var update in stockUpdates)
        {
            var updated = await service.IncreaseStockAsync(update.ProductId, update.Quantity);
            updatedProducts.Add(updated);
        }

        return updatedProducts;
    }

    /// <summary>
    /// Gets products by multiple category IDs
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="categoryIds">Collection of category identifiers</param>
    /// <returns>List of products in specified categories</returns>
    public static async Task<List<Product>> GetProductsByCategoriesAsync(
        this ProductService service,
        IEnumerable<int> categoryIds)
    {
        var allProducts = await service.GetActiveProductsAsync();
        var categorySet = new HashSet<int>(categoryIds);

        return allProducts.Where(p => categorySet.Contains(p.CategoryId)).ToList();
    }

    /// <summary>
    /// Searches products by name with case-insensitive comparison
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="searchTerm">Search term to match against product names</param>
    /// <returns>List of matching products</returns>
    public static async Task<List<Product>> SearchProductsCaseInsensitiveAsync(
        this ProductService service,
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Product>();

        var products = await service.SearchProductsAsync(searchTerm);
        return products;
    }

    /// <summary>
    /// Gets products within a specific price range (inclusive)
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="minPrice">Minimum price (inclusive)</param>
    /// <param name="maxPrice">Maximum price (inclusive)</param>
    /// <returns>List of products within price range</returns>
    public static async Task<List<Product>> GetProductsInPriceRangeAsync(
        this ProductService service,
        decimal minPrice,
        decimal maxPrice)
    {
        if (minPrice < 0)
            throw new ArgumentException("Minimum price cannot be negative");

        if (maxPrice < minPrice)
            throw new ArgumentException("Maximum price must be greater than or equal to minimum price");

        return await service.GetProductsByPriceAsync(minPrice, maxPrice);
    }

    /// <summary>
    /// Gets the total inventory value across all active products
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <returns>Total inventory value</returns>
    public static async Task<decimal> GetTotalInventoryValueAsync(this ProductService service)
    {
        return await service.GetInventoryValueAsync();
    }

    /// <summary>
    /// Gets the count of products that are low stock (below threshold)
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <param name="threshold">Stock threshold for low stock (default: 10)</param>
    /// <returns>Count of low stock products</returns>
    public static async Task<int> GetLowStockProductCountAsync(
        this ProductService service,
        int threshold = 10)
    {
        var lowStockProducts = await service.GetLowStockProductsAsync(threshold);
        return lowStockProducts.Count;
    }

    /// <summary>
    /// Gets the count of products that are out of stock
    /// </summary>
    /// <param name="service">The ProductService instance</param>
    /// <returns>Count of out of stock products</returns>
    public static async Task<int> GetOutOfStockProductCountAsync(this ProductService service)
    {
        var outOfStockProducts = await service.GetOutOfStockProductsAsync();
        return outOfStockProducts.Count;
    }
}