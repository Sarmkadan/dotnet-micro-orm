#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DotnetMicroOrm.Caching;
using Microsoft.Extensions.Logging;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Configuration options for <see cref="QueryPlanCache"/>.
/// </summary>
public sealed class QueryPlanCacheOptions
{
    /// <summary>
    /// Maximum number of plans held simultaneously before LRU eviction removes the oldest accessed entry.
    /// Defaults to <c>500</c>.
    /// </summary>
    public int Capacity { get; set; } = 1000;

    /// <summary>
    /// Default TTL applied when no explicit value is supplied to <see cref="IQueryPlanCache.StorePlanAsync"/>.
    /// Defaults to one hour.
    /// </summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(1);
}

/// <summary>
/// Thread-safe, LRU-evicting implementation of <see cref="IQueryPlanCache"/>.
/// Plans are indexed by a normalized SHA-256 SQL fingerprint and evicted by least-recently-used
/// order once <see cref="QueryPlanCacheOptions.Capacity"/> is reached.
/// Uses <see cref="ICacheProvider"/> for distributed caching support.
/// </summary>
public sealed class QueryPlanCache : IQueryPlanCache
{
    private readonly QueryPlanCacheOptions _options;
    private readonly ILogger<QueryPlanCache> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly ConcurrentDictionary<string, QueryPlanWithExpiry> _store = new(StringComparer.Ordinal);
    private long _hits;
    private long _misses;

    /// <summary>
    /// Initializes the cache with the supplied options, logger, and cache provider.
    /// </summary>
    /// <param name="options">Capacity and TTL configuration.</param>
    /// <param name="logger">Logger for diagnostic events.</param>
    /// <param name="cacheProvider">Cache provider for storing query plans. Uses MemoryCacheProvider by default if null.</param>
    public QueryPlanCache(QueryPlanCacheOptions options, ILogger<QueryPlanCache> logger, ICacheProvider? cacheProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheProvider = cacheProvider ?? new MemoryCacheProvider();
    }

    /// <inheritdoc/>
    public async Task<QueryPlan?> GetPlanAsync(string fingerprint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cachedPlan = await _cacheProvider.GetAsync<QueryPlanWithExpiry>(GetCacheKey(fingerprint), cancellationToken).ConfigureAwait(false);
        if (cachedPlan is null)
        {
            Interlocked.Increment(ref _misses);
            return null;
        }

        if (cachedPlan.ExpiresAt <= DateTime.UtcNow)
        {
            await _cacheProvider.RemoveAsync(GetCacheKey(fingerprint), cancellationToken).ConfigureAwait(false);
            Interlocked.Increment(ref _misses);
            return null;
        }

        Interlocked.Increment(ref _hits);
        cachedPlan.Plan.HitCount++;

        _logger.LogDebug("Query plan cache hit for fingerprint {Fingerprint}", fingerprint);
        return cachedPlan.Plan;
    }

    /// <inheritdoc/>
    public async Task StorePlanAsync(QueryPlan plan, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(plan);

        // Evict if necessary (LRU eviction based on capacity)
        await EvictIfNecessaryAsync(cancellationToken).ConfigureAwait(false);

        var expiresAt = DateTime.UtcNow.Add(ttl ?? _options.DefaultTtl);
        var planWithExpiry = new QueryPlanWithExpiry(plan, expiresAt);

        await _cacheProvider.SetAsync(GetCacheKey(plan.Fingerprint), planWithExpiry, ttl ?? _options.DefaultTtl, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Stored query plan {Fingerprint} (TTL={Ttl})", plan.Fingerprint, ttl ?? _options.DefaultTtl);
    }

    /// <inheritdoc/>
    public async Task<QueryPlan> GetOrAnalyzeAsync(
        string sql,
        Func<string, CancellationToken, Task<QueryPlan>> analyzer,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(analyzer);
        cancellationToken.ThrowIfCancellationRequested();

        var fingerprint = ComputeFingerprint(sql);
        var cached = await GetPlanAsync(fingerprint, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        _logger.LogDebug("Cache miss — invoking analyzer for fingerprint {Fingerprint}", fingerprint);
        var plan = await analyzer(sql, cancellationToken).ConfigureAwait(false);
        await StorePlanAsync(plan, ttl, cancellationToken).ConfigureAwait(false);
        return plan;
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync(string fingerprint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _cacheProvider.RemoveAsync(GetCacheKey(fingerprint), cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Invalidated query plan {Fingerprint}", fingerprint);
    }

    /// <inheritdoc/>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _cacheProvider.ClearAsync(cancellationToken).ConfigureAwait(false);
        _store.Clear();
        _logger.LogInformation("Query plan cache cleared");
    }

    /// <inheritdoc/>
    public async Task<(long Entries, long Hits, long Misses)> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var count = await _cacheProvider.GetCountAsync(cancellationToken).ConfigureAwait(false);
        return (count >= 0 ? count : 0, Interlocked.Read(ref _hits), Interlocked.Read(ref _misses));
    }

    /// <summary>
    /// Produces a stable, normalized fingerprint for the supplied SQL by collapsing whitespace,
    /// uppercasing the text, stripping trailing semicolons, replacing dynamic parameter names with a generic placeholder,
    /// and hashing with SHA-256.
    /// </summary>
    /// <param name="sql">Raw SQL statement to fingerprint.</param>
    /// <returns>A lowercase hex-encoded SHA-256 hash of the normalized SQL.</returns>
    public static string ComputeFingerprint(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL must not be empty", nameof(sql));

        // Normalize whitespace, uppercase, and strip trailing semicolons.
        var normalized = Regex.Replace(sql.Trim().ToUpperInvariant(), @"\s+", " ").TrimEnd(';');

        // Replace all parameter names (e.g., @p1, @paramName) with a generic placeholder (@p)
        // to ensure structurally identical queries produce the same fingerprint regardless of
        // dynamically generated parameter names.
        normalized = Regex.Replace(normalized, @"@\w+", "@P");

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task EvictIfNecessaryAsync(CancellationToken cancellationToken = default)
    {
        if (_store.Count < _options.Capacity)
            return;

        var lru = _store
            .OrderBy(kv => kv.Value.LastAccessedAt)
            .Select(kv => kv.Key)
            .FirstOrDefault();

        if (lru is not null)
        {
            _store.TryRemove(lru, out _);
            _logger.LogDebug("Evicted LRU query plan {Fingerprint}", lru);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // The cache provider will be disposed by its owner
        await _cacheProvider.ClearAsync().ConfigureAwait(false);
    }

    private string GetCacheKey(string fingerprint) => $"query-plan:{fingerprint}";

    /// <summary>
    /// Stores query plan with expiry information for proper TTL handling
    /// </summary>
    private sealed class QueryPlanWithExpiry
    {
        public QueryPlan Plan { get; }
        public DateTime ExpiresAt { get; }
        public DateTime LastAccessedAt { get; private set; } = DateTime.UtcNow;

        public QueryPlanWithExpiry(QueryPlan plan, DateTime expiresAt)
        {
            Plan = plan;
            ExpiresAt = expiresAt;
        }

        public void Touch() => LastAccessedAt = DateTime.UtcNow;
    }
}