# DefaultHttpClient

The `DefaultHttpClient` class provides a high-level wrapper around the standard .NET `HttpClient` to facilitate common HTTP operations within a micro-ORM environment. It simplifies the execution of asynchronous HTTP requests by incorporating standard REST verbs and built-in retry functionality to handle transient failures, ensuring consistent communication patterns for data-driven applications.

## API

### Constructors
- **`DefaultHttpClient()`**: Initializes a new instance of the `DefaultHttpClient` class with default configuration.

### Methods
- **`GetAsync(string url)`**: Sends an asynchronous GET request to the specified URL. Returns a `Task<HttpResponseData>` containing the response.
- **`PostAsync(string url, object content)`**: Sends an asynchronous POST request to the specified URL with the provided content. Returns a `Task<HttpResponseData>`.
- **`PutAsync(string url, object content)`**: Sends an asynchronous PUT request to the specified URL with the provided content. Returns a `Task<HttpResponseData>`.
- **`DeleteAsync(string url)`**: Sends an asynchronous DELETE request to the specified URL. Returns a `Task<HttpResponseData>`.
- **`GetWithRetryAsync(string url)`**: Sends an asynchronous GET request to the specified URL. If the request fails due to transient network issues, it automatically retries based on the configured `LinearRetryPolicy`. Returns a `Task<HttpResponseData>`.
- **`GetDelay()`**: Retrieves the `TimeSpan` interval used between retry attempts.
- **`DisposeAsync()`**: Performs an asynchronous release of resources used by the `DefaultHttpClient` instance.

### Properties
- **`LinearRetryPolicy`**: Gets or sets the retry policy configuration defining the behavior for `GetWithRetryAsync` operations.

## Usage

### Performing a Simple Request
```csharp
using (var client = new DefaultHttpClient())
{
    var response = await client.GetAsync("https://api.example.com/data/1");
    if (response.IsSuccess)
    {
        var data = response.Content;
    }
}
```

### Executing a Request with Retry Logic
```csharp
var client = new DefaultHttpClient();
// Configure retry policy if necessary
client.LinearRetryPolicy = new LinearRetryPolicy(delay: TimeSpan.FromSeconds(2), retries: 3);

try 
{
    var response = await client.GetWithRetryAsync("https://api.example.com/data/1");
    // Handle response...
}
catch (HttpRequestException ex)
{
    // Handle persistent failure
}
```

## Notes

- **Thread Safety**: The `DefaultHttpClient` is designed to be thread-safe for all HTTP operations (`GetAsync`, `PostAsync`, etc.). It is recommended to reuse instances for the lifetime of the application to avoid socket exhaustion.
- **Disposal**: While `DefaultHttpClient` implements `IAsyncDisposable`, it should only be disposed of when the application or the containing service is being shut down, rather than after every individual request.
- **Error Handling**: `GetWithRetryAsync` will throw an exception only after the maximum number of retry attempts defined by `LinearRetryPolicy` has been exhausted. Transient errors encountered during intermediate retries are handled internally.
