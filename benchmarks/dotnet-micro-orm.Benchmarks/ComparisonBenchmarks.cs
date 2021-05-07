using BenchmarkDotNet.Attributes;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Benchmarks comparing DotnetMicroOrm with raw ADO.NET and Dapper for performance comparison
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ComparisonBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IRepository<BenchmarkTestEntity> _repository;
    private IUnitOfWork _unitOfWork;
    private SqlConnection _rawConnection;
    private string _connectionString = string.Empty;
    private List<BenchmarkTestEntity> _testEntities;
    private List<BenchmarkTestEntity> _largeBatchEntities;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _connectionString = "Server=localhost;Database=DotnetMicroOrmBenchmarks;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;Pooling=true;Max Pool Size=100;";

        // Setup DotnetMicroOrm
        _serviceProvider = BenchmarkSetup.GetServiceProvider();
        _repository = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IRepository<BenchmarkTestEntity>>(_serviceProvider);
        _unitOfWork = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IUnitOfWork>(_serviceProvider);

        // Create raw ADO.NET connection
        _rawConnection = new SqlConnection(_connectionString);
        await _rawConnection.OpenAsync();

        // Clean up any existing test data
        await BenchmarkSetup.CleanupDatabaseAsync();

        // Create test data
        _testEntities = new List<BenchmarkTestEntity>();
        for (int i = 1; i <= 100; i++)
        {
            _testEntities.Add(new BenchmarkTestEntity
            {
                Name = "Entity " + i,
                Value = i * 100,
                Description = "Test entity for comparison benchmarks"
            });
        }

        _largeBatchEntities = new List<BenchmarkTestEntity>();
        for (int i = 0; i < 1000; i++)
        {
            _largeBatchEntities.Add(new BenchmarkTestEntity
            {
                Name = "Large Entity " + i,
                Value = i * 10,
                Description = "Large batch entity"
            });
        }

        await _repository.AddRangeAsync(_testEntities);
        await _unitOfWork.SaveChangesAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await BenchmarkSetup.CleanupDatabaseAsync();
        _rawConnection?.Close();
        _rawConnection?.Dispose();
    }

    // ====================================================================
    // Raw ADO.NET Benchmarks
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("RawADO")]
    public async Task RawADO_GetById()
    {
        using var cmd = new SqlCommand("SELECT Id, Name, Value, Description, CreatedDate, ModifiedDate FROM BenchmarkTestEntities WHERE Id = @Id", _rawConnection);
        cmd.Parameters.AddWithValue("@Id", 1);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var _ = reader.GetInt32(0); // Id
            var _2 = reader.GetString(1); // Name
            var _3 = reader.GetInt32(2); // Value
            var _4 = reader.IsDBNull(3) ? null : reader.GetString(3); // Description
        }
    }

    [Benchmark]
    [BenchmarkCategory("RawADO")]
    public async Task RawADO_GetAll()
    {
        using var cmd = new SqlCommand("SELECT Id, Name, Value, Description, CreatedDate, ModifiedDate FROM BenchmarkTestEntities", _rawConnection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var _ = reader.GetInt32(0); // Id
        }
    }

    [Benchmark]
    [BenchmarkCategory("RawADO")]
    public async Task RawADO_Add()
    {
        using var cmd = new SqlCommand(
            "INSERT INTO BenchmarkTestEntities (Name, Value, Description, CreatedDate) VALUES (@Name, @Value, @Description, @CreatedDate); SELECT CAST(SCOPE_IDENTITY() AS INT)",
            _rawConnection);
        cmd.Parameters.AddWithValue("@Name", "New Entity Raw ADO");
        cmd.Parameters.AddWithValue("@Value", 9999);
        cmd.Parameters.AddWithValue("@Description", "Raw ADO insert");
        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
    }

    [Benchmark]
    [BenchmarkCategory("RawADO")]
    public async Task RawADO_AddRange()
    {
        using var cmd = new SqlCommand(
            "INSERT INTO BenchmarkTestEntities (Name, Value, Description, CreatedDate) VALUES (@Name1, @Value1, @Description1, @CreatedDate1), (@Name2, @Value2, @Description2, @CreatedDate2), (@Name3, @Value3, @Description3, @CreatedDate3), (@Name4, @Value4, @Description4, @CreatedDate4), (@Name5, @Value5, @Description5, @CreatedDate5)",
            _rawConnection);
        cmd.Parameters.AddWithValue("@Name1", "Batch Entity 1");
        cmd.Parameters.AddWithValue("@Value1", 1);
        cmd.Parameters.AddWithValue("@Description1", "Batch 1");
        cmd.Parameters.AddWithValue("@CreatedDate1", DateTime.UtcNow);

        cmd.Parameters.AddWithValue("@Name2", "Batch Entity 2");
        cmd.Parameters.AddWithValue("@Value2", 2);
        cmd.Parameters.AddWithValue("@Description2", "Batch 2");
        cmd.Parameters.AddWithValue("@CreatedDate2", DateTime.UtcNow);

        cmd.Parameters.AddWithValue("@Name3", "Batch Entity 3");
        cmd.Parameters.AddWithValue("@Value3", 3);
        cmd.Parameters.AddWithValue("@Description3", "Batch 3");
        cmd.Parameters.AddWithValue("@CreatedDate3", DateTime.UtcNow);

        cmd.Parameters.AddWithValue("@Name4", "Batch Entity 4");
        cmd.Parameters.AddWithValue("@Value4", 4);
        cmd.Parameters.AddWithValue("@Description4", "Batch 4");
        cmd.Parameters.AddWithValue("@CreatedDate4", DateTime.UtcNow);

        cmd.Parameters.AddWithValue("@Name5", "Batch Entity 5");
        cmd.Parameters.AddWithValue("@Value5", 5);
        cmd.Parameters.AddWithValue("@Description5", "Batch 5");
        cmd.Parameters.AddWithValue("@CreatedDate5", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync();
    }

    // ====================================================================
    // DotnetMicroOrm Benchmarks (for comparison)
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("DotnetMicroOrm")]
    public async Task DotnetMicroOrm_GetById()
    {
        var entity = await _repository.GetByIdAsync(1);
        if (entity == null) throw new InvalidOperationException("Entity not found");
    }

    [Benchmark]
    [BenchmarkCategory("DotnetMicroOrm")]
    public async Task DotnetMicroOrm_GetAll()
    {
        var entities = await _repository.GetAllAsync();
        if (entities.Count < 1) throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("DotnetMicroOrm")]
    public async Task DotnetMicroOrm_Add()
    {
        var entity = new BenchmarkTestEntity
        {
            Name = "New Entity DotnetMicroOrm",
            Value = 9999,
            Description = "DotnetMicroOrm insert"
        };
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("DotnetMicroOrm")]
    public async Task DotnetMicroOrm_AddRange()
    {
        await _repository.AddRangeAsync(_largeBatchEntities.Take(5).ToList());
        await _unitOfWork.SaveChangesAsync();
    }

    // ====================================================================
    // Query Performance Comparison
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("QueryComparison")]
    public async Task Query_GetByValueGreaterThan()
    {
        var entities = await _repository.GetAsync(e => e.Value > 5000);
        if (entities.Count < 1) throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryComparison")]
    public async Task Query_GetWithOrdering()
    {
        var matches = await _repository.GetAsync(e => e.Value > 0);
        var entities = matches.OrderBy(e => e.Value).Skip(0).Take(50).ToList();
        if (entities.Count < 1) throw new InvalidOperationException("No entities found");
    }

    [Benchmark]
    [BenchmarkCategory("QueryComparison")]
    public async Task Query_Count()
    {
        var count = await _repository.CountAsync();
        if (count < 1) throw new InvalidOperationException("Count should be greater than 0");
    }

    [Benchmark]
    [BenchmarkCategory("QueryComparison")]
    public async Task Query_Any()
    {
        var exists = await _repository.ExistsAsync(e => e.Value > 0);
        if (!exists) throw new InvalidOperationException("Should have entities");
    }

    // ====================================================================
    // Batch Operations Comparison
    // ====================================================================

    [Benchmark]
    [BenchmarkCategory("BatchComparison")]
    public async Task BatchInsert_100_Entities()
    {
        await _repository.AddRangeAsync(_largeBatchEntities.Take(100).ToList());
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchComparison")]
    public async Task BatchUpdate_100_Entities()
    {
        var entities = _testEntities.Take(100).ToList();
        foreach (var entity in entities)
        {
            entity.Value = entity.Value * 2;
        }
        await _repository.UpdateRangeAsync(entities);
        await _unitOfWork.SaveChangesAsync();
    }

    [Benchmark]
    [BenchmarkCategory("BatchComparison")]
    public async Task BatchDelete_100_Entities()
    {
        await _repository.DeleteRangeAsync(_testEntities.Take(100).ToList());
        await _unitOfWork.SaveChangesAsync();
    }
}