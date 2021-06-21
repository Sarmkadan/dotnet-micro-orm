# OrderService

`OrderService` is an asynchronous service that manages the full lifecycle of orders in the `dotnet-micro-orm` project. It leverages a micro-ORM (e.g., Dapper) to perform data access operations, providing methods for creating, retrieving, updating status, and querying orders. The service implements `IAsyncDisposable` to allow deterministic cleanup of underlying resources such as database connections or transactions.

## API

### `public OrderService`

Constructor. Initializes a new instance of the service. Parameters and dependencies are resolved according to the project’s dependency injection configuration.

### `public async Task<Order> CreateOrderAsync`

Creates a new order with the provided details.  
**Returns:** The newly created `Order` object.  
**Throws:** Exceptions on database errors or validation failures (e.g., invalid user identifier, missing required fields).

### `public async Task<Order?> GetOrderAsync`

Retrieves a single order by its unique identifier.  
**Returns:** The matching `Order`, or `null` if no order with the given identifier exists.  
**Throws:** Exceptions on database errors.

### `public async Task<List<Order>> GetUserOrdersAsync`

Returns all orders associated with a specific user.  
**Returns:** A list of `Order` objects (may be empty).  
**Throws:** Exceptions on database errors or invalid user identifier.

### `public async Task<List<Order>> GetOrdersByStatusAsync`

Returns all orders that match a given status.  
**Returns:** A list of `Order` objects (may be empty).  
**Throws:** Exceptions on database errors or invalid status value.

### `public async Task<Order> AddOrderItemAsync`

Adds an item to an existing order.  
**Returns:** The updated `Order` object after the item has been added.  
**Throws:** Exceptions if the order does not exist, is in a state that does not allow modifications, or on database errors.

### `public async Task<Order> ConfirmOrderAsync`

Confirms a pending order, transitioning it to a confirmed state.  
**Returns:** The updated `Order` object.  
**Throws:** Exceptions if the order is not in a confirmable state, does not exist, or on database errors.

### `public async Task<Order> ShipOrderAsync`

Marks a confirmed order as shipped.  
**Returns:** The updated `Order` object.  
**Throws:** Exceptions if the order is not in a shippable state, does not exist, or on database errors.

### `public async Task<Order> DeliverOrderAsync`

Marks a shipped order as delivered.  
**Returns:** The updated `Order` object.  
**Throws:** Exceptions if the order is not in a deliverable state, does not exist, or on database errors.

### `public async Task<Order> CancelOrderAsync`

Cancels an order that has not yet been shipped or delivered.  
**Returns:** The updated `Order` object.  
**Throws:** Exceptions if the order cannot be cancelled (e.g., already shipped), does not exist, or on database errors.

### `public async Task<List<Order>> GetPendingOrdersAsync`

Returns all orders that are currently in a pending state.  
**Returns:** A list of `Order` objects (may be empty).  
**Throws:** Exceptions on database errors.

### `public async Task<decimal> GetTotalRevenueAsync`

Calculates the total revenue from all confirmed, shipped, or delivered orders.  
**Returns:** The total revenue as a `decimal`.  
**Throws:** Exceptions on database errors.

### `public async Task<List<Order>> GetOrdersByDateRangeAsync`

Returns all orders placed within a specified date range.  
**Returns:** A list of `Order` objects (may be empty).  
**Throws:** Exceptions on database errors or invalid date range (e.g., start date after end date).

### `public async ValueTask DisposeAsync`

Performs asynchronous cleanup of resources held by the service (e.g., database connections, transactions). After disposal, the service must not be used for further operations.

## Usage

### Example 1: Creating and confirming an order

```csharp
await using var orderService = new OrderService(/* dependencies */);

// Create a new order for user 42
var newOrder = await orderService.CreateOrderAsync(/* userId: 42, items: ... */);

// Add an item to the order
newOrder = await orderService.AddOrderItemAsync(/* orderId: newOrder.Id, productId: 101, quantity: 2 */);

// Confirm the order
newOrder = await orderService.ConfirmOrderAsync(/* orderId: newOrder.Id */);

Console.WriteLine($"Order {newOrder.Id} confirmed. Total: {newOrder.TotalAmount:C}");
```

### Example 2: Retrieving and processing pending orders

```csharp
await using var orderService = new OrderService(/* dependencies */);

// Get all pending orders
var pendingOrders = await orderService.GetPendingOrdersAsync();

foreach (var order in pendingOrders)
{
    // Attempt to ship each pending order
    try
    {
        var shippedOrder = await orderService.ShipOrderAsync(/* orderId: order.Id */);
        Console.WriteLine($"Order {shippedOrder.Id} shipped.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Order {order.Id} could not be shipped: {ex.Message}");
    }
}

// Calculate total revenue after processing
var revenue = await orderService.GetTotalRevenueAsync();
Console.WriteLine($"Total revenue: {revenue:C}");
```

## Notes

- **Thread safety:** `OrderService` is not guaranteed to be thread-safe. Concurrent calls on the same instance may lead to unexpected behavior or data corruption. Use separate instances or external synchronization when operating from multiple threads.
- **Disposal:** After calling `DisposeAsync`, the service must not be used. Any attempt to call methods on a disposed instance will result in an `ObjectDisposedException`.
- **State transitions:** Methods like `ConfirmOrderAsync`, `ShipOrderAsync`, `DeliverOrderAsync`, and `CancelOrderAsync` enforce business rules. Calling them on an order in an incompatible state will throw an `InvalidOperationException`. Always check the current order status before invoking state-changing operations.
- **Null and empty parameters:** Methods that accept identifiers or other parameters may throw `ArgumentException` or `ArgumentNullException` if invalid values are provided (e.g., null or empty strings, zero or negative IDs).
- **Database errors:** All methods may throw exceptions originating from the underlying data access layer (e.g., `SqlException`, `DbException`). Ensure proper error handling and consider implementing retry logic for transient faults.
- **Date range queries:** `GetOrdersByDateRangeAsync` expects a valid start and end date. Passing an end date earlier than the start date will likely throw an `ArgumentException`.
