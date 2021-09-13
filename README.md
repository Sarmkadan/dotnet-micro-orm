// ... (rest of the README content remains the same)

## CommandParser

The `CommandParser` class provides a fluent API for parsing command-line arguments and options, enabling easy construction of command-line interfaces with strongly-typed configuration.

### Example Usage

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var parser = new CommandParser("myapp");
        
        parser.RegisterCommand("greet", "Prints a greeting message", ctx => 
        {
            var name = ctx.GetArgument("name");
            Console.WriteLine($"Hello, {name ?? "World"}!");
        });
        
        parser.AddOption("verbose", "Enables verbose output", false);
        
        var context = parser.Parse(args);
        
        if (context.ShowHelp)
        {
            Console.WriteLine(parser.GetHelpText());
            return;
        }
        
        if (context.HasArgument("verbose"))
        {
            Console.WriteLine("Verbose mode enabled");
        }
        
        context.Handler?.Invoke(context);
    }
}
```

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
