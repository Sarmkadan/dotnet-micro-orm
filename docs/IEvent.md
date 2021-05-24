# IEvent

`IEvent` is the foundational interface for domain events within the `dotnet-micro-orm` framework, designed to support event sourcing, command query responsibility segregation (CQRS), and comprehensive audit logging. It provides a standardized contract for capturing system state changes, ensuring that all events carry essential metadata—such as provenance, timing, and aggregate identity—enabling consistent processing and persistence across diverse domain models.

## API

All members are properties defined within the `IEvent` interface.

*   `string EventId`: A unique identifier for the specific instance of the event.
*   `DateTime OccurredAt`: The timestamp indicating when the event took place.
*   `string InitiatedBy`: The identifier of the actor or process that triggered the event.
*   `int AggregateId`: (Abstract) The unique identifier of the domain aggregate that this event modifies.
*   `string AggregateType`: (Abstract) The type name of the domain aggregate associated with the event.
*   `string EventType`: (Abstract) A descriptive string identifier for the type of event.
*   `int UserId`: The identifier of the user involved in or associated with the event.
*   `string Username`: The username associated with the event.
*   `string Email`: The email address associated with the event.
*   `string ChangedFields`: A serialized representation of the data changes applied by this event.
*   `int ProductId`: The identifier of the product associated with the event.
*   `string Name`: The name associated with the aggregate or entity in the event.
*   `decimal Price`: The price value associated with the event, typically used for pricing updates.
*   `int OldStock`: The previous stock level, used for tracking inventory changes.
*   `int NewStock`: The new stock level, used for tracking inventory changes.
*   `int OrderId`: The identifier of the order associated with the event.

## Usage

### Consuming an Event

This example demonstrates how an event handler might process different event types by inspecting the `EventType` and casting or accessing the appropriate metadata.

```csharp
public void HandleEvent(IEvent @event)
{
    switch (@event.EventType)
    {
        case "OrderPlaced":
            Console.WriteLine($"Processing order: {@event.OrderId} for User ID: {@event.UserId}");
            break;
        case "ProductStockUpdated":
            Console.WriteLine($"Product {@event.ProductId} stock changed from {@event.OldStock} to {@event.NewStock}");
            break;
        default:
            Console.WriteLine($"Unhandled event type: {@event.EventType}");
            break;
    }
}
```

### Implementing a Custom Event

This example shows a simple implementation of `IEvent` for a specific domain action.

```csharp
public class UserEmailChangedEvent : IEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string InitiatedBy { get; set; } = "System";

    // Abstract members
    public int AggregateId { get; set; }
    public string AggregateType { get; set; } = "User";
    public string EventType { get; set; } = "UserEmailChanged";

    // Domain specific properties
    public int UserId { get; set; }
    public string Email { get; set; }

    // Unused properties set to defaults
    public string ChangedFields { get; set; }
    public string Username { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int OldStock { get; set; }
    public int NewStock { get; set; }
    public int OrderId { get; set; }
}
```

## Notes

*   **Immutability:** While the interface allows for property setters, it is strongly recommended that all `IEvent` implementations be treated as immutable once constructed. This prevents unintended side effects during event propagation or when processing events from an event store.
*   **Thread Safety:** As `IEvent` instances are intended to be passed across asynchronous boundaries (e.g., to background handlers or persistent stores), implementations should be inherently thread-safe. Immutable implementations natively satisfy this requirement.
*   **Data Integrity:** The `ChangedFields` property is intended to store serialized state changes. Ensure that the serialization format used is consistent throughout the application lifecycle to facilitate proper deserialization and auditing.
*   **Interface Design:** Given the broad range of properties, implementations often leave irrelevant fields at their default values (e.g., `0` for `int`, `null` for `string`). Ensure consumers handle these default values gracefully.
