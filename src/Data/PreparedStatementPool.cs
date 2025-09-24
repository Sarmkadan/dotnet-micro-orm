// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Configuration options for <see cref="PreparedStatementPool"/>.
/// </summary>
public sealed class PreparedStatementPoolOptions
{
    /// <summary>
    /// Maximum number of statements held in the pool before least-used eviction.
    /// Defaults to <c>200</c>.
    /// </summary>
    public int MaxPoolSize { get; set; } = 200;
}

/// <summary>
/// Thread-safe pool of <see cref="PreparedStatementEntry"/> objects that caches SQL
/// parameter shapes, eliminating redundant <see cref="System.Data.Common.DbCommand"/>
/// construction overhead on high-frequency query paths.
/// Evicts the least-used entry when the pool reaches <see cref="PreparedStatementPoolOptions.MaxPoolSize"/>.
/// </summary>
public sealed class PreparedStatementPool : IPreparedStatementPool
{
    private readonly PreparedStatementPoolOptions _options;
    private readonly ILogger<PreparedStatementPool> _logger;
    private readonly ConcurrentDictionary<string, PreparedStatementEntry> _pool = new(StringComparer.Ordinal);
    private long _totalBorrows;
    private long _poolHits;

    /// <summary>
    /// Initializes the pool with the supplied options and logger.
    /// </summary>
    /// <param name="options">Pool capacity configuration.</param>
    /// <param name="logger">Logger for diagnostic and eviction events.</param>
    public PreparedStatementPool(PreparedStatementPoolOptions options, ILogger<PreparedStatementPool> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<PreparedStatementEntry?> BorrowAsync(string statementKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(statementKey);

        Interlocked.Increment(ref _totalBorrows);

        if (!_pool.TryGetValue(statementKey, out var entry))
        {
            _logger.LogDebug("Prepared statement pool miss for key {StatementKey}", statementKey);
            return Task.FromResult<PreparedStatementEntry?>(null);
        }

        entry.LastUsedAt = DateTime.UtcNow;
        Interlocked.Increment(ref entry.UseCount);
        Interlocked.Increment(ref _poolHits);

        _logger.LogDebug("Prepared statement pool hit for key {StatementKey} (UseCount={UseCount})", statementKey, entry.UseCount);
        return Task.FromResult<PreparedStatementEntry?>(entry);
    }

    /// <inheritdoc/>
    public Task ReturnAsync(PreparedStatementEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(entry);

        if (_pool.Count >= _options.MaxPoolSize)
            EvictLeastUsed();

        _pool[entry.StatementKey] = entry;
        _logger.LogDebug("Registered prepared statement {StatementKey} in pool (PoolSize={PoolSize})", entry.StatementKey, _pool.Count);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ReleaseAsync(string statementKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(statementKey);

        _pool.TryRemove(statementKey, out _);
        _logger.LogDebug("Released prepared statement {StatementKey} from pool", statementKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<(int PoolSize, double HitRatio)> GetPoolStatsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var borrows = Interlocked.Read(ref _totalBorrows);
        var hits = Interlocked.Read(ref _poolHits);
        var hitRatio = borrows == 0 ? 0d : (double)hits / borrows;

        return Task.FromResult((_pool.Count, hitRatio));
    }

    private void EvictLeastUsed()
    {
        var target = _pool
            .OrderBy(kv => kv.Value.UseCount)
            .ThenBy(kv => kv.Value.LastUsedAt)
            .Select(kv => kv.Key)
            .FirstOrDefault();

        if (target is not null)
        {
            _pool.TryRemove(target, out _);
            _logger.LogDebug("Evicted least-used prepared statement {StatementKey}", target);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _pool.Clear();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Extension methods for registering query plan caching and prepared statement pooling
/// with an <see cref="IServiceCollection"/>.
/// </summary>
public static class QueryPlanCacheExtensions
{
    /// <summary>
    /// Registers <see cref="IQueryPlanCache"/> and <see cref="IPreparedStatementPool"/> as singletons,
    /// with optional configuration delegates for each service.
    /// </summary>
    /// <param name="services">The DI container to configure.</param>
    /// <param name="configurePlanCache">Optional delegate to adjust <see cref="QueryPlanCacheOptions"/>.</param>
    /// <param name="configureStatementPool">Optional delegate to adjust <see cref="PreparedStatementPoolOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddQueryPlanCaching(
        this IServiceCollection services,
        Action<QueryPlanCacheOptions>? configurePlanCache = null,
        Action<PreparedStatementPoolOptions>? configureStatementPool = null)
    {
        var planCacheOptions = new QueryPlanCacheOptions();
        configurePlanCache?.Invoke(planCacheOptions);
        services.AddSingleton(planCacheOptions);
        services.AddSingleton<IQueryPlanCache, QueryPlanCache>();

        var poolOptions = new PreparedStatementPoolOptions();
        configureStatementPool?.Invoke(poolOptions);
        services.AddSingleton(poolOptions);
        services.AddSingleton<IPreparedStatementPool, PreparedStatementPool>();

        return services;
    }
}
