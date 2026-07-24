#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Pluggable sink for profiler events (slow queries, N+1 patterns, etc.)
/// Implement this interface to route profiler diagnostics to your logging/monitoring system.
/// </summary>
public interface IProfilerSink
{
    /// <summary>
    /// Called when a slow query is detected
    /// </summary>
    /// <param name="profile">The query profile</param>
    /// <param name="thresholdMs">The slow query threshold in milliseconds</param>
    void ReportSlowQuery(QueryProfile profile, int thresholdMs);

    /// <summary>
    /// Called when an N+1 query pattern is detected
    /// </summary>
    /// <param name="queryText">The parameterized SQL query text</param>
    /// <param name="executionCount">Number of times the query was executed</param>
    /// <param name="threshold">The N+1 threshold</param>
    void ReportNPlusOneQuery(string queryText, int executionCount, int threshold);

    /// <summary>
    /// Called when a query execution fails
    /// </summary>
    /// <param name="profile">The query profile with error information</param>
    void ReportFailedQuery(QueryProfile profile);
}