# AuditServiceBenchmarks

The `AuditServiceBenchmarks` class provides a set of performance benchmarks for the `IAuditService` implementation within the `dotnet-micro-orm` library. It is designed to evaluate the latency and resource overhead of audit logging for entity insertion and update operations.

## API

### GlobalSetup()
Performs initialization required for the benchmark execution, including setting up the dependency injection container and preparing the test database.
- **Return value:** `Task`
- **Throws:** Throws `InvalidOperationException` if service registration or database initialization fails.

### GlobalCleanup()
Performs teardown of the benchmark environment by cleaning up the test database to ensure consistent state between test runs.
- **Return value:** `Task`
- **Throws:** Throws database-related exceptions if the cleanup operation fails.

### LogInsertAsync()
Benchmarks the performance of the `IAuditService.LogInsertAsync` operation, measuring the time taken to log the creation of a new entity.
- **Return value:** `Task`

### LogUpdateAsync()
Benchmarks the performance of the `IAuditService.LogUpdateAsync` operation, measuring the time taken to log the modification of an existing entity.
- **Return value:** `Task`

## Usage

### Example 1: Running with BenchmarkDotNet

```csharp
using BenchmarkDotNet.Running;
using DotnetMicroOrm.Benchmarks;

// Run all benchmarks in the AuditServiceBenchmarks class
var summary = BenchmarkRunner.Run<AuditServiceBenchmarks>();
```

### Example 2: Manual Lifecycle Management in a Custom Runner

```csharp
using DotnetMicroOrm.Benchmarks;

var benchmarks = new AuditServiceBenchmarks();

// Manually trigger setup
await benchmarks.GlobalSetup();

// Execute operations
await benchmarks.LogInsertAsync();
await benchmarks.LogUpdateAsync();

// Manually trigger cleanup
await benchmarks.GlobalCleanup();
```

## Notes

- **Asynchronous Execution:** All methods in this class are asynchronous and return a `Task`. BenchmarkDotNet handles the asynchronous execution of `[Benchmark]` methods correctly.
- **Thread Safety:** While BenchmarkDotNet can run benchmarks in parallel depending on the configuration, this class is designed to be executed by a single test runner instance. The underlying test database should be configured to handle concurrent operations if parallel execution is enabled.
- **Database State:** The `GlobalSetup` and `GlobalCleanup` methods are responsible for maintaining the test database state. Inaccurate cleanup can lead to skewed benchmark results due to data accumulation over multiple test iterations.
