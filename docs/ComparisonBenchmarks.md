# ComparisonBenchmarks

The `ComparisonBenchmarks` class provides a suite of performance benchmarking methods designed to evaluate the `DotnetMicroOrm` library's efficiency against direct ADO.NET implementations. This class serves as a controlled environment for measuring execution speed, latency, and resource utilization across standard database operations, ensuring rigorous performance analysis of CRUD operations, batch processing, and complex queries.

## API

### Lifecycle Methods

*   **`public async Task GlobalSetup()`**
    *   **Purpose:** Prepares the environment for benchmarking, typically by initializing database connections, creating necessary schema objects, or populating test data.
    *   **Parameters:** None.
    *   **Return Value:** `Task`.
    *   **Throws:** `DbException` or related database connection errors if the setup fails.

*   **`public async Task GlobalCleanup()`**
    *   **Purpose:** Performs necessary teardown after benchmarks complete, such as closing connections and cleaning up test tables or temporary records.
    *   **Parameters:** None.
    *   **Return Value:** `Task`.
    *   **Throws:** `DbException` if cleanup operations fail.

### Benchmark Methods

All benchmark methods below share the same signature:
*   **Parameters:** None.
*   **Return Value:** `Task`.
*   **Throws:** `DbException` or application-specific errors if database operations fail.

*   **`public async Task RawADO_GetById()`**
    *   **Purpose:** Measures the time taken to retrieve a single entity by its identifier using raw ADO.NET.

*   **`public async Task RawADO_GetAll()`**
    *   **Purpose:** Measures the time taken to retrieve all entities from a table using raw ADO.NET.

*   **`public async Task RawADO_Add()`**
    *   **Purpose:** Measures the time taken to insert a single entity using raw ADO.NET.

*   **`public async Task RawADO_AddRange()`**
    *   **Purpose:** Measures the time taken to insert a range of entities using raw ADO.NET.

*   **`public async Task DotnetMicroOrm_GetById()`**
    *   **Purpose:** Measures the time taken to retrieve a single entity by its identifier using `DotnetMicroOrm`.

*   **`public async Task DotnetMicroOrm_GetAll()`**
    *   **Purpose:** Measures the time taken to retrieve all entities from a table using `DotnetMicroOrm`.

*   **`public async Task DotnetMicroOrm_Add()`**
    *   **Purpose:** Measures the time taken to insert a single entity using `DotnetMicroOrm`.

*   **`public async Task DotnetMicroOrm_AddRange()`**
    *   **Purpose:** Measures the time taken to insert a range of entities using `DotnetMicroOrm`.

*   **`public async Task Query_GetByValueGreaterThan()`**
    *   **Purpose:** Measures performance of query filtering based on a "greater than" numeric comparison.

*   **`public async Task Query_GetWithOrdering()`**
    *   **Purpose:** Measures performance of query execution involving sorting/ordering result sets.

*   **`public async Task Query_Count()`**
    *   **Purpose:** Measures performance of retrieving a record count from the database.

*   **`public async Task Query_Any()`**
    *   **Purpose:** Measures performance of an existence check query.

*   **`public async Task BatchInsert_100_Entities()`**
    *   **Purpose:** Measures performance of inserting 100 entities as a single batch operation.

*   **`public async Task BatchUpdate_100_Entities()`**
    *   **Purpose:** Measures performance of updating 100 entities as a single batch operation.

*   **`public async Task BatchDelete_100_Entities()`**
    *   **Purpose:** Measures performance of deleting 100 entities as a single batch operation.

## Usage

### 1. Running via BenchmarkDotNet
The primary use case is executing the class via the BenchmarkDotNet harness.

```csharp
using BenchmarkDotNet.Running;

// Run all benchmarks defined in the class
var summary = BenchmarkRunner.Run<ComparisonBenchmarks>();
```

### 2. Manual Invocation in a Harness
While intended for benchmarking frameworks, methods can be invoked programmatically for validation.

```csharp
var benchmark = new ComparisonBenchmarks();

// Perform setup before running individual tests
await benchmark.GlobalSetup();

// Execute a specific benchmark
await benchmark.DotnetMicroOrm_GetAll();

// Ensure cleanup is always called
await benchmark.GlobalCleanup();
```

## Notes

*   **Thread Safety:** Instances of `ComparisonBenchmarks` are **not thread-safe**. Benchmarks should be executed sequentially, as they rely on a shared database state managed through `GlobalSetup` and `GlobalCleanup`.
*   **Database State:** For accurate comparisons, the database state must be consistent across benchmark runs. It is assumed that `GlobalSetup` establishes a predictable initial state.
*   **Connectivity:** All methods assume an active, valid, and reachable database connection. Network latency or database service unavailability will cause benchmark methods to throw exceptions, which will invalidate the benchmark results.
