using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Setup helper for benchmarks that initializes test data and services
/// </summary>
public static class BenchmarkSetup
{
    private static ServiceProvider? _serviceProvider;
    private static string _connectionString = string.Empty;
    private static bool _databaseInitialized = false;

    public static IServiceProvider GetServiceProvider()
    {
        if (_serviceProvider != null)
            return _serviceProvider;

        // Use a test database
        _connectionString = "Server=localhost;Database=DotnetMicroOrmBenchmarks;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;Pooling=true;Max Pool Size=100;MultipleActiveResultSets=true;";

        var services = new ServiceCollection();

        // Configure DotnetMicroOrm
        services.AddDotnetMicroOrm(_connectionString, DatabaseProvider.SqlServer, options =>
        {
            options.EnableChangeTracking = true;
            options.EnableExpressionCaching = false; // Disable caching for clean benchmarks
            options.CommandTimeout = 30;
        });

        _serviceProvider = services.BuildServiceProvider();
        return _serviceProvider;
    }

    public static async Task InitializeDatabaseAsync()
    {
        if (_databaseInitialized)
            return;

        try
        {
            var serviceProvider = GetServiceProvider();
            var databaseContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDatabaseContext>(serviceProvider);

            // Create database if not exists
            await databaseContext.OpenAsync();

            // Check if tables exist
            var tables = await databaseContext.ExecuteQueryAsync(
                "SELECT name FROM sys.tables WHERE name IN ('Products', 'Categories', 'BenchmarkTestEntities')");

            if (tables.Count < 3) // Products, Categories, BenchmarkTestEntities
            {
                // Create test table for CRUD operations
                await databaseContext.ExecuteNonQueryAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BenchmarkTestEntities')
                    BEGIN
                        CREATE TABLE BenchmarkTestEntities (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Name NVARCHAR(255) NOT NULL,
                            Value INT NOT NULL,
                            Description NVARCHAR(500),
                            CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
                            ModifiedDate DATETIME
                        );

                        CREATE INDEX IX_BenchmarkTestEntities_Name ON BenchmarkTestEntities(Name);
                        CREATE INDEX IX_BenchmarkTestEntities_Value ON BenchmarkTestEntities(Value);
                    END");

                // Insert sample data
                await databaseContext.ExecuteNonQueryAsync(@"
                    DELETE FROM BenchmarkTestEntities;
                    INSERT INTO BenchmarkTestEntities (Name, Value, Description, CreatedDate)
                    SELECT TOP 1000
                        'Entity_' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR(10)),
                        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)),
                        'Test entity for benchmarking purposes',
                        DATEADD(DAY, -ROW_NUMBER() OVER (ORDER BY (SELECT NULL)), GETUTCDATE())
                    FROM sys.objects a
                    CROSS JOIN sys.objects b");
            }

            _databaseInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize database: {ex.Message}");
            Console.WriteLine("Benchmarks will run but may fail. Ensure SQL Server is available.");
        }
    }

    public static async Task CleanupDatabaseAsync()
    {
        try
        {
            var serviceProvider = GetServiceProvider();
            var databaseContext = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDatabaseContext>(serviceProvider);
            await databaseContext.OpenAsync();
            await databaseContext.ExecuteNonQueryAsync("DELETE FROM BenchmarkTestEntities");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public static async Task<BenchmarkTestEntity> CreateTestEntityAsync(IRepository<BenchmarkTestEntity> repository, int id = 0)
    {
        var entity = new BenchmarkTestEntity
        {
            Name = "Benchmark Entity " + Guid.NewGuid().ToString()[..8],
            Value = new Random().Next(1, 10000),
            Description = "Test entity for performance measurement"
        };

        if (id > 0)
            entity.Id = id;

        return await repository.AddAsync(entity);
    }
}

public class BenchmarkTestEntity : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}