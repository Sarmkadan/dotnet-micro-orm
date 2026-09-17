# Background Jobs

The `DotnetMicroOrm.BackgroundJobs` namespace provides a lightweight, in-process
job scheduling system for work that runs outside the request pipeline. It is
built around three pieces:

- `IBackgroundJob` — the contract every job implements.
- `JobScheduler` — the engine that registers, schedules, executes, and tracks jobs.
- `DataCleanupJob` — a ready-made job that prunes stale data from the database.

## `IBackgroundJob`

The interface every job must implement:

```csharp
public interface IBackgroundJob
{
    string JobId { get; }          // Unique identifier for this job type
    string Name { get; }           // Human-readable name
    string Description { get; }    // What the job does
    Task ExecuteAsync();           // The job logic
    bool CanExecute();             // Whether the job may run right now
    Task OnFailureAsync(Exception ex); // Called when execution fails
}
```

- `JobId` must be unique within a scheduler instance; it is used to look jobs up
  in history and to trigger them manually.
- `CanExecute()` is checked before every run. Returning `false` records a failed
  execution with the message *"Job cannot execute in current context"* and skips
  the work.
- `OnFailureAsync` is invoked after retries are exhausted, and is the place to
  log or alert.

## `JobScheduler`

`JobScheduler` is a thread-safe, `IAsyncDisposable` engine that owns the job
registry, the timers, and the execution history.

### Registering a job

```csharp
using var scheduler = new JobScheduler();

scheduler.Register(job, new JobScheduleConfig
{
    RunOnStartup = true,
    Interval = TimeSpan.FromHours(1),
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromSeconds(30),
    ExecutionTimeout = TimeSpan.FromMinutes(5),
    Enabled = true
});

await scheduler.StartAsync();
```

`JobScheduleConfig` controls scheduling and resilience:

| Property            | Default              | Purpose                                            |
| ------------------- | -------------------- | -------------------------------------------------- |
| `RunOnStartup`      | `false`              | Run the job once when `StartAsync` is called.      |
| `Interval`          | `null`               | Fixed interval between runs.                       |
| `CronExpression`    | `null`               | Five-field cron schedule (e.g. `"0 2 * * *"`).     |
| `MaxRetries`        | `3`                  | Retry count on failure.                            |
| `RetryDelay`        | `30s`                | Delay between retries.                             |
| `ExecutionTimeout`  | `1h`                 | Per-attempt timeout.                               |
| `Enabled`           | `true`               | If `false`, the job is skipped entirely.           |

A job is scheduled by `Interval` when set, otherwise by `CronExpression`. If
neither is set, the job only runs on startup (when `RunOnStartup` is `true`).

### Convenience overloads

`JobSchedulerExtensions` adds fluent registration and inspection helpers:

```csharp
// Interval-based registration
scheduler.Register(job, TimeSpan.FromMinutes(5), enabled: true, runOnStartup: true);

// Cron-based registration
scheduler.Register(job, "0 2 * * *");

// Run a registered job immediately by id
var result = await scheduler.ExecuteJobAsync("data-cleanup");

// Inspect history and statistics
var history = scheduler.GetExecutionHistory("data-cleanup");
var lastOk   = scheduler.GetLastSuccessfulExecution("data-cleanup");
var lastFail = scheduler.GetLastFailedExecution("data-cleanup");
var stats    = scheduler.GetStatistics();
```

### Lifecycle

- `StartAsync()` — registers timers and runs any `RunOnStartup` jobs.
- `StopAsync()` — disposes all timers and cancels pending runs.
- `DisposeAsync()` — calls `StopAsync` and releases the execution lock.
- `ExecuteJobAsync(job, config)` — runs a job immediately with retry logic and
  records the result.
- `GetExecutionHistory(jobId)` / `GetRecentExecutions(count)` / `ClearHistory()`
  — read and manage the in-memory history (capped at 1000 entries).

### Cron support

`JobScheduler.GetNextOccurrence(cronExpression, after)` computes the next UTC
occurrence of a five-field expression (`minute hour day-of-month month
day-of-week`). Each field supports `*`, single values, comma-separated lists,
`a-b` ranges, and `*/step` or `a-b/step` increments. Invalid or never-matching
expressions return `null`.

## `DataCleanupJob`

A concrete `IBackgroundJob` that maintains database health by removing old audit
logs, soft-deleted records, and expired temporary data, and optionally rebuilding
indexes. It runs its steps in batches to keep transaction-log growth bounded.

```csharp
var cleanup = new DataCleanupJob(dbContext, new DataCleanupConfig
{
    AuditLogRetentionDays = 90,
    DeletedRecordRetentionDays = 30,
    CleanupAuditLogs = true,
    CleanupSoftDeletedRecords = true,
    CleanupTemporaryData = true,
    RebuildIndexes = false,   // heavy; off by default
    BatchSize = 1000
});
```

`CanExecute()` restricts it to the off-peak window of **2–4 AM UTC**, so it is
typically registered with a nightly cron schedule:

```csharp
scheduler.Register(cleanup, "0 2 * * *");
```

`DataCleanupConfig` defaults: 90-day audit-log retention, 30-day soft-delete
retention, audit/soft-delete/temp cleanup enabled, index rebuild disabled, batch
size 1000.

## Registering a job end-to-end

```csharp
using var scheduler = new JobScheduler();

// 1. Build the job (resolve dependencies however your app does)
var cleanup = new DataCleanupJob(dbContext);

// 2. Register it with a schedule
scheduler.Register(cleanup, "0 2 * * *");

// 3. Start the scheduler (typically at application startup)
await scheduler.StartAsync();

// ... application runs ...

// 4. Stop gracefully on shutdown
await scheduler.DisposeAsync();
```

## Notes

- **Thread safety**: `JobScheduler` serializes executions with a semaphore, so
  jobs do not overlap within a single instance.
- **History is in-memory**: execution history is not persisted; it is lost when
  the scheduler is disposed.
- **Failures**: exceptions are retried up to `MaxRetries` with `RetryDelay`
  between attempts, then `OnFailureAsync` is invoked and the failure is recorded.