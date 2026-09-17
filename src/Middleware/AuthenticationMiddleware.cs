#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotnetMicroOrm.Middleware;

/// <summary>
/// Middleware for API authentication and authorization.
/// Supports API key authentication, bearer token validation, and role-based access.
/// Populates AuthenticationInfo in middleware context for downstream handlers.
/// </summary>
public sealed class AuthenticationMiddleware : IMiddleware
{
    private readonly Dictionary<string, (int userId, string role)> _apiKeys = [];

    /// <summary>
    /// Gets the execution order of this middleware.
    /// Executes after error handling but before rate limiting.
    /// </summary>
    public int Order => 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationMiddleware"/> class
    /// with a set of example API keys.
    /// </summary>
    public AuthenticationMiddleware()
    {
        // Initialize with example API keys (in production, load from secure storage)
        _apiKeys["demo-key-12345"] = (1, "admin");
        _apiKeys["demo-key-67890"] = (2, "user");
    }

    /// <summary>
    /// Authenticates the request using an API key or bearer token and invokes the next middleware.
    /// On successful authentication, populates <see cref="MiddlewareContext.User"/> with the
    /// associated <see cref="AuthenticationInfo"/>. On failure, sets an unauthorized response
    /// and marks the request as handled.
    /// </summary>
    /// <param name="context">The middleware context for the current request.</param>
    /// <param name="next">The delegate to invoke the next middleware in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        // Try to authenticate using API key
        var apiKey = ExtractApiKey(context);

        if (!string.IsNullOrEmpty(apiKey))
        {
            if (_apiKeys.TryGetValue(apiKey, out var userInfo))
            {
                context.User = new AuthenticationInfo
                {
                    UserId = userInfo.userId,
                    Role = userInfo.role,
                    AuthenticatedAt = DateTime.UtcNow
                };
            }
            else
            {
                context.Exception = new UnauthorizedAccessException("Invalid API key");
                context.ResponseData = new ErrorResponse
                {
                    Code = "INVALID_API_KEY",
                    Message = "Provided API key is invalid",
                    RequestId = context.RequestId,
                    Timestamp = DateTime.UtcNow
                };
                context.IsHandled = true;
                return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// Extracts API key from context metadata
    /// In a real API scenario, would extract from request headers
    /// </summary>
    private string? ExtractApiKey(MiddlewareContext context)
    {
        // Look for API key in metadata (would be from HTTP headers in real scenario)
        if (context.Metadata.TryGetValue("Authorization", out var authValue))
        {
            var authString = authValue?.ToString() ?? string.Empty;

            if (authString.StartsWith("Bearer "))
            {
                return authString["Bearer ".Length..];
            }
            else if (authString.StartsWith("ApiKey "))
            {
                return authString["ApiKey ".Length..];
            }
        }

        return null;
    }

    /// <summary>
    /// Registers a new API key for a user
    /// </summary>
    public void RegisterApiKey(string apiKey, int userId, string role)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));

        _apiKeys[apiKey] = (userId, role);
    }

    /// <summary>
    /// Revokes an API key
    /// </summary>
    public void RevokeApiKey(string apiKey)
    {
        if (!string.IsNullOrEmpty(apiKey))
        {
            _apiKeys.Remove(apiKey);
        }
    }

    /// <summary>
    /// Verifies if a user has required role
    /// </summary>
    public static bool HasRole(AuthenticationInfo? user, string requiredRole)
    {
        if (user is null)
            return false;

        return user.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies if a user has any of the required roles
    /// </summary>
    public static bool HasAnyRole(AuthenticationInfo? user, params string[] requiredRoles)
    {
        if (user is null)
            return false;

        return requiredRoles.Any(role =>
            user.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Middleware for role-based authorization
/// Ensures user has required role before allowing operation
/// </summary>
public sealed class AuthorizationMiddleware : IMiddleware
{
    private readonly Dictionary<string, string[]> _operationRoles = [];

    /// <summary>
    /// Gets the execution order of this middleware.
    /// Executes after authentication.
    /// </summary>
    public int Order => 25;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationMiddleware"/> class
    /// with the default role requirements for built-in operations.
    /// </summary>
    public AuthorizationMiddleware()
    {
        // Define role requirements for operations
        _operationRoles["admin-operations"] = ["admin"];
        _operationRoles["user-operations"] = ["admin", "user"];
        _operationRoles["product-management"] = ["admin"];
    }

    /// <summary>
    /// Enforces role-based authorization for the current operation and invokes the next middleware.
    /// If the operation requires roles and the user lacks them, sets a forbidden response
    /// and marks the request as handled.
    /// </summary>
    /// <param name="context">The middleware context for the current request.</param>
    /// <param name="next">The delegate to invoke the next middleware in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        // Check if operation requires authorization
        if (_operationRoles.TryGetValue(context.Operation, out var requiredRoles))
        {
            if (context.User is null || !requiredRoles.Contains(context.User.Role, StringComparer.OrdinalIgnoreCase))
            {
                context.Exception = new UnauthorizedAccessException("Insufficient permissions");
                context.ResponseData = new ErrorResponse
                {
                    Code = "FORBIDDEN",
                    Message = "You do not have permission to perform this operation",
                    RequestId = context.RequestId,
                    Timestamp = DateTime.UtcNow
                };
                context.IsHandled = true;
                return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// Sets role requirement for an operation
    /// </summary>
    public void SetOperationRoles(string operation, params string[] roles)
    {
        if (string.IsNullOrEmpty(operation))
            throw new ArgumentException("Operation name cannot be empty", nameof(operation));

        if (roles is null || roles.Length == 0)
        {
            _operationRoles.Remove(operation);
        }
        else
        {
            _operationRoles[operation] = roles;
        }
    }
}
