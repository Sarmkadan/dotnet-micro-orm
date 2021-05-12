using System;
using System.Threading.Tasks;
using DotnetMicroOrm.Benchmarks;

public static class BatchOperationsBenchmarksExtensions
{
    public static async Task WarmupCache(this BatchOperationsBenchmarks benchmarks)
    {
        await benchmarks.AddRangeAsync_1000_Entities();
        await benchmarks.AddRangeAsync_5000_Entities();
    }

    public static async Task RunAllBenchmarks(this BatchOperationsBenchmarks benchmarks)
    {
        await benchmarks.GlobalSetup();
        await benchmarks.AddRangeAsync_1000_Entities();
        await benchmarks.AddRangeAsync_5000_Entities();
        await benchmarks.UpdateRangeAsync_1000_Entities();
        await benchmarks.UpdateRangeAsync_5000_Entities();
        await benchmarks.DeleteRangeAsync_1000_Entities();
        await benchmarks.DeleteRangeAsync_5000_Entities();
        await benchmarks.BulkInsert_100_Entities();
        await benchmarks.BulkInsert_10000_Entities();
        await benchmarks.BatchInsert_1000_Entities_With_Relations();
        await benchmarks.BatchUpdate_Complex_Predicate();
        await benchmarks.BatchDelete_With_Where_Clause();
        await benchmarks.GlobalCleanup();
    }

    public static async Task MeasureMemoryUsage(this BatchOperationsBenchmarks benchmarks)
    {
        var memBefore = GC.GetTotalMemory(true);
        await benchmarks.AddRangeAsync_1000_Entities();
        var memAfter = GC.GetTotalMemory(true);
        Console.WriteLine($"Memory usage: {memAfter - memBefore} bytes");
    }
}
