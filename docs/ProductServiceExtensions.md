# ProductServiceExtensions

Provides a set of extension methods for working with `Product` entities using the Micro-ORM, including bulk operations, inventory management, and search capabilities.

## API

### `CreateProductAsync`
Creates a single product in the database.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `Product product` – The product to create.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<Product>` – The created product with updated identifiers.

**Exceptions:**
- Throws `ArgumentNullException` if `product` is null.
- Throws `DbUpdateException` on database errors.

---

### `CreateProductsBulkAsync`
Creates multiple products in a single transaction.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `IEnumerable<Product> products` – The products to create.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<List<Product>>` – The list of created products with updated identifiers.

**Exceptions:**
- Throws `ArgumentNullException` if `products` is null.
- Throws `DbUpdateException` on database errors.

---

### `GetProductBySkuAsync`
Retrieves a product by its SKU.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `string sku` – The SKU to search for.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<Product?>` – The matching product or null if not found.

**Exceptions:**
- Throws `ArgumentNullException` if `sku` is null or empty.

---
### `UpdateProductPriceAsync`
Updates the price of an existing product.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `Product product` – The product with updated price.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<Product>` – The updated product.

**Exceptions:**
- Throws `ArgumentNullException` if `product` is null.
- Throws `InvalidOperationException` if the product does not exist.
- Throws `DbUpdateException` on database errors.

---
### `UpdateProductNameAsync`
Updates the name of an existing product.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `Product product` – The product with updated name.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<Product>` – The updated product.

**Exceptions:**
- Throws `ArgumentNullException` if `product` is null.
- Throws `InvalidOperationException` if the product does not exist.
- Throws `DbUpdateException` on database errors.

---
### `IncreaseStockBulkAsync`
Increases the stock levels of multiple products in a single transaction.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `IEnumerable<(string sku, int quantity)> stockUpdates` – Pairs of SKU and quantity to add.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<List<Product>>` – The list of updated products.

**Exceptions:**
- Throws `ArgumentNullException` if `stockUpdates` is null.
- Throws `InvalidOperationException` if any SKU is not found.
- Throws `DbUpdateException` on database errors.

---
### `GetProductsByCategoriesAsync`
Retrieves all products belonging to one or more categories.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `IEnumerable<string> categoryNames` – The category names to filter by.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<List<Product>>` – The list of matching products.

**Exceptions:**
- Throws `ArgumentNullException` if `categoryNames` is null.

---
### `SearchProductsCaseInsensitiveAsync`
Searches products by name or description using a case-insensitive partial match.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `string searchTerm` – The term to search for.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<List<Product>>` – The list of matching products.

**Exceptions:**
- Throws `ArgumentNullException` if `searchTerm` is null.

---
### `GetProductsInPriceRangeAsync`
Retrieves products whose price falls within a specified range.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `decimal minPrice` – The minimum price (inclusive).
- `decimal maxPrice` – The maximum price (inclusive).
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<List<Product>>` – The list of matching products.

---
### `GetTotalInventoryValueAsync`
Calculates the total monetary value of all products in stock.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<decimal>` – The total inventory value.

---
### `GetLowStockProductCountAsync`
Counts the number of products with stock below a threshold.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `int threshold` – The stock level below which a product is considered low.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<int>` – The count of low-stock products.

---
### `GetOutOfStockProductCountAsync`
Counts the number of products with zero stock.

**Parameters:**
- `IProductRepository repository` – The repository instance.
- `CancellationToken cancellationToken` – Optional cancellation token.

**Returns:**
- `Task<int>` – The count of out-of-stock products.

## Usage
