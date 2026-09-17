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

## WebhookHandler

`WebhookHandler` (in `src/Integration/WebhookHandler.cs`) is a sealed, `IAsyncDisposable`
class that receives incoming webhooks with **HMAC-SHA256 signature verification** and
delivers outgoing webhooks with **SSRF protection**, circuit breaking, retry, and
dead-letter support. It is the entry point for the webhook subsystem and is backed by
`WebhookSignatureValidator` (signature + anti-replay) and `UrlSsrfValidator` (SSRF
protection).

### Signature Validation

Incoming webhooks are verified in `ProcessAsync` before any handler runs. The signature
header uses the format `t={unixTimestamp},v1={hexSignature}` and is checked by
`WebhookSignatureValidator`:

- **HMAC-SHA256** — the signature is computed over `"{timestamp}.{payloadJson}"` using
  the shared secret, so the payload cannot be tampered with without the secret.
- **Timestamp anti-replay** — the `t` value must fall within the configured tolerance
  window (default: 5 minutes) of the current time, rejecting replayed or stale requests.
- **Constant-time comparison** — signatures are compared with
  `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.

A failed check returns a `WebhookResult` with `Success = false` and
`Error = "Invalid signature"`; no handlers are invoked.

### SSRF Protection via UrlSsrfValidator

Outgoing deliveries are protected against Server-Side Request Forgery. The default
`IHttpClient` is constructed with `UrlSsrfValidator.CreateSsrfProtectedConfig()`, and
`DeliverAsync` calls `UrlSsrfValidator.IsUrlSafeAtDeliveryTimeAsync(url)` before posting.
The validator blocks unsafe targets by checking:

- **Scheme** — only `https` is allowed.
- **Host** — rejects loopback (`localhost`, `127.0.0.1`, `::1`), private ranges
  (`10/8`, `172.16/12`, `192.168/16`), link-local, and multicast addresses.
- **DNS resolution** — resolves the host at delivery time and rejects it if any resolved
  IP falls in a forbidden range, mitigating DNS-rebinding attacks.

A URL that fails SSRF validation returns a `WebhookResult` with
`FinalDisposition = "ssrf_violation"` and no request is sent.

### Delivery Pipeline

`DeliverAsync` combines SSRF validation with a per-URL `CircuitBreakerPolicy`, exponential
backoff retries (default 3 attempts, doubling delay capped at 5 minutes), and a dead-letter
store for failed deliveries. Non-retryable `4xx` responses short-circuit to the dead-letter
store, while `5xx`/timeouts/exceptions are retried.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotnetMicroOrm.Integration;

// Create a handler with a shared secret
await using var handler = new WebhookHandler("your-webhook-secret");

// Register a handler for an event type
handler.Subscribe(WebhookEvents.OrderCreated, async payload =>
{
    Console.WriteLine($"Order event received: {payload.Id}");
});

// Process an incoming webhook (signature verified before handlers run)
var incoming = new WebhookPayload { EventType = WebhookEvents.OrderCreated };
var signatureHeader = handler.GenerateSignatureHeader(incoming);
var result = await handler.ProcessAsync(incoming, signatureHeader);
Console.WriteLine(result.Success ? "Verified and processed" : $"Rejected: {result.Error}");

// Deliver an outgoing webhook (SSRF-protected, with retry + circuit breaker)
var delivery = await handler.DeliverAsync(
    new WebhookPayload { EventType = WebhookEvents.OrderShipped },
    url: "https://partner.example.com/hooks/order-shipped");
Console.WriteLine(delivery.Success ? "Delivered" : $"Failed: {delivery.Error}");
```

## MigrationRunner

`MigrationRunner` (in `src/Migrations/MigrationRunner.cs`) is the default implementation of `IMigrationRunner`. It discovers registered `IMigration` implementations, orders them by version, and applies the pending ones to the database, persisting state in a `dbo._MigrationHistory` table that is created automatically on first use.

### How migrations are discovered

Migrations are not auto-scanned from the assembly. Instead, each migration is registered explicitly with the DI container:

- `AddMigration<TMigration>()` registers a migration as a transient `IMigration` (`src/Configuration/ServiceCollectionExtensions.cs`).
- `MigrationRunner` receives all registered `IMigration` instances through its constructor as an `IEnumerable<IMigration>`.
- The runner sorts them by `IMigration.Version` using `StringComparer.OrdinalIgnoreCase`, so the order in which they are registered does not matter.

