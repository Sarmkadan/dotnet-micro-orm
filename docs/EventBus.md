# EventBus

The `EventBus` class provides an in-memory, decoupled communication mechanism using the publish-subscribe pattern, enabling components within a .NET application to exchange information without direct dependencies. By registering handler types for specific event types, publishers can broadcast events asynchronously to any number of interested subscribers, facilitating a modular and extensible architecture.

## API

### EventBus()
Initializes a new instance of the `EventBus` class.

### async Task PublishAsync<TEvent>(TEvent @event)
Asynchronously broadcasts the provided event instance to all subscribers registered for the type `TEvent`.
*   **Parameters:**
    *   `@event`: The event object to be published.
*   **Remarks:** This method executes subscriber handlers concurrently or sequentially depending on the internal implementation. Exceptions occurring within handlers may propagate depending on the implementation's error-handling policy.

### void Subscribe<TEvent, THandler>()
Registers a handler type for a specific event type.
*   **Remarks:** The `EventBus` expects that `THandler` is capable of handling events of type `TEvent`. This typically implies that handlers are resolved and instantiated via a Dependency Injection container.

### void Unsubscribe<TEvent, THandler>()
Removes the registration of a handler type for a specific event type, ensuring the handler no longer receives subsequent publications of `TEvent`.

### int GetSubscriberCount<TEvent>()
Returns the total number of registered handler types currently subscribed to the specified event type `TEvent`.
*   **Returns:** The count of registered subscribers.

### void ClearSubscribers()
Removes all registered handler subscriptions from the event bus, resetting the bus to its initial state.

## Usage

### Basic Subscription and Publishing
```csharp
// Assuming an event class and handler interface exist
public record UserRegisteredEvent(string Username);
public interface IEventHandler<TEvent> { Task HandleAsync(TEvent @event); }

var eventBus = new EventBus();

// Register a handler for the event
eventBus.Subscribe<UserRegisteredEvent, UserRegisteredHandler>();

// Publish the event
await eventBus.PublishAsync(new UserRegisteredEvent("jdoe"));
```

### Managing Subscriptions
```csharp
var eventBus = new EventBus();

// Subscribe
eventBus.Subscribe<OrderPlacedEvent, OrderPlacedHandler>();
int count = eventBus.GetSubscriberCount<OrderPlacedEvent>(); // Returns 1

// Unsubscribe
eventBus.Unsubscribe<OrderPlacedEvent, OrderPlacedHandler>();

// Clear all
eventBus.ClearSubscribers();
```

## Notes

*   **Thread Safety:** The `EventBus` implementation is thread-safe, allowing concurrent subscriptions, unsubscriptions, and publishing from multiple threads.
*   **Handler Lifecycle:** The `EventBus` manages handler registration by type. It is expected that the underlying infrastructure (such as a Dependency Injection container) handles the instantiation and disposal lifecycle of the `THandler` objects when `PublishAsync` is invoked.
*   **Asynchronous Execution:** While `PublishAsync` returns a `Task`, subscribers are responsible for handling their own internal task management to ensure that slow handlers do not block the bus or other subscribers unnecessarily.
