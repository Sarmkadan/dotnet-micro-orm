#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Represents the profiling data captured for a single query execution
/// </summary>
public sealed class QueryProfile
{
    /// <summary>Unique identifier for this profile entry</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The SQL query that was executed</summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>Parameters bound to the query at execution time</summary>
    public Dictionary<string, object>? Parameters { get; init; }

    /// <summary>Total wall-clock time the operation took</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>UTC timestamp when the query began executing</summary>
    public DateTime ExecutedAt { get; init; }

    /// <summary>Whether the query completed without an exception</summary>
    public bool Succeeded { get; init; }

    /// <summary>Exception message when the query failed; <c>null</c> on success</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Name of the calling method that triggered the query</summary>
    public string? CallerMemberName { get; init; }

    /// <summary>Number of rows affected or returned by the query, if available</summary>
    public int? RowsAffected { get; init; }
}

/// <summary>
/// Aggregated statistics across all captured profiles in a profiling session
/// </summary>
public sealed class QueryProfilerSummary
{
    /// <summary>Total number of queries executed</summary>
    public int TotalQueries { get; init; }

    /// <summary>Combined duration of all queries</summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>Mean duration per query</summary>
    public TimeSpan AverageDuration { get; init; }

    /// <summary>Longest single query duration</summary>
    public TimeSpan MaxDuration { get; init; }

    /// <summary>Shortest single query duration</summary>
    public TimeSpan MinDuration { get; init; }

    /// <summary>Number of queries that threw an exception</summary>
    public int FailedQueries { get; init; }

    /// <summary>Profile entry for the slowest query executed</summary>
    public QueryProfile? SlowestQuery { get; init; }
}
