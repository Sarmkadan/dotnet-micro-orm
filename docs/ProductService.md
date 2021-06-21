# ProductService

The `ProductService` class provides asynchronous operations for managing products in an e-commerce or inventory system using the `dotnet-micro-orm` framework. It encapsulates core product lifecycle operations such as creation, retrieval, updates, stock management, and reporting, while ensuring thread-safe access to product data through asynchronous patterns.

## API

### `ProductService`

Initializes a new instance of the `ProductService` class, typically configured with a database context or connection for product operations.

### `async Task<Product> CreateProductAsync(Product product)`

Creates a new product in the system.

- **Parameters**: `product` – The `Product` entity to create. Must not be `null`.
- **Return value**: A `Task<Product>` resolving to the created product with updated identifiers.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `DbUpdateException` if the product cannot be persisted due to database constraints.

### `async Task<Product?> GetProductAsync(int id)`

Retrieves a single product by its unique identifier.

- **Parameters**: `id` – The product identifier.
- **Return value**: A `Task<Product?>` resolving to the product if found, or `null` otherwise.
- **Throws**: No documented exceptions under normal operation.

### `async Task<List<Product>> GetActiveProductsAsync()`

Retrieves all products currently marked as active.

- **Return value**: A `Task<List<Product>>` containing all active products.
- **Throws**: No documented exceptions under normal operation.

### `async Task<List<Product>> GetCategoryProductsAsync(string category)`

Retrieves all products belonging to a specified category.

- **Parameters**: `category` – The category name to filter by. Case-sensitive.
- **Return value**: A `Task<List<Product>>` containing matching products.
- **Throws**: `ArgumentNullException` if `category` is `null`.

### `async Task<Product> UpdateProductAsync(Product product)`

Updates an existing product with new values.

- **Parameters**: `product` – The updated `Product` entity. Must not be `null`.
- **Return value**: A `Task<Product>` resolving to the updated product.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `DbUpdateConcurrencyException` if the product was modified by another process.
- **Throws**: `DbUpdateException` if the update violates database constraints.

### `async Task<Product> UpdateStockAsync(Product product, int newStock)`

Updates the stock level of a product to an exact value.

- **Parameters**:
  - `product` – The `Product` entity whose stock is being updated. Must not be `null`.
  - `newStock` – The new stock quantity (must be non-negative).
- **Return value**: A `Task<Product>` resolving to the updated product.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `newStock` is negative.
- **Throws**: `DbUpdateConcurrencyException` if the product was modified concurrently.

### `async Task<Product> IncreaseStockAsync(Product product, int quantity)`

Increases the stock level of a product by a specified amount.

- **Parameters**:
  - `product` – The `Product` entity to update. Must not be `null`.
  - `quantity` – The amount to add (must be positive).
- **Return value**: A `Task<Product>` resolving to the updated product.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `quantity` is not positive.
- **Throws**: `DbUpdateConcurrencyException` if the product was modified concurrently.

### `async Task<Product> DecreaseStockAsync(Product product, int quantity)`

Decreases the stock level of a product by a specified amount.

- **Parameters**:
  - `product` – The `Product` entity to update. Must not be `null`.
  - `quantity` – The amount to subtract (must be positive and not exceed current stock).
- **Return value**: A `Task<Product>` resolving to the updated product.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `quantity` is not positive or exceeds available stock.
- **Throws**: `DbUpdateConcurrencyException` if the product was modified concurrently.

### `async Task<List<Product>> GetLowStockProductsAsync(int threshold)`

Retrieves products whose stock levels are below a specified threshold.

- **Parameters**: `threshold` – The minimum stock level to consider "low".
- **Return value**: A `Task<List<Product>>` containing products with stock below `threshold`.
- **Throws**: No documented exceptions under normal operation.

### `async Task<List<Product>> GetOutOfStockProductsAsync()`

Retrieves all products currently out of stock.

- **Return value**: A `Task<List<Product>>` containing products with zero or negative stock.
- **Throws**: No documented exceptions under normal operation.

### `async Task<List<Product>> SearchProductsAsync(string query)`

Searches products by name, description, or category using a free-text query.

- **Parameters**: `query` – The search term. Can be `null` or empty to return all products.
- **Return value**: A `Task<List<Product>>` containing matching products.
- **Throws**: No documented exceptions under normal operation.

### `async Task<List<Product>> GetProductsByPriceAsync(decimal minPrice, decimal maxPrice)`

Retrieves products within a specified price range.

- **Parameters**:
  - `minPrice` – The lower bound of the price range (inclusive).
  - `maxPrice` – The upper bound of the price range (inclusive).
- **Return value**: A `Task<List<Product>>` containing products priced between `minPrice` and `maxPrice`.
- **Throws**: `ArgumentOutOfRangeException` if `minPrice` > `maxPrice`.

### `async Task<Product> DeactivateProductAsync(Product product)`

Marks a product as inactive (e.g., discontinued or unavailable).

- **Parameters**: `product` – The `Product` entity to deactivate. Must not be `null`.
- **Return value**: A `Task<Product>` resolving to the deactivated product.
- **Throws**: `ArgumentNullException` if `product` is `null`.
- **Throws**: `DbUpdateConcurrencyException` if the product was modified concurrently.

### `async Task<decimal> GetInventoryValueAsync()`

Calculates the total monetary value of all products in stock.

- **Return value**: A `Task<decimal>` resolving to the sum of (unit price × stock) for all products.
- **Throws**: No documented exceptions under normal operation.

### `async Task<int> GetProductCountAsync()`

Returns the total number of products in the system.

- **Return value**: A `Task<int>` resolving to the count of products.
- **Throws**: No documented exceptions under normal operation.

### `public async ValueTask DisposeAsync()`

Releases any unmanaged resources and cleans up database connections or sessions used by the service.

- **Return value**: A `ValueTask` representing the asynchronous disposal.
- **Throws**: No documented exceptions under normal operation.

## Usage
