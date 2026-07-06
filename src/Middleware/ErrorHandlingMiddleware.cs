#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Exceptions;

namespace DotnetMicroOrm.Middleware;

/// <summary>
/// Middleware that catches exceptions and converts them to standardized error responses.
/// Prevents unhandled exceptions from propagating and ensures consistent error format.
/// Logs errors for debugging and monitoring purposes.
/// </summary>
public sealed class ErrorHandlingMiddleware : IMiddleware
{
    public int Order => 1; // Execute first to wrap all other middleware

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (OrmException ex)
        {
            context.Exception = ex;
            context.ResponseData = new ErrorResponse
            {
                Code = "ORM_ERROR",
                Message = ex.Message,
                RequestId = context.RequestId,
                Timestamp = DateTime.UtcNow
            };
            context.IsHandled = true;
        }
        catch (ArgumentException ex)
        {
            context.Exception = ex;
            context.ResponseData = new ErrorResponse
            {
                Code = "INVALID_ARGUMENT",
                Message = ex.Message,
                RequestId = context.RequestId,
                Timestamp = DateTime.UtcNow
            };
            context.IsHandled = true;
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Exception = ex;
            context.ResponseData = new ErrorResponse
            {
                Code = "UNAUTHORIZED",
                Message = "Access denied",
                RequestId = context.RequestId,
                Timestamp = DateTime.UtcNow
            };
            context.IsHandled = true;
        }
        catch (TimeoutException ex)
        {
            context.Exception = ex;
            context.ResponseData = new ErrorResponse
            {
                Code = "TIMEOUT",
                Message = "Operation timed out",
                RequestId = context.RequestId,
                Timestamp = DateTime.UtcNow
            };
            context.IsHandled = true;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            context.ResponseData = new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred",
                RequestId = context.RequestId,
                Timestamp = DateTime.UtcNow
            };
            context.IsHandled = true;
        }
    }
}

/// <summary>
/// Standardized error response format returned to clients
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>Machine-readable error code</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable error message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Request ID for tracing</summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>When the error occurred</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Detailed error information (only in debug mode)</summary>
    public string? Details { get; set; }

    /// <summary>Stack trace (only in debug mode)</summary>
    public string? StackTrace { get; set; }
}
