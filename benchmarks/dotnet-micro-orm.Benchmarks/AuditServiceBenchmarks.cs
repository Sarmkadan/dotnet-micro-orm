using BenchmarkDotNet.Attributes;
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Benchmarks for AuditService - logging entity changes
/// </summary>
[MemoryDiagnoser]
public class AuditServiceBenchmarks
{
    private IServiceProvider _serviceProvider;
    private IAuditService _auditService;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _serviceProvider = BenchmarkSetup.GetServiceProvider();
        _auditService = _serviceProvider.GetRequiredService<IAuditService>();

        // Clean up any existing audit logs
        await BenchmarkSetup.CleanupDatabaseAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await BenchmarkSetup.CleanupDatabaseAsync();
    }

    [Benchmark]
    [BenchmarkCategory("AuditLogging")]
    public async Task LogInsertAsync()
    {
        await _auditService.LogInsertAsync("Product", 1, "{\"Name\": \"Test\"}", 1, "user1");
    }

    [Benchmark]
    [BenchmarkCategory("AuditLogging")]
    public async Task LogUpdateAsync()
    {
        await _auditService.LogUpdateAsync("Product", 1, "{\"Name\": \"Old\"}", "{\"Name\": \"New\"}", "Name", 1, "user1");
    }
}
