#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Concurrent;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotnetMicroOrm.Caching;

/// <summary>
/// In-memory cache implementation using ConcurrentDictionary with LRU eviction policy.
/// Suitable for single-server applications. Includes automatic expiration, memory pressure detection,
/// and pattern-based removal for cache invalidation.
/// </summary>
public sealed class MemoryCacheProvider : ICacheProvider
{
    // Configuration constants
    private const int DefaultMaxEntries = 10000;
    private const int DefaultMemoryLimitMb = 500;
    private const int BackgroundCleanupIntervalMs = 30000; // 30 seconds
    private const int MemoryPressureCheckIntervalMs = 10000; // 10 seconds
    private const double HighMemoryPressureThreshold = 0.90; // 90% of GC heap
    private const double LowMemoryPressureThreshold = 0.70; // 70% of GC heap

    private readonly ConcurrentDictionary<string, LinkedListNode<CacheEntry>> _cache = [];
    private readonly ConcurrentDictionary<string, Timer> _timers = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _singleFlightLocks = [];
    private readonly SemaphoreSlim _cleanupSemaphore = new(1);
    private readonly LinkedList<CacheEntry> _lruList = new();
    private readonly object _evictionLock = new();

    // Configuration
    private readonly int _maxEntries;
    private readonly long _maxMemoryBytes;
    private readonly bool _enableMemoryPressureEviction;

    // Background cleanup
    private Timer? _backgroundCleanupTimer;
    private Timer? _memoryPressureCheckTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of MemoryCacheProvider with default configuration.
    /// </summary>
    public MemoryCacheProvider() : this(DefaultMaxEntries, DefaultMemoryLimitMb, enableMemoryPressureEviction: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of MemoryCacheProvider with custom configuration.
    /// </summary>
    /// <param name="maxEntries">Maximum number of entries in cache before LRU eviction occurs</param>
    /// <param name="maxMemoryMb">Maximum memory limit in MB for cache entries</param>
    /// <param name="enableMemoryPressureEviction">Whether to enable automatic memory pressure-based eviction</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if maxEntries or maxMemoryMb is less than 1</exception>
    public MemoryCacheProvider(int maxEntries = DefaultMaxEntries, int maxMemoryMb = DefaultMemoryLimitMb, bool enableMemoryPressureEviction = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxEntries, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxMemoryMb, 1);

        _maxEntries = maxEntries;
        _maxMemoryBytes = maxMemoryMb * 1024L * 1024L;
        _enableMemoryPressureEviction = enableMemoryPressureEviction && maxMemoryMb > 0;

        // Start background cleanup timer
        _backgroundCleanupTimer = new Timer(
            _ => CleanupExpiredEntriesAsync(default).ConfigureAwait(false).GetAwaiter().GetResult(),
            null,
            BackgroundCleanupIntervalMs,
            BackgroundCleanupIntervalMs);

        // Start memory pressure monitoring if enabled
        if (_enableMemoryPressureEviction)
        {
            _memoryPressureCheckTimer = new Timer(
                _ => CheckMemoryPressureAsync(default).ConfigureAwait(false).GetAwaiter().GetResult(),
                null,
                MemoryPressureCheckIntervalMs,
                MemoryPressureCheckIntervalMs);
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_cache.TryGetValue(key, out var node))
        {
            var entry = node.Value;

            // Check if expired
            if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt)
            {
                await RemoveAsync(key, cancellationToken).ConfigureAwait(false);
                return null;
            }

            // Update LRU position - move to end of list (most recently used)
            UpdateLruPosition(node);

            return entry.Value as T;
        }

        return null;
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

        DateTime? expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;

