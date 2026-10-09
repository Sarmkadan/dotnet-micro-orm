# ProductService

`DotnetMicroOrm.Services.ProductService` is a sealed service for product catalog and inventory operations: creation, lookup, updates, stock management, and reporting. It wraps `ProductRepository`, which is created internally from an `IDatabaseContext` passed to the constructor.

Source: `src/Services/ProductService.cs`. Extension methods: `src/Services/ProductServiceExtensions.cs` (see [Extension methods](#extension-methods)).

## Construction and lifetime

```csharp
var service = new ProductService(databaseContext);
```

- The service implements `IAsyncDisposable`. `DisposeAsync()` disposes the **underlying `IDatabaseContext`**, not just the service. Do not dispose the service if the context is shared with other components.
- The service holds no other state, so its lifetime is bounded by the context's lifetime.

## Constants

| Constant | Value | Purpose |
|---|---|---|
| `MinSkuLength` | `3` | Minimum SKU length on create |
| `DefaultLowStockThreshold` | `10` | Default threshold for `GetLowStockProductsAsync` |
| `MinimumPrice` | `0m` | Price floor used by `UpdateProductAsync` |
| `MinimumStockQuantity` | `0` | Lower bound for `UpdateStockAsync` |

Error messages are private constants: `"Product not found"`, `"SKU must be at least 3 characters"`, `"Product with this SKU already exists"`, `"Quantity cannot be negative"`.

## Methods

### Creation and lookup

| Method | Behavior | Throws |
|---|---|---|
| `Task<Product> CreateProductAsync(string sku, string name, decimal price, int categoryId, string? description = null)` | Validates SKU, checks SKU uniqueness via `GetBySkuAsync`, sets `CreatedDate` to UTC now, persists. | `ArgumentNullException` if `sku` or `name` is null. `ArgumentException` if SKU is empty/whitespace or shorter than 3. `InvalidOperationException` if SKU already exists. |
| `Task<Product?> GetProductAsync(int productId)` | Lookup by ID. | None. Returns `null` if not found. |
| `Task<List<Product>> GetActiveProductsAsync()` | All active products. | None. |
| `Task<List<Product>> GetCategoryProductsAsync(int categoryId)` | Products in one category. | None. |
| `Task<List<Product>> SearchProductsAsync(string searchTerm)` | Name search through the repository. | `ArgumentNullException` if `searchTerm` is null. |
| `Task<List<Product>> GetProductsByPriceAsync(decimal minPrice, decimal maxPrice)` | Price range, inclusive on both ends. No argument validation at this layer. | None. |

### Updates

| Method | Behavior | Throws |
|---|---|---|
| `Task<Product> UpdateProductAsync(int productId, string? name = null, decimal? price = null, string? description = null)` | Partial update. `name` is applied only if non-whitespace. `price` is applied only if `> 0`; other values are **silently ignored**. `description` is applied if non-null (an empty string clears it). Sets `ModifiedDate`. | `InvalidOperationException` if the product does not exist. |
| `Task<Product> DeactivateProductAsync(int productId)` | Sets `IsActive = false` and `ModifiedDate`. The product is not deleted. | `InvalidOperationException` if not found. |

### Stock

| Method | Behavior | Throws |
|---|---|---|
| `Task<Product> UpdateStockAsync(int productId, int quantity)` | Sets stock to an absolute value. | `InvalidOperationException` if not found. `ArgumentException` if `quantity < 0`. |
| `Task<Product> IncreaseStockAsync(int productId, int quantity)` | Delegates to `Product.IncreaseStock`. | `InvalidOperationException` if not found. `ArgumentException` if `quantity <= 0` (from the entity). |
| `Task<Product> DecreaseStockAsync(int productId, int quantity)` | Delegates to `Product.DecreaseStock`. | `InvalidOperationException` if not found, or if stock is insufficient (from the entity). `ArgumentException` if `quantity <= 0`. |
| `Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)` | Products with `StockQuantity <= threshold`. | None. |
| `Task<List<Product>> GetOutOfStockProductsAsync()` | Products with no stock. | None. |

Stock operations are read-modify-write: the product is loaded, changed in memory, then saved. There is no locking or concurrency check in the service itself. Concurrent decrements can race unless the database layer enforces a concurrency token.

### Reporting

| Method | Behavior |
|---|---|
| `Task<decimal> GetInventoryValueAsync()` | Sum of `Price * StockQuantity` over active products. |
| `Task<int> GetProductCountAsync()` | Count of active products. Loads the full list to count it. |

## Extension methods

`ProductServiceExtensions` adds helpers on `ProductService`. Several duplicate instance methods or add extra validation. Read the notes before relying on them.

| Extension | Notes |
|---|---|
| `CreateProductAsync(sku, name, price, categoryId, description)` | **Shadowed.** The instance method has the same signature, so C# always binds to the instance method. The extension's `price > 0` check never runs when called as `service.CreateProductAsync(...)`. To use the extension, call it through a static reference or rename the call site. |
| `CreateProductsBulkAsync(products)` | Calls `CreateProductAsync` in a loop. Not transactional: a failure midway leaves earlier products persisted. |
| `GetProductBySkuAsync(sku)` | Loads **all active products** and filters in memory with `OrdinalIgnoreCase`. Inactive products are never found. Costly on large catalogs. |
| `UpdateProductPriceAsync(productId, newPrice)` | Throws `ArgumentException` if `newPrice <= 0`. The instance `UpdateProductAsync` would ignore such a value silently. |
| `UpdateProductNameAsync(productId, newName)` | Throws if the name is null or empty. Whitespace passes the check and then is ignored by the instance method, so the call returns the unchanged product. |
| `IncreaseStockBulkAsync(updates)` | Sequential `IncreaseStockAsync` calls. Not transactional. |
| `GetProductsByCategoriesAsync(categoryIds)` | Loads all active products and filters in memory. |
| `SearchProductsCaseInsensitiveAsync(searchTerm)` | Returns an empty list for null/whitespace. Otherwise delegates to `SearchProductsAsync`. |
| `GetProductsInPriceRangeAsync(minPrice, maxPrice)` | Validates `minPrice >= 0` and `maxPrice >= minPrice`, then delegates. |
| `GetTotalInventoryValueAsync()` | Alias for `GetInventoryValueAsync`. |
| `GetLowStockProductCountAsync(threshold = 10)` | Throws if `threshold < 0`. Returns the count. |
| `GetOutOfStockProductCountAsync()` | Count of out-of-stock products. |

## Known gaps

- `CreateProductAsync` accepts `price` of zero or negative values. Only the extension validates price, and it is shadowed (see above).
- `GetProductsByPriceAsync` does not validate its range; `minPrice > maxPrice` is passed to the repository unchanged.
- `UpdateProductAsync` silently ignores invalid prices and whitespace-only names instead of reporting them.
- Several extension methods scan the full active catalog in memory. Prefer repository-level queries for large data sets.
- Bulk operations are not atomic.

## Example

```csharp
await using var service = new ProductService(context);

var product = await service.CreateProductAsync(
    sku: "PIZZA-MARG-01",
    name: "Margherita",
    price: 9.50m,
    categoryId: 1,
    description: "Classic tomato and mozzarella");

await service.IncreaseStockAsync(product.Id, 25);

var lowStock = await service.GetLowStockProductsAsync();
var value = await service.GetInventoryValueAsync();
```

Note that `await using` disposes `context` as well, per the lifetime notes above.
