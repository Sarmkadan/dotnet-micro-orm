# JobScheduler

Central component of the dotnet-micro-orm job execution system that schedules, executes, and tracks background jobs with minimal overhead and built-in history tracking.

## API

### `void Register(JobDescriptor descriptor)`

Registers a new job definition with the scheduler. The job will be available for execution once registered.

- **Parameters**
  - `descriptor` – A `JobDescriptor` instance describing the job’s metadata (name, handler type, recurrence, etc.).
- **Exceptions**
  - `ArgumentNullException` – Thrown if `descriptor` is `null`.
  - `InvalidOperationException` – Thrown if a job with the same name is already registered.

---

### `async Task StartAsync(CancellationToken cancellationToken = default)`

Begins executing all registered jobs according to their schedules. Jobs are started in fire-and-forget fashion; failures are recorded in history.

- **Parameters**
  - `cancellationToken` – Optional token to stop the scheduler gracefully.
- **Exceptions**
  - `InvalidOperationException` – Thrown if the scheduler is already running.
  - `OperationCanceledException` – Thrown if `cancellationToken` is triggered before completion.

---

### `async Task StopAsync(CancellationToken cancellationToken = default)`

Stops all active job executions and prevents new jobs from starting. Awaits completion of any currently running jobs.

- **Parameters**
  - `cancellationToken` – Optional token to abort the shutdown early.
- **Exceptions**
  - `OperationCanceledException` – Thrown if `cancellationToken` is triggered before completion.

---
### `async Task<JobExecutionResult> ExecuteJobAsync(string jobName, CancellationToken cancellationToken = default)`

Executes a single job instance immediately, bypassing its normal schedule. Useful for manual triggers or testing.

- **Parameters**
  - `jobName` – Name of the registered job to run.
  - `cancellationToken` – Optional token to cancel the execution.
- **Return Value**
  - A `JobExecutionResult` containing outcome, duration, and any exception.
- **Exceptions**
  - `ArgumentNullException` – Thrown if `jobName` is `null`.
  - `KeyNotFoundException` – Thrown if no job named `jobName` is registered.
  - `InvalidOperationException` – Thrown if the scheduler is not running.

---
### `IEnumerable<JobExecutionResult> GetExecutionHistory()`

Returns a complete chronological record of all job executions performed by this scheduler.

- **Return Value**
  - An enumerable of `JobExecutionResult` objects, ordered from oldest to newest.
- **Exceptions**
  - None.

---
### `IEnumerable<JobExecutionResult> GetRecentExecutions(int count)`

Returns the most recent executions, limited by `count`.

- **Parameters**
  - `count` – Maximum number of results to return; must be ≥ 0.
- **Return Value**
  - An enumerable of up to `count` `JobExecutionResult` objects, ordered newest to oldest.
- **Exceptions**
  - `ArgumentOutOfRangeException` – Thrown if `count` < 0.

---
### `void ClearHistory()`

Removes all recorded execution history from the scheduler.

- **Exceptions**
  - None.

---
### `async ValueTask DisposeAsync()`

Stops the scheduler if running and releases any unmanaged resources. Safe to call multiple times.

- **Exceptions**
  - None.

## Usage

### Register and start a recurring job

```csharp
using var scheduler = new JobScheduler();

// Register a job that runs every 5 minutes
scheduler.Register(new JobDescriptor(
    name: "CleanupTempFiles",
    handlerType: typeof(CleanupTempFilesJob),
    recurrence: JobRecurrence.Every(TimeSpan.FromMinutes(5))));

// Start the scheduler
await scheduler.StartAsync();

// Later, stop gracefully
await scheduler.StopAsync();
```

---
### Manually trigger and inspect a job

```csharp
using var scheduler = new JobScheduler();
scheduler.Register(new JobDescriptor(
    name: "SendWelcomeEmail",
    handlerType: typeof(SendWelcomeEmailJob),
    recurrence: JobRecurrence.Once()));

// Start the scheduler
await scheduler.StartAsync();

// Trigger the job immediately
var result = await scheduler.ExecuteJobAsync("SendWelcomeEmail");
Console.WriteLine($"Job finished in {result.Duration.TotalMilliseconds} ms with status {result.Status}");

// Inspect recent executions
foreach (var exec in scheduler.GetRecentExecutions(5))
{
    Console.WriteLine($"{exec.JobName} at {exec.StartTime} => {exec.Status}");
}

// Clean up history
scheduler.ClearHistory();
```

## Notes

- **Thread Safety**: All public members are safe to call concurrently from any thread. Internally, the scheduler uses a single background timer for scheduling and a thread-safe queue for execution history.
- **Overlapping Executions**: Recurring jobs may overlap if their previous run exceeds the recurrence interval. The scheduler allows concurrent executions unless the job descriptor explicitly prevents it.
- **Cancellation**: `StopAsync` waits for currently running jobs to finish; long-running jobs may delay shutdown. Use `CancellationToken` to enforce stricter limits.
- **History Size**: `GetExecutionHistory` streams results; avoid materializing large histories in memory. Use `GetRecentExecutions` to limit payload size.
