#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Thread-safe in-process query profiler. Profiles are stored in a
/// bounded ring-buffer so memory usage stays predictable under sustained load.
/// </summary>
public sealed class QueryProfiler : IQueryProfiler
{
    private readonly int _maxProfiles;
    private readonly ConcurrentQueue<QueryProfile> _profiles = new();
    private readonly QueryProfilerOptions _options;
    private readonly IProfilerSink _sink;
    private readonly ConcurrentDictionary<string, QueryExecutionStats> _executionStats = new();

    private sealed class QueryExecutionStats
    {
        public int ExecutionCount { get; set; }
        public DateTime FirstExecutionAt { get; set; } = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new profiler instance.
    /// </summary>
    /// <param name="maxProfiles">
    /// Maximum number of profiles kept in memory. Oldest entries are evicted
    /// once the limit is reached. Defaults to 1000.
    /// </param>
    /// <param name="options">
    /// Profiler configuration options. When null, defaults are used.
    /// </param>
    /// <param name="sink">
    /// Sink for profiler diagnostics. When null, <see cref="ConsoleProfilerSink.Instance"/> is used.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxProfiles is less than or equal to zero.</exception>
    public QueryProfiler(
        int maxProfiles = 1000,
        QueryProfilerOptions? options = null,
        IProfilerSink? sink = null)
    {
        if (maxProfiles <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxProfiles), "Must be greater than zero.");
        _maxProfiles = maxProfiles;
        _options = options ?? QueryProfilerOptions.Default;
        _sink = sink ?? ConsoleProfilerSink.Instance;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when query or operation is null.</exception>
    public async Task<T> ProfileAsync<T>(
        string query,
        Func<Task<T>> operation,
        Dictionary<string, object>? parameters = null,
        [System.Runtime.CompilerServices.CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!IsEnabled)
            return await operation();

        var executedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        bool succeeded = true;
        string? errorMessage = null;
        T result;

        try
        {
            result = await operation();
        }
        catch (Exception ex)
        {
            succeeded = false;
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var profile = new QueryProfile
            {
                Query = query,
                Parameters = parameters,
                Duration = stopwatch.Elapsed,
                ExecutedAt = executedAt,
                Succeeded = succeeded,
                ErrorMessage = errorMessage,
                CallerMemberName = callerMemberName
            };

            Enqueue(profile);

            // Check for slow queries
            CheckForSlowQuery(profile);

            // Check for N+1 patterns
            CheckForNPlusOnePattern(query);

            // Report failures
            if (!profile.Succeeded)
            {
                _sink.ReportFailedQuery(profile);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if the query execution exceeds the slow query threshold and reports it
    /// </summary>
    /// <param name="profile">The query profile to check</param>
    private void CheckForSlowQuery(QueryProfile profile)
    {
        if (!_options.EnableSlowQueryDetection || profile.Duration.TotalMilliseconds < _options.SlowQueryThresholdMs)
            return;

        _sink.ReportSlowQuery(profile, _options.SlowQueryThresholdMs);
    }

    /// <summary>
    /// Checks if the same query is being executed repeatedly (potential N+1 pattern)
    /// </summary>
    /// <param name="query">The SQL query text to check</param>
    private void CheckForNPlusOnePattern(string query)
    {
        if (!_options.EnableNPlusOneDetection)
            return;

        // Create a key from the query text for tracking
        var queryKey = GetQueryKey(query);

        var stats = _executionStats.AddOrUpdate(
            queryKey,
            _ => new QueryExecutionStats { ExecutionCount = 1 },
            (_, existing) =>
            {
                existing.ExecutionCount++;
                return existing;
            });

        // Check if we've exceeded the threshold
        if (stats.ExecutionCount > _options.NPlusOneThreshold)
        {
            _sink.ReportNPlusOneQuery(query, stats.ExecutionCount, _options.NPlusOneThreshold);
        }
    }

    /// <summary>
    /// Creates a normalized key for query tracking
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <returns>Normalized query key</returns>
    private static string GetQueryKey(string query)
    {
        // Normalize whitespace and remove parameter values for pattern matching
        var normalized = new StringBuilder();
        bool inQuotes = false;
        bool inWhitespace = false;

        foreach (char c in query)
        {
            if (c == '"' || c == '\'')
            {
                inQuotes = !inQuotes;
                normalized.Append(c);
            }
            else if (inQuotes)
            {
                normalized.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                if (!inWhitespace)
                {
                    normalized.Append(' ');
                    inWhitespace = true;
                }
            }
            else
            {
                normalized.Append(char.ToUpperInvariant(c));
                inWhitespace = false;
            }
        }

        return normalized.ToString();
    }

    /// <inheritdoc/>
    public IReadOnlyList<QueryProfile> GetProfiles() =>
        _profiles.OrderByDescending(p => p.ExecutedAt).ToList();

    /// <inheritdoc/>
    public QueryProfilerSummary GetSummary()
    {
        var profiles = _profiles.ToList();
        if (profiles.Count == 0)
            return new QueryProfilerSummary();

        var durations = profiles.Select(p => p.Duration.Ticks).ToList();

        return new QueryProfilerSummary
        {
            TotalQueries = profiles.Count,
            TotalDuration = TimeSpan.FromTicks(durations.Sum()),
            AverageDuration = TimeSpan.FromTicks((long)durations.Average()),
            MaxDuration = TimeSpan.FromTicks(durations.Max()),
            MinDuration = TimeSpan.FromTicks(durations.Min()),
            FailedQueries = profiles.Count(p => !p.Succeeded),
            SlowestQuery = profiles.MaxBy(p => p.Duration)
        };
    }

    /// <inheritdoc/>
    public void Clear()
    {
        while (_profiles.TryDequeue(out _)) { }
        _executionStats.Clear();
    }

    // Adds a profile to the queue and evicts the oldest entry if the cap is reached.
    private void Enqueue(QueryProfile profile)
    {
        _profiles.Enqueue(profile);
        while (_profiles.Count > _maxProfiles)
            _profiles.TryDequeue(out _);
    }
}