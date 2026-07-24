#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using StackExchange.Redis;

namespace DotnetMicroOrm.Caching;

/// <summary>
/// Distributed Redis implementation of <see cref="ICacheProvider"/>.
/// Provides shared cache across multiple application instances, enabling horizontal scaling
/// and distributed query plan caching for web farms.
/// </summary>
public sealed class RedisCacheProvider : ICacheProvider
{
    private readonly IDatabase _database;
    private readonly string _keyPrefix;
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
    private bool _disposed;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _singleFlightLocks = new();

    /// <summary>
    /// Initializes a new Redis cache provider with connection multiplexer and optional key prefix.
    /// </summary>
    /// <param name="connectionMultiplexer">Redis connection multiplexer</param>
    /// <param name="keyPrefix">Optional prefix for all cache keys to avoid cross-tenant collisions</param>
    /// <exception cref="ArgumentNullException">Thrown when connectionMultiplexer is null</exception>
    public RedisCacheProvider(IConnectionMultiplexer connectionMultiplexer, string keyPrefix = "dotnet-micro-orm")
    {
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);

        _database = connectionMultiplexer.GetDatabase();
        _keyPrefix = keyPrefix.TrimEnd(':') + ':';
    }

    /// <summary>
    /// Initializes a new Redis cache provider with connection string and optional key prefix.
    /// </summary>
    /// <param name="connectionString">Redis connection string</param>
    /// <param name="keyPrefix">Optional prefix for all cache keys to avoid cross-tenant collisions</param>
    /// <exception cref="ArgumentException">Thrown when connectionString is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when keyPrefix is null</exception>
    public RedisCacheProvider(string connectionString, string keyPrefix = "dotnet-micro-orm")
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);
        ArgumentNullException.ThrowIfNull(keyPrefix);

        var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        _database = connectionMultiplexer.GetDatabase();
        _keyPrefix = keyPrefix.TrimEnd(':') + ':';
    }

    private string GetPrefixedKey(string key) => _keyPrefix + key;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);

        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var value = await _database.StringGetAsync(prefixedKey).ConfigureAwait(false);
            return value.HasValue ? System.Text.Json.JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions) : null;
        }
        catch
        {
            // Redis failures should not crash the application
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (value is null)
        {
            await RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            return;
        }

        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var serialized = System.Text.Json.JsonSerializer.Serialize(value, _jsonOptions);
            var redisValue = new RedisValue(serialized);

            if (expiration.HasValue && expiration.Value > TimeSpan.Zero)
            {
                await _database.StringSetAsync(prefixedKey, redisValue, expiration).ConfigureAwait(false);
            }
            else
            {
                await _database.StringSetAsync(prefixedKey, redisValue).ConfigureAwait(false);
            }
        }
        catch
        {
            // Redis failures should not crash the application
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        // Try to get from cache first
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached;
        }

        // Use single-flight pattern to prevent cache stampede
        var singleFlightLock = GetSingleFlightLock(key);
        await singleFlightLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Double-check cache after acquiring lock
            cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cached is not null)
            {
                return cached;
            }

            var value = await factory().ConfigureAwait(false);
            if (value is not null)
            {
                await SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
            }

            return value!;
        }
        finally
        {
            singleFlightLock.Release();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(key))
            return;

        try
        {
            var prefixedKey = GetPrefixedKey(key);
            await _database.KeyDeleteAsync(prefixedKey).ConfigureAwait(false);
        }
        catch
        {
            // Redis failures should not crash the application
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(pattern))
            return;

        try
        {
            // Convert wildcard pattern to Redis pattern
            var redisPattern = GetPrefixedKey(pattern);
            await _database.ExecuteAsync("EVAL",
                "local keys = redis.call('KEYS', KEYS[1])\n" +
                "for i=1,#keys do\n" +
                " redis.call('DEL', keys[i])\n" +
                "end\n" +
                "return #keys",
                1,
                redisPattern).ConfigureAwait(false);
        }
        catch
        {
            // Redis failures should not crash the application
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Use EVAL to clear all keys with our prefix
            await _database.ExecuteAsync("EVAL",
                "local keys = redis.call('KEYS', KEYS[1])\n" +
                "for i=1,#keys do\n" +
                " redis.call('DEL', keys[i])\n" +
                "end\n" +
                "return #keys",
                1,
                GetPrefixedKey("*")).ConfigureAwait(false);
        }
        catch
        {
            // Redis failures should not crash the application
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(key))
            return false;

        try
        {
            var prefixedKey = GetPrefixedKey(key);
            return await _database.KeyExistsAsync(prefixedKey).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // For distributed cache, we can't easily get an accurate count without SCAN
        // Return -1 to indicate we can't determine the count
        return -1;
    }

    /// <summary>
    /// Gets or creates a single-flight lock for the given key.
    /// The lock is automatically cleaned up when released.
    /// </summary>
    private SemaphoreSlim GetSingleFlightLock(string key)
    {
        // Use GetOrAdd to atomically create the lock if it doesn't exist
        return _singleFlightLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _singleFlightLocks.Clear();
        await Task.CompletedTask;
    }
}