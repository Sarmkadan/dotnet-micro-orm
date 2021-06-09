# JobSchedulerExtensions

Provides extension methods for registering and managing background jobs in a .NET application using Micro ORM. These extensions simplify job scheduling, execution tracking, and statistics collection for recurring or one-off tasks.

## API

### `Register<TJob>(this IServiceCollection services, string jobName, TimeSpan interval)`
Registers a job with a specified execution interval in the dependency injection container.

- **Parameters**
  - `services`: The `IServiceCollection` instance to register the job with.
  - `jobName`: A unique identifier for the job.
  - `interval`: The time interval between job executions.
- **Throws**
  - `ArgumentNullException` if `services` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if a job with the same name is already registered.

---

### `Register<TJob>(this IServiceCollection services, string jobName, CronExpression cronExpression)`
Registers a job with a cron expression-based schedule in the dependency injection container.

- **Parameters**
  - `services`: The `IServiceCollection` instance to register the job with.
  - `jobName`: A unique identifier for the job.
  - `cronExpression`: A cron expression defining the job's schedule.
- **Throws**
  - `ArgumentNullException` if `services`, `jobName`, or `cronExpression` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if a job with the same name is already registered.

---

### `ExecuteJobAsync(this IJobScheduler scheduler, string jobName, CancellationToken cancellationToken = default)`
Executes a registered job immediately and returns the execution result.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to execute the job.
  - `jobName`: The name of the registered job to execute.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Returns**
  - A `Task<JobExecutionResult>` representing the asynchronous operation. The result contains execution details such as success status, duration, and any exception.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---

### `GetExecutionHistory(this IJobScheduler scheduler, string jobName, int limit = 100)`
Retrieves the execution history for a registered job.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
  - `jobName`: The name of the registered job.
  - `limit`: The maximum number of historical executions to return.
- **Returns**
  - An `IEnumerable<JobExecutionResult>` containing the most recent executions, ordered by execution time descending.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---
### `GetLastSuccessfulExecution(this IJobScheduler scheduler, string jobName)`
Retrieves the most recent successful execution of a registered job.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
  - `jobName`: The name of the registered job.
- **Returns**
  - A `JobExecutionResult?` representing the last successful execution, or `null` if none exists.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---
### `GetLastFailedExecution(this IJobScheduler scheduler, string jobName)`
Retrieves the most recent failed execution of a registered job.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
  - `jobName`: The name of the registered job.
- **Returns**
  - A `JobExecutionResult?` representing the last failed execution, or `null` if none exists.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---
### `HasSuccessfulExecutions(this IJobScheduler scheduler, string jobName)`
Determines whether a registered job has any successful executions recorded.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
  - `jobName`: The name of the registered job.
- **Returns**
  - `true` if at least one successful execution exists; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---
### `GetStatistics(this IJobScheduler scheduler, string jobName)`
Retrieves aggregated statistics for a registered job.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
  - `jobName`: The name of the registered job.
- **Returns**
  - A `JobStatistics` object containing counts of total, successful, and failed executions, success rate, average execution time, and last execution time.
- **Throws**
  - `ArgumentNullException` if `scheduler` or `jobName` is `null`.
  - `ArgumentException` if `jobName` is empty or whitespace.
  - `InvalidOperationException` if no job with the specified name is registered.

---
### `GetRegisteredJobsCount(this IJobScheduler scheduler)`
Retrieves the total number of jobs currently registered in the scheduler.

- **Parameters**
  - `scheduler`: The `IJobScheduler` instance to query.
- **Returns**
  - An `int` representing the number of registered jobs.

---
### `TotalExecutions` (property of `JobStatistics`)
Gets the total number of executions recorded for the job.

- **Type**: `int`
- **Access**: Read-only

---
### `SuccessfulExecutions` (property of `JobStatistics`)
Gets the number of successful executions recorded for the job.

- **Type**: `int`
- **Access**: Read-only

---
### `FailedExecutions` (property of `JobStatistics`)
Gets the number of failed executions recorded for the job.

- **Type**: `int`
- **Access**: Read-only

---
### `SuccessRate` (property of `JobStatistics`)
Gets the success rate of the job as a value between 0.0 and 1.0.

- **Type**: `double`
- **Access**: Read-only

---
### `AverageExecutionTime` (property of `JobStatistics`)
Gets the average duration of all executions for the job.

- **Type**: `TimeSpan`
- **Access**: Read-only

---
### `LastExecutionTime` (property of `JobStatistics`)
Gets the timestamp of the most recent execution for the job.

- **Type**: `DateTime?`
- **Access**: Read-only

## Usage

### Registering and executing a job
```csharp
// Setup services
var services = new ServiceCollection();
services.AddJobScheduler();

// Register a job with a 5-minute interval
services.Register<MyBackgroundJob>("cleanup-temp-files", TimeSpan.FromMinutes(5));

// Build service provider
var provider = services.BuildServiceProvider();

// Resolve scheduler and execute job
var scheduler = provider.GetRequiredService<IJobScheduler>();
var result = await scheduler.ExecuteJobAsync("cleanup-temp-files");

if (result.IsSuccessful)
{
    Console.WriteLine($"Job succeeded in {result.Duration.TotalSeconds:F2}s");
}
else
{
    Console.WriteLine($"Job failed: {result.Exception?.Message}");
}
```

### Retrieving job statistics
```csharp
var scheduler = provider.GetRequiredService<IJobScheduler>();
var stats = scheduler.GetStatistics("cleanup-temp-files");

Console.WriteLine($"Total executions: {stats.TotalExecutions}");
Console.WriteLine($"Success rate: {stats.SuccessRate:P}");
Console.WriteLine($"Average duration: {stats.AverageExecutionTime.TotalSeconds:F2}s");
```

## Notes

- All methods are thread-safe and can be called concurrently from multiple threads.
- Execution history is persisted and survives application restarts if the underlying storage supports it.
- Time-based job scheduling (e.g., `TimeSpan` or `CronExpression`) is handled internally; no manual triggering is required for scheduled jobs.
- If a job throws an unhandled exception during execution, the failure is recorded, and the job may be retried according to its schedule.
- Statistics are calculated on-demand and reflect the current state of the execution history; no background aggregation is performed.
- The `SuccessRate` property returns `0.0` if no executions exist to avoid division by zero.
- The `LastExecutionTime` property returns `null` if no executions have occurred.
