// entire file content ...

// ... goes in between

## MemoryCacheProviderTests

The `MemoryCacheProviderTests` class provides comprehensive unit tests for the `MemoryCacheProvider` class, ensuring its caching functionality works as expected. This class contains various test methods to validate different scenarios, such as cache hits, misses, expiration, and removal.

### Example Usage

```csharp
using DotnetMicroOrm.Caching;
using DotnetMicroOrm.Tests;

var cache = new MemoryCacheProvider();

// Test setting and getting a value from the cache
await cache.SetAsync("key", "value");
var result = await cache.GetAsync<string>("key");
Console.WriteLine(result); // Output: value

// Test removing a key from the cache
await cache.RemoveAsync("key");
result = await cache.GetAsync<string>("key");
Console.WriteLine(result); // Output: 

// Test clearing all entries from the cache
await cache.SetAsync("key1", "value1");
await cache.SetAsync("key2", "value2");
await cache.ClearAsync();
var count = await cache.GetCountAsync();
Console.WriteLine(count); // Output: 0
```

## ValidationExample

The `ValidationExample` class demonstrates how to validate business rules and constraints before persisting entities. It shows validation for product fields such as name, price, stock quantity, and description, with comprehensive error reporting through the `IsValid` property and `Errors` collection.

### Example Usage

```csharp
using DotnetMicroOrm.Examples;

// Create validation example with connection string
var example = new ValidationExample("Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;");

// Run validation demonstration
await example.RunAsync();

// Or use as standalone validation
var validationResult = new ValidationExample.ValidationResult
{
    IsValid = true,
    Errors = new List<string>()
};

if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

## ExpressionAndCachingBenchmarksExtensions

`ExpressionAndCachingBenchmarksExtensions` provides a set of helper extension methods that simplify the creation, cloning, and inspection of expression trees used in benchmark scenarios. These methods enable quick generation of simple and complex lambda expressions, deep‑clone existing expressions, and retrieve the body of a lambda expression.

### Example Usage

```csharp
using System;
using System.Linq.Expressions;
using DotnetMicroOrm.Benchmarks;

// Assume an instance of the benchmark class exists
var benchmarks = new ExpressionAndCachingBenchmarks();

// Create a simple lambda expression that returns a constant value
Expression<Func<int>> simpleExpr = benchmarks.CreateExpression<int>(
    paramName: "unused",
    body: Expression.Constant(42));

Console.WriteLine(simpleExpr); // Output: () => 42

// Create a more complex lambda expression that evaluates a boolean condition
Expression<Func<int, bool>> complexExpr = benchmarks.CreateComplexExpression<int>(
    paramName: "x",
    body: Expression.Constant(true));

Console.WriteLine(complexExpr); // Output: x => True

// Clone the previously created expression
Expression clonedExpr = benchmarks.CloneExpression(simpleExpr);
Console.WriteLine(clonedExpr); // Output: () => 42 (cloned)

// Retrieve the body of the simple expression
Expression body = benchmarks.GetBody(simpleExpr);
Console.WriteLine(body); // Output: 42
```

## WebhookHandlerExtensions

`WebhookHandlerExtensions` offers a collection of extension methods for `WebhookHandler` that streamline common webhook processing scenarios, including retry logic, batch handling, payload creation, data extraction, event‑type checking, age calculation, and required‑field validation. These helpers encapsulate guard clauses and repetitive patterns, allowing developers to work with webhooks in a concise and type‑safe manner.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Integration;

// Assume a concrete WebhookHandler implementation is available
var handler = new WebhookHandler(/* dependencies */);

// Create a payload for an "order.created" event
var payload = handler.CreatePayload(
    eventType: "order.created",
    data: new Dictionary<string, object>
    {
        ["orderId"] = 123,
        ["amount"] = 49.99m
    });

// Process the payload with automatic retry logic
var result = await handler.ProcessWithRetryAsync(
    payload,
    handler.GenerateSignature(payload));

Console.WriteLine(result.Success
    ? "Webhook processed successfully"
    : $"Processing failed: {result.Error}");

// Extract strongly‑typed data from the payload
var orderInfo = handler.GetData<OrderInfo>(payload);
if (orderInfo != null && handler.IsEventType(payload, "order.created"))
{
    Console.WriteLine($"Order {orderInfo.OrderId} amount {orderInfo.Amount}");
}

// Process a batch of payloads
var batch = new[]
{
    handler.CreatePayload("order.created"),
    handler.CreatePayload("order.cancelled")
};

var batchResults = await handler.ProcessBatchAsync(batch);
foreach (var kvp in batchResults)
{
    Console.WriteLine($"{kvp.Key}: {(kvp.Value.Success ? "OK" : "Failed")}");
}
```

## MigrationRecordExtensions

`MigrationRecordExtensions` provides a set of extension methods for `MigrationRecord` that simplify common migration operations such as checking success status, retrieving error messages, comparing migration timestamps, formatting records for display, and filtering failed migrations. These helpers encapsulate repetitive patterns and provide a fluent API for working with migration records.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Migrations;

