# RepositoryBenchmarks

`RepositoryBenchmarks` provides a benchmarking suite designed for measuring the performance of repository-based data access operations within the `dotnet-micro-orm` framework. It utilizes asynchronous methods to simulate realistic database workloads, facilitating accurate performance profiling of CRUD operations, batch processing, and complex query execution scenarios.

## API

*   **GlobalSetup**
    *   Purpose: Initializes the database context and required resources for benchmarking.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
    *   Throws: Exceptions related to database connectivity or resource allocation.
*   **GlobalCleanup**
    *   Purpose: Releases database resources after benchmarking completes.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
    *   Throws: Exceptions related to resource disposal failures.
*   **GetByIdAsync**
    *   Purpose: Measures the performance of retrieving a single entity by its identifier.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **AddAsync**
    *   Purpose: Measures the performance of inserting a single entity.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **UpdateAsync**
    *   Purpose: Measures the performance of updating a single entity.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **DeleteAsync**
    *   Purpose: Measures the performance of removing a single entity.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **AddRangeAsync_100_Entities**
    *   Purpose: Measures the performance of bulk inserting 100 entities.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **UpdateRangeAsync_100_Entities**
    *   Purpose: Measures the performance of bulk updating 100 entities.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **DeleteRangeAsync_100_Entities**
    *   Purpose: Measures the performance of bulk deleting 100 entities.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetAllAsync**
    *   Purpose: Measures the performance of retrieving all entities.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetByPredicateAsync**
    *   Purpose: Measures the performance of retrieving entities filtered by a predicate.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **CountAsync**
    *   Purpose: Measures the performance of executing a count query.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **AnyAsync**
    *   Purpose: Measures the performance of checking for entity existence.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetPagedAsync**
    *   Purpose: Measures the performance of paged entity retrieval.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetWithOrdering**
    *   Purpose: Measures the performance of retrieving entities with explicit ordering.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetWithMultiplePredicates**
    *   Purpose: Measures the performance of queries utilizing multiple combined predicates.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetWithComplexSpecification**
    *   Purpose: Measures the performance of complex specification-based queries.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **GetPagedPerformance**
    *   Purpose: Measures the overhead and latency of paged data retrieval.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.
*   **FirstOrDefaultPerformance**
    *   Purpose: Measures the latency for retrieving the first matching entity or the default value.
    *   Parameters: None.
    *   Return value: `Task` representing the asynchronous operation.

## Usage

```csharp
// Example 1: Executing benchmarks using BenchmarkDotNet
using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        // Run all benchmarks defined in the RepositoryBenchmarks class
        var summary = BenchmarkRunner.Run<RepositoryBenchmarks>();
    }
}
```

```csharp
// Example 2: Manual execution of benchmark methods for diagnostic profiling
var benchmarks = new RepositoryBenchmarks();

// Perform necessary initialization
await benchmarks.GlobalSetup();

try
{
    // Execute specific benchmark operation
    await benchmarks.AddAsync();
    await benchmarks.GetByIdAsync();
}
finally
{
    // Ensure cleanup of resources
    await benchmarks.GlobalCleanup();
}
```

## Notes

*   **Thread Safety:** These benchmark methods are intended to be executed by a benchmarking harness (e.g., BenchmarkDotNet), which typically serializes their execution to ensure measurement consistency. The underlying `dotnet-micro-orm` repository implementation should be evaluated independently for thread safety if it is to be utilized in multi-threaded production environments.
*   **Environmental Factors:** Benchmark results are sensitive to infrastructure conditions, including database server latency, local network topology, and available system resources. For consistent results, execute in a controlled environment.
*   **Workload Configuration:** The range-based benchmarks (e.g., `AddRangeAsync_100_Entities`) are configured for a static workload of 100 entities. Changes to the repository's internal data constraints or schema may necessitate updates to these benchmark parameters to remain representative.
