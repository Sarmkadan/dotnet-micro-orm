// ... (rest of the README content remains the same)

## MigrationRecord

The `MigrationRecord` class represents a persisted record of a migration that has been applied to the database. It stores metadata about the migration, including its version, description, application timestamp, success status, and error message if applicable.

### Example Usage

```csharp
using DotnetMicroOrm.Migrations;

public class MigrationRunner
{
    public async Task ApplyMigrationAsync(string migrationVersion)
    {
        var migrationRecord = new MigrationRecord
        {
            Version = migrationVersion,
            Description = "Applied migration to update user table",
            AppliedAt = DateTime.UtcNow,
            Success = true
        };

        // Save the migration record to the database
        await using var dbContext = new DatabaseContext();
        dbContext.MigrationRecords.Add(migrationRecord);
        await dbContext.SaveChangesAsync();
    }
}
```

## MigrationRunner

The `MigrationRunner` class manages the execution of database migrations in version order. It automatically creates and maintains a `_MigrationHistory` table to track which migrations have been applied, enabling reliable migration management across environments and deployments.

### Example Usage

```csharp
using DotnetMicroOrm.Migrations;
using DotnetMicroOrm.Data;

// Discover and register your migrations (typically done via assembly scanning)
var migrations = new List<IMigration> 
{
    new CreateUsersTableMigration(),
    new AddEmailIndexMigration(),
    new SeedInitialDataMigration()
};

// Create a database context (configured for your database provider)
await using var dbContext = new DatabaseContext();

// Initialize the migration runner
var runner = new MigrationRunner(dbContext, migrations);

// Apply all pending migrations
await runner.MigrateAsync();

// Check which migrations are pending
var pendingMigrations = await runner.GetPendingMigrationsAsync();
Console.WriteLine($"Pending migrations: {pendingMigrations.Count}");

// Check which migrations have been applied
var appliedMigrations = await runner.GetAppliedMigrationsAsync();
Console.WriteLine($"Applied migrations: {appliedMigrations.Count}");

// Migrate to a specific version
await runner.MigrateToAsync("2.1.0");

// Rollback to a previous version
await runner.RollbackToAsync("1.0.0");
```

## QueryProfile

The `QueryProfile` class represents a single captured profiling record for a database query execution. It contains metadata about the query including the SQL statement, execution parameters, timing information, success status, and caller context. This type is typically consumed through the `QueryProfiler` class which aggregates multiple profiles into a `QueryProfilerSummary`.








### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

public class UserRepository
{
    private readonly QueryProfiler _profiler = new QueryProfiler();

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var profile = new QueryProfile
        {
            Query = "SELECT id, name, email FROM users WHERE id = @id",
            Parameters = new Dictionary<string, object> { ["@id"] = userId },
            ExecutedAt = DateTime.UtcNow,
            CallerMemberName = nameof(GetUserByIdAsync)
        };

        try
        {
            // Simulate query execution
            await Task.Delay(5);
            profile.Duration = TimeSpan.FromMilliseconds(5);
            profile.RowsAffected = 1;
            profile.Succeeded = true;
        }
        catch (Exception ex)
        {
            profile.Succeeded = false;
            profile.ErrorMessage = ex.Message;
        }

