# ProductRepository
The `ProductRepository` class is designed to encapsulate data access and retrieval for products, providing a range of methods to fetch products based on various criteria such as SKU, category, price range, and more. It inherits from a base class and takes an `IDatabaseContext` in its constructor, indicating its reliance on a database for product data.

## API
* `public ProductRepository(IDatabaseContext context) : base(context)`: Constructs a new instance of `ProductRepository`, passing the provided `IDatabaseContext` to the base class constructor.
* `public async Task<Product?> GetBySkuAsync`: Retrieves a product by its SKU. Returns a `Product` object if found, or `null` if not. Throws if database access fails.
* `public async Task<List<Product>> GetByCategoryAsync`: Fetches a list of products belonging to a specific category. Returns an empty list if no products are found. Throws if database access fails.
* `public async Task<List<Product>> GetActiveProductsAsync`: Retrieves a list of active products. Returns an empty list if no active products are found. Throws if database access fails.
* `public async Task<List<Product>> GetLowStockProductsAsync`: Fetches a list of products with low stock levels. Returns an empty list if no such products are found. Throws if database access fails.
* `public async Task<List<Product>> GetOutOfStockProductsAsync`: Retrieves a list of out-of-stock products. Returns an empty list if no such products are found. Throws if database access fails.
* `public async Task<List<Product>> GetByPriceRangeAsync`: Fetches a list of products within a specified price range. Returns an empty list if no products are found within the range. Throws if database access fails.
* `public async Task<List<Product>> GetExpensiveProductsAsync`: Retrieves a list of expensive products. Returns an empty list if no such products are found. Throws if database access fails.
* `public async Task<List<Product>> SearchByNameAsync`: Searches for products by name. Returns a list of matching products, or an empty list if none are found. Throws if database access fails.
* `public async Task<List<Product>> GetMostProfitableAsync`: Fetches a list of the most profitable products. Returns an empty list if no products are found. Throws if database access fails.
* `public async Task<decimal> GetInventoryValueAsync`: Calculates the total value of the current inventory. Throws if database access fails.

## Usage
```csharp
// Example 1: Fetching a product by SKU
var context = new DatabaseContext();
var repository = new ProductRepository(context);
var product = await repository.GetBySkuAsync("ABC123");
if (product != null)
{
    Console.WriteLine($"Product {product.Name} found.");
}
else
{
    Console.WriteLine("Product not found.");
}

// Example 2: Retrieving active products
var context = new DatabaseContext();
var repository = new ProductRepository(context);
var activeProducts = await repository.GetActiveProductsAsync();
foreach (var product in activeProducts)
{
    Console.WriteLine($"Active product: {product.Name}");
}
```

## Notes
* All methods are asynchronous, indicating that they are designed to be non-blocking and suitable for use in concurrent environments.
* The `GetBySkuAsync` method returns a single product or `null`, while other methods return lists of products, which may be empty if no matching products are found.
* The `GetInventoryValueAsync` method returns a decimal value, which may be zero if the inventory is empty.
* The class relies on an `IDatabaseContext` for database access, which should be properly configured and disposed of to avoid resource leaks.
* Thread-safety is not explicitly guaranteed by the class, but the use of asynchronous methods and dependency on an `IDatabaseContext` suggests that it is intended for use in multithreaded environments. However, callers should still ensure that the `IDatabaseContext` is properly synchronized if necessary.
