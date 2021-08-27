// ... (rest of the README content remains the same)
## ProductRepositoryExtensions

The `ProductRepositoryExtensions` class provides a set of extensions for managing product-related operations. It allows you to retrieve products by various criteria, such as SKU, ID, price range, category, and more.

### Example Usage

```csharp
// Get products by SKU
var products = await ProductRepositoryExtensions.GetBySkusAsync(new[] { "EX-001", "EX-002" });
foreach (var product in products)
{
    Console.WriteLine($"Product: {product.Name}, SKU: {product.Sku}");
}

// Get products by ID
var productsById = await ProductRepositoryExtensions.GetByIdsAsync(new[] { 1, 2, 3 });
foreach (var product in productsById)
{
    Console.WriteLine($"Product: {product.Name}, ID: {product.Id}");
}

// Get products by price range
var productsByPrice = await ProductRepositoryExtensions.GetByPriceRangeAsync(10.00m, 20.00m);
foreach (var product in productsByPrice)
{
    Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
}

// Get products by category paged
var (productsByCategory, totalCount) = await ProductRepositoryExtensions.GetByCategoryPagedAsync(1, 10, "Electronics");
foreach (var product in productsByCategory)
{
    Console.WriteLine($"Product: {product.Name}, Category: {product.Category}");
}
Console.WriteLine($"Total count: {totalCount}");

// Get low stock products
var lowStockProducts = await ProductRepositoryExtensions.GetLowStockProductsAsync();
foreach (var product in lowStockProducts)
{
    Console.WriteLine($"Product: {product.Name}, Stock: {product.Stock}");
}

// Get most profitable products
var mostProfitableProducts = await ProductRepositoryExtensions.GetMostProfitableAsync();
foreach (var product in mostProfitableProducts)
{
    Console.WriteLine($"Product: {product.Name}, Profit: {product.Profit}");
}

// Get products by category IDs
var productsByCategoryIds = await ProductRepositoryExtensions.GetByCategoryIdsAsync(new[] { 1, 2, 3 });
foreach (var product in productsByCategoryIds)
{
    Console.WriteLine($"Product: {product.Name}, Category ID: {product.CategoryId}");
}
```

// ... (rest of the README content remains the same)
