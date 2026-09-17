# Middleware

The `dotnet-micro-orm` middleware system provides a composable request-processing pipeline for cross-cutting concerns such as logging, authentication, rate limiting, and error handling. Middleware components live in the `DotnetMicroOrm.Middleware` namespace and are assembled and executed by `PipelineBuilder` in the `DotnetMicroOrm.Pipeline` namespace.

## Overview

Every middleware implements the `IMiddleware` interface:

```csharp
public interface IMiddleware
{
    Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next);
    int Order => 100; // lower = earlier
}
```

- `InvokeAsync` performs the middleware's work and calls `next(context)` to continue down the chain, unless it short-circuits by setting `context.IsHandled = true` and returning early.
- `Order` controls execution order. Lower values run first. The default is `100`; each built-in middleware overrides it.

### MiddlewareContext

The `MiddlewareContext` carries request/response data through the pipeline and lets middleware communicate with each other:

| Member | Description |
|--------|-------------|
| `RequestId` | Unique request identifier for tracing and logging (GUID by default) |
| `Operation` | Operation name or endpoint being called |
| `RequestData` | Request payload |
| `ResponseData` | Response payload |
| `User` | `AuthenticationInfo` populated by the auth middleware |
| `Exception` | Exception that occurred during processing |
| `StartTime` | Start timestamp of request processing |
| `Metadata` | Custom dictionary for middleware communication (e.g. `Authorization` header) |
| `IsHandled` | When `true`, processing short-circuits and `next` is not called |
| `ElapsedTime` | Time elapsed since `StartTime` |

`AuthenticationInfo` holds `UserId`, `Username`, `Email`, `Role`, and `AuthenticatedAt`.

## Built-in middleware

| Middleware | Order | Responsibility |
|------------|-------|----------------|
| `ErrorHandlingMiddleware` | 1 | Wraps the whole chain, converts exceptions to `ErrorResponse` |
| `RateLimitingMiddleware` | 5 | Token-bucket rate limiting per user |
| `LoggingMiddleware` | 10 | Logs request/response activity with timing |
| `AuthenticationMiddleware` | 20 | API-key / bearer-token authentication |
| `AuthorizationMiddleware` | 25 | Role-based authorization per operation |

### ErrorHandlingMiddleware (Order 1)

Runs first so it wraps all other middleware. It invokes `next` inside a `try/catch` and converts any thrown exception into a standardized `ErrorResponse`, setting `context.IsHandled = true`:

| Exception | Error code |
|-----------|-----------|
| `OrmException` | `ORM_ERROR` |
| `ArgumentException` | `INVALID_ARGUMENT` |
| `UnauthorizedAccessException` | `UNAUTHORIZED` |
| `TimeoutException` | `TIMEOUT` |
| Any other `Exception` | `INTERNAL_ERROR` |

`ErrorResponse` contains `Code`, `Message`, `RequestId`, `Timestamp`, and optional `Details` / `StackTrace` for debug mode.

### RateLimitingMiddleware (Order 5)

Implements a token-bucket rate limiter keyed by user ID (`context.User?.UserId`, falling back to `"anonymous"`). When the bucket is exhausted it sets a `RATE_LIMITED` error response and short-circuits. `CleanupExpiredBuckets()` removes idle buckets to prevent memory leaks. Behavior is configurable via `RateLimitConfig`:

| Property | Default | Description |
|----------|---------|-------------|
| `MaxRequests` | `100` | Maximum requests allowed per window |
| `WindowDuration` | `1 minute` | Time window for the limit |
| `Enabled` | `true` | Master switch (can be disabled for testing) |

Note: because rate limiting runs at order 5, before authentication (order 20), `context.User` is not yet populated when the bucket key is computed, so requests are bucketed as `"anonymous"` unless the caller pre-populates `User`.

### LoggingMiddleware (Order 10)

Logs the operation start, request data (debug level), completion with elapsed milliseconds, and response data (debug level). On failure it logs the error at error level. It re-throws exceptions after recording them so `ErrorHandlingMiddleware` can still catch them. Uses `ILogger<T>`; a minimal `ILogger<T>` interface and a `ConsoleLogger<T>` implementation are provided for compilation and demo purposes.

### AuthenticationMiddleware (Order 20)

