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
        /// Executes a warmup sequence for batch operations to stabilize measurements.
        /// This should be called before actual benchmark measurements to account for JIT compilation and cache warming.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>Task representing the warmup operation.</returns>
        public static async Task WarmupCache(this BatchOperationsBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.AddRangeAsync_1000_Entities();
            await benchmarks.AddRangeAsync_5000_Entities();
        }

        /// <summary>
        /// Executes all benchmark methods on the provided <see cref="BatchOperationsBenchmarks"/> instance.
        /// This runs the complete suite of batch operation benchmarks including setup, execution, and cleanup.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>Task representing the complete benchmark run.</returns>
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
        /// Measures memory allocation during batch insert operations by executing a standard batch insert
        /// and comparing memory usage before and after the operation.
        /// </summary>
        /// <param name="benchmarks">The <see cref="BatchOperationsBenchmarks"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>Task representing the memory measurement operation.</returns>
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
