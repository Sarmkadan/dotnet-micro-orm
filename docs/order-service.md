# OrderService

`DotnetMicroOrm.Services.OrderService` is a sealed service for order processing: creation, item management, status transitions (Pending, Confirmed, Shipped, Delivered, Cancelled), and reporting. It wraps `Repository<Order>` and `Repository<OrderItem>`, both created internally from the `IDatabaseContext` passed to the constructor.

Source: `src/Services/OrderService.cs`. Entities: `src/Domain/Models/Order.cs`, `src/Domain/Models/OrderItem.cs`.

## Construction and lifetime

```csharp
var service = new OrderService(databaseContext);
```

- The constructor throws `ArgumentNullException` if `context` is `null`.
- The service implements `IAsyncDisposable`. `DisposeAsync()` disposes the **underlying `IDatabaseContext`**, not just the service. Do not dispose the service if the context is shared with other components.
- The service holds no other state, so its lifetime is bounded by the context's lifetime.

## Constants

| Constant | Value | Purpose |
|---|---|---|
| `MinShippingAddressLength` | `10` | Minimum shipping address length on create |
| `StatusPending` / `StatusConfirmed` / `StatusShipped` / `StatusDelivered` | `"Pending"` / `"Confirmed"` / `"Shipped"` / `"Delivered"` | Status values used by the service |
| `OrderNotFoundMessage` | `"Order not found"` | Message for missing orders |

`Cancelled` is not a service constant; `CancelOrderAsync` delegates to `Order.Cancel()`, which sets it.

## Methods

### Creation and lookup

| Method | Behavior | Throws |
|---|---|---|
| `Task<Order> CreateOrderAsync(int userId, string shippingAddress)` | Creates an order with status `Pending`, sets `CreatedDate` to UTC now, persists it. | `ArgumentNullException` if `shippingAddress` is null. `ArgumentException` if `userId <= 0`, or if the address is empty/whitespace or shorter than 10 characters. |
| `Task<Order?> GetOrderAsync(int orderId)` | Lookup by ID. | None. Returns `null` if not found. |
| `Task<List<Order>> GetUserOrdersAsync(int userId)` | Orders for one user, sorted by `OrderDate` descending. | None. |
| `Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)` | Orders with `OrderDate` in the range, inclusive on both ends, sorted by `OrderDate` descending. | None. No argument validation. |
| `Task<List<Order>> GetOrdersByStatusAsync(string status)` | Status match, case-insensitive. | `ArgumentNullException` if `status` is null. `ArgumentException` if empty/whitespace. |
| `Task<List<Order>> GetPendingOrdersAsync()` | Shortcut for `GetOrdersByStatusAsync("Pending")`. | Same as above. |

### Items

| Method | Behavior | Throws |
|---|---|---|
| `Task<Order> AddOrderItemAsync(int orderId, int productId, string productName, int quantity, decimal unitPrice)` | Loads the order, builds an `OrderItem`, validates it, computes the line total, saves the item, adds it to the order, recalculates `TotalAmount`, updates the order. | `ArgumentNullException` if `productName` is null. `InvalidOperationException` if the order is not found or item validation fails. |

`AddOrderItemAsync` does **not** check the order's status. Items can be added to a Confirmed, Shipped, Delivered or Cancelled order.

### Status transitions

| Method | Behavior | Throws |
|---|---|---|
| `Task<Order> ConfirmOrderAsync(int orderId)` | Requires status `Pending` and at least one item. Sets `Confirmed` and `ModifiedDate`. | `InvalidOperationException` if not found, not pending, or has no items. |
| `Task<Order> ShipOrderAsync(int orderId, DateTime? shipDate = null)` | Delegates to `Order.Ship(shipDate ?? UtcNow)`. Requires status `Confirmed`. | `InvalidOperationException` if not found or not confirmed (from the entity). |
| `Task<Order> DeliverOrderAsync(int orderId)` | Delegates to `Order.MarkAsDelivered()`. **No status check.** | `InvalidOperationException` if not found. |
| `Task<Order> CancelOrderAsync(int orderId)` | Delegates to `Order.Cancel()`. Refused if status is `Shipped` or `Delivered`. | `InvalidOperationException` if not found, or if already shipped or delivered (from the entity). |

Each transition is read-modify-write: the order is loaded, changed in memory, then saved. There is no locking or concurrency check in the service. Two concurrent transitions on the same order can race.

### Reporting

| Method | Behavior | Throws |
|---|---|---|
| `Task<decimal> GetTotalRevenueAsync()` | Sum of `TotalAmount` for orders with status `Shipped` or `Delivered`. | None. |

## Behavior notes

- **In-memory filtering.** `GetUserOrdersAsync`, `GetOrdersByStatusAsync`, `GetOrdersByDateRangeAsync` and `GetTotalRevenueAsync` call `Repository<Order>.GetAllAsync()` and filter in C#. They load every order in the table. On large tables, push these filters into SQL with a query or specification.
- **Status checks are split.** `ConfirmOrderAsync` checks status in the service. `ShipOrderAsync` and `CancelOrderAsync` check it in the entity. `DeliverOrderAsync` checks nothing. A `Pending` order can be marked as delivered.
- **Total recalculation.** `Order.AddItem` calls `RecalculateTotals()`, which sums `Quantity * UnitPrice` over all items. `Discount` and `TaxAmount` on items are not part of this sum.
- **Status comparison.** `ConfirmOrderAsync` and `Order` use case-sensitive comparisons (`!=`, `is "Shipped"`). `GetOrdersByStatusAsync` is case-insensitive. Pass exact-case status values to the entity-level checks.

## Usage

```csharp
await using var service = new OrderService(databaseContext);

var order = await service.CreateOrderAsync(userId: 42, shippingAddress: "Musterstrasse 1, 20095 Hamburg");
order = await service.AddOrderItemAsync(order.Id, productId: 7, productName: "Pizza Margherita", quantity: 2, unitPrice: 9.50m);
order = await service.ConfirmOrderAsync(order.Id);
order = await service.ShipOrderAsync(order.Id);
order = await service.DeliverOrderAsync(order.Id);

decimal revenue = await service.GetTotalRevenueAsync();
```

`await using` disposes the service and therefore the database context. Use a context that is not shared with other code.
