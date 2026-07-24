#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Dispatches domain events to registered handlers with support for transaction boundaries.
///
/// <para>Key Features:</para>
/// <list type="bullet">
/// <item><description>DI-based handler resolution - handlers are automatically discovered and registered</description></item>
/// <item><description>Transaction-aware dispatching - events fire only after successful commit</description></item>
/// <item><description>Ordered execution - handlers execute in priority order (lower = higher priority)</description></item>
/// <item><description>Fail-fast semantics - exceptions from any handler will fail the entire dispatch operation</description></item>
/// <item><description>Async support with CancellationToken for cancellation and timeouts</description></item>
/// </list>
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches events from an aggregate root after changes are committed.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="event">The event to dispatch</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <exception cref="ArgumentNullException">Thrown when event is null</exception>
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;

    /// <summary>
    /// Dispatches multiple events atomically - all succeed or none are dispatched.
    /// </summary>
    /// <param name="events">Collection of events to dispatch</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <exception cref="ArgumentNullException">Thrown when events is null</exception>
    Task DispatchBatchAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of registered handlers for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <returns>Number of registered handlers</returns>
    int GetHandlerCount<TEvent>() where TEvent : IEvent;

    /// <summary>
    /// Checks if there are any handlers registered for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <returns>True if handlers are registered, false otherwise</returns>
    bool HasHandlers<TEvent>() where TEvent : IEvent;

    /// <summary>
    /// Begins a transaction context for event dispatching.
    /// Events dispatched within this context will be queued and dispatched only after <see cref="CommitTransactionAsync"/> is called.
    /// If the transaction is rolled back, all queued events will be discarded.
    /// </summary>
    /// <returns>A disposable transaction context that commits events on disposal if successful</returns>
    IDisposable BeginTransaction();

    /// <summary>
    /// Commits the current transaction context, dispatching all queued events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no transaction context is active</exception>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollbacks the current transaction context, discarding all queued events.
    /// </summary>
    void RollbackTransaction();
}
