#nullable enable

namespace DotnetMicroOrm.Services;

/// <summary>
/// Extension methods for AnalyticsService providing additional functionality
/// for working with metrics and events.
/// </summary>
public static class AnalyticsServiceExtensions
{
    /// <summary>
    /// Records a metric with additional tags for better categorization
    /// </summary>
    public static void RecordMetric(this AnalyticsService service, string metricName, double value, string tagKey, string tagValue)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(metricName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tagKey);

        var tags = new Dictionary<string, string> { { tagKey, tagValue } };
        service.RecordMetric(metricName, value, tags);
    }

    /// <summary>
    /// Gets events with specific property value
    /// </summary>
    public static List<Event> GetEventsByProperty(this AnalyticsService service, string propertyName, object propertyValue, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return service.GetRecentEvents(limit)
            .Where(e => e.Properties.TryGetValue(propertyName, out var val) && val?.ToString() == propertyValue?.ToString())
            .ToList();
    }

    /// <summary>
    /// Gets top N metrics by average value
    /// </summary>
    public static List<MetricSummary> GetTopMetricsByAverage(this AnalyticsService service, int topN = 5)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetAllMetrics()
            .Select(metricName => service.GetMetricSummary(metricName))
            .Where(s => s is not null)
            .OrderByDescending(s => s!.Average)
            .Take(topN)
            .ToList()!;
    }

    /// <summary>
    /// Gets events by type with pagination support
    /// </summary>
    public static List<Event> GetEvents(this AnalyticsService service, string eventType, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        return service.GetEvents(eventType, skip + take)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    private static double GetMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count == 0)
            return 0;

        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;

        return sorted[count / 2];
    }
}
