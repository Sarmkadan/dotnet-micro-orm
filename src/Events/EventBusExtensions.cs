#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Extension methods for <see cref="EventBus"/> providing additional convenience functionality
/// for event bus operations including bulk operations and diagnostic utilities.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Subscribes multiple handlers at once for the same event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to</typeparam>
    /// <typeparam name="THandler">The handler type implementing <see cref="IEventHandler{TEvent}"/></typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <param name="handlers">Collection of handlers to subscribe</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bus"/> or <paramref name="handlers"/> is null</exception>
    public static void SubscribeRange<TEvent, THandler>(
        this EventBus bus,
        IEnumerable<THandler> handlers)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(handlers);

        foreach (var handler in handlers)
        {
            bus.Subscribe<TEvent, THandler>(handler);
        }
    }

    /// <summary>
    /// Publishes an event synchronously and waits for all handlers to complete.
    /// This is a convenience method that calls <see cref="EventBus.PublishAsync{TEvent}"/> and awaits the result.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <param name="event">The event to publish</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bus"/> or <paramref name="event"/> is null</exception>
    public static async Task PublishSyncAsync<TEvent>(
        this EventBus bus,
        TEvent @event)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(@event);

        await bus.PublishAsync(@event).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the total number of subscribers across all event types.
    /// </summary>
    /// <param name="bus">The event bus instance</param>
    /// <returns>Total subscriber count</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bus"/> is null</exception>
    public static int GetTotalSubscriberCount(this EventBus bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        return bus.GetType()
            .GetField("_subscribers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(bus) is System.Collections.Concurrent.ConcurrentDictionary<Type, List<object>> subscribersDict
            ? subscribersDict.Values.Sum(list => list.Count)
            : 0;
    }

    /// <summary>
    /// Checks if there are any subscribers registered for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type to check</typeparam>
    /// <param name="bus">The event bus instance</param>
    /// <returns>True if there are subscribers, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bus"/> is null</exception>
    public static bool HasSubscribers<TEvent>(this EventBus bus)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(bus);

        return bus.GetSubscriberCount<TEvent>() > 0;
    }

}