// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Represents a cached query execution plan with associated metadata.
/// </summary>
public sealed class QueryPlan
{
    /// <summary>
    /// Normalized SQL fingerprint used as the cache key.
    /// </summary>
    public required string Fingerprint { get; init; }

    /// <summary>
    /// Original SQL statement text.
    /// </summary>
    public required string Sql { get; init; }

    /// <summary>
    /// Estimated optimizer cost; zero when not available from the database engine.
    /// </summary>
    public double EstimatedCost { get; init; }

    /// <summary>
    /// Descriptors for each parameter expected by the statement.
    /// </summary>
    public IReadOnlyList<QueryParameterDescriptor> Parameters { get; init; } = [];

    /// <summary>
    /// UTC time when this plan was first stored.
    /// </summary>
    public DateTime CachedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times this plan has been served from cache.
    /// </summary>
    public long HitCount { get; set; }
}

/// <summary>
/// Describes a SQL parameter by name, ADO.NET type, and optional maximum size.
/// </summary>
public sealed class QueryParameterDescriptor
{
    /// <summary>Parameter name including the provider prefix (e.g. <c>@id</c>).</summary>
    public required string Name { get; init; }

    /// <summary>ADO.NET type used when creating the parameter on a <see cref="System.Data.Common.DbCommand"/>.</summary>
    public required DbType DbType { get; init; }

    /// <summary>Maximum size in bytes or characters; <c>null</c> for fixed-width types.</summary>
    public int? Size { get; init; }
}

/// <summary>
/// Represents pooled prepared-statement metadata for efficient <see cref="System.Data.Common.DbCommand"/> hydration.
/// </summary>
public sealed class PreparedStatementEntry
{
    /// <summary>Unique key identifying the prepared statement in the pool.</summary>
    public required string StatementKey { get; init; }

    /// <summary>SQL text of the prepared statement.</summary>
    public required string Sql { get; init; }

    /// <summary>Parameter descriptors in the order they appear in <see cref="Sql"/>.</summary>
    public IReadOnlyList<QueryParameterDescriptor> Parameters { get; init; } = [];

    /// <summary>UTC time this entry was last retrieved from the pool.</summary>
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Total number of times this entry has been borrowed from the pool.</summary>
    public long UseCount { get; set; }
}

/// <summary>
/// Caches query execution plans to avoid repeated SQL parsing and optimizer overhead.
/// </summary>
public interface IQueryPlanCache : IAsyncDisposable
{
    /// <summary>
    /// Returns the cached <see cref="QueryPlan"/> for the given fingerprint, or <c>null</c> if not present or expired.
    /// </summary>
    Task<QueryPlan?> GetPlanAsync(string fingerprint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a <see cref="QueryPlan"/> in the cache with an optional time-to-live.
    /// </summary>
    Task StorePlanAsync(QueryPlan plan, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a cached plan for the SQL text, invoking <paramref name="analyzer"/> on a cache miss to produce and store one.
    /// </summary>
    /// <param name="sql">Raw SQL statement to look up or analyze.</param>
    /// <param name="analyzer">Async factory invoked on a cache miss; receives the SQL and a cancellation token.</param>
    /// <param name="ttl">Optional TTL override for the produced plan.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task<QueryPlan> GetOrAnalyzeAsync(
        string sql,
        Func<string, CancellationToken, Task<QueryPlan>> analyzer,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a specific plan from the cache by fingerprint.</summary>
    Task InvalidateAsync(string fingerprint, CancellationToken cancellationToken = default);

    /// <summary>Removes all cached plans.</summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns cache statistics: total entries currently held, cumulative hits, and cumulative misses.
    /// </summary>
    Task<(long Entries, long Hits, long Misses)> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages a pool of prepared-statement metadata for fast command construction across repeated executions.
/// </summary>
public interface IPreparedStatementPool : IAsyncDisposable
{
    /// <summary>
    /// Returns the pooled <see cref="PreparedStatementEntry"/> for the given key, or <c>null</c> if not pooled.
    /// </summary>
    Task<PreparedStatementEntry?> BorrowAsync(string statementKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers or refreshes a <see cref="PreparedStatementEntry"/> in the pool.
    /// Evicts the least-used entry when the pool is at capacity.
    /// </summary>
    Task ReturnAsync(PreparedStatementEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Removes a statement from the pool and frees its slot.</summary>
    Task ReleaseAsync(string statementKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current pool occupancy and cumulative hit ratio since startup.
    /// </summary>
    Task<(int PoolSize, double HitRatio)> GetPoolStatsAsync(CancellationToken cancellationToken = default);
}
