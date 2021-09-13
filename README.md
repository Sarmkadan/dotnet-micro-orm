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