A migration implements `IMigration`:

```csharp
public interface IMigration
{
    string Version { get; }        // e.g. "20240101_001" (YYYYMMdd_seq)
    string Description { get; }
    Task UpAsync(IDatabaseContext context);
    Task DownAsync(IDatabaseContext context);
}
```

### How migrations are applied

`MigrateAsync()` is the entry point:

1. `EnsureHistoryTableAsync()` creates `dbo._MigrationHistory` if it does not exist.
2. `GetAppliedVersionsAsync()` reads the versions already recorded with `Success = 1`.
3. For each migration whose version is not in the applied set (in ascending version order), `ApplyMigrationAsync()` runs `UpAsync(context)` and then records the outcome in the history table.

Key behaviors:

- **Idempotent**: already-applied versions are skipped, so running `MigrateAsync()` repeatedly is safe.
- **Failure handling**: if `UpAsync` throws, the runner records the migration as failed (`Success = 0` with the error message) and rethrows an `OrmException` with code `MIGRATION_FAILED`.
- **Targeted apply**: `MigrateToAsync(targetVersion)` applies only migrations with `Version <= targetVersion`.
- **Rollback**: `RollbackToAsync(targetVersion)` runs `DownAsync` in descending version order for applied migrations above the target, then deletes their history rows.
- **Dry runs**: `MigrateDryRunAsync` / `RollbackDryRunAsync` generate the SQL that would run without executing it, by replaying each migration against an in-memory `SqlCaptureContext` that captures statements.
- **Introspection**: `GetAppliedMigrationsAsync`, `GetPendingMigrationsAsync`, and `GetPendingMigrationsWithDetailsAsync` report applied/pending state; `GenerateUpSqlAsync` / `GenerateDownSqlAsync` produce SQL for a single version.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Migrations;

// Implement a migration
public sealed class CreateUsersTable : IMigration
{
    public string Version => "20240101_001";
    public string Description => "Create users table";

    public Task UpAsync(IDatabaseContext context) =>
        context.ExecuteNonQueryAsync("CREATE TABLE Users (Id INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL)");

    public Task DownAsync(IDatabaseContext context) =>
        context.ExecuteNonQueryAsync("DROP TABLE Users");
}

// Register it with DI
services.AddMigration<CreateUsersTable>();

// Resolve the runner and apply pending migrations
var runner = services.BuildServiceProvider().GetRequiredService<IMigrationRunner>();
await runner.MigrateAsync();
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

## QueryBuilder

`QueryBuilder<T>` (in `src/Data/QueryBuilder.cs`) is a fluent, deferred-execution query builder for composing complex queries against an `IRepository<T>`. Clauses are accumulated and executed only when an async materialization method is called. See [docs/QueryBuilder.md](docs/QueryBuilder.md) for the full API reference.

### Public API

- `Where(Expression<Func<T, bool>> predicate)` — adds a filter; multiple calls are combined with a logical AND. Throws `ArgumentNullException` if `predicate` is `null`.
- `OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)` — orders results ascending. Throws `ArgumentNullException` if `keySelector` is `null`.
- `OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)` — orders results descending. Throws `ArgumentNullException` if `keySelector` is `null`.
- `Take(int count)` — limits the number of returned rows. Throws `ArgumentException` if `count <= 0`.
- `Skip(int count)` — skips the first `count` rows. Throws `ArgumentException` if `count < 0`.
- `Include(Expression<Func<T, object>> navigationProperty)` — records a navigation property to materialize with the results. Throws `ArgumentException` if the expression does not point at a member of `T`.
- `IncludedProperties` — `IReadOnlyCollection<string>` of navigation properties registered via `Include`.
- `ToListAsync()` — executes the query and returns `List<T>`.
- `FirstOrDefaultAsync()` — executes the query and returns the first match or `null`.
- `CountAsync()` — executes the query and returns the number of matching rows.

### Example Usage

