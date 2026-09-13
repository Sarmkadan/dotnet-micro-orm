# Events architecture

The `DotnetMicroOrm.Events` namespace provides two in-process ways to deliver domain events. `EventBus` is a lightweight publish-subscribe component whose subscribers are registered as instances. `EventDispatcher` is the dependency-injection-based path and can populate its handler registry through `EventHandlerAutoDiscovery`.

Neither component is a durable or distributed message broker. Events and subscriptions exist only in the current process, so use an outbox or external broker when delivery must survive a process failure.

## Event contracts

`IEvent` is the common event contract. Every event supplies an event ID, UTC occurrence time, initiator, aggregate ID and type, and an application-level event type string. `DomainEvent` supplies generated `EventId` and `OccurredAt` values plus a default `InitiatedBy` value, leaving aggregate-specific properties to subclasses.

Handlers implement `IEventHandler<TEvent>`:

```csharp
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event);
    int Priority => 100;
}
```

Lower `Priority` values run first. Event objects should be treated as immutable after publication because the same instance may be passed to multiple handlers.

## EventBus

`EventBus` implements `IEventBus` and keeps an in-memory collection of subscribed handler instances keyed by their exact event type.

- `Subscribe` and `Unsubscribe` manage handler instances; subscribing the same instance twice does not duplicate it.
- `PublishAsync` orders handlers by priority. With the default `executeAsync: true`, it starts them together and awaits them with `Task.WhenAll`; with `false`, it awaits each handler in priority order.
- A handler exception is written to the console and suppressed so other handlers can run.
- Publishing an event with no subscribers completes without error.
- `GetSubscriberCount` and `ClearSubscribers` provide basic inspection and reset operations.

This path is useful when the application owns the handler instances and wants failure isolation between subscribers.

## EventDispatcher

`EventDispatcher` implements `IEventDispatcher`. It stores an event-type-to-handler-type registration and resolves handler instances from `IServiceProvider` when an event is dispatched.

- Handlers execute one at a time in ascending priority order.
- Handler exceptions propagate immediately; later handlers are not run.
- Dispatching an event type without a registered handler throws `InvalidOperationException`.
- `DispatchBatchAsync` dispatches events sequentially. If one fails, earlier handlers may already have completed; the method does not undo their side effects.
- `BeginTransaction` creates an async-flow-local queue. Calls to `DispatchAsync` or `DispatchBatchAsync` enqueue events until `CommitTransactionAsync`; `RollbackTransaction` discards the queue. This is a dispatch boundary only and does not create or commit a database transaction.
- `HasHandlers<TEvent>` and `GetHandlerCount<TEvent>` inspect the dispatcher registry. The registry keeps the first handler type registered for each event type, so the reported count is zero or one.

Use this path when handlers should be created by dependency injection, cancellation should be supported, or handler failure should fail the calling operation.

## EventHandlerAutoDiscovery

`EventHandlerAutoDiscovery` reflects over concrete, non-generic classes that implement `IEventHandler<TEvent>` and registers their event/handler type pairs with the dispatcher. It scans all currently loaded assemblies when `ScanAssemblies` is empty, or loads and scans only the configured assembly names.

Discovery registers types with `EventDispatcher`; it does not add handler implementations to the DI container. Register each discovered concrete handler type with `IServiceCollection` as well. Discovery also does not run automatically when the service provider is built: call `dispatcher.DiscoverAndRegisterHandlers(serviceProvider)` during application startup.

## Usage example

The following example defines an event and handler, registers the handler in DI, limits discovery to the application assembly, performs discovery, and dispatches the event:

```csharp
using DotnetMicroOrm.Events;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddEventHandlers(options =>
    options.ScanAssembly(typeof(OrderPlaced).Assembly.GetName().Name!));
services.AddScoped<SendOrderConfirmation>();

using var serviceProvider = services.BuildServiceProvider();
var dispatcher = serviceProvider.GetRequiredService<IEventDispatcher>();

// Build the dispatcher's event-to-handler map after the provider is ready.
dispatcher.DiscoverAndRegisterHandlers(serviceProvider);

await dispatcher.DispatchAsync(new OrderPlaced(42, "customer@example.com"));

public sealed class OrderPlaced(int orderId, string email) : DomainEvent
{
    public int OrderId { get; } = orderId;
    public string Email { get; } = email;
    public override int AggregateId => OrderId;
    public override string AggregateType => "Order";
    public override string EventType => "order.placed";
}

public sealed class SendOrderConfirmation : IEventHandler<OrderPlaced>
{
    public int Priority => 10;

    public Task HandleAsync(OrderPlaced @event)
    {
        Console.WriteLine($"Sending confirmation for order {@event.OrderId} to {@event.Email}");
        return Task.CompletedTask;
    }
}
```

For the instance-based bus, the equivalent delivery setup is direct:

```csharp
var bus = new EventBus(executeAsync: false);
var handler = new SendOrderConfirmation();

bus.Subscribe<OrderPlaced, SendOrderConfirmation>(handler);
await bus.PublishAsync(new OrderPlaced(42, "customer@example.com"));
bus.Unsubscribe<OrderPlaced, SendOrderConfirmation>(handler);
```

Choose one delivery path for a workflow so its registration, failure, and ordering semantics remain clear.
