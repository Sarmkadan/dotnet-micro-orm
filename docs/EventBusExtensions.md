# EventBusExtensions

`EventBusExtensions` provides a set of static extension methods for a lightweight in-process event bus, enabling type-safe subscription of handlers to events, synchronous publication with asynchronous handler execution, and introspection of the current subscriber state. It is designed for scenarios where a simple mediator-like dispatch is needed without external infrastructure.

## API

### SubscribeRange\<TEvent, THandler\>

```csharp
public static void SubscribeRange<TEvent, THandler>(this IEventBus bus)
```

Registers all concrete types assignable to `THandler` as subscribers for the event type `TEvent`. The method scans the assembly containing `THandler` and subscribes every non-abstract, instantiable class that implements the handler contract for `TEvent`. This allows bulk registration of handlers discovered at startup.

- **Type Parameters**
  - `TEvent`: The event type to subscribe to.
  - `THandler`: A base type or interface that handlers for `TEvent` must implement.
- **Exceptions**
  - May throw `InvalidOperationException` if the bus implementation does not support bulk subscription or if handler instantiation fails.
  - May throw `AmbiguousMatchException` if multiple constructors exist and resolution is not possible.

### PublishSyncAsync\<TEvent\>

```csharp
public static async Task PublishSyncAsync<TEvent>(this IEventBus bus, TEvent event)
```

Publishes an event synchronously from the caller’s perspective but invokes all registered handlers asynchronously. The method returns a `Task` that completes only after every subscriber’s asynchronous handling has finished. This ensures that callers can await full propagation before proceeding.

- **Parameters**
  - `event`: The event instance to publish. Must not be null.
- **Returns**
  - A `Task` representing the completion of all handler invocations.
- **Exceptions**
  - `ArgumentNullException` if `event` is null.
  - Exceptions thrown by individual handlers are aggregated and surfaced through the returned `Task` (typically as an `AggregateException` or via the bus implementation’s error policy).

### GetSubscribers\<TEvent\>

```csharp
public static IEnumerable<object> GetSubscribers<TEvent>(this IEventBus bus)
```

Returns an enumeration of all currently registered subscriber instances for the event type `TEvent`. The returned objects are the handler instances themselves, allowing inspection or manual invocation if needed.

- **Returns**
  - A lazy enumeration of subscriber objects. May be empty if no handlers are registered.
- **Remarks**
  - The enumeration reflects the state at the time of the call. Subsequent subscriptions or unsubscriptions are not reflected in an already-materialized collection.

### GetTotalSubscriberCount

```csharp
public static int GetTotalSubscriberCount(this IEventBus bus)
```

Gets the total number of subscriber registrations across all event types in the bus. This is a point-in-time snapshot and counts each handler registration individually, even if the same handler instance is subscribed to multiple events.

- **Returns**
  - A non-negative integer representing the total registration count.

### HasSubscribers\<TEvent\>

```csharp
public static bool HasSubscribers<TEvent>(this IEventBus bus)
```

Determines whether at least one subscriber is registered for the event type `TEvent`. This is a fast check intended for conditional publishing or diagnostics.

- **Returns**
  - `true` if one or more handlers are registered for `TEvent`; otherwise `false`.

## Usage

### Example 1: Bulk subscription at startup and publishing an event

```csharp
// Define an event and a handler interface
public record OrderPlaced(Guid OrderId, decimal Amount);

public interface IOrderPlacedHandler
{
    Task HandleAsync(OrderPlaced @event);
}

// Implement a concrete handler
public class EmailNotificationHandler : IOrderPlacedHandler
{
    public Task HandleAsync(OrderPlaced @event)
    {
        Console.WriteLine($"Sending email for order {@event.OrderId}");
        return Task.CompletedTask;
    }
}

// At application startup
IEventBus bus = new InMemoryEventBus();
bus.SubscribeRange<OrderPlaced, IOrderPlacedHandler>();

// Later, when an order is placed
var orderEvent = new OrderPlaced(Guid.NewGuid(), 99.99m);
await bus.PublishSyncAsync(orderEvent);
```

### Example 2: Conditional publishing based on subscriber presence

```csharp
public async Task ProcessPayment(PaymentReceived payment, IEventBus bus)
{
    // Core processing logic
    await SavePaymentAsync(payment);

    // Only publish if someone is listening
    if (bus.HasSubscribers<PaymentReceived>())
    {
        await bus.PublishSyncAsync(payment);
    }
    else
    {
        Console.WriteLine("No subscribers for PaymentReceived — skipping publish.");
    }
}

// Diagnostics
int totalRegistrations = bus.GetTotalSubscriberCount();
Console.WriteLine($"Total bus registrations: {totalRegistrations}");

foreach (object handler in bus.GetSubscribers<PaymentReceived>())
{
    Console.WriteLine($"Registered handler: {handler.GetType().Name}");
}
```

## Notes

- **Thread safety**: The methods are designed for use on an `IEventBus` instance whose implementation governs thread safety. `GetSubscribers<TEvent>`, `HasSubscribers<TEvent>`, and `GetTotalSubscriberCount` are read operations that may return stale data if subscriptions are modified concurrently on a non-synchronized bus. `PublishSyncAsync<TEvent>` should be safe to call concurrently if the underlying bus serializes handler invocation or handlers themselves are idempotent.
- **Handler lifetime**: `SubscribeRange<TEvent, THandler>` typically creates a new instance of each discovered handler type using its default constructor. Handlers that depend on external services may require a bus implementation that supports dependency injection; this extension method does not resolve dependencies beyond simple instantiation.
- **Empty registrations**: Calling `PublishSyncAsync<TEvent>` when no handlers are registered completes immediately with a successful `Task`. It does not throw or indicate a no-op condition beyond returning promptly.
- **Exception aggregation**: If multiple handlers throw during `PublishSyncAsync<TEvent>`, the behavior depends on the bus implementation. Common strategies include wrapping exceptions in an `AggregateException` or stopping at the first failure. Callers should consult the specific bus documentation for error-handling semantics.
- **Enumeration materialization**: `GetSubscribers<TEvent>` returns a lazy sequence. If the underlying subscriber collection is modified during enumeration, behavior is undefined and may throw `InvalidOperationException`. Materialize the sequence (e.g., via `.ToList()`) if a stable snapshot is required.
