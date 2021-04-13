#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Instruments query execution to measure duration and capture diagnostics.
/// Wrap any database call with <see cref="ProfileAsync{T}"/> to record it.
/// </summary>
public interface IQueryProfiler
{
    /// <summary>
    /// Executes <paramref name="operation"/> and records a <see cref="QueryProfile"/>
    /// containing the SQL text, bound parameters, and wall-clock duration.
    /// When <see cref="IsEnabled"/> is <c>false</c> the delegate is invoked directly
    /// with no overhead.
    /// </summary>
    /// <typeparam name="T">Return type of the operation.</typeparam>
    /// <param name="query">SQL text that will be executed.</param>
    /// <param name="operation">Async factory that runs the query.</param>
    /// <param name="parameters">Optional parameter map logged alongside the query.</param>
    /// <param name="callerMemberName">Populated automatically by the compiler.</param>
    Task<T> ProfileAsync<T>(
        string query,
        Func<Task<T>> operation,
        Dictionary<string, object>? parameters = null,
        [System.Runtime.CompilerServices.CallerMemberName] string? callerMemberName = null);

    /// <summary>
    /// Returns all captured profiles ordered by execution time descending.
    /// </summary>
    IReadOnlyList<QueryProfile> GetProfiles();

    /// <summary>
    /// Returns aggregated statistics for all captured profiles.
    /// </summary>
    QueryProfilerSummary GetSummary();

    /// <summary>
    /// Removes all captured profiles from memory.
    /// </summary>
    void Clear();

    /// <summary>
    /// When <c>false</c> profiling is skipped entirely and the wrapped operation
    /// runs at full speed.  Defaults to <c>true</c>.
    /// </summary>
    bool IsEnabled { get; set; }
}