```csharp
using System;
using System.Linq.Expressions;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Obtain an IRepository<T> (typically injected via DI)
IRepository<User> repository = /* ... */;

var builder = new QueryBuilder<User>(repository);

// Compose a query: filter, order, paginate, and include a navigation property
var recentActiveUsers = await builder
    .Where(u => u.IsActive && u.LastLogin > DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(u => u.LastLogin)
    .Skip(20)
    .Take(10)
    .Include(u => u.Profile)
    .ToListAsync();

// Count matching rows without materializing them
var total = await new QueryBuilder<User>(repository)
    .Where(u => u.IsActive)
    .CountAsync();

// Fetch a single match
var first = await new QueryBuilder<User>(repository)
    .Where(u => u.Email == "john@example.com")
    .FirstOrDefaultAsync();
```

## Specification Pattern

The specification pattern is implemented in `src/Data/Specification.cs` and
`src/Data/SpecificationCombinators.cs`. It encapsulates query logic (filters,
eager-loading includes, ordering, and pagination) into reusable, composable
objects instead of scattering `Where` clauses across call sites.

### Specification<T>

`Specification<T>` (in `src/Data/Specification.cs`) is the abstract base class.
A concrete specification sets `Criteria` (an `Expression<Func<T, bool>>`
predicate) and optionally configures includes, ordering, and paging through the
protected `Apply*` helpers:

| Member | Purpose |
| --- | --- |
| `Criteria` | Predicate that filters results |
| `Includes` / `IncludeStrings` | Eager-loading includes (expression or raw SQL) |
| `OrderBy` / `OrderByDescending` | Ordering expressions |
| `PageNumber` / `PageSize` / `IsPagingEnabled` | Pagination settings |
| `AddInclude(...)` | Adds an eager-loading include |
| `ApplyPaging(page, size)` | Enables pagination |
| `ApplyOrderBy(...)` / `ApplyOrderByDescending(...)` | Sets ordering |

The file also ships ready-made specifications for the demo domain models, e.g.
`ActiveProductsSpecification`, `ProductsByPriceRangeSpecification`,
`LowStockProductsSpecification`, `ActiveUsersSpecification`,
`UserByIdSpecification`, `UsersByEmailSpecification`, `UserOrdersSpecification`,
`PendingOrdersSpecification`, and `RecentOrdersSpecification`.

```csharp
using DotnetMicroOrm.Data;

// Reuse a built-in specification
var active = new ActiveProductsSpecification();

// Or define your own
public sealed class ProductsOverPriceSpecification : Specification<Product>
{
    public ProductsOverPriceSpecification(decimal minPrice)
    {
        Criteria = p => p.Price >= minPrice && p.IsActive;
        ApplyOrderBy(p => p.Price);
    }
}
```

### SpecificationCombinators

`SpecificationCombinators` (in `src/Data/SpecificationCombinators.cs`) provides
extension methods to compose specifications into new ones. Composition unions
the `Includes` and `IncludeStrings` of both operands and combines their
`Criteria` into a single expression tree (rebinding the right-hand parameter
onto the left-hand one):

| Method | Result |
| --- | --- |
| `spec1.And(spec2)` | Criteria is `spec1.Criteria AND spec2.Criteria` |
| `spec1.Or(spec2)` | Criteria is `spec1.Criteria OR spec2.Criteria` |
| `spec.Not()` | Criteria is the logical negation of `spec.Criteria` |

A `null` criteria is treated as always-true, so combining a filter with a
filter-less specification still yields the filter.

```csharp
using DotnetMicroOrm.Data;

var active = new ActiveProductsSpecification();
var inStock = new ProductsByPriceRangeSpecification(0, 1000);

// Active AND in the price range
var activeInRange = active.And(inStock);

// Active OR in the price range
var activeOrInRange = active.Or(inStock);

// Not active
var inactive = active.Not();
```

Related validation helpers live in `src/Data/SpecificationValidation.cs`
(`Validate`, `IsValid`, `EnsureValid`) and
`src/Data/SpecificationCombinatorsValidation.cs` (composition validation).

## Repository

`Repository<T>` (in `src/Data/Repository.cs`) is the generic base repository
implementing `IRepository<T>`. It provides CRUD, query, bulk, and paging
operations for any entity that derives from `BaseEntity`. The repository maps
entity properties to table columns via `[Table]`, `[Column]`, `[NotMapped]`,
and `[ConcurrencyToken]` attributes and executes SQL through an injected
`IDatabaseContext`.

### Generic Repository Pattern

`Repository<T>` is a single generic implementation reused for every entity
type. Instead of writing a dedicated repository per aggregate, you register one
generic instance per entity (typically via DI) and get the full set of data
operations for free:

