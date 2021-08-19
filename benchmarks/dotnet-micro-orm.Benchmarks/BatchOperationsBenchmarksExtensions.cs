using System;
using System.Threading.Tasks;

#nullable enable

namespace DotnetMicroOrm.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="BatchOperationsBenchmarks"/> to simplify benchmark execution and setup.
    /// </summary>
    public static class BatchOperationsBenchmarksExtensions
    {
        /// <summary>
        /// Executes a warm‑up sequence for batch operations to stabilize measurements.
        /// This method runs a small (1,000 entities) and a larger (5,000 entities) add‑range benchmark
        /// to trigger JIT compilation and warm up any relevant caches before the actual measurements.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance on which the warm‑up operations are performed.</param>
        /// <returns>A <see cref="Task"/> that completes when both warm‑up operations have finished.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static async Task WarmupCache(this BatchOperationsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.AddRangeAsync_1000_Entities();
            await benchmarks.AddRangeAsync_5000_Entities();
        }

        /// <summary>
        /// Executes the full suite of batch‑operation benchmarks on the supplied <see cref="BatchOperationsBenchmarks"/> instance.
        /// The sequence includes global setup, a variety of add, update, delete, bulk‑insert and batch‑operation
        /// scenarios, and finally global cleanup. This method is intended for a single, comprehensive benchmark run.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance whose benchmark methods will be invoked.</param>
        /// <returns>A <see cref="Task"/> that completes when the entire benchmark run (including cleanup) has finished.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static async Task RunAllBenchmarks(this BatchOperationsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

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

        /// <summary>
        /// Measures the amount of memory allocated by a standard batch insert of 1,000 entities.
        /// It records the total managed memory before and after the operation and writes the
        /// difference to the console.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance used to perform the insert operation.</param>
        /// <returns>A <see cref="Task"/> that completes after the memory measurement and console output are finished.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <c>null</c>.</exception>
        public static async Task MeasureMemoryUsage(this BatchOperationsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            var memBefore = GC.GetTotalMemory(true);
            await benchmarks.AddRangeAsync_1000_Entities();
            var memAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"Memory usage: {memAfter - memBefore} bytes");
        }
    }
}
