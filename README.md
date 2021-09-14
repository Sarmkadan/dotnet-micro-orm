// ... (rest of the README content remains the same)

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

// ... (rest of the README content remains the same)

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
