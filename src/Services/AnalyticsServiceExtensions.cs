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
    /// <param name="service">The analytics service instance</param>
    /// <param name="eventType">Type of events to retrieve</param>
    /// <param name="skip">Number of events to skip</param>
    /// <param name="take">Number of events to return</param>
    /// <returns>List of events matching the criteria</returns>
    /// <exception cref="ArgumentNullException">Thrown when service or eventType is null</exception>
    /// <exception cref="ArgumentException">Thrown when eventType is empty or whitespace</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when skip or take is negative</exception>
    public static List<Event> GetEvents(this AnalyticsService service, string eventType, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return service.GetEvents(eventType, skip, take)
            .ToList();
    }

    private static double GetMedian(List<double> values)
        => values.Count == 0
            ? 0
            : GetMedianInternal(values.OrderBy(x => x).ToList());

    private static double GetMedianInternal(IReadOnlyList<double> sorted)
        => sorted.Count % 2 == 0
            ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2
            : sorted[sorted.Count / 2];
}
