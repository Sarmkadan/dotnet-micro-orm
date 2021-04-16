using BenchmarkDotNet.Attributes;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Benchmarks for expression compilation and caching performance
/// </summary>
[MemoryDiagnoser]
public class ExpressionAndCachingBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IRepository<BenchmarkTestEntity> _repository;
    private IUnitOfWork _unitOfWork;
    private List<BenchmarkTestEntity> _testEntities;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serviceProvider = BenchmarkSetup.GetServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IRepository<BenchmarkTestEntity>>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Clean up any existing test data
        await BenchmarkSetup.CleanupDatabaseAsync();

        // Create test data
        _testEntities = new List<BenchmarkTestEntity>();
        for (int i = 1; i <= 1000; i++)
        {
            _testEntities.Add(new BenchmarkTestEntity
            {
                Name = "Entity " + i,
                Value = i,
                Description = "Test entity"
            });
        }
        await _repository.AddRangeAsync(_testEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await BenchmarkSetup.CleanupDatabaseAsync();
    }

    // ====================================================================
    // Expression Compilation Benchmarks
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("ExpressionCompilation")]
    public async Task ExpressionCompile_FirstCall()
    {
        // First call to compile the expression
        var entities = await _repository.GetAsync(e => e.Value > 500);
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("ExpressionCompilation")]
    public async Task ExpressionCompile_SubsequentCall()
    {
        // Subsequent calls should use cached compiled expression
        var entities = await _repository.GetAsync(e => e.Value > 500);
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("ExpressionCompilation")]
    public async Task ComplexExpression_FirstCall()
    {
        // Complex expression with multiple predicates
        var entities = await _repository.GetAsync(
            e => e.Value > 100 && e.Value < 900 && e.Name.Contains("Entity") && e.Description != null
        );
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("ExpressionCompilation")]
    public async Task ComplexExpression_SubsequentCall()
    {
        // Complex expression - subsequent call
        var entities = await _repository.GetAsync(
            e => e.Value > 100 && e.Value < 900 && e.Name.Contains("Entity") && e.Description != null
        );
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    // ====================================================================
    // Query Performance with Different Predicates
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("QueryPerformance")]
    public async Task SimplePredicateQuery()
    {
        var entities = await _repository.GetAsync(e => e.Value > 500);
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryPerformance")]
    public async Task MultiplePredicateQuery()
    {
        var entities = await _repository.GetAsync(
            e => e.Value > 100 && e.Value < 900 && e.Name.StartsWith("Entity")
        );
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryPerformance")]
    public async Task RangeQuery()
    {
        var entities = await _repository.GetAsync(e => e.Value >= 100 && e.Value <= 900);
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryPerformance")]
    public async Task OrderByQuery()
    {
        var entities = await _repository.GetAsync(
            e => e.Value > 0,
            orderBy: e => e.Value,
            descending: false
        );
        if (entities.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryPerformance")]
    public async Task PagedQuery()
    {
        var result = await _repository.GetPagedAsync(2, 50);
        if (result.Count < 1)
            throw new InvalidOperationException("No entities found");
    }

    // ====================================================================
    // Count Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("CountOperations")]
    public async Task CountAll()
    {
        var count = await _repository.CountAsync();
        if (count < 1)
            throw new InvalidOperationException("Count should be greater than 0");
    }

    [Benchmark]
    [BenchmarkCategory("CountOperations")]
    public async Task CountWithPredicate()
    {
        var count = await _repository.CountAsync(e => e.Value > 500);
        if (count < 1)
            throw new InvalidOperationException("Count should be greater than 0");
    }

    [Benchmark]
    [BenchmarkCategory("CountOperations")]
    public async Task AnyOperation()
    {
        var exists = await _repository.ExistsAsync(e => e.Value > 0);
        if (!exists)
            throw new InvalidOperationException("Should have entities");
    }

    // ====================================================================
    // FirstOrDefault Operations
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("FirstOrDefault")]
    public async Task FirstOrDefaultWithPredicate()
    {
        var entity = await _repository.FirstOrDefaultAsync(e => e.Value == 500);
        if (entity == null)
            throw new InvalidOperationException("Entity should exist");
    }

    [Benchmark]
    [BenchmarkCategory("FirstOrDefault")]
    public async Task FirstOrDefaultNoMatch()
    {
        var entity = await _repository.FirstOrDefaultAsync(e => e.Value > 10000);
        if (entity != null)
            throw new InvalidOperationException("Entity should not exist");
    }
}
