#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// <typeparam name="T">The type of value to retrieve</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value or null if not found</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty</exception>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in the cache with optional expiration
    /// </summary>
    /// <typeparam name="T">The type of value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty</exception>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets a value or creates it if not cached using a single-flight pattern to prevent cache stampede.
    /// Only one concurrent call to the factory will execute for any given key; other concurrent calls
    /// will wait for the result, preventing thundering herds when cache entries expire.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="factory"/> is null</exception>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a key from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty</exception>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys matching a pattern
    /// </summary>
    /// <param name="pattern">Pattern to match keys (supports * wildcard)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is null or empty</exception>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all entries from the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    /// <param name="key">The cache key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the key exists, false otherwise</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is null or empty</exception>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of items in the cache
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of items in the cache, or -1 if the count cannot be determined</returns>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
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