Authenticates via an API key or bearer token. It reads the `Authorization` metadata entry and accepts both `Bearer <key>` and `ApiKey <key>` prefixes. On success it populates `context.User` with `AuthenticationInfo`. On an invalid key it sets an `INVALID_API_KEY` error and short-circuits. Keys are registered in the constructor (e.g. `demo-key-12345` → admin, `demo-key-67890` → user) and can be managed at runtime via `RegisterApiKey` / `RevokeApiKey`. Static helpers `HasRole` and `HasAnyRole` check a user's role.

### AuthorizationMiddleware (Order 25)

Enforces role-based access per operation. If the current `context.Operation` has configured role requirements and the user lacks them, it sets a `FORBIDDEN` error and short-circuits. Default requirements map `admin-operations` and `product-management` to `admin`, and `user-operations` to `admin`/`user`. Requirements are managed via `SetOperationRoles(operation, roles)`.

## Pipeline execution order

`PipelineBuilder` sorts middleware by `Order` ascending, so the built-in pipeline executes as:

```
ErrorHandling (1) → RateLimiting (5) → Logging (10) → Authentication (20) → Authorization (25)
```

Because each middleware calls `next` before returning, the chain unwinds in reverse: the outer middleware's post-`next` code (e.g. logging completion, error catching) runs after the inner middleware completes.

## PipelineBuilder

`PipelineBuilder` (in `DotnetMicroOrm.Pipeline`) composes and executes middleware:

| Member | Description |
|--------|-------------|
| `Use(middleware)` | Adds a middleware instance |
| `UseAll(params IMiddleware[])` | Adds multiple middleware at once |
| `Build()` | Returns a `Func<MiddlewareContext, Task>` that runs all middleware in `Order` sequence |
| `ExecuteAsync(context)` | Builds and runs the pipeline for a context |
| `Count` | Number of registered middleware |
| `Clear()` | Removes all middleware |
| `RemoveWhere(predicate)` | Removes middleware matching a predicate |
| `GetOrdered()` | Middleware in execution order |

### Extension methods

`PipelineBuilderExtensions` adds higher-level composition helpers:

| Method | Description |
|--------|-------------|
| `Use(builder, middleware, order)` | Adds middleware with an explicit order |
| `UseAll(builder, (middleware, order)[])` | Adds multiple middleware with explicit orders |
| `UseWhen(builder, predicate, middleware)` | Runs middleware only when the predicate matches (order `int.MaxValue`) |
| `UseTransform(builder, transformer, middleware)` | Transforms the context before invoking middleware (order `int.MaxValue - 1`) |
| `UseOnce(builder, middleware, key)` | Runs middleware only once (order `0`) |
| `RemoveAll<TMiddleware>()` | Removes all middleware of a type |
| `ExecuteAndGetContextAsync(context)` | Runs the pipeline and returns the context |
| `Clone()` | Shallow-copies the builder |
| `GetMiddlewareTypeNames()` | Middleware type names in execution order (diagnostics) |
| `GetMiddlewareCountString()` | Formatted middleware count (telemetry) |

## Wiring it up

`ApplicationBuilder` (in `DotnetMicroOrm.Configuration`) exposes `AddMiddleware(IMiddleware)` for custom middleware and `WithDefaultMiddleware()` which registers the four core middleware:

```csharp
var builder = new ApplicationBuilder()
    .WithDatabaseConnection(connectionString, DatabaseProvider.SqlServer)
    .WithDefaultMiddleware()
    .AddMiddleware(new MyCustomMiddleware());

var config = builder.Build();
await config.Pipeline(new MiddlewareContext { Operation = "user-operations" });
```

`Build()` returns an `ApplicationConfiguration` whose `Pipeline` delegate executes the assembled middleware chain for each request.

## Custom middleware

Implement `IMiddleware` and override `Order` to place it in the chain:

```csharp
public sealed class RequestTimingMiddleware : IMiddleware
{
    public int Order => 15; // between logging (10) and authentication (20)

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        var started = DateTime.UtcNow;
        await next(context);
        context.Metadata["duration"] = DateTime.UtcNow - started;
    }
}
```

Register it with `AddMiddleware` or `Use` and it will run at its declared position in the pipeline.