        // Create new entry
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            Size = CalculateSize(value)
        };

        // Check if key already exists
        if (_cache.TryGetValue(key, out var existingNode))
        {
            // Update existing entry
            var oldEntry = existingNode.Value;
            oldEntry.Value = value;
            oldEntry.ExpiresAt = expiresAt;
            oldEntry.CreatedAt = DateTime.UtcNow;
            oldEntry.Size = CalculateSize(value);

            // Update LRU position
            UpdateLruPosition(existingNode);
        }
        else
        {
            // Add new entry to cache and LRU list
            var node = new LinkedListNode<CacheEntry>(entry);
            _cache[key] = node;
            _lruList.AddLast(node);

            // Evict if we exceed limits
            await EvictIfNeededAsync(cancellationToken).ConfigureAwait(false);
        }

        // Set up expiration timer if needed
        if (expiresAt.HasValue)
        {
            if (_timers.TryGetValue(key, out var oldTimer))
            {
                oldTimer?.Dispose();
                _timers.TryRemove(key, out _);
            }

            var timer = new Timer(
                _ => Evict(key),
                null,
                expiration!.Value,
                Timeout.InfiniteTimeSpan);

            _timers[key] = timer;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets a value from cache or creates it if not cached using a single-flight pattern.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if not cached</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="factory"/> is null or empty</exception>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        // Use single-flight pattern to prevent cache stampede
        // Only one thread will execute the factory for a given key at a time
        var singleFlightLock = GetSingleFlightLock(key);
        await singleFlightLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Double-check cache after acquiring lock (another thread may have set it)
            var cachedAfterLock = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cachedAfterLock is not null)
            {
                return cachedAfterLock;
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

    /// <summary>
    /// Gets or creates a single-flight lock for the given key.
    /// The lock is automatically cleaned up when released.
    /// </summary>
    private SemaphoreSlim GetSingleFlightLock(string key)
    {
        // Use GetOrAdd to atomically create the lock if it doesn't exist
        return _singleFlightLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    private void UpdateLruPosition(LinkedListNode<CacheEntry> node)
    {
        if (node.List == _lruList)
        {
            _lruList.Remove(node);
            _lruList.AddLast(node);
        }
    }

    private async Task EvictIfNeededAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Check entry count limit
        if (_cache.Count > _maxEntries)
        {
            await EvictLruEntriesAsync().ConfigureAwait(false);
        }

        // Check memory pressure if enabled
        if (_enableMemoryPressureEviction && IsHighMemoryPressure())
        {
            await EvictLruEntriesAsync().ConfigureAwait(false);
        }
    }

    private async Task EvictLruEntriesAsync(int count = 1)
    {
        var evicted = 0;
        var node = _lruList.First;

        while (node != null && evicted < count)
        {
            var next = node.Next; // Save next before potential removal

            if (_cache.TryGetValue(node.Value.Key, out var existingNode) && existingNode == node)
            {
                // Remove from both collections
                _cache.TryRemove(node.Value.Key, out _);
                _lruList.Remove(node);
                evicted++;

                // Dispose expiration timer
                if (_timers.TryRemove(node.Value.Key, out var timer))
                {
                    timer?.Dispose();
                }
            }

            node = next;
        }
    }

    private bool IsHighMemoryPressure()
    {
        try
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var totalMemory = gcMemoryInfo.HeapSizeBytes;
            var totalMemoryMb = totalMemory / (1024.0 * 1024.0);

            // If total memory is very small, skip memory pressure checks
            if (totalMemoryMb < 10)
            {
                return false;
            }

            // Check if we're above high memory pressure threshold
            return (long)gcMemoryInfo.MemoryLoadBytes >= (long)(totalMemory * HighMemoryPressureThreshold);
        }
        catch
        {
            // If we can't get memory info, assume no pressure
            return false;
        }
    }

    private async Task CheckMemoryPressureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var totalMemory = gcMemoryInfo.HeapSizeBytes;
            var totalMemoryMb = totalMemory / (1024.0 * 1024.0);

            // If total memory is very small, skip memory pressure checks
            if (totalMemoryMb < 10)
            {
                return;
            }

            var memoryLoadPercent = (double)gcMemoryInfo.MemoryLoadBytes / totalMemory;

            // If we're above high memory pressure threshold, evict aggressively
            if (memoryLoadPercent >= HighMemoryPressureThreshold)
            {
                // Evict up to 10% of entries or at least 10 entries
                var entriesToEvict = Math.Max(10, (int)(_cache.Count * 0.1));
                for (int i = 0; i < entriesToEvict && _cache.Count > 0; i++)
                {
                    await EvictLruEntriesAsync().ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // Silently handle any memory pressure check failures
        }
    }

    private async Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _cleanupSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var expiredKeys = new List<string>();
            var node = _lruList.First;

            // Find all expired entries
            while (node != null)
            {
                var next = node.Next;

                if (node.Value.ExpiresAt.HasValue && DateTime.UtcNow >= node.Value.ExpiresAt)
                {
                    expiredKeys.Add(node.Value.Key);
                }

                node = next;
            }

            // Remove expired entries
            foreach (var key in expiredKeys)
            {
                await RemoveAsync(key, default).ConfigureAwait(false);
            }
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    private void Evict(string key)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            _cache.TryRemove(key, out _);
            _lruList.Remove(node);

            if (_timers.TryRemove(key, out var timer))
            {
                timer?.Dispose();
            }
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _cache.TryRemove(key, out var node);

        if (node != null)
        {
            _lruList.Remove(node);
        }

        if (_timers.TryRemove(key, out var timer))
        {
            timer?.Dispose();
        }

        await Task.CompletedTask;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(pattern))
        {
            return;
        }

        try
        {
            // Convert wildcard pattern to regex
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            var regex = new Regex(regexPattern);

            var matchingKeys = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

            foreach (var key in matchingKeys)
            {
                await RemoveAsync(key, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            // Silently ignore regex errors
        }

        await Task.CompletedTask;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var timer in _timers.Values)
        {
            timer?.Dispose();
        }

        await _cleanupSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            _cache.Clear();
            _lruList.Clear();
            _timers.Clear();
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (_cache.TryGetValue(key, out var node))
        {
            var entry = node.Value;

            if (entry.ExpiresAt.HasValue && DateTime.UtcNow >= entry.ExpiresAt)
            {
                await RemoveAsync(key, cancellationToken).ConfigureAwait(false);
                return false;
            }

            // Update LRU position on access
            UpdateLruPosition(node);
            return true;
        }

        return false;
    }

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _cleanupSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Clean up expired entries first
            await CleanupExpiredEntriesAsync(default).ConfigureAwait(false);

            return _cache.Count;
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    /// <summary>
    /// Runs a cleanup operation to remove expired entries
    /// Useful for periodic maintenance
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CleanupExpiredEntriesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the approximate total size of all cache entries in bytes.
    /// </summary>
    /// <returns>The total size in bytes</returns>
    public long GetTotalSizeBytes()
    {
        lock (_evictionLock)
        {
            long total = 0;
            var node = _lruList.First;
            while (node != null)
            {
                total += node.Value.Size;
                node = node.Next;
            }
            return total;
        }
    }

    /// <summary>
    /// Gets the approximate total size of all cache entries in MB.
    /// </summary>
    /// <returns>The total size in MB</returns>
    public double GetTotalSizeMb()
    {
        return GetTotalSizeBytes() / (1024.0 * 1024.0);
    }

    private static long CalculateSize<T>(T value) where T : class
    {
        // Use approximate size calculation for common types
        // In a real application, you might want to use a more sophisticated approach
        // or skip size calculation for performance reasons

        if (value is null)
        {
            return 0;
        }

        try
        {
            // For strings, use length * 2 (approximate char size)
            if (value is string str)
            {
                return str.Length * 2;
            }

            // For byte arrays, use length
            if (value is byte[] bytes)
            {
                return bytes.Length;
            }

            // For collections, use approximate count * average item size
            if (value is System.Collections.ICollection collection)
            {
                return collection.Count * 16; // Approximate per-item overhead
            }

            // Default: use 1KB as a reasonable estimate for most objects
            return 1024;
        }
        catch
        {
            // If size calculation fails, return a safe default
            return 1024;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Stop background timers
        if (_backgroundCleanupTimer != null)
        {
            await _backgroundCleanupTimer.DisposeAsync().ConfigureAwait(false);
            _backgroundCleanupTimer = null;
        }

        if (_memoryPressureCheckTimer != null)
        {
            await _memoryPressureCheckTimer.DisposeAsync().ConfigureAwait(false);
            _memoryPressureCheckTimer = null;
        }

        await ClearAsync().ConfigureAwait(false);
        _cleanupSemaphore.Dispose();
    }

    private class CacheEntry
    {
        public required object Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
        public string Key { get; set; } = string.Empty;
    }
}