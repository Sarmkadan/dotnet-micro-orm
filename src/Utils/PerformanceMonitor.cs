#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Monitors application performance metrics including execution time, memory usage,
/// and database query metrics. Provides detailed performance reports and alerting
/// when thresholds are exceeded.
/// </summary>
public class sealed PerformanceMonitor : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly Dictionary<string, object> _metrics = [];
    private DateTime _startTime;
    private long _startMemory;

    public PerformanceMonitor(string operationName)
    {
        _operationName = operationName ?? "Unknown Operation";
        _stopwatch = Stopwatch.StartNew();
        _startTime = DateTime.UtcNow;
        _startMemory = GC.GetTotalMemory(false);

        _metrics["operation"] = _operationName;
        _metrics["start_time"] = _startTime;
    }

    /// <summary>
    /// Records a checkpoint in the operation
    /// </summary>
    public void Checkpoint(string name)
    {
        _metrics[$"checkpoint_{name}"] = _stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Records a custom metric value
    /// </summary>
    public void RecordMetric(string key, object value)
    {
        _metrics[key] = value;
    }

    /// <summary>
    /// Records the count of items processed
    /// </summary>
    public void RecordItemCount(int count)
    {
        _metrics["item_count"] = count;
    }

    /// <summary>
    /// Gets the current elapsed time
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the current elapsed milliseconds
    /// </summary>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    /// <summary>
    /// Generates a performance report
    /// </summary>
    public PerformanceReport GetReport()
    {
        var endMemory = GC.GetTotalMemory(false);
        var memoryDelta = endMemory - _startMemory;

        return new PerformanceReport
        {
            OperationName = _operationName,
            StartTime = _startTime,
            EndTime = DateTime.UtcNow,
            ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
            MemoryDeltaMb = memoryDelta / (1024 * 1024.0),
            Metrics = new Dictionary<string, object>(_metrics)
        };
    }

    /// <summary>
    /// Gets a summary of performance
    /// </summary>
    public string GetSummary()
    {
        var report = GetReport();
        var itemCount = report.Metrics.TryGetValue("item_count", out var count) ? count : null;

        var summary = $"{_operationName}: {report.ElapsedMilliseconds}ms ({report.MemoryDeltaMb:F2}MB)";

        if (itemCount is not null)
            summary += $" [{itemCount} items]";

        return summary;
    }

    /// <summary>
    /// Logs performance summary to console
    /// </summary>
    public void LogSummary()
    {
        Console.WriteLine($"[PERF] {GetSummary()}");
    }

    /// <summary>
    /// Creates a child performance monitor for tracking sub-operations
    /// </summary>
    public PerformanceMonitor CreateChild(string childName)
    {
        var fullName = $"{_operationName} > {childName}";
        return new PerformanceMonitor(fullName);
    }

    public void Dispose()
    {
        _stopwatch?.Stop();
    }
}

/// <summary>
/// Performance metrics report
/// </summary>
public class sealed PerformanceReport
{
    /// <summary>Operation name</summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>When operation started</summary>
    public DateTime StartTime { get; set; }

    /// <summary>When operation ended</summary>
    public DateTime EndTime { get; set; }

    /// <summary>Total execution time in milliseconds</summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>Memory usage change in MB</summary>
    public double MemoryDeltaMb { get; set; }

    /// <summary>Custom metrics collected</summary>
    public Dictionary<string, object> Metrics { get; set; } = [];

    /// <summary>Indicates if operation exceeded time threshold</summary>
    public bool ExceededTimeThreshold(long thresholdMs) => ElapsedMilliseconds > thresholdMs;

    /// <summary>Indicates if operation exceeded memory threshold</summary>
    public bool ExceededMemoryThreshold(double thresholdMb) => MemoryDeltaMb > thresholdMb;

    /// <summary>Gets performance grade (A = excellent, F = terrible)</summary>
    public char GetPerformanceGrade()
    {
        return ElapsedMilliseconds switch
        {
            < 10 => 'A',      // Excellent
            < 50 => 'B',      // Good
            < 100 => 'C',     // Acceptable
            < 500 => 'D',     // Slow
            < 1000 => 'E',    // Very Slow
            _ => 'F'          // Terrible
        };
    }
}

/// <summary>
/// Static helper for simple performance measurements
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Measures the time of a synchronous operation
    /// </summary>
    public static (T result, long elapsedMs) Measure<T>(Func<T> operation, string? label = null)
    {
        var sw = Stopwatch.StartNew();
        var result = operation();
        sw.Stop();

        if (!string.IsNullOrEmpty(label))
        {
            Console.WriteLine($"[PERF] {label}: {sw.ElapsedMilliseconds}ms");
        }

        return (result, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Measures the time of an asynchronous operation
    /// </summary>
    public static async Task<(T result, long elapsedMs)> MeasureAsync<T>(
        Func<Task<T>> operation, string? label = null)
    {
        var sw = Stopwatch.StartNew();
        var result = await operation();
        sw.Stop();

        if (!string.IsNullOrEmpty(label))
        {
            Console.WriteLine($"[PERF] {label}: {sw.ElapsedMilliseconds}ms");
        }

        return (result, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Measures the time of a void operation
    /// </summary>
    public static long Measure(Action operation, string? label = null)
    {
        var sw = Stopwatch.StartNew();
        operation();
        sw.Stop();

        if (!string.IsNullOrEmpty(label))
        {
            Console.WriteLine($"[PERF] {label}: {sw.ElapsedMilliseconds}ms");
        }

        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Measures the time of an async void operation
    /// </summary>
    public static async Task<long> MeasureAsync(
        Func<Task> operation, string? label = null)
    {
        var sw = Stopwatch.StartNew();
        await operation();
        sw.Stop();

        if (!string.IsNullOrEmpty(label))
        {
            Console.WriteLine($"[PERF] {label}: {sw.ElapsedMilliseconds}ms");
        }

        return sw.ElapsedMilliseconds;
    }
}
