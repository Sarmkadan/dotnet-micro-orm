// ... (rest of the README content remains the same)

## IEvent

The `IEvent` interface represents a domain event, which is an important occurrence in the system. It provides properties for event identification, timestamp, and aggregate information.

### Example Usage

```csharp
public class MyEvent : IEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string InitiatedBy { get; set; } = "System";

    public int AggregateId { get; } = 1;
    public string AggregateType { get; } = "User";
    public string EventType { get; } = "user.created";

    // Example with a specific event type (UserCreatedEvent)
    public int UserId { get; set; } = 1;
    public string Username { get; set; } = "johnDoe";
    public string Email { get; set; } = "john.doe@example.com";
}

public class Program
{
    public static void Main(string[] args)
    {
        var myEvent = new MyEvent();
        Console.WriteLine($"Event ID: {myEvent.EventId}");
        Console.WriteLine($"Occurred at: {myEvent.OccurredAt}");
        Console.WriteLine($"Initiated by: {myEvent.InitiatedBy}");
        Console.WriteLine($"Aggregate ID: {myEvent.AggregateId}");
        Console.WriteLine($"Aggregate type: {myEvent.AggregateType}");
        Console.WriteLine($"Event type: {myEvent.EventType}");
        Console.WriteLine($"User ID: {myEvent.UserId}");
        Console.WriteLine($"Username: {myEvent.Username}");
        Console.WriteLine($"Email: {myEvent.Email}");
    }
}
```

// ... (rest of the README content remains the same)
