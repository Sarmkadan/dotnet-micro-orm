# ApplicationBuilder

The `ApplicationBuilder` serves as the central configuration hub for constructing the runtime environment within the `dotnet-micro-orm` framework. It facilitates a fluent interface for registering dependencies, configuring database contexts, establishing caching strategies, and defining the HTTP client pipeline. By chaining configuration methods, developers can assemble a tailored `ApplicationConfiguration` instance that encapsulates all required services and middleware pipelines necessary for the application's execution lifecycle.

## API

### Configuration Methods

These methods configure the builder state and return the `ApplicationBuilder` instance to support fluent chaining.

#### `WithDatabase`
Configures the primary database implementation for the application.
*   **Parameters**: Implementation-specific arguments required to initialize the database context.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws an exception if the database configuration is invalid or if called multiple times with conflicting settings.

#### `WithDatabaseConnection`
Explicitly sets the connection string or connection factory for the database context.
*   **Parameters**: Connection string or configuration object.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws if the connection string is null, empty, or malformed.

#### `AddMiddleware`
Appends a specific middleware delegate to the processing pipeline.
*   **Parameters**: A delegate or type representing the middleware logic.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws if the middleware type is invalid or cannot be instantiated.

#### `WithMemoryCache`
Configures the application to use an in-memory caching implementation.
*   **Parameters**: Optional configuration options for memory cache size or expiration policies.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: None.

#### `WithCacheProvider`
Registers a custom implementation of the `ICacheProvider` interface.
*   **Parameters**: An instance or type definition of the custom cache provider.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws if the provided type does not implement `ICacheProvider`.

#### `WithHttpClient`
Configures the `IHttpClient` implementation used for external HTTP requests.
*   **Parameters**: Configuration for base addresses, timeouts, or default headers.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws if the HTTP client configuration is invalid.

#### `RegisterService<T>`
Registers a custom service of type `T` into the application's service dictionary.
*   **Parameters**: The service instance or factory method.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: Throws if a service of type `T` is already registered and overwrites are not permitted.

#### `WithDefaultMiddleware`
Populates the pipeline with the framework's standard set of middleware components.
*   **Parameters**: None.
*   **Returns**: `ApplicationBuilder`
*   **Throws**: None.

#### `Build`
Finalizes the configuration and constructs the immutable `ApplicationConfiguration` object.
*   **Parameters**: None.
*   **Returns**: `ApplicationConfiguration`
*   **Throws**: Throws `InvalidOperationException` if any required properties (e.g., `DatabaseContext`, `Pipeline`) have not been initialized prior to calling this method.

### Runtime Properties

These properties represent the constructed state of the application. They are marked as `required`, meaning they must be assigned before the object is considered valid.

#### `DatabaseContext`
*   **Type**: `IDatabaseContext`
*   **Description**: The active database context instance used for ORM operations.

#### `Pipeline`
*   **Type**: `Func<MiddlewareContext, Task>`
*   **Description**: The compiled delegate representing the full middleware execution chain.

#### `CacheProvider`
*   **Type**: `ICacheProvider`
*   **Description**: The active caching mechanism used by the application.

#### `HttpClient`
*   **Type**: `IHttpClient`
*   **Description**: The configured client for outbound HTTP communications.

#### `Services`
*   **Type**: `Dictionary<string, object>`
*   **Description**: A dictionary containing all registered services keyed by their string identifier.

### Utility Methods

#### `GetService<T>`
Retrieves a registered service of type `T` from the internal service collection.
*   **Parameters**: None (generic type argument `T`).
*   **Returns**: `T?` (The service instance if found; otherwise `null`).
*   **Throws**: None.

#### `LogInformation`
Writes an informational message to the application logger.
*   **Parameters**: Message string and optional arguments.
*   **Returns**: `void`
*   **Throws**: None.

#### `LogDebug`
Writes a debug-level message to the application logger.
*   **Parameters**: `string message`, `params object?[] args`.
*   **Returns**: `void`
*   **Throws**: None.

#### `LogError`
Writes an error-level message to the application logger.
*   **Parameters**: Message string, exception details, and optional arguments.
*   **Returns**: `void`
*   **Throws**: None.

## Usage

### Example 1: Standard Configuration
This example demonstrates a typical setup where the builder configures the database, enables default middleware, and sets up memory caching before building the final configuration.

```csharp
var builder = new ApplicationBuilder();

var config = builder
    .WithDatabaseConnection("Server=localhost;Database=MyApp;Trusted_Connection=True;")
    .WithMemoryCache()
    .WithDefaultMiddleware()
    .RegisterService<IMyCustomService>(new MyCustomService())
    .Build();

// Access the constructed context
var db = config.DatabaseContext;
```

### Example 2: Custom Pipeline and Services
This example illustrates replacing default components with custom implementations and manually adding specific middleware to the pipeline.

```csharp
var builder = new ApplicationBuilder();

builder
    .WithDatabase(new CustomDbContext())
    .WithCacheProvider(new RedisCacheProvider("localhost:6379"))
    .WithHttpClient(new ConfiguredHttpClient())
    .AddMiddleware(async context => 
    {
        builder.LogInformation("Processing request...");
        await context.Next();
        builder.LogDebug("Request completed.");
    });

// Retrieve a service before building if necessary for validation
var service = builder.GetService<IValidationService>();

var config = builder.Build();
```

## Notes

*   **Required Properties**: The properties `DatabaseContext`, `Pipeline`, `CacheProvider`, `HttpClient`, and `Services` are marked as `required`. Attempting to access the `ApplicationBuilder` instance or call `Build()` without satisfying these requirements will result in a runtime exception or compiler error depending on the initialization context.
*   **Fluent Interface Immutability**: While the builder methods return `ApplicationBuilder` for chaining, they modify the internal state of the instance. Ensure that the same instance is used throughout the chain; splitting the chain across different variable references without careful handling may lead to inconsistent configuration states.
*   **Thread Safety**: The `ApplicationBuilder` is not thread-safe. Configuration methods should be called sequentially on a single thread during the application startup phase. Concurrent calls to `AddMiddleware`, `RegisterService`, or property assignment may result in race conditions within the `Services` dictionary or the middleware pipeline construction.
*   **Service Retrieval**: The `GetService<T>` method returns `null` if the service is not found rather than throwing an exception. Callers must handle potential null returns, especially when retrieving optional dependencies before the `Build()` phase.
*   **Logging Scope**: The logging methods (`LogInformation`, `LogDebug`, `LogError`) available on the builder are intended for startup-time diagnostics. Once `Build()` is called, logging should typically be delegated to the logger instance resolved from the resulting `ApplicationConfiguration` or the middleware context.
