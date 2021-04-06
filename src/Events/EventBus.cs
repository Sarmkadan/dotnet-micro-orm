#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetMicroOrm.Events;

/// <summary>
/// In-process event bus implementation using pub-sub pattern.
/// Handlers are executed synchronously or asynchronously based on configuration.
/// Thread-safe and designed for single-application use (for distributed use, consider message queues).
/// </summary>
public class sealed EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _subscribers = [];
    private readonly bool _executeAsync;

    public EventBus(bool executeAsync = true)
    {
        _executeAsync = executeAsync;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);

        if (!_subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
            return; // No handlers registered

        // Sort by priority
        var sortedHandlers = handlers
            .Cast<IEventHandler<TEvent>>()
            .OrderBy(h => h.Priority)
            .ToList();

        if (_executeAsync)
        {
            var tasks = sortedHandlers.Select(h => SafeExecuteAsync(h, @event));
            await Task.WhenAll(tasks);
        }
        else
        {
            foreach (var handler in sortedHandlers)
            {
                await SafeExecuteAsync(handler, @event);
            }
        }
    }

    public void Subscribe<TEvent, THandler>(THandler handler)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);

        _subscribers.AddOrUpdate(
            eventType,
            new List<object> { handler },
            (key, existing) =>
            {
                if (!existing.Contains(handler))
                    existing.Add(handler);
                return existing;
            });
    }

    public void Unsubscribe<TEvent, THandler>(THandler handler)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        if (handler is null)
            return;

        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _subscribers.TryRemove(eventType, out _);
            }
        }
    }

    public int GetSubscriberCount<TEvent>() where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        return _subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
    }

    public void ClearSubscribers()
    {
        _subscribers.Clear();
    }

    private async Task SafeExecuteAsync<TEvent>(IEventHandler<TEvent> handler, TEvent @event)
        where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(@event);
        }
        catch (Exception ex)
        {
            // Log error but don't rethrow - allow other handlers to execute
            Console.WriteLine($"Error in event handler {handler.GetType().Name}: {ex.Message}");
        }
    }
}