- **CRUD** — `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- **Querying** — `GetAllAsync`, `GetAsync(predicate)`, `FirstOrDefaultAsync`,
  `ExistsAsync`, `CountAsync`.
- **Bulk operations** — `AddRangeAsync`, `UpdateRangeAsync`, `DeleteRangeAsync`.
- **Paging** — `GetPagedAsync`, `GetPagedResultAsync`, `GetPagedWithCountAsync`.
- **Streaming / queryable** — `Query()` (in-memory `IQueryable<T>` backing
  `QueryBuilder<T>`) and `QueryStreamAsync` for row-by-row reads.

Entities are validated (`Validate`) and normalized (`PreSave` / `PostLoad`)
automatically. If an entity declares a `[ConcurrencyToken]` column, `UpdateAsync`
and `DeleteAsync` include an optimistic-concurrency check and throw
`ConcurrencyException` on conflict.

### Entity Mapping

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

[Table(Name = "Users", Schema = "dbo")]
public class User : BaseEntity
{
    [Column(Name = "Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column(Name = "Email")]
    public string Email { get; set; } = "";

    [Column(Name = "Status")]
    public string Status { get; set; } = "Active";

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
```

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotnetMicroOrm.Data;

// Obtain an IRepository<T> (typically injected via DI)
IRepository<User> repository = /* ... */;

// Create
var user = new User { Email = "john@example.com", Status = "Active" };
await repository.AddAsync(user);

// Read
var byId = await repository.GetByIdAsync(user.Id);
var active = await repository.GetAsync(u => u.Status == "Active");
var first = await repository.FirstOrDefaultAsync(u => u.Email == "john@example.com");
bool exists = await repository.ExistsAsync(u => u.Email == "john@example.com");
int total = await repository.CountAsync();

// Update
user.Status = "Inactive";
await repository.UpdateAsync(user);

// Delete
bool deleted = await repository.DeleteAsync(user.Id);

// Bulk insert
var batch = new List<User>
{
    new User { Email = "a@example.com" },
    new User { Email = "b@example.com" }
};
await repository.AddRangeAsync(batch);

// Paging
var page = await repository.GetPagedResultAsync(pageNumber: 1, pageSize: 20);
Console.WriteLine($"Page {page.PageNumber} of {page.TotalCount} total users");
```

## UnitOfWork

`UnitOfWork` (in `src/Data/UnitOfWork.cs`) is the unit-of-work pattern
implementation for transaction management. It wraps an `IDatabaseContext`,
caches one repository instance per entity type, and tracks a lightweight
in-memory change set. It implements `IUnitOfWork` and is `sealed`.

### Repository Caching

`Repository<T>()` returns a cached `IRepository<T>` for the entity type. The
first call constructs a `Repository<T>` over the shared `IDatabaseContext` and
stores it in a `ConcurrentDictionary`; subsequent calls return the same
instance. All repositories obtained from one `UnitOfWork` therefore share the
same underlying context and transaction.

### Transaction Semantics

Transactions are explicit — nothing is committed implicitly. The lifecycle is
`BeginTransactionAsync` → work → `CommitAsync` (or `RollbackAsync`).

- **`BeginTransactionAsync(isolationLevel = ReadCommitted)`** — starts a
  transaction on the underlying context. Throws `OrmException` with code
  `UOW_TRANSACTION_ACTIVE` if a transaction is already active. Failures are
  wrapped in `ConfigurationException`.
- **`CommitAsync()`** — commits the active transaction and clears the change
  set. Throws `OrmException` with code `UOW_NO_TRANSACTION` if no transaction
  is active. If the commit itself fails, the transaction is automatically
  rolled back before the exception is rethrown.
- **`RollbackAsync()`** — rolls back the active transaction and clears the
  change set. If no transaction is active it is a no-op that returns `true`.
  Rollback failures are wrapped in `OrmException`.

### Change Set Tracking

`SaveChangesAsync()` and `HasChanges()` operate on an in-memory change set
that is **not** persisted to the database. `SaveChangesAsync()` returns the
number of tracked changes and clears the set (or `0` when empty); it does not
issue any SQL. Actual persistence is performed by the repository operations
against the shared context, and durability is governed by the explicit
transaction above.

### Disposal

`DisposeAsync()` rolls back any still-active transaction, disposes the
underlying `IDatabaseContext`, and clears cached repositories and the change
set. It is idempotent.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

await using var uow = new UnitOfWork(context);

var users = uow.Repository<User>();

await uow.BeginTransactionAsync();
try
{
    await users.AddAsync(new User { Email = "a@example.com" });
    await users.AddAsync(new User { Email = "b@example.com" });
    await uow.CommitAsync();
}
catch
{
    await uow.RollbackAsync();
    throw;
}
```

