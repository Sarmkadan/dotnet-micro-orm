// ... (rest of the README content remains the same)

## RepositoryBenchmarks

The `RepositoryBenchmarks` class provides a set of benchmarks for measuring the performance of repository operations, including CRUD operations and queries.

### Example Usage

```csharp
var benchmark = new RepositoryBenchmarks();
await benchmark.GlobalSetup();

// Run benchmarks
await benchmark.GetByIdAsync();
await benchmark.AddAsync();
await benchmark.UpdateAsync();
await benchmark.DeleteAsync();
await benchmark.AddRangeAsync_100_Entities();
await benchmark.UpdateRangeAsync_100_Entities();
await benchmark.DeleteRangeAsync_100_Entities();
await benchmark.GetAllAsync();
await benchmark.GetByPredicateAsync();
await benchmark.CountAsync();
await benchmark.AnyAsync();
await benchmark.GetPagedAsync();
await benchmark.GetWithOrdering();
await benchmark.GetWithMultiplePredicates();
await benchmark.GetWithComplexSpecification();
await benchmark.GetPagedPerformance();
await benchmark.FirstOrDefaultPerformance();

// Clean up
await benchmark.GlobalCleanup();
```

## ComparisonBenchmarks
The `ComparisonBenchmarks` class provides a set of benchmarks for comparing the performance of DotnetMicroOrm with raw ADO.NET. It includes benchmarks for CRUD operations, queries, and batch operations. Here's an example of how to use it:
```csharp
var comparisonBenchmarks = new ComparisonBenchmarks();
await comparisonBenchmarks.GlobalSetup();

// Run raw ADO.NET benchmarks
await comparisonBenchmarks.RawADO_GetById();
await comparisonBenchmarks.RawADO_GetAll();
await comparisonBenchmarks.RawADO_Add();
await comparisonBenchmarks.RawADO_AddRange();

// Run DotnetMicroOrm benchmarks
await comparisonBenchmarks.DotnetMicroOrm_GetById();
await comparisonBenchmarks.DotnetMicroOrm_GetAll();
await comparisonBenchmarks.DotnetMicroOrm_Add();
await comparisonBenchmarks.DotnetMicroOrm_AddRange();

// Run query benchmarks
await comparisonBenchmarks.Query_GetByValueGreaterThan();
await comparisonBenchmarks.Query_GetWithOrdering();
await comparisonBenchmarks.Query_Count();
await comparisonBenchmarks.Query_Any();

// Run batch operation benchmarks
await comparisonBenchmarks.BatchInsert_100_Entities();
await comparisonBenchmarks.BatchUpdate_100_Entities();
await comparisonBenchmarks.BatchDelete_100_Entities();

// Clean up
await comparisonBenchmarks.GlobalCleanup();
```

// ... (rest of the README content remains the same)
