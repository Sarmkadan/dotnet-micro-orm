# ProductRepositoryExtensions
The `ProductRepositoryExtensions` class provides a set of extension methods for retrieving products from a repository, allowing for various filtering and pagination options. These methods enable efficient data retrieval and manipulation, making it easier to work with product data in a .NET application.

## API
The following methods are available:
* `GetBySkusAsync`: Retrieves a list of products by their SKUs. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetByIdsAsync`: Retrieves a list of products by their IDs. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetByPriceRangeAsync`: Retrieves a list of products within a specified price range. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetByCategoryPagedAsync`: Retrieves a list of products by category, with pagination support. Returns a tuple containing the list of products and the total count of products. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetLowStockProductsAsync`: Retrieves a list of products with low stock levels. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetMostProfitableAsync`: Retrieves a list of the most profitable products. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.
* `GetByCategoryIdsAsync`: Retrieves a list of products by category IDs. Returns a `List<Product>`. Throws if the repository is not properly initialized or if an error occurs during data retrieval.

## Usage
```csharp
// Example 1: Retrieving products by SKU
var skus = new[] { "sku1", "sku2", "sku3" };
var products = await ProductRepositoryExtensions.GetBySkusAsync(skus);
foreach (var product in products)
{
    Console.WriteLine(product.Name);
}

// Example 2: Retrieving paginated products by category
var categoryId = 1;
var pageNumber = 1;
var pageSize = 10;
var (products, totalCount) = await ProductRepositoryExtensions.GetByCategoryPagedAsync(categoryId, pageNumber, pageSize);
Console.WriteLine($"Total count: {totalCount}");
foreach (var product in products)
{
    Console.WriteLine(product.Name);
}
```

## Notes
When using these extension methods, consider the following:
* All methods are asynchronous, so ensure that the calling code is also asynchronous to avoid deadlocks.
* The methods may throw exceptions if the repository is not properly initialized or if an error occurs during data retrieval. Handle these exceptions accordingly in your application.
* The `GetByCategoryPagedAsync` method returns a tuple containing the list of products and the total count of products. Use this information to implement pagination in your application.
* The methods are designed to be thread-safe, but ensure that the underlying repository is also thread-safe to avoid concurrency issues.
