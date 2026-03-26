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
public class AuthenticationMiddleware : IMiddleware
{
    private readonly Dictionary<string, (int userId, string role)> _apiKeys = [];

    public int Order => 20; // Execute after error handling but before rate limiting

    public AuthenticationMiddleware()
    {
        // Initialize with example API keys (in production, load from secure storage)
        _apiKeys["demo-key-12345"] = (1, "admin");
        _apiKeys["demo-key-67890"] = (2, "user");
    }

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
public class AuthorizationMiddleware : IMiddleware
{
    private readonly Dictionary<string, string[]> _operationRoles = [];

    public int Order => 25; // Execute after authentication

    public AuthorizationMiddleware()
    {
        // Define role requirements for operations
        _operationRoles["admin-operations"] = ["admin"];
        _operationRoles["user-operations"] = ["admin", "user"];
        _operationRoles["product-management"] = ["admin"];
    }

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        // Check if operation requires authorization
        if (_operationRoles.TryGetValue(context.Operation, out var requiredRoles))
        {
            if (context.User is null || !requiredRoles.Contains(context.User.Role))
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
