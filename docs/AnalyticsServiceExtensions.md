# AnalyticsServiceExtensions

The `AnalyticsServiceExtensions` class provides a set of extension methods for an analytics service (typically implementing `IAnalyticsService`). These methods simplify common operations such as recording metrics, querying events by property, retrieving top metrics by average value, and fetching all events. The class is part of the `dotnet-micro-orm` library and is designed to be used with dependency injection or directly on an analytics service instance.

## API

### `RecordMetric`

Records a metric with the specified name and value.

- **Parameters**  
  - `this IAnalyticsService service` – The analytics service instance.  
  - `string metricName` – The name of the metric to record. Must not be null or empty.  
  - `double value` – The numeric value of the metric.

- **Returns**  
  `void`

- **Throws**  
  - `ArgumentNullException` if `metricName` is `null` or empty.  
  - Any exception thrown by the underlying `IAnalyticsService.RecordMetric` implementation.

### `GetEventsByProperty`

Retrieves all events that match a given property name and value.

- **Parameters**  
  - `this IAnalyticsService service` – The analytics service instance.  
  - `string propertyName` – The name of the property to filter by. Must not be null.  
  - `object propertyValue` – The value that the property must equal.

- **Returns**  
  `List<Event>` – A list of matching events. Returns an empty list if no events match.

- **Throws**  
  - `ArgumentNullException` if `propertyName` is `null`.  
  - Any exception thrown by the underlying `IAnalyticsService.GetEventsByProperty` implementation.

### `GetTopMetricsByAverage`

Returns the top `count` metrics sorted by their average value in descending order.

- **Parameters**  
  - `this IAnalyticsService service` – The analytics service instance.  
  - `int count` – The number of metrics to return. Must be greater than zero.

- **Returns**  
  `List<MetricSummary>` – A list of `MetricSummary` objects, each containing the metric name and its average value, ordered from highest average to lowest.

- **Throws**  
  - `ArgumentOutOfRangeException` if `count` is less than or equal to zero.  
  - Any exception thrown by the underlying `IAnalyticsService.GetTopMetricsByAverage` implementation.

### `GetEvents`

Retrieves all events, optionally filtered by a date range.

- **Parameters**  
  - `this IAnalyticsService service` – The analytics service instance.  
  - `DateTime? from = null` – The inclusive start of the date range. If `null`, no lower bound is applied.  
  - `DateTime? to = null` – The inclusive end of the date range. If `null`, no upper bound is applied.

- **Returns**  
  `List<Event>` – A list of events within the specified range, or all events if both parameters are `null`. Returns an empty list if no events exist.

- **Throws**  
  - `ArgumentException` if `from` is greater than `to`.  
  - Any exception thrown by the underlying `IAnalyticsService.GetEvents` implementation.

## Usage

### Example 1: Recording a metric and retrieving top metrics

```csharp
using DotNetMicroOrm.Analytics;

public class MetricsReporter
{
    private readonly IAnalyticsService _analytics;

    public MetricsReporter(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public void ReportPageLoad(string pageName, double loadTimeMs)
    {
        _analytics.RecordMetric($"page_load_{pageName}", loadTimeMs);
    }

    public void ShowTopSlowPages(int topN)
    {
        var topMetrics = _analytics.GetTopMetricsByAverage(topN);
        foreach (var metric in topMetrics)
        {
            Console.WriteLine($"{metric.Name}: {metric.Average:F2} ms");
        }
    }
}
```

### Example 2: Querying events by property and retrieving all events

```csharp
using DotNetMicroOrm.Analytics;

public class EventAuditor
{
    private readonly IAnalyticsService _analytics;

    public EventAuditor(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public List<Event> GetErrorsBySeverity(string severity)
    {
        return _analytics.GetEventsByProperty("Severity", severity);
    }

    public List<Event> GetRecentEvents(DateTime since)
    {
        return _analytics.GetEvents(from: since);
    }
}
```

## Notes

- **Edge cases**  
  - All methods return an empty `List<T>` when no data matches the query criteria; they never return `null`.  
  - `RecordMetric` does not validate the `value` parameter; negative or zero values are accepted unless the underlying service rejects them.  
  - `GetEventsByProperty` performs an equality comparison on `propertyValue`. Complex types are compared using the default equality comparer.  
  - `GetTopMetricsByAverage` with a `count` larger than the number of available metrics returns all metrics without error.  
  - `GetEvents` with `from` and `to` both `null` returns the entire event store; this may be expensive for large datasets.

- **Thread safety**  
  These extension methods are static and stateless; they delegate all work to the `IAnalyticsService` instance. Thread safety depends entirely on the implementation of that service. If the service is not designed for concurrent access, callers must synchronize access externally. The methods themselves do not introduce any additional locking or synchronization.
