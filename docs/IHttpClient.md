# IHttpClient

The `IHttpClient` interface defines a standardized contract for performing HTTP operations within the `dotnet-micro-orm` ecosystem, providing properties to configure requests and inspect response metadata.

## API

*   `int StatusCode`: Gets or sets the HTTP status code returned by the server.
*   `string Body`: Gets or sets the response body content as a string.
*   `Dictionary<string, string> Headers`: Gets or sets the collection of HTTP headers associated with the response.
*   `TimeSpan Duration`: Gets or sets the total time elapsed for the HTTP request to complete.
*   `Exception? Exception`: Gets or sets the exception, if any, that occurred during the execution of the request.
*   `TimeSpan Timeout`: Gets or sets the maximum duration permitted for the HTTP request before it is cancelled.
*   `string UserAgent`: Gets or sets the User-Agent string header sent with the request.
*   `int MaxRedirects`: Gets or sets the maximum number of HTTP redirects to follow automatically.
*   `bool AutoDecompression`: Gets or sets a value indicating whether the client should automatically decompress GZip or Deflate encoded response content.
*   `bool PoolConnections`: Gets or sets a value indicating whether HTTP connections should be pooled for reuse.

## Usage

### Configuring a Request

```csharp
var client = GetHttpClientInstance(); // Implementation specific
client.Timeout = TimeSpan.FromSeconds(30);
client.UserAgent = "MyMicroOrmApp/1.0";
client.AutoDecompression = true;
client.PoolConnections = true;
client.MaxRedirects = 3;
```

### Inspecting a Response

```csharp
var client = PerformRequest(); // Implementation specific
if (client.Exception != null)
{
    Console.WriteLine($"Request failed: {client.Exception.Message}");
}
else if (client.StatusCode >= 200 && client.StatusCode < 300)
{
    Console.WriteLine($"Success. Received {client.Body.Length} bytes in {client.Duration.TotalMilliseconds}ms.");
}
else
{
    Console.WriteLine($"Request returned non-success status code: {client.StatusCode}");
}
```

## Notes

*   **Thread Safety**: Implementations of `IHttpClient` are not inherently thread-safe. Instances should not be shared across multiple threads without external synchronization if configuration properties are modified concurrently.
*   **Exception Handling**: The `Exception` property will be `null` if the request completed successfully. It is recommended to check this property before accessing `StatusCode` or `Body` when a request might have failed at the transport level.
*   **Initialization**: Ensure all required configuration properties (e.g., `Timeout`) are set before initiating a request, as default values may vary based on the specific implementation.
