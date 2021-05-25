# DefaultHttpClientExtensions

`DefaultHttpClientExtensions` provides a suite of extension methods for `HttpClient` designed to streamline common HTTP operations within the `dotnet-micro-orm` ecosystem. These methods encapsulate boilerplate code for request execution, response handling, JSON deserialization, and implementing retry policies, ensuring consistent behavior and robust error management across micro-orm service integrations.

## API

### GetAsync
Performs a GET request to the specified URI.
*   **Parameters:** `HttpClient client`, `string requestUri`, `CancellationToken cancellationToken`.
*   **Returns:** A `Task<HttpResponseData>` representing the response.
*   **Throws:** `HttpRequestException` if the request fails, or `TaskCanceledException` if the operation times out.

### PostFormAsync
Executes a POST request sending `application/x-www-form-urlencoded` content.
*   **Parameters:** `HttpClient client`, `string requestUri`, `IEnumerable<KeyValuePair<string, string>> content`, `CancellationToken cancellationToken`.
*   **Returns:** A `Task<HttpResponseData>` representing the server response.
*   **Throws:** `HttpRequestException` on network failure or non-success status codes.

### GetFromJsonAsync&lt;TResponse&gt;
Performs a GET request and deserializes the JSON response body into an object of type `TResponse`.
*   **Parameters:** `HttpClient client`, `string requestUri`, `CancellationToken cancellationToken`.
*   **Returns:** A `Task<TResponse?>` containing the deserialized object, or `null` if the response content is empty.
*   **Throws:** `JsonException` if deserialization fails, or `HttpRequestException`.

### SendWithRetryAsync
Executes an `HttpRequestMessage` using a predefined retry policy for transient failures.
*   **Parameters:** `HttpClient client`, `HttpRequestMessage request`, `int retryCount`, `CancellationToken cancellationToken`.
*   **Returns:** A `Task<HttpResponseData>` representing the final response after retries are exhausted or a successful response is received.
*   **Throws:** `HttpRequestException` if the request fails after all retries.

### SendAsync
Executes a raw `HttpRequestMessage` and wraps the result in `HttpResponseData`.
*   **Parameters:** `HttpClient client`, `HttpRequestMessage request`, `CancellationToken cancellationToken`.
*   **Returns:** A `Task<HttpResponseData>` representing the response.
*   **Throws:** `HttpRequestException`.

## Usage

### Retrieving JSON Data
```csharp
var client = new HttpClient();
var user = await client.GetFromJsonAsync<User>("https://api.example.com/users/123");

if (user != null)
{
    Console.WriteLine($"User: {user.Name}");
}
```

### Sending a Request with Retry Policy
```csharp
var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/data");
request.Content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");

// Attempt the request up to 3 times on failure
var response = await client.SendWithRetryAsync(request, retryCount: 3);

if (response.IsSuccess)
{
    // Process response
}
```

## Notes

*   **Thread Safety:** These extension methods are thread-safe, provided the underlying `HttpClient` instance is managed according to best practices (e.g., using `IHttpClientFactory` to prevent socket exhaustion).
*   **Cancellation:** All methods support `CancellationToken` to enable timely cancellation of long-running requests. It is recommended to always pass a valid token in high-throughput environments.
*   **Exception Handling:** These methods do not suppress exceptions. Callers should implement appropriate `try-catch` blocks to handle network-level exceptions (`HttpRequestException`), deserialization errors (`JsonException`), or task cancellations.
*   **Response Handling:** `HttpResponseData` is a wrapper intended to simplify access to status codes and content streams; ensure that `HttpResponseData` is disposed of appropriately if it holds underlying `HttpContent` streams to prevent memory leaks.
