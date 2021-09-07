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

## WebhookHandler

The `WebhookHandler` class is responsible for receiving webhook payloads, verifying their HMAC‑SHA256 signatures, and dispatching them to registered handlers based on the event type. It allows multiple handlers per event and provides a simple API for generating signatures for testing.

Example usage:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetMicroOrm.Integration;

class Program
{
    static async Task Main()
    {
        // Secret shared with the webhook provider
        var secret = "super-secret-key";

        // Create the handler
        var handler = new WebhookHandler(secret);

        // Register a handler for the "order.created" event
        handler.Subscribe(WebhookEvents.OrderCreated, async payload =>
        {
            Console.WriteLine($"Received order created event: {payload.Id}");
            // Process payload.Data as needed
            await Task.CompletedTask;
        });

        // Build a sample payload
        var payload = new WebhookPayload
        {
            EventType = WebhookEvents.OrderCreated,
            Data = new Dictionary<string, object>
            {
                { "OrderId", 123 },
                { "Amount", 49.99 }
            }
        };

        // Generate a signature for the payload (for testing)
        var signature = handler.GenerateSignature(payload);

        // Process the webhook
        var result = await handler.ProcessAsync(payload, signature);

        if (result.Success)
        {
            Console.WriteLine("Webhook processed successfully.");
        }
        else
        {
            Console.WriteLine($"Error processing webhook: {result.Error}");
        }
    }
}
```