        // In real usage, this would be consumed by QueryProfiler
        return new User { Id = userId, Name = "Example User", Email = "user@example.com" };
    }
}
```

## QueryProfiler

The `QueryProfiler` class provides a thread-safe in-process query profiler. It stores profiles in a bounded ring-buffer, ensuring predictable memory usage under sustained load. You can use it to monitor and analyze the performance of your database queries.

### Example Usage

```csharp
public class Program
{
    public static async Task Main()
    {
        var profiler = new QueryProfiler();
        profiler.IsEnabled = true;

        var result = await profiler.ProfileAsync<int>("SELECT * FROM users", async () => await Task.FromResult(42));
        Console.WriteLine($"Result: {result}");

        var profiles = profiler.GetProfiles();
        foreach (var profile in profiles)
        {
            Console.WriteLine($"Query: {profile.Query}, Duration: {profile.Duration}, Succeeded: {profile.Succeeded}");
        }

        var summary = profiler.GetSummary();
        Console.WriteLine($"Total Queries: {summary.TotalQueries}, Total Duration: {summary.TotalDuration}, Average Duration: {summary.AverageDuration}");

        profiler.Clear();
    }
}
```

## IBackgroundJob

The `IBackgroundJob` interface defines a contract for background job execution with support for scheduling, execution tracking, and error handling. It enables asynchronous processing outside the request pipeline with configurable retry logic, timeouts, and execution constraints.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;

public class CleanupJob : IBackgroundJob
{
    public string JobId => "cleanup_job";
    public string Name => "Database Cleanup";
    public string Description => "Removes old records from the database";

    public Task ExecuteAsync()
    {
        Console.WriteLine("Cleaning up old records...");
        // Your cleanup logic here
        return Task.CompletedTask;
    }

    public bool CanExecute()
    {
        // Only run if database is available
        return true;
    }

    public Task OnFailureAsync(Exception ex)
    {
        Console.WriteLine($"Cleanup job failed: {ex.Message}");
        return Task.CompletedTask;
    }
}

// Usage with JobScheduleConfig
var job = new CleanupJob();
var config = new JobScheduleConfig
{
    RunOnStartup = false,
    Interval = TimeSpan.FromHours(1),
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromMinutes(5),
    ExecutionTimeout = TimeSpan.FromMinutes(30),
    Enabled = true
};

// Execute the job
await job.ExecuteAsync();
```

## JobScheduler

The `JobScheduler` class provides a thread-safe scheduler for executing and managing background jobs with support for both interval-based and cron-based scheduling. It handles job registration, execution with retry logic, execution history tracking, and graceful shutdown. The scheduler is designed for distributed scenarios and maintains bounded execution history to prevent memory leaks.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;

// Create a job
public class ReportGenerationJob : IBackgroundJob
{
public string JobId => "report_generation";
public string Name => "Report Generation";
public string Description => "Generates daily reports";

public async Task ExecuteAsync()
{
Console.WriteLine("Generating reports...");
// Report generation logic
await Task.Delay(100);
}

public bool CanExecute() => true;
public Task OnFailureAsync(Exception ex) => Task.CompletedTask;
}

// Configure the scheduler
var scheduler = new JobScheduler();

// Register jobs with different scheduling strategies
scheduler.Register(
new ReportGenerationJob(),
new JobScheduleConfig
{
RunOnStartup = true,
Interval = TimeSpan.FromHours(1),
MaxRetries = 3,
RetryDelay = TimeSpan.FromSeconds(30),
ExecutionTimeout = TimeSpan.FromMinutes(10),
Enabled = true
}
);

scheduler.Register(
new CleanupJob(),
new JobScheduleConfig
{
RunOnStartup = false,
CronExpression = "0 2 * * *", // Run at 2 AM daily
MaxRetries = 2,
RetryDelay = TimeSpan.FromMinutes(2),
ExecutionTimeout = TimeSpan.FromMinutes(5),
Enabled = true
}
);

// Start the scheduler
await scheduler.StartAsync();

// Monitor execution history
var recentExecutions = scheduler.GetRecentExecutions(50);
foreach (var execution in recentExecutions)
{
Console.WriteLine($"Job {execution.JobId} executed at {execution.StartTime}: {(execution.Success ? "SUCCESS" : "FAILED")}");
}

// Get history for a specific job
var jobHistory = scheduler.GetExecutionHistory("report_generation");

// Execute a job manually
var result = await scheduler.ExecuteJobAsync(
new ReportGenerationJob(),
new JobScheduleConfig { MaxRetries = 3 }
);

Console.WriteLine($"Execution result: {result.Success}, Duration: {result.Duration}");

