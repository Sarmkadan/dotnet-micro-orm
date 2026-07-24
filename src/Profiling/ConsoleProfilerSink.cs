#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Default profiler sink that writes to console output
/// </summary>
public sealed class ConsoleProfilerSink : IProfilerSink
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static readonly ConsoleProfilerSink Instance = new();

    /// <summary>
    /// Creates a new console profiler sink
    /// </summary>
    public ConsoleProfilerSink() { }

    /// <inheritdoc/>
    public void ReportSlowQuery(QueryProfile profile, int thresholdMs)
    {
        Console.Error.WriteLine($"[QueryProfiler] SLOW QUERY DETECTED (>{thresholdMs}ms):");
        Console.Error.WriteLine($"  Query: {TruncateQuery(profile.Query)}");
        Console.Error.WriteLine($"  Duration: {profile.Duration.TotalMilliseconds:F2}ms");
        Console.Error.WriteLine($"  Executed: {profile.ExecutedAt:O}");
        Console.Error.WriteLine($"  Caller: {profile.CallerMemberName ?? "unknown"}");
        if (profile.Parameters?.Count > 0)
        {
            Console.Error.WriteLine($"  Parameters: {FormatParameters(profile.Parameters)}");
        }
        if (!profile.Succeeded && profile.ErrorMessage is not null)
        {
            Console.Error.WriteLine($"  Error: {profile.ErrorMessage}");
        }
    }

    /// <inheritdoc/>
    public void ReportNPlusOneQuery(string queryText, int executionCount, int threshold)
    {
        Console.Error.WriteLine($"[QueryProfiler] N+1 QUERY PATTERN DETECTED (>{threshold} executions):");
        Console.Error.WriteLine($"  Query: {TruncateQuery(queryText)}");
        Console.Error.WriteLine($"  Executions: {executionCount}");
        Console.Error.WriteLine($"  This pattern suggests inefficient data loading - consider batching or joins.");
    }

    /// <inheritdoc/>
    public void ReportFailedQuery(QueryProfile profile)
    {
        Console.Error.WriteLine($"[QueryProfiler] FAILED QUERY:");
        Console.Error.WriteLine($"  Query: {TruncateQuery(profile.Query)}");
        Console.Error.WriteLine($"  Error: {profile.ErrorMessage}");
        Console.Error.WriteLine($"  Duration: {profile.Duration.TotalMilliseconds:F2}ms");
    }

    private static string TruncateQuery(string query)
    {
        const int maxLength = 200;
        if (query.Length <= maxLength)
            return query;
        return query[..maxLength] + "... [truncated]";
    }

    private static string FormatParameters(Dictionary<string, object> parameters)
    {
        var pairs = new List<string>();
        foreach (var param in parameters)
        {
            var valueStr = param.Value switch
            {
                null => "null",
                string s => $"\"{s}\"",
                _ => param.Value.ToString() ?? "null"
            };
            pairs.Add($"{param.Key}={valueStr}");
        }
        return string.Join(", ", pairs);
    }
}