## EventBus and IEventBus

The `EventBus` class is an in-process implementation of the publisher-subscriber pattern, allowing loose communication between components via domain events. It implements the `IEventBus` interface.

### Example Usage

```csharp
using DotnetMicroOrm.Events;
using System.Threading.Tasks;

// 1. Define an event handler for UserCreatedEvent
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    public Task HandleAsync(UserCreatedEvent @event)
    {
        Console.WriteLine($"User created: {@event.Username} ({@event.Email})");
        return Task.CompletedTask;
    }
}

// 2. Create an instance of the event bus
var eventBus = new EventBus();

// 3. Subscribe the handler to the event bus
eventBus.Subscribe<UserCreatedEvent, UserCreatedEventHandler>(new UserCreatedEventHandler());

// 4. Publish a UserCreatedEvent
await eventBus.PublishAsync(new UserCreatedEvent
{
    UserId = 1,
    Username = "john_doe",
    Email = "john@example.com",
    // Note: EventId, OccurredAt, InitiatedBy are set by the base class DomainEvent
    // You can set InitiatedBy if needed, otherwise it defaults to "System"
    InitiatedBy = "UserService"
});
```

### Handler Execution

By default, the `EventBus` executes handlers asynchronously. To execute handlers synchronously, pass `false` to the constructor:

```csharp
var synchronousEventBus = new EventBus(executeAsync: false);
```

### Lifetime and Scope

The `EventBus` is designed for single-application use. For distributed scenarios, consider using a message queue.

## CircuitBreakerPolicy

The `CircuitBreakerPolicy` class (in `src/Integration/CircuitBreakerPolicy.cs`) implements the circuit breaker pattern to prevent cascading failures in distributed systems. It monitors failures and opens the circuit when a threshold is reached, allowing the system to recover before attempting operations again.

### States

The circuit breaker has three states:
- **Closed**: Normal operation. Failures are counted, and when the failure threshold is reached, the circuit opens.
- **Open**: Operations are short-circuited and immediately fail with a `CircuitBreakerOpenException`. After the break duration elapses, the circuit transitions to half-open.
- **Half-open**: A limited number of operations are allowed to test if the underlying has recovered. If successful, the circuit closes; if not, it reopens.

### Configuration

The policy is configured via constructor parameters:
- `failureThreshold`: Number of failures before opening the circuit (default: 5)
- `breakDuration`: Duration to keep the circuit open before transitioning to half-open (default: 30 seconds)
- `halfOpenAttempts`: Number of attempts in half-open state before determining final state (default: 3)

### Example Usage