// Assume we have a collection of migration records
var migrations = new List<MigrationRecord>
{
    new MigrationRecord
    {
        Id = 1,
        Version = "1.0.0",
        Description = "Initial migration",
        AppliedAt = new DateTime(2024, 1, 15, 10, 30, 0),
        Success = true,
        ErrorMessage = null
    },
    new MigrationRecord
    {
        Id = 2,
        Version = "1.0.1",
        Description = "Add users table",
        AppliedAt = new DateTime(2024, 1, 16, 9, 15, 0),
        Success = false,
        ErrorMessage = "Table already exists"
    },
    new MigrationRecord
    {
        Id = 3,
        Version = "1.0.2",
        Description = "Create indexes",
        AppliedAt = new DateTime(2024, 1, 17, 14, 20, 0),
        Success = true,
        ErrorMessage = null
    }
};

// Check if a specific migration was successful
var migration1 = migrations[0];
bool isSuccessful = migration1.WasSuccessful();
Console.WriteLine($"Migration 1 successful: {isSuccessful}"); // Output: Migration 1 successful: True

// Get the error message if migration failed
var migration2 = migrations[1];
string errorMessage = migration2.GetErrorMessage();
Console.WriteLine($"Migration 2 error: {errorMessage}"); // Output: Migration 2 error: Table already exists

// Check if migration was applied before a specific date
var cutoffDate = new DateTime(2024, 1, 16, 12, 0, 0);
bool appliedBefore = migration1.WasAppliedBefore(cutoffDate);
Console.WriteLine($"Migration 1 applied before cutoff: {appliedBefore}"); // Output: Migration 1 applied before cutoff: True

// Format a migration record as a display string
string displayString = migration3.ToDisplayString();
Console.WriteLine(displayString);
// Output: Migration 3: 1.0.2 - Create indexes (Applied: 2024-01-17 14:20:00) Status: SUCCESS

// Get all failed migrations from a collection
IReadOnlyList<MigrationRecord> failedMigrations = migrations.GetFailedMigrations();
Console.WriteLine($"Number of failed migrations: {failedMigrations.Count}"); // Output: Number of failed migrations: 1
```

## PipelineBuilderExtensions

`PipelineBuilderExtensions` provides a collection of extension methods for `PipelineBuilder` that simplify middleware pipeline construction and management. It offers features for adding ordered middleware, conditional execution, context transformation, cloning pipelines, removing middleware by type, and diagnostic utilities to inspect pipeline composition. These helpers enable clean, fluent pipeline configuration while maintaining type safety.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotnetMicroOrm.Middleware;
using DotnetMicroOrm.Pipeline;

// Create a simple middleware that adds a message to the context
public class MessageMiddleware : IMiddleware
{
    private readonly string _message;
    
    public MessageMiddleware(string message) => _message = message;
    
    public Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        context.Items["Messages"] = context.Items.GetValueOrDefault("Messages", new List<string>()) as List<string> ?? new List<string>();
        ((List<string>)context.Items["Messages"]).Add(_message);
        return next(context);
    }
}

// Create a pipeline with ordered middleware
var builder = new PipelineBuilder();

// Add middleware with explicit ordering (lower values execute first)
builder.Use(new MessageMiddleware("First"), order: 10);
builder.Use(new MessageMiddleware("Second"), order: 20);
builder.Use(new MessageMiddleware("Third"), order: 30);

// Add multiple middleware with explicit orders
builder.UseAll(
    (new MessageMiddleware("Ordered 1"), 5),
    (new MessageMiddleware("Ordered 2"), 15),
    (new MessageMiddleware("Ordered 3"), 25)
);

// Add conditional middleware that only executes when context has a specific flag
builder.UseWhen(
    ctx => ctx.Items.ContainsKey("ShouldLog"),
    new MessageMiddleware("Conditional log middleware")
);

// Add middleware that transforms the context before passing to next
builder.UseTransform(
    ctx => 
    {
        ctx.Items["Transformed"] = true;
        return ctx;
    },
    new MessageMiddleware("Middleware after transformation")
);

// Clone the pipeline for reuse
var clonedBuilder = builder.Clone();

// Remove all middleware of a specific type
builder.RemoveAll<MessageMiddleware>();

// Get diagnostic information
Console.WriteLine(builder.GetMiddlewareCountString());
var middlewareTypes = builder.GetMiddlewareTypeNames();
Console.WriteLine(string.Join(", ", middlewareTypes));

// Execute pipeline and get the resulting context
var context = new MiddlewareContext();
await builder.ExecuteAndGetContextAsync(context);
```

## OrderExtensions

`OrderExtensions` adds a set of handy extension methods for the `Order` domain model, allowing you to calculate total weight, determine urgency, format a readable display string, and estimate delivery dates without modifying the original `Order` class.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Domain.Models;

