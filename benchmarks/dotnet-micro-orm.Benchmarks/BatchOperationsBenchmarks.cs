using BenchmarkDotNet.Attributes;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Benchmarks for batch operations - insert, update, delete large datasets
/// </summary>
[MemoryDiagnoser]
public class BatchOperationsBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IRepository<BenchmarkTestEntity> _repository;
    private IUnitOfWork _unitOfWork;
    private List<BenchmarkTestEntity> _batchEntities;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serviceProvider = BenchmarkSetup.GetServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IRepository<BenchmarkTestEntity>>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Clean up any existing test data
        await BenchmarkSetup.CleanupDatabaseAsync();

        // Create test data for batch operations
        _batchEntities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 1000; i++)
        {
            _batchEntities.Add(new BenchmarkTestEntity
            {
                Name = "Batch Entity " + i,
                Value = i * 10,
                Description = "Batch operation test entity"
            });
        }
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await BenchmarkSetup.CleanupDatabaseAsync();
    }

    // ====================================================================
    // Batch Insert Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BatchInsert")]
    public async Task AddRangeAsync_1000_Entities()
    {
        await _repository.AddRangeAsync(_batchEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchInsert")]
    public async Task AddRangeAsync_5000_Entities()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 5000; i++)
        {
            entities.Add(new BenchmarkTestEntity
            {
                Name = "Large Batch Entity " + i,
                Value = i * 10,
                Description = "Large batch insert test"
            });
        }
        await _repository.AddRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Batch Update Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BatchUpdate")]
    public async Task UpdateRangeAsync_1000_Entities()
    {
        // Update all entities
        foreach (var entity in _batchEntities)
        {
            entity.Value = entity.Value * 2;
            entity.Description = "Updated: " + entity.Description;
        }

        await _repository.UpdateRangeAsync(_batchEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchUpdate")]
    public async Task UpdateRangeAsync_5000_Entities()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 5000; i++)
        {
            entities.Add(new BenchmarkTestEntity
            {
                Id = i + 1,
                Name = "Large Batch Entity " + i,
                Value = i * 100,
                Description = "Updated large batch"
            });
        }
        await _repository.UpdateRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Batch Delete Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BatchDelete")]
    public async Task DeleteRangeAsync_1000_Entities()
    {
        await _repository.DeleteRangeAsync(_batchEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchDelete")]
    public async Task DeleteRangeAsync_5000_Entities()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 5000; i++)
        {
            entities.Add(new BenchmarkTestEntity { Id = i + 1 });
        }
        await _repository.DeleteRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Bulk Operations with Different Batch Sizes
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BulkOperations")]
    public async Task BulkInsert_100_Entities()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 100; i++)
        {
            entities.Add(new BenchmarkTestEntity
            {
                Name = "Bulk Entity " + i,
                Value = i * 10,
                Description = "Bulk insert test"
            });
        }
        await _repository.AddRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BulkOperations")]
    public async Task BulkInsert_10000_Entities()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 10000; i++)
        {
            entities.Add(new BenchmarkTestEntity
            {
                Name = "Very Large Batch Entity " + i,
                Value = i * 10,
                Description = "Very large bulk insert test"
            });
        }
        await _repository.AddRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Complex Batch Operations with Different Data Sizes
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("ComplexBatch")]
    public async Task BatchInsert_1000_Entities_With_Relations()
    {
        var entities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 1000; i++)
        {
            entities.Add(new BenchmarkTestEntity
            {
                Name = "Complex Entity " + i,
                Value = i * 100,
                Description = "Entity with longer description text to test serialization and deserialization performance"
            });
        }
        await _repository.AddRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("ComplexBatch")]
    public async Task BatchUpdate_Complex_Predicate()
    {
        // Update entities based on complex predicate
        var entities = await _repository.GetAsync(e => e.Value > 100 && e.Value < 9000 && e.Name.Contains("Batch"));
        foreach (var entity in entities)
        {
            entity.Description = "Updated: " + entity.Description + " - " + Guid.NewGuid().ToString()[..8];
        }
        await _repository.UpdateRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("ComplexBatch")]
    public async Task BatchDelete_With_Where_Clause()
    {
        // Delete entities matching a specific pattern
        var entities = await _repository.GetAsync(e => e.Name.StartsWith("Batch"));
        await _repository.DeleteRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }
}
