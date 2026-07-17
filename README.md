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
