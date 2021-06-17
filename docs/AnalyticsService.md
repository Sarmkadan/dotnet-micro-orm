# AnalyticsService

A lightweight in-memory analytics service for recording metrics, counters, and events. Designed for micro-ORM and service-oriented applications requiring simple telemetry without external dependencies.

## API

### `void RecordMetric(MetricValue value)`

Records a single metric value with associated metadata.

- **Parameters**
  - `value`: A `MetricValue` containing `MetricName`, `Value`, `Timestamp`, and optional `Tags`.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.
  - Throws `ArgumentException` if `MetricName` is null or empty.

---

### `void IncrementCounter(string metricName, Dictionary<string, string>? tags = null)`

Increments a counter metric by one. If the metric does not exist, it is created.

- **Parameters**
  - `metricName`: Name of the counter metric.
  - `tags`: Optional dictionary of key-value tags to associate with the counter.
- **Exceptions**
  - Throws `ArgumentNullException` if `metricName` is `null`.
  - Throws `ArgumentException` if `metricName` is empty.

---

### `void RecordEvent(Event ev)`

Records a structured event with metadata and timestamp.

- **Parameters**
  - `ev`: An `Event` containing `Name`, `Timestamp`, and optional `Tags`.
- **Exceptions**
  - Throws `ArgumentNullException` if `ev` is `null`.

---
### `MetricSummary? GetMetricSummary(string metricName)`

Retrieves the aggregated summary for a specific metric.

- **Parameters**
  - `metricName`: Name of the metric to summarize.
- **Return Value**
  - Returns `MetricSummary` containing `Count`, `Min`, `Max`, and `Average` if the metric exists; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `metricName` is `null`.

---
### `List<Event> GetEvents()`

Returns all recorded events in insertion order.

- **Return Value**
  - A new list containing all events.

---
### `List<Event> GetRecentEvents(int count)`

Returns the most recent events up to the specified count.

- **Parameters**
  - `count`: Maximum number of events to return.
- **Return Value**
  - A list of up to `count` most recent events, in reverse chronological order.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is less than zero.

---
### `List<Event> GetEventsByTimeRange(DateTime start, DateTime end)`

Returns events that occurred within a specified time range (inclusive).

- **Parameters**
  - `start`: Start of the time range.
  - `end`: End of the time range.
- **Return Value**
  - A list of events within the range, ordered by timestamp ascending.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `start` is after `end`.

---
### `List<string> GetAllMetrics()`

Returns the names of all recorded metrics.

- **Return Value**
  - A list of metric names.

---
### `void ClearMetrics()`

Removes all recorded metrics and their summaries.

---
### `void ClearEvents()`

Removes all recorded events.

---
### `AnalyticsReport GenerateReport()`

Generates a consolidated report of all metrics and events.

- **Return Value**
  - An `AnalyticsReport` containing summaries of all metrics and all events.

---
### `string Name` (property)

Gets the name of the analytics service instance.

- **Return Value**
  - The instance name.

---
### `List<MetricValue> Values` (property)

Gets the raw list of recorded metric values.

- **Return Value**
  - A list of `MetricValue` objects.

---
### `double Value` (property)

Gets or sets the numeric value of a metric.

- **Return Value**
  - The current value.

---
### `DateTime Timestamp` (property)

Gets or sets the timestamp of a metric or event.

- **Return Value**
  - The current timestamp.

---
### `Dictionary<string, string> Tags` (property)

Gets or sets the metadata tags associated with a metric or event.

- **Return Value**
  - A dictionary of tag key-value pairs.

---
### `string MetricName` (property)

Gets or sets the name of a metric.

- **Return Value**
  - The metric name.

---
### `int Count` (property)

Gets the number of times a counter has been incremented.

- **Return Value**
  - The count value.

---
### `double Min` (property)

Gets the minimum recorded value for a metric.

- **Return Value**
  - The minimum value.

---
### `double Max` (property)

Gets the maximum recorded value for a metric.

- **Return Value**
  - The maximum value.

## Usage
