// ... (rest of the README content remains the same)

## EventBus

The `EventBus` class is a pub-sub implementation that allows you to publish events to a pool of registered handlers. It provides a thread-safe way to handle events asynchronously or synchronously based on configuration.

### Example Usage

```csharp
public class MyEventHandler : IEventHandler<MyEvent>
{
    public async Task HandleAsync(MyEvent @event)
    {
        Console.WriteLine($"Received event: {@event.Message}");
    }
}

public class MyEvent : IEvent
{
    public string Message { get; set; }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var eventBus = new EventBus();
        eventBus.Subscribe<MyEvent, MyEventHandler>();

        var @event = new MyEvent { Message = "Hello, world!" };
        await eventBus.PublishAsync(@event);
    }
}
```

// ... (rest of the README content remains the same)