```csharp
using DotnetMicroOrm.Integration;

// Create circuit breaker with default settings
var circuitBreaker = new CircuitBreakerPolicy();

// Or customize settings
var customBreaker = new CircuitBreakerPolicy(
    failureThreshold: 3,
    breakDuration: TimeSpan.FromSeconds(60),
    halfOpenAttempts: 2);

// Execute operations through the circuit breaker
await circuitBreaker.ExecuteAsync(async () =>
{
    // Your operation that might fail
    await riskyOperationAsync();
});

// For operations that return a value
var result = await circuitBreaker.ExecuteAsync(async () =>
{
    return await fetchDataFromServiceAsync();
});

## JobScheduler and IBackgroundJob

The background job subsystem (in `src/BackgroundJobs/`) provides a thread-safe scheduler for running asynchronous work outside the request pipeline, with support for interval- and cron-based scheduling, retries, timeouts, and execution history tracking.

### IBackgroundJob interface

Any job you want to schedule implements `IBackgroundJob`:

```csharp
public interface IBackgroundJob
{
    string JobId { get; }        // Unique identifier for this job type
    string Name { get; }         // Human-readable name
    string Description { get; }  // What the job does
    Task ExecuteAsync();         // The job logic
    bool CanExecute();           // Whether the job can run in the current context
    Task OnFailureAsync(Exception ex); // Called on failure (logging/alerting)
}
```

### Scheduling model

`JobScheduler` (in `src/BackgroundJobs/JobScheduler.cs`) is a sealed, `IAsyncDisposable` class that owns the scheduling lifecycle:

- **Registration** — jobs are registered with a `JobScheduleConfig` via `Register(IBackgroundJob, JobScheduleConfig)`. The config controls `Enabled`, `RunOnStartup`, `Interval`, `CronExpression`, `MaxRetries`, `RetryDelay`, and `ExecutionTimeout`.
- **Start** — `StartAsync()` iterates registered jobs, runs enabled jobs immediately when `RunOnStartup` is set and `CanExecute()` returns true, then schedules each job by interval (`System.Threading.Timer` repeating) or by cron expression (a one-shot timer re-armed after each run).
- **Stop** — `StopAsync()` disposes all timers and clears the timer map; `DisposeAsync()` stops the scheduler and releases the execution semaphore.
- **Execution** — `ExecuteJobAsync` runs a job under a `SemaphoreSlim` so only one job executes at a time. It honors `CanExecute()`, applies `ExecutionTimeout` via `WaitAsync`, and retries up to `MaxRetries` times with `RetryDelay` between attempts. On final failure it invokes `OnFailureAsync` and records the failure.
- **Cron expressions** — `GetNextOccurrence(string, DateTime)` is a public static helper that computes the next UTC match for a five-field expression (`minute hour day-of-month month day-of-week`), supporting `*`, single values, comma lists, `a-b` ranges, and `*/step` / `a-b/step` increments. It returns `null` for invalid or never-matching expressions.
- **History** — every run is recorded as a `JobExecutionResult` (job id, start time, duration, success, error message/stack trace, and optional output). History is kept in memory, capped at 1000 entries, and queryable via `GetExecutionHistory(jobId)`, `GetRecentExecutions(count)`, and `ClearHistory()`.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;

// Implement a job
public sealed class DataCleanupJob : IBackgroundJob
{
    public string JobId => "data-cleanup";
    public string Name => "Data Cleanup";
    public string Description => "Purges stale records";

    public bool CanExecute() => true;

    public Task ExecuteAsync()
    {
        // ... job logic ...
        return Task.CompletedTask;
    }

    public Task OnFailureAsync(Exception ex)
    {
        // ... log / alert ...
        return Task.CompletedTask;
    }
}

// Register and run
var scheduler = new JobScheduler();
scheduler.Register(new DataCleanupJob(), new JobScheduleConfig
{
    RunOnStartup = true,
    CronExpression = "0 2 * * *", // 2 AM daily
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromSeconds(30),
    ExecutionTimeout = TimeSpan.FromMinutes(5)
});

await scheduler.StartAsync();
// ...
await scheduler.DisposeAsync();
```

## QueryProfiler and IProfilerSink

The `QueryProfiler` class provides thread-safe, in-process query profiling. Wrap any
database call with `ProfileAsync<T>` to measure its wall-clock duration and capture
diagnostics such as the SQL text, bound parameters, and the calling member. Profiles
are stored in a bounded ring-buffer (default 1000 entries) so memory usage stays
predictable under sustained load.

### Public API

- `ProfileAsync<T>(query, operation, parameters, callerMemberName)` — executes the
  wrapped operation and records a `QueryProfile`. When `IsEnabled` is `false` the
  delegate runs directly with no overhead.
- `GetProfiles()` — returns all captured profiles ordered by execution time descending.
- `GetSummary()` — returns aggregated statistics (`QueryProfilerSummary`) across all
  captured profiles.
- `Clear()` — removes all captured profiles from memory.
- `IsEnabled` — when `false`, profiling is skipped entirely. Defaults to `true`.

### Diagnostics

The profiler automatically detects and reports:

- **Slow queries** — queries exceeding `SlowQueryThresholdMs` (default 500ms).
- **N+1 patterns** — the same parameterized SQL executed more than `NPlusOneThreshold`
  times (default 3), suggesting inefficient data loading.
- **Failed queries** — executions that threw an exception.

These thresholds and toggles are configured via `QueryProfilerOptions`.

### IProfilerSink

`IProfilerSink` is a pluggable sink for profiler events. Implement it to route
diagnostics to your own logging or monitoring system:

- `ReportSlowQuery(profile, thresholdMs)`
- `ReportNPlusOneQuery(queryText, executionCount, threshold)`
- `ReportFailedQuery(profile)`