// Stop the scheduler when application shuts down
await scheduler.StopAsync();
```

## QueryProfile

The `QueryProfile` class represents a single captured profiling record for a database query execution. It contains metadata about the query including the SQL statement, execution parameters, timing information, success status, and caller context. This type is typically consumed through the `QueryProfiler` class which aggregates multiple profiles into a `QueryProfilerSummary`.








### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

public class UserRepository
{
    private readonly QueryProfiler _profiler = new QueryProfiler();

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var profile = new QueryProfile
        {
            Query = "SELECT id, name, email FROM users WHERE id = @id",
            Parameters = new Dictionary<string, object> { ["@id"] = userId },
            ExecutedAt = DateTime.UtcNow,
            CallerMemberName = nameof(GetUserByIdAsync)
        };

        try
        {
            // Simulate query execution
            await Task.Delay(5);
            profile.Duration = TimeSpan.FromMilliseconds(5);
            profile.RowsAffected = 1;
            profile.Succeeded = true;
        }
        catch (Exception ex)
        {
            profile.Succeeded = false;
            profile.ErrorMessage = ex.Message;
        }

        // In real usage, this would be consumed by QueryProfiler
        return new User { Id = userId, Name = "Example User", Email = "user@example.com" };
    }
}
```

## DataCleanupJob

The `DataCleanupJob` is a background job that maintains database health by removing old audit logs, expired sessions, and soft-deleted records. It operates on a configurable schedule, allowing for fine-tuned control over retention periods, batch sizes for cleanup, and optional database index rebuilding.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;
using DotnetMicroOrm.Data;

public class CleanupTask
{
    public async Task RunCleanupAsync(IDatabaseContext dbContext)
    {
        var config = new DataCleanupConfig
        {
            AuditLogRetentionDays = 30,
            DeletedRecordRetentionDays = 15,
            CleanupAuditLogs = true,
            CleanupSoftDeletedRecords = true,
            CleanupTemporaryData = true,
            RebuildIndexes = false,
            BatchSize = 500
        };

        var job = new DataCleanupJob(dbContext, config);

        if (job.CanExecute())
        {
            try
            {
                await job.ExecuteAsync();
            }
            catch (Exception ex)
            {
                await job.OnFailureAsync(ex);
            }
        }
    }
}
```

## PipelineBuilder

The `PipelineBuilder` class constructs a middleware pipeline for processing requests through a sequence of middleware components. It enables composing multiple middleware in a specific order, with support for both sequential execution and custom ordering via the `Order` property on middleware. The pipeline executes middleware in FIFO order, allowing for flexible request/response processing patterns.

### Example Usage

```csharp
using DotnetMicroOrm.Pipeline;
using DotnetMicroOrm.Middleware;

// Define custom middleware
public class LoggingMiddleware : IMiddleware
{
    public int Order => 1; // Lower order executes first
    
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        Console.WriteLine($"Before middleware execution at {DateTime.UtcNow}");
        await next(context);
        Console.WriteLine("After middleware execution");
    }
}

public class AuthMiddleware : IMiddleware
{
    public int Order => 2;
    
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        Console.WriteLine("Authenticating request...");
        await next(context);
    }
}

// Build and execute a pipeline
var builder = new PipelineBuilder();

// Add middleware to the pipeline
builder.Use(new LoggingMiddleware());
builder.Use(new AuthMiddleware());

// Alternatively, add multiple middleware at once
var additionalMiddlewares = new IMiddleware[]
{
    new ErrorHandlingMiddleware(),
    new MetricsMiddleware()
};
builder.UseAll(additionalMiddlewares);

// Build the pipeline delegate
var pipeline = builder.Build();

// Create a context and execute the pipeline
var context = new MiddlewareContext
{
    Items = new Dictionary<string, object>(),
    Request = new HttpRequestMessage(),
    Response = null
};

await builder.ExecuteAsync(context);

// Inspect the ordered middleware
var orderedMiddlewares = builder.GetOrdered();
Console.WriteLine($"Pipeline contains {builder.Count} middleware components");

// Clear the pipeline when needed
builder.Clear();
```
```