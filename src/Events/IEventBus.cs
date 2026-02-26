#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Event bus interface for pub-sub pattern implementation.
/// Allows publishing domain events and subscribing to event handlers.
/// Supports both synchronous and asynchronous event processing.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all registered subscribers
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;

    /// <summary>
    /// Subscribes a handler to events of a specific type
    /// </summary>
    void Subscribe<TEvent, THandler>(THandler handler)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// Unsubscribes a handler from events
    /// </summary>
    void Unsubscribe<TEvent, THandler>(THandler handler)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;

    /// <summary>
    /// Gets the number of subscribers for a specific event type
    /// </summary>
    int GetSubscriberCount<TEvent>() where TEvent : IEvent;

    /// <summary>
    /// Clears all subscribers
    /// </summary>
    void ClearSubscribers();
}
