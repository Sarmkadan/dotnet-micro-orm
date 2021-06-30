# ServiceCollectionExtensions
The `ServiceCollectionExtensions` class provides a set of extension methods for the `IServiceCollection` interface, allowing for the configuration of various services and settings related to the dotnet-micro-orm project. These extensions enable the addition of services such as migrations, repositories, and ORM services, as well as the configuration of settings like command timeout, batch size, and caching.

## API
* `public static IServiceCollection AddDotnetMicroOrm`: Adds the dotnet-micro-orm services to the service collection. Returns the `IServiceCollection` instance. Throws no exceptions.
* `public static IServiceCollection AddMigration<TMigration>`: Adds a migration of type `TMigration` to the service collection. Returns the `IServiceCollection` instance. Throws no exceptions.
* `public static IServiceCollection AddRepository<TEntity, TRepository>`: Adds a repository of type `TRepository` for the entity type `TEntity` to the service collection. Returns the `IServiceCollection` instance. Throws no exceptions.
* `public static IServiceCollection AddOrmService<TService>`: Adds an ORM service of type `TService` to the service collection. Returns the `IServiceCollection` instance. Throws no exceptions.
* `public int CommandTimeout`: Gets or sets the command timeout in seconds. Default value is not specified.
* `public int DefaultBatchSize`: Gets or sets the default batch size. Default value is not specified.
* `public bool EnableChangeTracking`: Gets or sets a value indicating whether change tracking is enabled. Default value is not specified.
* `public bool EnableAuditLogging`: Gets or sets a value indicating whether audit logging is enabled. Default value is not specified.
* `public bool EnableExpressionCaching`: Gets or sets a value indicating whether expression caching is enabled. Default value is not specified.
* `public int MaxCachedExpressions`: Gets or sets the maximum number of cached expressions. Default value is not specified.
* `public int ConnectionRetryAttempts`: Gets or sets the number of connection retry attempts. Default value is not specified.
* `public int ConnectionRetryDelayMs`: Gets or sets the connection retry delay in milliseconds. Default value is not specified.

## Usage
The following examples demonstrate how to use the `ServiceCollectionExtensions` class:
```csharp
// Example 1: Adding dotnet-micro-orm services and configuring settings
var services = new ServiceCollection();
services.AddDotnetMicroOrm();
services.AddRepository<MyEntity, MyRepository>();
services.CommandTimeout = 30;
services.DefaultBatchSize = 100;

// Example 2: Adding migrations and ORM services
var services = new ServiceCollection();
services.AddMigration<MyMigration>();
services.AddOrmService<MyOrmService>();
services.EnableChangeTracking = true;
services.EnableAuditLogging = true;
```

## Notes
When using the `ServiceCollectionExtensions` class, consider the following edge cases and thread-safety remarks:
* The `CommandTimeout` and `DefaultBatchSize` properties have default values that may not be suitable for all applications. It is recommended to explicitly set these values based on the specific requirements of the application.
* The `EnableChangeTracking`, `EnableAuditLogging`, and `EnableExpressionCaching` properties are boolean flags that can be set to enable or disable the corresponding features. These flags should be set carefully, as they can significantly impact the behavior of the application.
* The `MaxCachedExpressions` property controls the maximum number of cached expressions. If this value is set too low, it may lead to performance issues due to excessive caching. If set too high, it may lead to memory issues due to excessive caching.
* The `ConnectionRetryAttempts` and `ConnectionRetryDelayMs` properties control the connection retry behavior. These values should be set based on the specific requirements of the application and the underlying database system.
* The `ServiceCollectionExtensions` class is designed to be thread-safe, allowing it to be safely used in multi-threaded environments. However, it is still important to ensure that the underlying service collection is properly synchronized to avoid any potential issues.
