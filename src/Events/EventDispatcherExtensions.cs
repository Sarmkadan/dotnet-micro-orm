#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Extension methods for <see cref="EventDispatcher"/> providing convenience functionality
/// for event dispatcher operations including handler registration and diagnostic utilities.
/// </summary>
public static class EventDispatcherExtensions
{
    /// <summary>
    /// Registers a handler type for automatic discovery and resolution.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <typeparam name="THandler">The handler type implementing IEventHandler&lt;TEvent&gt;</typeparam>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public static void RegisterHandler<TEvent, THandler>(this IEventDispatcher dispatcher)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        if (dispatcher is EventDispatcher concreteDispatcher)
        {
            concreteDispatcher.RegisterHandler<TEvent, THandler>();
        }
        else
        {
            throw new InvalidOperationException(
                $"EventDispatcher must be of type {typeof(EventDispatcher).FullName} to support this operation.");
        }
    }

    /// <summary>
    /// Discovers and registers all event handlers from assemblies.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="serviceProvider">The DI service provider for resolving handlers</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher or serviceProvider is null</exception>
    public static void DiscoverAndRegisterHandlers(this IEventDispatcher dispatcher, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var autoDiscovery = serviceProvider.GetRequiredService<EventHandlerAutoDiscovery>();
        autoDiscovery.DiscoverAndRegisterHandlers(dispatcher);
    }

    /// <summary>
    /// Dispatches an event synchronously and waits for all handlers to complete.
    /// This is a convenience method that calls <see cref="IEventDispatcher.DispatchAsync{TEvent}(TEvent, CancellationToken)"/> and awaits the result.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="event">The event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher or event is null</exception>
    public static async Task DispatchSyncAsync<TEvent>(
        this IEventDispatcher dispatcher,
        TEvent @event,
        CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(@event);

        await dispatcher.DispatchAsync(@event, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dispatches multiple events atomically - all succeed or none are dispatched.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="events">Collection of events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher or events is null</exception>
    public static async Task DispatchBatchAsync(
        this IEventDispatcher dispatcher,
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(events);

        await dispatcher.DispatchBatchAsync(events, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of registered handlers for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <returns>Number of registered handlers</returns>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public static int GetHandlerCount<TEvent>(this IEventDispatcher dispatcher) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        return dispatcher.GetHandlerCount<TEvent>();
    }

    /// <summary>
    /// Checks if there are any handlers registered for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <returns>True if handlers are registered, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public static bool HasHandlers<TEvent>(this IEventDispatcher dispatcher) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        return dispatcher.HasHandlers<TEvent>();
    }

    /// <summary>
    /// Begins a transaction context for event dispatching.
    /// Events dispatched within this context will be queued and dispatched only after <see cref="IEventDispatcher.CommitTransactionAsync"/> is called.
    /// If the transaction is rolled back, all queued events will be discarded.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <returns>A disposable transaction context that commits events on disposal if successful</returns>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public static IDisposable BeginTransaction(this IEventDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        return dispatcher.BeginTransaction();
    }

    /// <summary>
    /// Commits the current transaction context, dispatching all queued events.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when no transaction context is active</exception>
    public static async Task CommitTransactionAsync(this IEventDispatcher dispatcher, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        await dispatcher.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rollbacks the current transaction context, discarding all queued events.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public static void RollbackTransaction(this IEventDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        dispatcher.RollbackTransaction();
    }

    /// <summary>
    /// Executes an operation within a transaction context, automatically committing events on success.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher or operation is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when transaction context management fails</exception>
    public static async Task ExecuteInTransactionAsync(this IEventDispatcher dispatcher, Func<IEventDispatcher, Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(operation);

        using var transaction = dispatcher.BeginTransaction();

        try
        {
            await operation(dispatcher).ConfigureAwait(false);
            await dispatcher.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            dispatcher.RollbackTransaction();
            throw;
        }
    }

    /// <summary>
    /// Registers event handlers in the service collection for automatic discovery.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration action for dispatcher options</param>
    /// <returns>The configured service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        Action<IEventDispatcherOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure dispatcher with options
        services.AddEventDispatcher(configureOptions);

        return services;
    }

    /// <summary>
    /// Registers specific event handler types in the service collection.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The configured service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IEventHandler<TEvent>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers multiple event handler types in the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="handlerTypes">Collection of (eventType, handlerType) tuples</param>
    /// <returns>The configured service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or handlerTypes is null</exception>
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params (Type EventType, Type HandlerType)[] handlerTypes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(handlerTypes);

        foreach (var (eventType, handlerType) in handlerTypes)
        {
            var registerMethod = typeof(EventDispatcherExtensions).GetMethod(
                nameof(AddEventHandlerGeneric),
                BindingFlags.NonPublic | BindingFlags.Static);

            if (registerMethod != null)
            {
                var genericMethod = registerMethod.MakeGenericMethod(eventType, handlerType);
                genericMethod.Invoke(null, [services]);
            }
        }

        return services;
    }

    /// <summary>
    /// Generic implementation for AddEventHandlers with type parameters.
    /// </summary>
    private static IServiceCollection AddEventHandlerGeneric<TEvent, THandler>(IServiceCollection services)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        return services;
    }
}
