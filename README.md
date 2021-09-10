// ... (rest of the README content remains the same)

## IHttpClient

The `IHttpClient` interface provides a contract for HTTP client operations with built-in retry support, timeout handling, and request/response logging.

### Example Usage

```csharp
public class Program
{
    public static async Task Main()
    {
        var client = new DefaultHttpClient();

        var response = await client.GetAsync("https://example.com/api/data");
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Response body: {response.Body}");
        Console.WriteLine($"Response headers: {string.Join(", ", response.Headers.Select(x => $"{x.Key}: {x.Value}"))}");
        Console.WriteLine($"Request duration: {response.Duration}");
        Console.WriteLine($"Exception: {response.Exception?.Message}");

        await client.DisposeAsync();
    }
}
```

You can also use the `GetWithRetryAsync` method to handle server errors with retries:

```csharp
public class Program
{
    public static async Task Main()
    {
        var client = new DefaultHttpClient();

        var response = await client.GetWithRetryAsync("https://example.com/api/data", maxRetries: 3);
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Response body: {response.Body}");
        Console.WriteLine($"Response headers: {string.Join(", ", response.Headers.Select(x => $"{x.Key}: {x.Value}"))}");
        Console.WriteLine($"Request duration: {response.Duration}");
        Console.WriteLine($"Exception: {response.Exception?.Message}");

        await client.DisposeAsync();
    }
}
```

Note that you can customize the retry policy by passing an instance of `IRetryPolicy` to the `DefaultHttpClient` constructor.

```csharp
public class Program
{
    public static async Task Main()
    {
        var retryPolicy = new LinearRetryPolicy(TimeSpan.FromSeconds(1));
        var client = new DefaultHttpClient(retryPolicy: retryPolicy);

        var response = await client.GetWithRetryAsync("https://example.com/api/data", maxRetries: 3);
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Response body: {response.Body}");
        Console.WriteLine($"Response headers: {string.Join(", ", response.Headers.Select(x => $"{x.Key}: {x.Value}"))}");
        Console.WriteLine($"Request duration: {response.Duration}");
        Console.WriteLine($"Exception: {response.Exception?.Message}");

        await client.DisposeAsync();
    }
}
```

You can also use the `DisposeAsync` method to dispose of the client when you're done with it.

```csharp
public class Program
{
    public static async Task Main()
    {
        var client = new DefaultHttpClient();

        var response = await client.GetAsync("https://example.com/api/data");
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Response body: {response.Body}");
        Console.WriteLine($"Response headers: {string.Join(", ", response.Headers.Select(x => $"{x.Key}: {x.Value}"))}");
        Console.WriteLine($"Request duration: {response.Duration}");
        Console.WriteLine($"Exception: {response.Exception?.Message}");

        await client.DisposeAsync();
    }
}
```
