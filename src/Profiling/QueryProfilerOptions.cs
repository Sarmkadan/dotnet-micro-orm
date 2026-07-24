#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Configuration options for the query profiler
/// </summary>
public sealed class QueryProfilerOptions
{
    /// <summary>
    /// Default instance with recommended settings
    /// </summary>
    public static readonly QueryProfilerOptions Default = new();

    /// <summary>
    /// Threshold in milliseconds for a query to be considered "slow"
    /// Defaults to 500ms
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 500;

    /// <summary>
    /// Threshold for N+1 detection: maximum number of times the same parameterized SQL
    /// can execute within a single connection/unit-of-work scope before being flagged.
    /// Defaults to 3 executions
    /// </summary>
    public int NPlusOneThreshold { get; set; } = 3;

    /// <summary>
    /// When true, enables automatic detection of N+1 query patterns
    /// </summary>
    public bool EnableNPlusOneDetection { get; set; } = true;

    /// <summary>
    /// When true, enables slow query detection
    /// </summary>
    public bool EnableSlowQueryDetection { get; set; } = true;
}