// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Middleware;

/// <summary>
/// Base interface for middleware components in the request processing pipeline.
/// Implementations handle cross-cutting concerns like logging, auth, and error handling.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Processes the middleware logic asynchronously.
    /// Should call next middleware in chain unless explicitly stopping processing.
    /// </summary>
    Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next);

    /// <summary>
    /// Gets the priority order for middleware execution (lower = earlier)
    /// </summary>
    int Order => 100;
}

/// <summary>
/// Context passed through middleware pipeline containing request/response data
/// </summary>
public class MiddlewareContext
{
    /// <summary>Unique request identifier for tracing and logging</summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Operation name or endpoint being called</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Request payload data</summary>
    public object? RequestData { get; set; }

    /// <summary>Response payload data</summary>
    public object? ResponseData { get; set; }

    /// <summary>Authenticated user information</summary>
    public AuthenticationInfo? User { get; set; }

    /// <summary>Exception that occurred during processing</summary>
    public Exception? Exception { get; set; }

    /// <summary>Start timestamp of request processing</summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>Custom metadata dictionary for middleware communication</summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>Indicates if processing should short-circuit</summary>
    public bool IsHandled { get; set; }

    /// <summary>Calculates elapsed time since request start</summary>
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
}

/// <summary>
/// Authentication information attached to context after auth middleware processes
/// </summary>
public class AuthenticationInfo
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AuthenticatedAt { get; set; } = DateTime.UtcNow;
}
