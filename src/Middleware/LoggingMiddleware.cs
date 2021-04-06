#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Middleware;

/// <summary>
/// Middleware that logs all request/response activity with timing information.
/// Captures method name, parameters, return values, and execution time.
/// Used for debugging, performance monitoring, and audit trails.
/// </summary>
public class sealed LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public int Order => 10; // Execute early in pipeline

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        var requestId = context.RequestId;
        var operation = context.Operation;

        try
        {
            _logger.LogInformation(
                "[{RequestId}] Operation starting: {Operation}",
                requestId, operation);

            if (context.RequestData is not null)
            {
                _logger.LogDebug(
                    "[{RequestId}] Request data: {@Data}",
                    requestId, context.RequestData);
            }

            await next(context);

            var elapsed = context.ElapsedTime.TotalMilliseconds;

            if (context.Exception is null)
            {
                _logger.LogInformation(
                    "[{RequestId}] Operation completed: {Operation} - {ElapsedMs}ms",
                    requestId, operation, (int)elapsed);

                if (context.ResponseData is not null)
                {
                    _logger.LogDebug(
                        "[{RequestId}] Response data: {@Data}",
                        requestId, context.ResponseData);
                }
            }
            else
            {
                _logger.LogError(
                    "[{RequestId}] Operation failed: {Operation} - {Error} ({ElapsedMs}ms)",
                    requestId, operation, context.Exception.Message, (int)elapsed);
            }
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            _logger.LogError(
                "[{RequestId}] Middleware error in operation: {Operation} - {Error}",
                requestId, operation, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Dummy implementation of ILogger for compile-time testing
/// Real implementation would use actual logging framework (Serilog, etc)
/// </summary>
public interface ILogger<out T>
{
    void LogInformation(string message, params object?[] args);
    void LogDebug(string message, params object?[] args);
    void LogError(string message, params object?[] args);
}
