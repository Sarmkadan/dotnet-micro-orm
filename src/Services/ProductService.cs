#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Data.Repositories;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Product service for catalog and inventory management
/// </summary>
public class sealed ProductService : IAsyncDisposable
{
    private readonly ProductRepository _productRepository;
    private readonly IDatabaseContext _context;

    public ProductService(IDatabaseContext context)
    {
        _context = context;
        _productRepository = new ProductRepository(context);
    }

    // Creates new product
    public async Task<Product> CreateProductAsync(string sku, string name, decimal price, int categoryId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Length < 3)
            throw new ArgumentException("SKU must be at least 3 characters");

        var existingSku = await _productRepository.GetBySkuAsync(sku);
        if (existingSku is not null)
            throw new InvalidOperationException("Product with this SKU already exists");

        var product = new Product(sku, name, price, categoryId)
        {
            Description = description,
            CreatedDate = DateTime.UtcNow
        };

        return await _productRepository.AddAsync(product);
    }

    // Gets product by id
    public async Task<Product?> GetProductAsync(int productId)
    {
        return await _productRepository.GetByIdAsync(productId);
    }

    // Gets all active products
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _productRepository.GetActiveProductsAsync();
    }

    // Gets products by category
    public async Task<List<Product>> GetCategoryProductsAsync(int categoryId)
    {
        return await _productRepository.GetByCategoryAsync(categoryId);
    }

    // Updates product
    public async Task<Product> UpdateProductAsync(int productId, string? name = null, decimal? price = null, string? description = null)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        if (!string.IsNullOrWhiteSpace(name))
            product.Name = name;

        if (price.HasValue && price > 0)
            product.Price = price.Value;

        if (description is not null)
            product.Description = description;

        product.ModifiedDate = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(product);
    }

    // Updates stock
    public async Task<Product> UpdateStockAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative");

        product.StockQuantity = quantity;
        product.ModifiedDate = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(product);
    }

    // Increases stock
    public async Task<Product> IncreaseStockAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.IncreaseStock(quantity);
        return await _productRepository.UpdateAsync(product);
    }

    // Decreases stock
    public async Task<Product> DecreaseStockAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.DecreaseStock(quantity);
        return await _productRepository.UpdateAsync(product);
    }

    // Gets low stock products
    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _productRepository.GetLowStockProductsAsync(threshold);
    }

    // Gets out of stock products
    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        return await _productRepository.GetOutOfStockProductsAsync();
    }

    // Searches products
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _productRepository.SearchByNameAsync(searchTerm);
    }

    // Gets products by price range
    public async Task<List<Product>> GetProductsByPriceAsync(decimal minPrice, decimal maxPrice)
    {
        return await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
    }

    // Deactivates product
    public async Task<Product> DeactivateProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.IsActive = false;
        product.ModifiedDate = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(product);
    }

    // Gets inventory value
    public async Task<decimal> GetInventoryValueAsync()
    {
        return await _productRepository.GetInventoryValueAsync();
    }

    // Gets product count
    public async Task<int> GetProductCountAsync()
    {
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Count;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
