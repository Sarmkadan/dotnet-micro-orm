// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

/// <summary>
/// Service for tracking and reporting application analytics and metrics.
/// Collects data on usage patterns, performance metrics, and business KPIs.
/// Supports real-time metrics and historical aggregation.
/// </summary>
public class AnalyticsService
{
    private readonly Dictionary<string, Metric> _metrics = [];
    private readonly List<Event> _events = [];
    private readonly object _lock = new();

    /// <summary>
    /// Records a metric value (counter, gauge, histogram)
    /// </summary>
    public void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("Metric name cannot be empty", nameof(metricName));

        lock (_lock)
        {
            if (!_metrics.TryGetValue(metricName, out var metric))
            {
                metric = new Metric { Name = metricName, Values = [] };
                _metrics[metricName] = metric;
            }

            metric.Values.Add(new MetricValue
            {
                Value = value,
                Timestamp = DateTime.UtcNow,
                Tags = tags ?? []
            });

            // Keep only last 1000 values per metric
            if (metric.Values.Count > 1000)
            {
                metric.Values = metric.Values.Skip(metric.Values.Count - 1000).ToList();
            }
        }
    }

    /// <summary>
    /// Increments a counter metric
    /// </summary>
    public void IncrementCounter(string metricName, int amount = 1)
    {
        RecordMetric(metricName, amount);
    }

    /// <summary>
    /// Records a custom event
    /// </summary>
    public void RecordEvent(string eventType, string description, Dictionary<string, object>? properties = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        lock (_lock)
        {
            _events.Add(new Event
            {
                Type = eventType,
                Description = description,
                Properties = properties ?? [],
                Timestamp = DateTime.UtcNow
            });

            // Keep only last 10000 events
            if (_events.Count > 10000)
            {
                _events.RemoveRange(0, _events.Count - 10000);
            }
        }
    }

    /// <summary>
    /// Gets summary statistics for a metric
    /// </summary>
    public MetricSummary? GetMetricSummary(string metricName)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(metricName, out var metric) || metric.Values.Count == 0)
                return null;

            var values = metric.Values.Select(v => v.Value).ToList();

            return new MetricSummary
            {
                MetricName = metricName,
                Count = values.Count,
                Min = values.Min(),
                Max = values.Max(),
                Average = values.Average(),
                Sum = values.Sum(),
                Median = GetMedian(values),
                LastValue = values.Last(),
                LastRecordedAt = metric.Values.Last().Timestamp
            };
        }
    }

    /// <summary>
    /// Gets events of a specific type
    /// </summary>
    public List<Event> GetEvents(string eventType, int limit = 100)
    {
        lock (_lock)
        {
            return _events
                .Where(e => e.Type == eventType)
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Gets recent events
    /// </summary>
    public List<Event> GetRecentEvents(int limit = 100)
    {
        lock (_lock)
        {
            return _events
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Gets events within a time range
    /// </summary>
    public List<Event> GetEventsByTimeRange(DateTime startTime, DateTime endTime)
    {
        lock (_lock)
        {
            return _events
                .Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }
    }

    /// <summary>
    /// Gets all recorded metrics
    /// </summary>
    public List<string> GetAllMetrics()
    {
        lock (_lock)
        {
            return _metrics.Keys.ToList();
        }
    }

    /// <summary>
    /// Clears all metrics
    /// </summary>
    public void ClearMetrics()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    /// <summary>
    /// Clears all events
    /// </summary>
    public void ClearEvents()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    /// <summary>
    /// Generates an analytics report
    /// </summary>
    public AnalyticsReport GenerateReport()
    {
        lock (_lock)
        {
            var summaries = _metrics
                .Select(kvp => GetMetricSummary(kvp.Key))
                .Where(s => s is not null)
                .Cast<MetricSummary>()
                .ToList();

            var eventCounts = _events
                .GroupBy(e => e.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            return new AnalyticsReport
            {
                GeneratedAt = DateTime.UtcNow,
                MetricCount = _metrics.Count,
                EventCount = _events.Count,
                Metrics = summaries,
                EventTypes = eventCounts.ToDictionary(x => x.Type, x => x.Count)
            };
        }
    }

    private static double GetMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;

        return sorted[count / 2];
    }
}

/// <summary>
/// Represents a metric with recorded values
/// </summary>
public class Metric
{
    public string Name { get; set; } = string.Empty;
    public List<MetricValue> Values { get; set; } = [];
}

/// <summary>
/// Individual metric value with timestamp
/// </summary>
public class MetricValue
{
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
}

/// <summary>
/// Summary statistics for a metric
/// </summary>
public class MetricSummary
{
    public string MetricName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Average { get; set; }
    public double Sum { get; set; }
    public double Median { get; set; }
    public double LastValue { get; set; }
    public DateTime LastRecordedAt { get; set; }
}

/// <summary>
/// Represents a tracked event
/// </summary>
public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = [];
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Complete analytics report
/// </summary>
public class AnalyticsReport
{
    public DateTime GeneratedAt { get; set; }
    public int MetricCount { get; set; }
    public int EventCount { get; set; }
    public List<MetricSummary> Metrics { get; set; } = [];
    public Dictionary<string, int> EventTypes { get; set; } = [];

    /// <summary>Gets summary statistics</summary>
    public string GetSummary()
    {
        return $"Analytics Report - Generated: {GeneratedAt:G}, Metrics: {MetricCount}, Events: {EventCount}, Event Types: {EventTypes.Count}";
    }
}