var order = new Order
{
    OrderNumber = 123,
    OrderDate = DateTime.UtcNow.AddDays(-1),
    Status = "Pending",
    TotalAmount = 150.00m,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 1, Quantity = 2 },
        new OrderItem { ProductId = 2, Quantity = 1 }
    },
    ShippingAddress = "123 Main St, Springfield",
    CreatedDate = DateTime.UtcNow.AddDays(-1)
};

decimal totalWeight = order.GetTotalWeight();               // uses default weight
bool isUrgent = order.IsUrgent();                           // true for recent pending orders
string display = order.ToDisplayString();                   // formatted order details
DateTime? estimatedDelivery = order.GetEstimatedDeliveryDate();

Console.WriteLine($"Total weight: {totalWeight} kg");
Console.WriteLine($"Is urgent: {isUrgent}");
Console.WriteLine(display);
Console.WriteLine($"Estimated delivery: {estimatedDelivery}");
```

## SpecificationCombinatorsValidation

`SpecificationCombinatorsValidation` provides validation helpers for specification compositions and individual specifications. It offers methods to validate specification compositions (And, Or, Not) and individual specifications, returning detailed error messages or throwing exceptions when validation fails. This class is useful for ensuring specifications are correctly composed before using them in queries or business logic.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Specifications;

// Define a simple specification for filtering active users
public class ActiveUserSpecification : Specification<User>
{
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Filter?.Status))
            errors.Add("Status filter cannot be null or empty");
        return errors.AsReadOnly();
    }

    public override bool IsSatisfiedBy(User entity) =>
        entity.Status == "Active";
}

// Define another specification for filtering users by role
public class AdminUserSpecification : Specification<User>
{
    public override bool IsSatisfiedBy(User entity) =>
        entity.Role == "Admin";
}

// Create specifications
var activeUsers = new ActiveUserSpecification();
var adminUsers = new AdminUserSpecification();

// Validate individual specifications
var activeUserErrors = SpecificationCombinatorsValidation.Validate(activeUsers);
if (activeUserErrors.Count > 0)
{
    Console.WriteLine("Active user specification has errors:");
    foreach (var error in activeUserErrors)
        Console.WriteLine($"- {error}");
}

// Check if specifications are valid
bool isAdminValid = SpecificationCombinatorsValidation.IsValid(adminUsers);
Console.WriteLine($"Admin specification is valid: {isAdminValid}");

// Validate specification composition (e.g., Active AND Admin users)
var compositionErrors = SpecificationCombinatorsValidation.ValidateComposition(
    activeUsers,
    adminUsers
);

if (compositionErrors.Count > 0)
{
    Console.WriteLine("Composition is invalid:");
    foreach (var error in compositionErrors)
        Console.WriteLine($"- {error}");
}
else
{
    Console.WriteLine("Composition is valid - both specifications are correctly defined");
}

// Ensure composition is valid, throws exception if invalid
try
{
    SpecificationCombinatorsValidation.EnsureValidComposition(
        activeUsers,
        adminUsers
    );
    Console.WriteLine("Composition validated successfully!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate individual specification and ensure it's valid
try
{
    SpecificationCombinatorsValidation.EnsureValid(activeUsers);
    Console.WriteLine("Active user specification is valid!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## UserRepositoryValidation

`UserRepositoryValidation` provides a set of extension methods for validating `UserRepository` instances and their parameters. It includes methods to validate repository instances, usernames, emails, date ranges, and inactive user thresholds, ensuring data integrity before operations. The validation methods return boolean results while the Ensure methods throw exceptions on failure, making them suitable for guard clauses in repository methods.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Data.Repositories;

// Create a UserRepository instance (typically injected via DI)
var userRepository = new UserRepository(/* database context */);

// Validate the repository instance itself
bool isRepositoryValid = userRepository.IsValid();
Console.WriteLine($"Repository is valid: {isRepositoryValid}");

// Validate repository and throw if invalid
try
{
    userRepository.EnsureValid();
    Console.WriteLine("Repository passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Repository validation failed: {ex.Message}");
}

// Validate username parameter before calling repository methods
string username = "john_doe";
bool isUsernameValid = username.IsValidUsername();
Console.WriteLine($"Username '{username}' is valid: {isUsernameValid}");

// Validate email parameter
string email = "john@example.com";
bool isEmailValid = email.IsValidEmail();
Console.WriteLine($"Email '{email}' is valid: {isEmailValid}");

// Validate date range for queries
DateTime startDate = DateTime.UtcNow.AddDays(-30);
DateTime endDate = DateTime.UtcNow;
bool isDateRangeValid = startDate.IsValidDateRange(endDate);
Console.WriteLine($"Date range is valid: {isDateRangeValid}");

// Validate days inactive parameter for GetInactiveUsersAsync
int daysInactive = 30;
bool isDaysInactiveValid = daysInactive.IsValidDaysInactive();
Console.WriteLine($"Days inactive threshold is valid: {isDaysInactiveValid}");
```