The default `ConsoleProfilerSink` writes diagnostics to standard error. Pass a custom
sink to the `QueryProfiler` constructor to override it.

### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

// Custom sink routing diagnostics to your logging system
public sealed class LoggingProfilerSink : IProfilerSink
{
    public void ReportSlowQuery(QueryProfile profile, int thresholdMs)
        => Console.WriteLine($"Slow query ({profile.Duration.TotalMilliseconds:F0}ms): {profile.Query}");

    public void ReportNPlusOneQuery(string queryText, int executionCount, int threshold)
        => Console.WriteLine($"N+1 pattern ({executionCount} executions): {queryText}");

    public void ReportFailedQuery(QueryProfile profile)
        => Console.WriteLine($"Failed query: {profile.ErrorMessage}");
}

var profiler = new QueryProfiler(
    maxProfiles: 500,
    options: new QueryProfilerOptions { SlowQueryThresholdMs = 250 },
    sink: new LoggingProfilerSink());

var users = await profiler.ProfileAsync(
    "SELECT * FROM Users WHERE Id = @id",
    () => connection.QueryAsync<User>("SELECT * FROM Users WHERE Id = @id", new { id = 42 }),
    parameters: new Dictionary<string, object> { ["id"] = 42 });

var summary = profiler.GetSummary();
Console.WriteLine($"Total queries: {summary.TotalQueries}, avg: {summary.AverageDuration.TotalMilliseconds:F2}ms");

## PreparedStatementPool

`PreparedStatementPool` (in `src/Data/PreparedStatementPool.cs`) is a thread-safe pool of prepared-statement entries that caches SQL parameter shapes, eliminating redundant `DbCommand` construction overhead on high-frequency query paths. It is registered alongside `IQueryPlanCache` via `AddQueryPlanCaching`.

### Options

`PreparedStatementPoolOptions` controls pool capacity:

- `MaxPoolSize` — maximum number of statements held in the pool before least-used eviction. Defaults to `200`.

Fluent configuration helpers live in `src/Data/PreparedStatementPoolOptionsExtensions.cs`:

- `WithMaxPoolSize(int maxPoolSize)` — sets the maximum pool size explicitly.
- `WithMemoryBasedMaxPoolSize(int reservedMemoryMb)` — sizes the pool from available memory using a ~1KB-per-statement heuristic (minimum 10).
- `WithNoEviction()` — sets `MaxPoolSize` to `int.MaxValue`, effectively disabling eviction.
- `WithDefaultSize()` — restores the default of `200`.

JSON serialization helpers live in `src/Data/PreparedStatementPoolOptionsJsonExtensions.cs` (`ToJson`, `FromJson`, `TryFromJson`).

### Public API

- `BorrowAsync(string statementKey, CancellationToken)` — returns the cached entry for a key, or `null` on a miss.
- `ReturnAsync(PreparedStatementEntry entry, CancellationToken)` — registers an entry, evicting the least-used entry when the pool is at capacity.
- `ReleaseAsync(string statementKey, CancellationToken)` — removes an entry from the pool.
- `GetPoolStatsAsync(CancellationToken)` — returns `(PoolSize, HitRatio)` where `HitRatio` is successful borrows over total borrow attempts.
- `DisposeAsync()` — clears the pool.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register the pool (and query plan cache) as singletons with custom capacity
services.AddQueryPlanCaching(
    configureStatementPool: options => options.WithMaxPoolSize(500));

var provider = services.BuildServiceProvider();
var pool = provider.GetRequiredService<IPreparedStatementPool>();

// Borrow a cached statement shape, or null on a miss
var entry = await pool.BorrowAsync("SELECT * FROM Users WHERE Id = @id");
if (entry is null)
{
    // Compile the statement, then register it for reuse
    entry = new PreparedStatementEntry
    {
        StatementKey = "SELECT * FROM Users WHERE Id = @id",
        Sql = "SELECT * FROM Users WHERE Id = @id",
        Parameters = [new QueryParameterDescriptor { Name = "@id", DbType = DbType.Int32 }]
    };
    await pool.ReturnAsync(entry);
}

var (poolSize, hitRatio) = await pool.GetPoolStatsAsync();
Console.WriteLine($"Pool size: {poolSize}, hit ratio: {hitRatio:P0}");
```
```