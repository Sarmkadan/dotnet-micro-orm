# UserCreatedEventHandler

The `UserCreatedEventHandler` is responsible for processing events triggered when a new user is registered within the application. It encapsulates the necessary logic to handle the `UserCreatedEvent`, ensuring that downstream actions, such as user notification or profile initialization, are executed asynchronously in response to the user creation process.

## API

### UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
Initializes a new instance of the `UserCreatedEventHandler` class.

*   **Parameters:**
    *   `logger`: An `ILogger<UserCreatedEventHandler>` instance used for logging event handling activities and potential errors.

### Task HandleAsync(UserCreatedEvent @event)
Processes the specified `UserCreatedEvent` asynchronously.

*   **Parameters:**
    *   `@event`: The `UserCreatedEvent` instance containing the details of the newly created user.
*   **Return Value:**
    *   A `Task` representing the asynchronous operation.
*   **Exceptions:**
    *   Throws an `ArgumentNullException` if the `@event` parameter is null.
    *   Throws an `Exception` if the processing of the event fails.

## Usage

### Example 1: Basic invocation via an event bus
```csharp
// Example of manually invoking the handler from an event bus subscription
var handler = new UserCreatedEventHandler(logger);
var userEvent = new UserCreatedEvent(userId: "123", email: "user@example.com");

await handler.HandleAsync(userEvent);
```

### Example 2: Integration within an event processing pipeline
```csharp
// Registering the handler in an event processing pipeline
public void ConfigureEventHandlers(IServiceCollection services)
{
    services.AddTransient<UserCreatedEventHandler>();
}

// ... within an event mediator or bus
var handler = serviceProvider.GetRequiredService<UserCreatedEventHandler>();
await handler.HandleAsync(newUserEvent);
```

## Notes

*   **Thread Safety:** The `UserCreatedEventHandler` implementation is designed to be stateless regarding event data. It is thread-safe, assuming that the injected `ILogger` implementation is thread-safe, which is standard for Microsoft.Extensions.Logging.
*   **Error Handling:** It is recommended to wrap the `HandleAsync` call in a try-catch block when consuming it from an event bus to handle potential transient failures or domain-specific exceptions gracefully.
*   **Idempotency:** Implementations of event handlers should strive to be idempotent, ensuring that processing the same event multiple times does not lead to inconsistent application state.
