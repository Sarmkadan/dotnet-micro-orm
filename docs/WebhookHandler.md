# WebhookHandler

The `WebhookHandler` class provides a structured mechanism for managing, validating, and processing incoming webhook notifications within the `dotnet-micro-orm` framework. It enables the registration of event subscriptions, secures communication through signature generation, and handles the asynchronous execution of webhook logic while tracking the execution state and metadata of each processed request.

## API

### Constructors

- **`WebhookHandler()`**
  Initializes a new instance of the `WebhookHandler` class.

### Methods

- **`void Subscribe()`**
  Registers a callback or action to be executed when a specific webhook event is received.

- **`async Task<WebhookResult> ProcessAsync()`**
  Asynchronously processes an incoming webhook request. Returns a `WebhookResult` representing the outcome of the operation.

- **`string GenerateSignature()`**
  Computes a cryptographic signature for a given payload, intended for verifying the authenticity and integrity of incoming webhook requests.

### Properties

- **`string Id`**
  The unique identifier associated with the webhook instance or the processed request.

- **`string EventType`**
  The type or name of the webhook event being handled.

- **`DateTime Timestamp`**
  The timestamp indicating when the webhook event was originally created or sent.

- **`Dictionary<string, object> Data`**
  A collection of key-value pairs containing the payload data associated with the webhook.

- **`string Version`**
  The API or schema version of the webhook payload.

- **`bool Success`**
  Indicates whether the webhook processing operation was successful.

- **`string? Message`**
  An optional message providing details or context about the processing result.

- **`string? Error`**
  An optional error description if the processing operation failed.

- **`DateTime ProcessedAt`**
  The timestamp indicating when the webhook processing was completed.

## Usage

### Registering an Event Subscription

```csharp
var handler = new WebhookHandler();

// Registering a subscription for a specific event
handler.Subscribe();
```

### Processing a Webhook Request

```csharp
var handler = new WebhookHandler();

// Asynchronously processing the request and checking the result
WebhookResult result = await handler.ProcessAsync();

if (result.Success)
{
    Console.WriteLine($"Processed event {result.EventType} successfully at {result.ProcessedAt}");
}
else
{
    Console.WriteLine($"Error processing webhook: {result.Error}");
}
```

## Notes

### Thread Safety
The `WebhookHandler` instance should be treated as thread-safe for reading its properties, but concurrent modifications to the handler's state—such as registering subscriptions via `Subscribe` during active processing—may lead to undefined behavior depending on the underlying implementation. It is recommended to configure handlers during the application startup phase.

### Error Handling
The `ProcessAsync` method is designed to catch internal exceptions and populate the `Success`, `Message`, and `Error` properties of the returned `WebhookResult` rather than throwing exceptions, allowing the caller to handle failures gracefully by inspecting the returned result object.

### Signature Validation
The `GenerateSignature` method should be used in conjunction with a shared secret to validate the `X-Webhook-Signature` (or equivalent) header provided by the source service. Ensure that the payload used for signature generation matches exactly the raw input received from the request body to avoid signature mismatch errors.
