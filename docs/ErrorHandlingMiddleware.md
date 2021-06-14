# ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` component provides centralized exception handling for HTTP request pipelines. It intercepts unhandled exceptions thrown during request processing, captures contextual information such as the request identifier and timestamp, and returns a structured error response containing a machine-readable code, a human-readable message, and optional diagnostic details. This prevents raw exception details from leaking to clients while preserving enough information for troubleshooting.

## API

### InvokeAsync

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
```

Invokes the next middleware in the pipeline and catches any unhandled exceptions. When an exception occurs, it populates the error response properties (`Code`, `Message`, `RequestId`, `Timestamp`, `Details`, `StackTrace`) and writes a structured JSON error response to the HTTP response stream with an appropriate status code.

**Parameters:**
- `context` — The current `HttpContext` for the request.
- `next` — The `RequestDelegate` representing the remainder of the pipeline.

**Returns:** A `Task` that completes when the response has been written or the pipeline has executed without error.

**Throws:** This method does not throw. All exceptions from downstream middleware are caught and translated into error responses.

---

### Code

```csharp
public string Code { get; set; }
```

A stable, machine-readable identifier for the error category (for example, `"VALIDATION_ERROR"` or `"INTERNAL_ERROR"`). Clients can switch on this value to handle specific error types programmatically.

---

### Message

```csharp
public string Message { get; set; }
```

A human-readable summary of the error intended for display to end users or for logging. It should not contain sensitive internal state.

---

### RequestId

```csharp
public string RequestId { get; set; }
```

A correlation identifier taken from the current HTTP context (typically the trace identifier or a custom header). This value allows operators to locate the corresponding request in logs and tracing systems.

---

### Timestamp

```csharp
public DateTime Timestamp { get; set; }
```

The UTC instant at which the error was captured. This is recorded at the point the exception is caught, providing a precise timeline reference.

---

### Details

```csharp
public string? Details { get; set; }
```

Optional supplementary information about the error. In development environments this may contain exception messages or validation failure descriptions. In production it is typically `null` or a generic message to avoid information disclosure.

---

### StackTrace

```csharp
public string? StackTrace { get; set; }
```

Optional stack trace string captured from the exception. Intended for diagnostic purposes in non-production environments. This property should remain `null` in production deployments to prevent leaking internal implementation details.

## Usage

### Example 1: Basic Registration in Startup

Register the middleware early in the pipeline so that it can catch exceptions from all downstream components.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/", () =>
{
    throw new InvalidOperationException("Something went wrong.");
});

app.Run();
```

When the endpoint throws, the middleware catches the exception and returns a JSON response similar to:

```json
{
  "code": "INTERNAL_ERROR",
  "message": "An unexpected error occurred.",
  "requestId": "0HMPHLK0FQG8R:00000001",
  "timestamp": "2025-03-15T10:42:00Z",
  "details": null,
  "stackTrace": null
}
```

### Example 2: Environment-Aware Detail Exposure

A common pattern is to conditionally populate `Details` and `StackTrace` based on the hosting environment. The following example shows a custom factory method that configures the middleware response accordingly.

```csharp
public static ErrorHandlingMiddleware CreateForEnvironment(IWebHostEnvironment env)
{
    return new ErrorHandlingMiddleware
    {
        // In development, include full details for debugging.
        Details = env.IsDevelopment() ? "See stack trace for more information." : null,
        StackTrace = env.IsDevelopment() ? Environment.StackTrace : null
    };
}

// Registration:
var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>(CreateForEnvironment(app.Environment));
```

## Notes

- **Response writing is not thread-safe by default:** The `InvokeAsync` method writes directly to the `HttpContext.Response` stream. If multiple error-handling paths could attempt to write concurrently (for example, due to unawaited tasks), ensure that only one writer is active per context. The ASP.NET Core pipeline serializes request processing per context under normal conditions, so concurrent writes are not expected in typical use.
- **Exception swallowing:** `InvokeAsync` intentionally swallows all exceptions. This means that logging or telemetry must be performed inside the catch block before the response is written; otherwise the exception will be silently discarded with no record.
- **Sensitive data exposure:** The `StackTrace` and `Details` properties can contain internal paths, method names, and parameter values. Always set these to `null` or sanitized values in production environments.
- **Status code selection:** The middleware must determine the appropriate HTTP status code (e.g., 400, 500) based on the exception type. If the mapping logic relies on mutable state, ensure it is not modified by other parts of the pipeline during request processing.
- **Ordering in the pipeline:** Register this middleware early. Middleware registered before it will not have their exceptions caught. Middleware that short-circuits the pipeline (such as static file handlers) may prevent the error handler from executing for certain requests.
