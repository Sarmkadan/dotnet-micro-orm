#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Caching;

/// <summary>
/// Interface for cache providers with support for expiration, patterns, and async operations.
/// Allows swapping between different cache implementations (memory, Redis, etc).
/// </summary>
public interface ICacheProvider : IAsyncDisposable
{
    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in the cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Gets a value or creates it if not cached
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a key from the cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all keys matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Clears all entries from the cache
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets the number of items in the cache
    /// </summary>
    Task<long> GetCountAsync();
}

/// <summary>
/// Helper class for building consistent cache keys
/// </summary>
public static class CacheKey
{
    private const string Separator = ":";

    /// <summary>
    /// Creates a cache key from parts with consistent separator
    /// </summary>
    public static string Create(params object?[] parts)
    {
        if (parts is null || parts.Length == 0)
            throw new ArgumentException("At least one part is required", nameof(parts));

        var keyParts = parts.Select(p => (p?.ToString() ?? "null").ToLowerInvariant());
        return string.Join(Separator, keyParts);
    }

    /// <summary>
    /// Creates a pattern for cache key matching (with * wildcard)
    /// </summary>
    public static string CreatePattern(params object?[] parts)
    {
        var keyParts = parts.Select(p => p?.ToString() ?? "null");
        return string.Join(Separator, keyParts) + Separator + "*";
    }

    /// <summary>
    /// User-specific cache key
    /// </summary>
    public static string ForUser(int userId, string subKey)
        => Create("user", userId, subKey);

    /// <summary>
    /// Product-specific cache key
    /// </summary>
    public static string ForProduct(int productId, string subKey)
        => Create("product", productId, subKey);

    /// <summary>
    /// Order-specific cache key
    /// </summary>
    public static string ForOrder(int orderId, string subKey)
        => Create("order", orderId, subKey);

    /// <summary>
    /// Query result cache key
    /// </summary>
    public static string ForQuery(string queryName, params object?[] parameters)
        => Create("query", queryName, string.Join("_", parameters));

    /// <summary>
    /// Configuration cache key
    /// </summary>
    public static string ForConfig(string configKey)
        => Create("config", configKey);
}
