// ... (rest of the README content remains the same)

## DefaultHttpClient

The `DefaultHttpClient` class is a reusable HTTP client implementation that provides retry logic, timeout handling, and logging capabilities. It is designed to be thread-safe and can be used across multiple requests.

### Example Usage

```csharp
public class Program
{
    public static async Task Main()
    {
        var client = new DefaultHttpClient();

        var response = await client.GetAsync("https://example.com/api/data");
        Console.WriteLine($"Status code: {response.StatusCode}");

        var body = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");

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

        var body = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");

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

        var body = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");

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

        var body = await response.Body.ReadAsStringAsync();
        Console.WriteLine($"Response body: {body}");

        await client.DisposeAsync();
    }
}
```
```