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

// ... (rest of the README content remains the same)
