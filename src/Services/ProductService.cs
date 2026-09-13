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
public sealed class ProductService : IAsyncDisposable
{
    private readonly ProductRepository _productRepository;
    private readonly IDatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="context">The database context used to access the product repository.</param>
    public ProductService(IDatabaseContext context)
    {
        _context = context;
        _productRepository = new ProductRepository(context);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="sku">The stock keeping unit of the product.</param>
    /// <param name="name">The name of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <param name="categoryId">The identifier of the category the product belongs to.</param>
    /// <param name="description">An optional description of the product.</param>
    /// <returns>The newly created product.</returns>
    /// <exception cref="ArgumentException">Thrown when the SKU is null, empty, or shorter than three characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a product with the same SKU already exists.</exception>
    public async Task<Product> CreateProductAsync(string sku, string name, decimal price, int categoryId, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(name);

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

    /// <summary>
    /// Gets a product by its identifier.
    /// </summary>
    /// <param name="productId">The identifier of the product.</param>
    /// <returns>The product, or <c>null</c> if no product with the given identifier exists.</returns>
    public async Task<Product?> GetProductAsync(int productId)
    {
        return await _productRepository.GetByIdAsync(productId);
    }

    /// <summary>
    /// Gets all active products.
    /// </summary>
    /// <returns>A list of all active products.</returns>
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _productRepository.GetActiveProductsAsync();
    }

    /// <summary>
    /// Gets products belonging to a category.
    /// </summary>
    /// <param name="categoryId">The identifier of the category.</param>
    /// <returns>A list of products in the given category.</returns>
    public async Task<List<Product>> GetCategoryProductsAsync(int categoryId)
    {
        return await _productRepository.GetByCategoryAsync(categoryId);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The identifier of the product to update.</param>
    /// <param name="name">An optional new name for the product.</param>
    /// <param name="price">An optional new price for the product.</param>
    /// <param name="description">An optional new description for the product.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no product with the given identifier exists.</exception>
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

    /// <summary>
    /// Updates the stock quantity of a product.
    /// </summary>
    /// <param name="productId">The identifier of the product.</param>
    /// <param name="quantity">The new stock quantity.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no product with the given identifier exists.</exception>
    /// <exception cref="ArgumentException">Thrown when the quantity is negative.</exception>
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

    /// <summary>
    /// Increases the stock quantity of a product.
    /// </summary>
    /// <param name="productId">The identifier of the product.</param>
    /// <param name="quantity">The amount to increase the stock by.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no product with the given identifier exists.</exception>
    public async Task<Product> IncreaseStockAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.IncreaseStock(quantity);
        return await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Decreases the stock quantity of a product.
    /// </summary>
    /// <param name="productId">The identifier of the product.</param>
    /// <param name="quantity">The amount to decrease the stock by.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no product with the given identifier exists.</exception>
    public async Task<Product> DecreaseStockAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.DecreaseStock(quantity);
        return await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Gets products whose stock quantity is at or below a threshold.
    /// </summary>
    /// <param name="threshold">The stock quantity threshold. Defaults to 10.</param>
    /// <returns>A list of products with low stock.</returns>
    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _productRepository.GetLowStockProductsAsync(threshold);
    }

    /// <summary>
    /// Gets products that are out of stock.
    /// </summary>
    /// <returns>A list of products with no stock available.</returns>
    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        return await _productRepository.GetOutOfStockProductsAsync();
    }

    /// <summary>
    /// Searches for products by name.
    /// </summary>
    /// <param name="searchTerm">The search term to match against product names.</param>
    /// <returns>A list of products matching the search term.</returns>
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);
        return await _productRepository.SearchByNameAsync(searchTerm);
    }

    /// <summary>
    /// Gets products within a price range.
    /// </summary>
    /// <param name="minPrice">The minimum price, inclusive.</param>
    /// <param name="maxPrice">The maximum price, inclusive.</param>
    /// <returns>A list of products whose price falls within the given range.</returns>
    public async Task<List<Product>> GetProductsByPriceAsync(decimal minPrice, decimal maxPrice)
    {
        return await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
    }

    /// <summary>
    /// Deactivates a product.
    /// </summary>
    /// <param name="productId">The identifier of the product to deactivate.</param>
    /// <returns>The updated product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no product with the given identifier exists.</exception>
    public async Task<Product> DeactivateProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.IsActive = false;
        product.ModifiedDate = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Gets the total inventory value.
    /// </summary>
    /// <returns>The total value of all products in stock.</returns>
    public async Task<decimal> GetInventoryValueAsync()
    {
        return await _productRepository.GetInventoryValueAsync();
    }

    /// <summary>
    /// Gets the number of active products.
    /// </summary>
    /// <returns>The count of active products.</returns>
    public async Task<int> GetProductCountAsync()
    {
        var products = await _productRepository.GetActiveProductsAsync();
        return products.Count;
    }

    /// <summary>
    /// Releases the database context resources used by this service.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
