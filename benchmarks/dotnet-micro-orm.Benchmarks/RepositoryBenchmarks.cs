using BenchmarkDotNet.Attributes;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Benchmarks for repository operations - CRUD operations and queries
/// </summary>
[MemoryDiagnoser]
public class RepositoryBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IRepository<BenchmarkTestEntity> _repository;
    private IUnitOfWork _unitOfWork;
    private BenchmarkTestEntity _testEntity;
    private List<BenchmarkTestEntity> _testEntities;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serviceProvider = BenchmarkSetup.GetServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IRepository<BenchmarkTestEntity>>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Clean up any existing test data
        await BenchmarkSetup.CleanupDatabaseAsync();

        // Create test entities
        _testEntities = new List<BenchmarkTestEntity>();
        for (int i = 1; i <= 100; i++)
        {
            _testEntities.Add(await BenchmarkSetup.CreateTestEntityAsync(_repository, i));
        }
        await _unitOfWork.SaveChangesAsync();

        // Get one entity for single operations
        _testEntity = await _repository.GetByIdAsync(1);
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await BenchmarkSetup.CleanupDatabaseAsync();
    }

    // ====================================================================
    // Single Entity Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("SingleEntity")]
    public async Task GetByIdAsync()
    {
        var entity = await _repository.GetByIdAsync(1);
        if (entity == null)
            throw new InvalidOperationException("Entity not found");
    }

    [Benchmark]
    [BenchmarkCategory("SingleEntity")]
    public async Task AddAsync()
    {
        var entity = new BenchmarkTestEntity
        {
            Name = "New Entity " + Guid.NewGuid().ToString()[..8],
            Value = new Random().Next(1, 10000),
            Description = "Newly created entity"
        };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("SingleEntity")]
    public async Task UpdateAsync()
    {
        _testEntity.Value = new Random().Next(1, 10000);
        await _repository.UpdateAsync(_testEntity);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("SingleEntity")]
    public async Task DeleteAsync()
    {
        await _repository.DeleteAsync(_testEntity.Id);
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Batch Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BatchOperations")]
    public async Task AddRangeAsync_100_Entities()
    {
        var newEntities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 100; i++)
        {
            newEntities.Add(new BenchmarkTestEntity
            {
                Name = "Batch Entity " + i,
                Value = i * 100,
                Description = "Batch insert test"
            });
        }
        await _repository.AddRangeAsync(newEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchOperations")]
    public async Task UpdateRangeAsync_100_Entities()
    {
        for (int i = 0; i < 100; i++)
        {
            _testEntities[i].Value = i * 1000;
        }
        await _repository.UpdateRangeAsync(_testEntities.Take(100).ToList());
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchOperations")]
    public async Task DeleteRangeAsync_100_Entities()
    {
        await _repository.DeleteRangeAsync(_testEntities.Take(100).ToList());
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Query Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("QueryOperations")]
    public async Task GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryOperations")]
    public async Task GetByPredicateAsync()
    {
        var entities = await _repository.GetAsync(e => e.Value > 5000);
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryOperations")]
    public async Task CountAsync()
    {
        var count = await _repository.CountAsync();
        if (count < 1)
            throw new InvalidOperationException("Count should be greater than 0");
    }

    [Benchmark]
    [BenchmarkCategory("QueryOperations")]
    public async Task AnyAsync()
    {
        var exists = await _repository.ExistsAsync(e => e.Value > 0);
        if (!exists)
            throw new InvalidOperationException("Should have entities");
    }

    [Benchmark]
    [BenchmarkCategory("QueryOperations")]
    public async Task GetPagedAsync()
    {
        var result = await _repository.GetPagedAsync(1, 20);
        if (result.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    // ====================================================================
    // Complex Query Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("ComplexQueries")]
    public async Task GetWithOrdering()
    {
        var matches = await _repository.GetAsync(e => e.Value > 0);
        var entities = matches.OrderBy(e => e.Value).Skip(0).Take(50).ToList();
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("ComplexQueries")]
    public async Task GetWithMultiplePredicates()
    {
        var entities = await _repository.GetAsync(
            e => e.Value > 1000 && e.Value < 9000 && e.Name.Contains("Entity")
        );
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    // ====================================================================
    // Advanced Query Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("AdvancedQueries")]
    public async Task GetWithComplexSpecification()
    {
        var matches = await _repository.GetAsync(
            e => e.Value > 100 && e.Value < 9000 && e.Name.StartsWith("Entity"));
        var entities = matches
            .OrderBy(e => e.Value)
            .ThenByDescending(e => e.Name)
            .Skip(10)
            .Take(50)
            .ToList();
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("AdvancedQueries")]
    public async Task GetPagedPerformance()
    {
        // Test different page sizes
        for (int page = 1; page <= 10; page++)
        {
            var result = await _repository.GetPagedAsync(page, 20);
            if (result.Count < 1 && page == 1)
                throw new InvalidOperationException("No entities found on first page");
        }
    }

    [Benchmark]
    [BenchmarkCategory("AdvancedQueries")]
    public async Task FirstOrDefaultPerformance()
    {
        var entity = await _repository.FirstOrDefaultAsync(e => e.Value == 5000);
        // Entity may not exist, that's okay for benchmark
    }
}
