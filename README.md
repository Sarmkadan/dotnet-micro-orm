// ... (rest of the README content remains the same)

## BenchmarkSetup

The `BenchmarkSetup` class provides a set of static methods for initializing and cleaning up the database used for benchmarks. It also provides a method for creating test entities.

### Example Usage

```csharp
var serviceProvider = BenchmarkSetup.GetServiceProvider();
await BenchmarkSetup.InitializeDatabaseAsync();
var repository = serviceProvider.GetService<IRepository<BenchmarkTestEntity>>();
var entity = await BenchmarkSetup.CreateTestEntityAsync(repository);
Console.WriteLine(entity.Name);
await BenchmarkSetup.CleanupDatabaseAsync();
```

// ... (rest of the README content remains the same)
