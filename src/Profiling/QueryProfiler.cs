#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Thread-safe in-process query profiler.  Profiles are stored in a
/// bounded ring-buffer so memory usage stays predictable under sustained load.
/// </summary>
public sealed class QueryProfiler : IQueryProfiler
{
    private readonly int _maxProfiles;
    private readonly ConcurrentQueue<QueryProfile> _profiles = new();

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new profiler instance.
    /// </summary>
    /// <param name="maxProfiles">
    /// Maximum number of profiles kept in memory.  Oldest entries are evicted
    /// once the limit is reached.  Defaults to 1000.
    /// </param>
    public QueryProfiler(int maxProfiles = 1000)
    {
        if (maxProfiles <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxProfiles), "Must be greater than zero.");
        _maxProfiles = maxProfiles;
    }

    /// <inheritdoc/>
    public async Task<T> ProfileAsync<T>(
        string query,
        Func<Task<T>> operation,
        Dictionary<string, object>? parameters = null,
        [System.Runtime.CompilerServices.CallerMemberName] string? callerMemberName = null)
    {
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
        }

        return result;
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
    }

    // Adds a profile to the queue and evicts the oldest entry if the cap is reached.
    private void Enqueue(QueryProfile profile)
    {
        _profiles.Enqueue(profile);
        while (_profiles.Count > _maxProfiles)
            _profiles.TryDequeue(out _);
    }
}
