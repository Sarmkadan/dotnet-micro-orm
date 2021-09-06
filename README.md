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

## UserCreatedEventHandler

The `UserCreatedEventHandler` class is responsible for handling user creation events. It logs the user creation for audit purposes and can be used to trigger additional actions such as sending a welcome email or creating default preferences.

### Example Usage

```csharp
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IAuditService _auditService;

    public UserCreatedEventHandler(IAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task HandleAsync(UserCreatedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            // Log user creation for audit purposes
            await _auditService.LogInsertAsync(
                "User",
                @event.UserId,
                $"{{\"username\":\"{@event.Username}\",\"email\":\"{@event.Email}\"}}",
                @event.UserId,
                @event.InitiatedBy);

            Console.WriteLine($"User created event handled: {@event.Username} ({@event.Email})");

            // In a real application, this would:
            // - Send welcome email
            // - Create default preferences
            // - Initialize user features
            // - Update analytics
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling user created event: {ex.Message}");
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var eventBus = new EventBus();
        eventBus.Subscribe<UserCreatedEvent, UserCreatedEventHandler>();

        var @event = new UserCreatedEvent { UserId = 1, Username = "johnDoe", Email = "john.doe@example.com" };
        await eventBus.PublishAsync(@event);
    }
}
```

// ... (rest of the README content remains the same)
```