# dotnet-micro-orm

## JobSchedulerExtensions

The `JobSchedulerExtensions` class provides a set of extensions for scheduling and managing background jobs. It allows you to register jobs, execute them asynchronously, and track their execution history.

### Example Usage

```csharp
var scheduler = new JobScheduler();

// Register a job
scheduler.Register<HelloWorldJob>();

// Execute a job asynchronously
var result = await JobSchedulerExtensions.ExecuteJobAsync<HelloWorldJob>(scheduler);
Console.WriteLine($"Job executed with result: {result.Success}");

// Get execution history
var history = JobSchedulerExtensions.GetExecutionHistory(scheduler);
foreach (var entry in history)
{
    Console.WriteLine($"Executed at {entry.ExecutedAt} with result: {entry.Success}");
}

// Get job statistics
var stats = scheduler.GetStatistics();
Console.WriteLine($"Total executions: {stats.TotalExecutions}");
Console.WriteLine($"Successful executions: {stats.SuccessfulExecutions}");
Console.WriteLine($"Failed executions: {stats.FailedExecutions}");
Console.WriteLine($"Success rate: {stats.SuccessRate:P}");
Console.WriteLine($"Average execution time: {stats.AverageExecutionTime}");
Console.WriteLine($"Last execution time: {stats.LastExecutionTime}");
```

## EventBusExtensions

The `EventBusExtensions` class provides methods for managing event subscriptions and publishing events. It allows you to subscribe multiple handlers to events, publish events synchronously, and inspect registered subscribers.

### Example Usage

```csharp
// Define an event and handler
public class MessageSentEvent : IEvent { public string Message { get; set; } }
public class MessageSentHandler : IEventHandler<MessageSentEvent> 
{ 
    public Task Handle(MessageSentEvent @event) 
    {
        Console.WriteLine($"Message received: {@event.Message}");
        return Task.CompletedTask;
    }
}

// Usage
var eventBus = new EventBus();

// Subscribe handler to event
EventBusExtensions.SubscribeRange<MessageSentEvent, MessageSentHandler>(eventBus);

// Publish event
await EventBusExtensions.PublishSyncAsync(eventBus, new MessageSentEvent { Message = "Hello, world!" });

// Check subscribers
if (EventBusExtensions.HasSubscribers<MessageSentEvent>(eventBus))
{
    Console.WriteLine($"Subscribers count: {EventBusExtensions.GetTotalSubscriberCount(eventBus)}");
}
```

## UserServiceExtensions

The `UserServiceExtensions` class provides utility methods for managing user-related operations, including checking user existence, retrieving user data, updating user details, and generating user statistics.

### Example Usage

```csharp
// Check if user exists
var userExists = await UserServiceExtensions.UserExistsAsync(user.Id);

// Retrieve user by email
var user = await UserServiceExtensions.GetUserByEmailAsync("user@example.com");

// Update user email
var updatedUser = await UserServiceExtensions.UpdateEmailAsync(user.Id, "newemail@example.com");

// Get active users
var activeUsers = await UserServiceExtensions.GetActiveUsersAsync();

// Authenticate user with details
var (authenticatedUser, success) = await UserServiceExtensions.AuthenticateWithDetailsAsync("username", "password");

// Get user statistics
var stats = await UserServiceExtensions.GetUserStatisticsAsync();
Console.WriteLine($"Active: {stats.ActiveCount}, Inactive: {stats.InactiveCount}, Total: {stats.TotalCount}");
```

// ... (rest of the README content remains the same)
