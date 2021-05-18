# AuditServiceBenchmarksExtensions

Provides extension methods for configuring and benchmarking audit service operations in the `dotnet-micro-orm` project. These methods facilitate performance testing of bulk insert and update scenarios while ensuring consistent setup and warmup procedures.

## API

### WithDefaultConfig

**Purpose**: Configures an `AuditServiceBenchmarks` instance with default settings for benchmarking.

**Parameters**: None.

**Return Value**: Returns a configured `AuditServiceBenchmarks` instance with default parameters.

**Exceptions**: Does not throw exceptions under normal operation.

---

### WarmupAsync

**Purpose**: Executes warmup operations to stabilize the audit service before performance measurements.

**Parameters**: 
- `instance` (`AuditServiceBenchmarks`): The benchmark instance to warm up.

**Return Value**: `Task` representing the asynchronous warmup operation.

**Exceptions**: Throws exceptions if warmup operations fail due to database connectivity issues or invalid configurations.

---

### MeasureBulkInsertAsync

**Purpose**: Measures the performance of bulk insert operations on the audit service.

**Parameters**: 
- `instance` (`AuditServiceBenchmarks`): The benchmark instance to use.
- `data` (`IEnumerable<AuditRecord>`): The data to insert.

**Return Value**: `Task` representing the asynchronous measurement operation.

**Exceptions**: Throws exceptions if bulk insert operations fail due to database errors or data validation issues.

---

### MeasureBulkUpdateAsync

**Purpose**: Measures the performance of bulk update operations on the audit service.

**Parameters**: 
- `instance` (`AuditServiceBenchmarks`): The benchmark instance to use.
- `data` (`IEnumerable<AuditRecord>`): The data to update.

**Return Value**: `Task` representing the asynchronous measurement operation.

**Exceptions**: Throws exceptions if bulk update operations fail due to database errors or data validation issues.

## Usage

```csharp
var benchmark = AuditServiceBenchmarks.WithDefaultConfig();
await benchmark.WarmupAsync();
await benchmark.MeasureBulkInsertAsync(auditRecords);
```

```csharp
var benchmark = AuditServiceBenchmarks.WithDefaultConfig();
await benchmark.WarmupAsync();
await benchmark.MeasureBulkUpdateAsync(existingRecords);
```

## Notes

- All methods are static and should not be inherited or overridden.
- Concurrent execution of these methods may lead to inconsistent results if the underlying audit service is not thread-safe.
- The `WarmupAsync` method must be called before `MeasureBulkInsertAsync` or `MeasureBulkUpdateAsync` to ensure accurate performance metrics.
- Exceptions thrown during async operations indicate critical failures in database interactions or data integrity and should be handled appropriately.
