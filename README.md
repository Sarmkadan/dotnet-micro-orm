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

// ... goes in between

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

