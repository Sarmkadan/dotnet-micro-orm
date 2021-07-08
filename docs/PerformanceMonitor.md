# PerformanceMonitor

PerformanceMonitor is a utility class for tracking and analyzing the performance characteristics of code execution, including elapsed time, memory consumption, and custom metrics. It supports hierarchical monitoring through child monitors and provides mechanisms to evaluate performance against configurable thresholds.

## API

### Constructors

#### `PerformanceMonitor()`
Initializes a new instance of the PerformanceMonitor with default settings.

---

### Methods

#### `void Checkpoint(string name)`
Records a named checkpoint in the operation timeline. Checkpoints are used to mark significant points during execution for granular timing analysis.

- **Parameters**: `name` (string) - The identifier for the checkpoint.
- **Exceptions**: None.

#### `void RecordMetric(string key, object value)`
Adds a custom metric to the monitor's collection. Metrics can be any object and are included in the final report.

- **Parameters**: `key` (string) - The metric identifier; `value` (object) - The metric value.
- **Exceptions**: None.

#### `void RecordItemCount(int count)`
Records the number of items processed during the operation. This is used to calculate throughput and item-level performance.

- **Parameters**: `count` (int) - The number of items to record.
- **Exceptions**: None.

#### `PerformanceReport GetReport()`
Generates a detailed performance report containing all recorded metrics, checkpoints, and threshold evaluations.

- **Returns**: `PerformanceReport` - A structured report object.
- **Exceptions**: None.

#### `string GetSummary()`
Returns a human-readable summary of the performance data, including elapsed time, memory delta, and threshold status.

- **Returns**: `string` - The summary text.
- **Exceptions**: None.

#### `void LogSummary()`
Outputs the performance summary to the configured logging infrastructure.

- **Exceptions**: None.

#### `PerformanceMonitor CreateChild(string operationName)`
Creates a nested PerformanceMonitor instance for tracking sub-operations. Child monitors inherit threshold configurations from their parent.

- **Parameters**: `operationName` (string) - The name of the child operation.
- **Returns**: `PerformanceMonitor` - A new child monitor instance.
- **Exceptions**: None.

#### `void Dispose()`
Releases resources and finalizes the monitoring operation. Triggers report generation if not already completed.

- **Exceptions**: None.

#### `static (T result, long elapsedMs) Measure<T>(Func<T> action)`
Executes a synchronous function while measuring its elapsed time.

- **Parameters**: `action` (Func<T>) - The function to execute.
- **Returns**: A tuple containing the result and elapsed milliseconds.
- **Exceptions**: Propagates exceptions thrown by `action`.

#### `static async Task<(T result, long elapsedMs)> MeasureAsync<T>(Func<Task<T>> action)`
Asynchronously executes a function while measuring its elapsed time.

- **Parameters**: `action` (Func<Task<T>>) - The asynchronous function to execute.
- **Returns**: A tuple containing the result and elapsed milliseconds.
- **Exceptions**: Propagates exceptions thrown by `action`.

---

### Properties

#### `string OperationName`
Gets the name of the monitored operation.

#### `DateTime StartTime`
Gets the UTC timestamp when monitoring began.

#### `DateTime EndTime`
Gets the UTC timestamp when monitoring ended. Returns `DateTime.MinValue` if the operation is still active.

#### `long ElapsedMilliseconds`
Gets the total elapsed time in milliseconds between start and end.

#### `double MemoryDeltaMb`
Gets the change in memory usage (in megabytes) during the operation.

#### `Dictionary<string, object> Metrics`
Gets the collection of custom metrics recorded during monitoring.

#### `bool ExceededTimeThreshold`
Indicates whether the operation's duration exceeded the configured time threshold.

#### `bool ExceededMemoryThreshold`
Indicates whether the operation's memory usage exceeded the configured memory threshold.

#### `char GetPerformanceGrade`
Returns a letter grade (A-F) representing the overall performance based on thresholds and metrics.

---

## Usage

### Example 1: Synchronous Operation Monitoring
```csharp
using (var monitor = new PerformanceMonitor("DatabaseQuery"))
{
    monitor.Checkpoint("ConnectionOpen");
    // Simulate work
    Thread.Sleep(100);
    
    monitor.RecordItemCount(500);
    monitor.RecordMetric("QueryType", "SELECT");
    
    monitor.Checkpoint("DataFetched");
    // More work
    Thread.Sleep(200);
    
    Console.WriteLine(monitor.GetSummary());
}
```

### Example 2: Asynchronous Operation with Child Monitor
```csharp
var parentMonitor = new PerformanceMonitor("BatchProcess");
await parentMonitor.MeasureAsync(async () =>
{
    using (var child = parentMonitor.CreateChild("ValidationStep"))
    {
        await ValidateDataAsync();
        child.RecordItemCount(1000);
    }
    
    using (var child = parentMonitor.CreateChild("ProcessingStep"))
    {
        await ProcessDataAsync();
        child.RecordItemCount(950);
    }
});

parentMonitor.LogSummary();
```

---

## Notes

- **Thread Safety**: PerformanceMonitor is not thread-safe. Concurrent access to the same instance from multiple threads may result in race conditions or corrupted metrics.
- **Disposal**: The monitor must be disposed to finalize timing and memory measurements. Failure to dispose may result in incomplete data.
- **Thresholds**: Threshold evaluation occurs during `GetReport()` or `GetSummary()`. Thresholds must be configured externally before monitoring begins.
- **Child Monitors**: Child monitors are independent but inherit threshold configurations. They do not automatically aggregate into parent reports.
- **Static Measure Methods**: These methods wrap the monitored action in a using block and handle disposal automatically. They rethrow any exceptions from the action after recording timing.
- **Memory Measurement**: Memory delta is calculated using `GC.GetTotalMemory()` and may not reflect real-time allocations accurately in high-frequency scenarios.
