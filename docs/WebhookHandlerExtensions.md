# WebhookHandlerExtensions

Provides extension methods for processing webhook payloads with retry logic, type-safe data extraction, and event validation. Designed for scenarios requiring idempotent webhook processing, such as payment confirmations or order updates.

## API

### `public static async Task<WebhookResult> ProcessWithRetryAsync(this IWebhookHandler handler, WebhookPayload payload, int maxRetries = 3, TimeSpan? retryDelay = null)`

Processes a webhook payload with automatic retry on transient failures. Retries occur for `HttpRequestException`, `TaskCanceledException`, and `TimeoutException`.

- **Parameters**
  - `handler`: The webhook handler implementing `IWebhookHandler`.
  - `payload`: The parsed webhook payload to process.
  - `maxRetries`: Maximum number of retry attempts (default: 3).
  - `retryDelay`: Optional delay between retries (default: exponential backoff starting at 200ms).

- **Return Value**
  Returns a `WebhookResult` indicating success, failure, or retry exhaustion.

- **Exceptions**
  Throws `ArgumentNullException` if `payload` is null.
  Throws `InvalidOperationException` if the handler is not properly configured.

---

### `public static async Task<WebhookResult> ProcessAsync(this IWebhookHandler handler, WebhookPayload payload)`

Processes a webhook payload without retry logic. Suitable for fire-and-forget scenarios where transient failures are acceptable.

- **Parameters**
  - `handler`: The webhook handler implementing `IWebhookHandler`.
  - `payload`: The parsed webhook payload to process.

- **Return Value**
  Returns a `WebhookResult` indicating success or failure.

- **Exceptions**
  Throws `ArgumentNullException` if `payload` is null.
  Throws `InvalidOperationException` if the handler is not properly configured.

---

### `public static async Task<IReadOnlyDictionary<string, WebhookResult>> ProcessBatchAsync(this IWebhookHandler handler, IEnumerable<WebhookPayload> payloads)`

Processes a batch of webhook payloads in parallel, returning results keyed by payload ID.

- **Parameters**
  - `handler`: The webhook handler implementing `IWebhookHandler`.
  - `payloads`: Collection of webhook payloads to process.

- **Return Value**
  Returns a dictionary mapping payload IDs to `WebhookResult` objects.

- **Exceptions**
  Throws `ArgumentNullException` if `payloads` is null.
  Throws `InvalidOperationException` if the handler is not properly configured.

---

### `public static WebhookPayload CreatePayload(string eventType, object? data = null, Dictionary<string, string>? headers = null)`

Constructs a `WebhookPayload` from an event type and optional data.

- **Parameters**
  - `eventType`: The event type identifier (e.g., `order.created`).
  - `data`: Optional structured data associated with the event.
  - `headers`: Optional HTTP headers to include in the payload.

- **Return Value**
  Returns a new `WebhookPayload` instance.

- **Exceptions**
  Throws `ArgumentException` if `eventType` is null or whitespace.

---
### `public static T? GetData<T>(this WebhookPayload payload)`

Extracts strongly-typed data from the payload's `Data` property.

- **Parameters**
  - `payload`: The webhook payload to extract data from.

- **Return Value**
  Returns the deserialized data as type `T`, or `null` if the payload's `Data` is null or incompatible.

---
### `public static bool IsEventType(this WebhookPayload payload, string expectedType)`

Checks if the payload's event type matches the expected value.

- **Parameters**
  - `payload`: The webhook payload to check.
  - `expectedType`: The event type to compare against.

- **Return Value**
  Returns `true` if the payload's event type matches `expectedType` (case-sensitive); otherwise, `false`.

- **Exceptions**
  Throws `ArgumentNullException` if `payload` or `expectedType` is null.

---
### `public static long GetAgeInMilliseconds(this WebhookPayload payload)`

Calculates the age of the payload in milliseconds based on its `Timestamp` property.

- **Parameters**
  - `payload`: The webhook payload to measure.

- **Return Value**
  Returns the duration in milliseconds between `Timestamp` and `DateTimeOffset.UtcNow`.

- **Exceptions**
  Throws `ArgumentNullException` if `payload` is null.
  Throws `InvalidOperationException` if `Timestamp` is not set.

---
### `public static bool HasRequiredData(this WebhookPayload payload)`

Determines whether the payload contains non-null data in its `Data` property.

- **Parameters**
  - `payload`: The webhook payload to inspect.

- **Return Value**
  Returns `true` if `Data` is not null; otherwise, `false`.

- **Exceptions**
  Throws `ArgumentNullException` if `payload` is null.

## Usage

### Basic Webhook Processing
```csharp
var handler = new MyWebhookHandler();
var payload = WebhookHandlerExtensions.CreatePayload("order.created", new { OrderId = 123 });

var result = await handler.ProcessAsync(payload);
if (result.Success)
{
    Console.WriteLine($"Processed order {result.Data.OrderId}");
}
```

### Batch Processing with Retry
```csharp
var handler = new MyWebhookHandler();
var payloads = new[]
{
    WebhookHandlerExtensions.CreatePayload("payment.received", new { Amount = 99.99m }),
    WebhookHandlerExtensions.CreatePayload("order.updated", new { OrderId = 456 })
};

var results = await handler.ProcessBatchAsync(payloads);
foreach (var kvp in results)
{
    if (kvp.Value.Success)
    {
        Console.WriteLine($"Payload {kvp.Key} succeeded");
    }
    else
    {
        Console.WriteLine($"Payload {kvp.Key} failed: {kvp.Value.Error}");
    }
}
```

## Notes

- **Retry Behavior**: `ProcessWithRetryAsync` uses exponential backoff by default. Transient failures (network issues, temporary service unavailability) are retried, but permanent failures (invalid payload, authorization errors) are not.

- **Thread Safety**: All methods are thread-safe when the underlying `IWebhookHandler` implementation is thread-safe. The extension methods themselves do not introduce shared state.

- **Idempotency**: Ensure your handler's `ProcessAsync` implementation is idempotent, as retries may invoke it multiple times for the same payload.

- **Data Validation**: `GetData<T>` performs no validation beyond type conversion. Validate payload structure in your handler's `ProcessAsync` method.

- **Timestamp Handling**: `GetAgeInMilliseconds` and `HasRequiredData` assume UTC timestamps. Local timezones may cause unexpected results.
