# QueryPlanCacheOptions
The `QueryPlanCacheOptions` type is designed to manage the caching of query plans in the `dotnet-micro-orm` project. It provides a set of options and methods to control the caching behavior, including setting the cache capacity, default time-to-live, and retrieving or storing query plans. This type is essential for optimizing the performance of database queries by reducing the overhead of query plan analysis.

## API
* `public int Capacity`: Gets the maximum number of entries in the cache.
* `public TimeSpan DefaultTtl`: Gets the default time-to-live for cache entries.
* `public QueryPlanCache`: Gets the underlying query plan cache.
* `public Task<QueryPlan?> GetPlanAsync`: Retrieves a query plan from the cache asynchronously. Returns `null` if the plan is not found.
* `public Task StorePlanAsync`: Stores a query plan in the cache asynchronously.
* `public async Task<QueryPlan> GetOrAnalyzeAsync`: Retrieves a query plan from the cache or analyzes a new plan if it's not found.
* `public Task InvalidateAsync`: Invalidates a query plan in the cache asynchronously.
* `public Task ClearAsync`: Clears all entries from the cache asynchronously.
* `public Task<(long Entries, long Hits, long Misses)> GetStatisticsAsync`: Retrieves cache statistics, including the number of entries, hits, and misses.
* `public static string ComputeFingerprint`: Computes a fingerprint for a query plan.
* `public ValueTask DisposeAsync`: Disposes of the cache asynchronously.
* `public QueryPlan Plan`: Gets the query plan associated with the cache entry.
* `public DateTime ExpiresAt`: Gets the expiration date and time of the cache entry.
* `public DateTime LastAccessedAt`: Gets the date and time the cache entry was last accessed.
* `public bool IsExpired`: Gets a value indicating whether the cache entry has expired.

## Usage
The following examples demonstrate how to use the `QueryPlanCacheOptions` type:
```csharp
// Example 1: Retrieving a query plan from the cache
var cacheOptions = new QueryPlanCacheOptions();
var queryPlan = await cacheOptions.GetPlanAsync("SELECT * FROM Customers");
if (queryPlan != null)
{
    Console.WriteLine("Query plan found in cache");
}
else
{
    Console.WriteLine("Query plan not found in cache");
}

// Example 2: Storing a query plan in the cache
var cacheOptions = new QueryPlanCacheOptions();
var queryPlan = new QueryPlan("SELECT * FROM Customers");
await cacheOptions.StorePlanAsync(queryPlan);
Console.WriteLine("Query plan stored in cache");
```

## Notes
When using the `QueryPlanCacheOptions` type, consider the following edge cases and thread-safety remarks:
* The cache is not thread-safe by default. If multiple threads access the cache concurrently, consider using synchronization mechanisms to ensure data integrity.
* The `GetOrAnalyzeAsync` method may throw an exception if the query plan analysis fails.
* The `InvalidateAsync` method may throw an exception if the cache entry is not found.
* The `ClearAsync` method may throw an exception if the cache is already cleared.
* The `GetStatisticsAsync` method may return inaccurate statistics if the cache is modified concurrently.
* The `ComputeFingerprint` method may return a different fingerprint for the same query plan if the plan is modified.
* The `DisposeAsync` method should be called when the cache is no longer needed to release system resources.
