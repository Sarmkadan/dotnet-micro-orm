#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Extension methods for <see cref="EventBus"/> providing additional convenience functionality
/// for event bus operations including bulk operations, conditional subscriptions,
/// and diagnostic utilities.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Subscribes multiple handlers at once for the same event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
    /// <typeparam name="THandler">The handler type implementing IEventHandler&lt;TEvent&gt;</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <param name="handlers">Collection of handlers to subscribe</param>
    public static void SubscribeRange<TEvent, THandler>(
        this EventBus bus,
        IEnumerable<THandler> handlers)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        if (bus is null)
            throw new ArgumentNullException(nameof(bus));

        if (handlers is null)
            throw new ArgumentNullException(nameof(handlers));

        foreach (var handler in handlers)
        {
            bus.Subscribe<TEvent, THandler>(handler);
        }
    }

    /// <summary>
    /// Publishes an event synchronously and waits for all handlers to complete.
    /// This is a convenience method that calls PublishAsync and awaits the result.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <param name="event">The event to publish</param>
    public static async Task PublishSyncAsync<TEvent>(
        this EventBus bus,
        TEvent @event)
        where TEvent : IEvent
    {
        if (bus is null)
            throw new ArgumentNullException(nameof(bus));

        await bus.PublishAsync(@event).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all subscribers for a specific event type as a strongly-typed collection.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <returns>Collection of subscribers for the event type</returns>
    public static IEnumerable<object> GetSubscribers<TEvent>(this EventBus bus)
        where TEvent : IEvent
    {
        if (bus is null)
            throw new ArgumentNullException(nameof(bus));

        var eventType = typeof(TEvent);
        var subscribersField = bus.GetType()
            .GetField("_subscribers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var subscribersDict = subscribersField?.GetValue(bus) as ConcurrentDictionary<Type, List<object>>;

        return subscribersDict?.TryGetValue(eventType, out var handlers) == true
            ? handlers.AsReadOnly()
            : Enumerable.Empty<object>();
    }

    /// <summary>
    /// Gets the total number of subscribers across all event types.
    /// </summary>
    /// <param name="bus">The event bus instance</param>
    /// <returns>Total subscriber count</returns>
    public static int GetTotalSubscriberCount(this EventBus bus)
    {
        if (bus is null)
            throw new ArgumentNullException(nameof(bus));

        var subscribersField = bus.GetType()
            .GetField("_subscribers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var subscribersDict = subscribersField?.GetValue(bus) as ConcurrentDictionary<Type, List<object>>;

        return subscribersDict?.Values.Sum(list => list.Count) ?? 0;
    }

    /// <summary>
    /// Checks if there are any subscribers registered for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <returns>True if there are subscribers, false otherwise</returns>
    public static bool HasSubscribers<TEvent>(this EventBus bus)
        where TEvent : IEvent
    {
        if (bus is null)
            throw new ArgumentNullException(nameof(bus));

        return bus.GetSubscriberCount<TEvent>() > 0;
    }
}