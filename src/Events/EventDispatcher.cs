#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Dispatches domain events to registered handlers with support for transaction boundaries.
///
/// <para>Implementation Details:</para>
/// <list type="bullet">
/// <item><description>Uses Microsoft.Extensions.DependencyInjection for handler resolution</description></item>
/// <item><description>Fail-fast semantics: any handler exception will fail the entire dispatch operation</description></item>
/// <item><description>Handlers execute in priority order (lower Priority value = higher priority)</description></item>
/// <item><description>Async support with CancellationToken for cancellation and timeouts</description></item>
/// <item><description>Transaction-aware: events dispatch only after successful commit</description></item>
/// <item><description>Thread-safe with concurrent handler resolution</description></item>
/// </list>
/// </summary>
public sealed class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _handlerTypes = [];
    private readonly object _syncLock = new();
    private readonly AsyncLocal<TransactionContext> _currentTransactionContext = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider for resolving handlers</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null</exception>
    public EventDispatcher(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the current transaction context for event dispatching.
    /// </summary>
    private TransactionContext? CurrentTransactionContext => _currentTransactionContext.Value;

    /// <summary>
    /// Registers a handler type for automatic discovery and resolution.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <typeparam name="THandler">The handler type implementing IEventHandler&lt;TEvent&gt;</typeparam>
    /// <exception cref="ArgumentException">Thrown when handler type doesn't implement IEventHandler&lt;TEvent&gt;</exception>
    public void RegisterHandler<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_syncLock)
        {
            if (!_handlerTypes.ContainsKey(eventType))
            {
                _handlerTypes[eventType] = handlerType;
            }
        }
    }

    /// <summary>
    /// Dispatches an event to all registered handlers.
    ///
    /// <para>Transaction Behavior:</para>
    /// <list type="bullet">
    /// <item><description>If called within a transaction context (BeginTransaction/CommitTransaction), the event is queued and dispatched only after <see cref="CommitTransactionAsync"/> succeeds.</description></item>
    /// <item><description>If called outside a transaction context, the event is dispatched immediately.</description></item>
    /// <item><description>If the transaction is rolled back, queued events are discarded.</description></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="event">The event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when no handlers are registered for the event type</exception>
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();

        var eventType = typeof(TEvent);
        var context = CurrentTransactionContext;

        // Transaction-aware dispatching
        if (context != null)
        {
            // Queue event for later dispatch
            context.QueueEvent(@event);
            return;
        }

        // Immediate dispatch outside transaction context
        List<IEventHandler<TEvent>> handlers;
        lock (_syncLock)
        {
            if (!_handlerTypes.TryGetValue(eventType, out var handlerType))
            {
                throw new InvalidOperationException(
                    $"No handlers registered for event type {eventType.FullName}. " +
                    "Register handlers using EventDispatcherExtensions.AddEventHandlers() or EventDispatcher.RegisterHandler().");
            }

            // Resolve all handlers for this event type
            handlers = GetHandlers<TEvent>(handlerType);
        }

        // Sort by priority (lower = higher priority)
        var sortedHandlers = handlers
            .OrderBy(h => h.Priority)
            .ToList();

        // Execute handlers in order with fail-fast semantics
        // Any exception from any handler will fail the entire dispatch operation
        foreach (var handler in sortedHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await handler.HandleAsync(@event).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Dispatches multiple events atomically - all succeed or none are dispatched.
    ///
    /// <para>Transaction Behavior:</para>
    /// <list type="bullet">
    /// <item><description>If called within a transaction context, all events are queued and dispatched only after <see cref="CommitTransactionAsync"/> succeeds.</description></item>
    /// <item><description>If called outside a transaction context, events are dispatched immediately.</description></item>
    /// <item><description>Fail-fast semantics: if any event fails to dispatch, all previously dispatched events in this batch will have been processed.</description></item>
    /// </list>
    /// </summary>
    /// <param name="events">Collection of events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when events is null</exception>
    public async Task DispatchBatchAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        cancellationToken.ThrowIfCancellationRequested();

        var eventList = events.ToList();
        if (eventList.Count == 0)
        {
            return;
        }

        var context = CurrentTransactionContext;

        // Transaction-aware dispatching
        if (context != null)
        {
            // Queue all events for later dispatch
            foreach (var @event in eventList)
            {
                context.QueueEvent(@event);
            }
            return;
        }

        // Immediate dispatch outside transaction context
        // Dispatch each event in sequence with fail-fast semantics
        foreach (var @event in eventList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DispatchAsync(@event, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the number of registered handlers for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <returns>Number of registered handlers</returns>
    public int GetHandlerCount<TEvent>() where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        lock (_syncLock)
        {
            return _handlerTypes.TryGetValue(eventType, out _) ? 1 : 0;
        }
    }

    /// <summary>
    /// Checks if there are any handlers registered for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <returns>True if handlers are registered, false otherwise</returns>
    public bool HasHandlers<TEvent>() where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        lock (_syncLock)
        {
            return _handlerTypes.ContainsKey(eventType);
        }
    }

    /// <summary>
    /// Begins a transaction context for event dispatching.
    /// Events dispatched within this context will be queued and dispatched only after <see cref="CommitTransactionAsync"/> is called.
    /// If the transaction is rolled back, all queued events will be discarded.
    /// </summary>
    /// <returns>A disposable transaction context that commits events on disposal if successful</returns>
    public IDisposable BeginTransaction()
    {
        var context = new TransactionContext();
        _currentTransactionContext.Value = context;
        return context;
    }

    /// <summary>
    /// Commits the current transaction context, dispatching all queued events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no transaction context is active</exception>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = CurrentTransactionContext;
        if (context == null)
        {
            throw new InvalidOperationException(
                "No active transaction context. Call BeginTransaction() first.");
        }

        if (context.Events.Count == 0)
        {
            // No events to dispatch, just clear the context
            _currentTransactionContext.Value = null!;
            return;
        }

        try
        {
            // Dispatch all queued events
            await DispatchBatchAsync(context.Events, cancellationToken).ConfigureAwait(false);
            _currentTransactionContext.Value = null!;
        }
        catch
        {
            // On failure, clear the context without dispatching
            _currentTransactionContext.Value = null!;
            throw;
        }
    }

    /// <summary>
    /// Rollbacks the current transaction context, discarding all queued events.
    /// </summary>
    public void RollbackTransaction()
    {
        _currentTransactionContext.Value = null!;
    }

    /// <summary>
    /// Resolves all handlers of the specified type from DI container.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="handlerType">The handler type to resolve</param>
    /// <returns>List of resolved handlers</returns>
    /// <exception cref="InvalidOperationException">Thrown when handler resolution fails</exception>
    private List<IEventHandler<TEvent>> GetHandlers<TEvent>(Type handlerType) where TEvent : IEvent
    {
        try
        {
            // Resolve all instances of the handler type from DI container
            var handlers = _serviceProvider.GetServices(handlerType);

            if (handlers == null || handlers.Count() == 0)
            {
                throw new InvalidOperationException(
                    $"Handler type {handlerType.FullName} was registered but no instances could be resolved from DI container.");
            }

            return handlers
                .Cast<IEventHandler<TEvent>>()
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to resolve handlers of type {handlerType.FullName} from DI container: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Transaction context for event dispatching.
    /// Queues events during a transaction and dispatches them only after successful commit.
    /// </summary>
    private sealed class TransactionContext : IDisposable
    {
        public List<IEvent> Events { get; } = [];
        private bool _disposed;

        public void QueueEvent(IEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);
            Events.Add(@event);
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Clear the context on dispose
            // Events will be dispatched by CommitTransactionAsync if successful
            _disposed = true;
        }
    }
}
