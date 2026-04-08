#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DotnetMicroOrm.Caching;

/// <summary>
/// In-memory cache implementation using ConcurrentDictionary.
/// Suitable for single-server applications. Includes automatic expiration
/// and pattern-based removal for cache invalidation.
/// </summary>
public class sealed MemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = [];
    private readonly ConcurrentDictionary<string, Timer> _timers = [];
    private readonly SemaphoreSlim _cleanupSemaphore = new(1);

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
            return null;

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt)
            {
                await RemoveAsync(key);
                return null;
            }

            return entry.Value as T;
        }

        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (value is null)
        {
            await RemoveAsync(key);
            return;
        }

        var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;

        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _cache[key] = entry;

        // Set up expiration timer if needed
        if (expiresAt.HasValue)
        {
            if (_timers.TryGetValue(key, out var oldTimer))
            {
                oldTimer?.Dispose();
                _timers.TryRemove(key, out _);
            }

            var timer = new Timer(
                _ => RemoveAsync(key).GetAwaiter().GetResult(),
                null,
                expiration.Value,
                Timeout.InfiniteTimeSpan);

            _timers[key] = timer;
        }

        await Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null)
            return cached;

        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        _cache.TryRemove(key, out _);

        if (_timers.TryRemove(key, out var timer))
        {
            timer?.Dispose();
        }

        await Task.CompletedTask;
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return;

        try
        {
            // Convert wildcard pattern to regex
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            var regex = new Regex(regexPattern);

            var matchingKeys = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

            foreach (var key in matchingKeys)
            {
                await RemoveAsync(key);
            }
        }
        catch
        {
            // Silently ignore regex errors
        }

        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        foreach (var timer in _timers.Values)
        {
            timer?.Dispose();
        }

        _cache.Clear();
        _timers.Clear();

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        var exists = _cache.TryGetValue(key, out var entry);

        if (exists && entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt)
        {
            await RemoveAsync(key);
            return false;
        }

        return exists;
    }

    public async Task<long> GetCountAsync()
    {
        // Clean up expired entries first
        var expiredKeys = _cache
            .Where(x => x.Value.ExpiresAt.HasValue && DateTime.UtcNow >= x.Value.ExpiresAt)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            await RemoveAsync(key);
        }

        return _cache.Count;
    }

    /// <summary>
    /// Runs a cleanup operation to remove expired entries
    /// Useful for periodic maintenance
    /// </summary>
    public async Task CleanupAsync()
    {
        await _cleanupSemaphore.WaitAsync();

        try
        {
            var expiredKeys = _cache
                .Where(x => x.Value.ExpiresAt.HasValue && DateTime.UtcNow >= x.Value.ExpiresAt)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                await RemoveAsync(key);
            }
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ClearAsync();
        _cleanupSemaphore?.Dispose();
    }

    private class CacheEntry
    {
        public required object Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
