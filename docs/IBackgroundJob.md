# IBackgroundJob

`IBackgroundJob` is a contract for defining background jobs in .NET applications using dotnet-micro-orm. It specifies the minimal set of properties required to track job execution, retry behavior, scheduling, and outcome reporting.

## API

### `JobId`
A unique identifier for the background job. Must be non-null and unique within the system.

### `StartTime`
The timestamp when the job execution began. Set by the scheduler before execution.

### `Duration`
The total time taken to execute the job, including retries. Measured in milliseconds.

### `Success`
Indicates whether the job completed successfully. `true` if the job ran without throwing an exception; `false` otherwise.

### `ErrorMessage`
The exception message if the job failed. `null` if `Success` is `true`.

### `StackTrace`
The full stack trace of the exception that caused the job to fail. `null` if `Success` is `true`.

### `Output`
A dictionary of key-value pairs capturing job-specific output data. Populated during execution.

### `RunOnStartup`
If `true`, the job should run immediately when the application starts. If `false`, it is scheduled normally.

### `Interval`
The fixed time interval between job executions, in milliseconds. Used when `CronExpression` is `null`. Mutually exclusive with `CronExpression`.

### `CronExpression`
A cron expression defining the schedule for recurring job execution. Mutually exclusive with `Interval`. `null` for one-time jobs.

### `MaxRetries`
The maximum number of retry attempts if the job fails. Must be non-negative.

### `RetryDelay`
The fixed delay between retry attempts, in milliseconds. Applied consistently between all retries.

### `ExecutionTimeout`
The maximum allowed duration for a single job execution attempt. If exceeded, the job is aborted and retried. `null` means no timeout.

### `Enabled`
Controls whether the job is active. If `false`, the scheduler will not start or retry the job.

## Usage
