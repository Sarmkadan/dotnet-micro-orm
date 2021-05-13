using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DotnetMicroOrm.Benchmarks
{
    /// <summary>
    /// Extension methods that add convenient helper functionality to <see cref="RepositoryBenchmarks"/>.
    /// </summary>
    public static class RepositoryBenchmarksExtensions
    {
        /// <summary>
        /// Executes all benchmark methods on the provided <see cref="RepositoryBenchmarks"/> instance
        /// and returns the elapsed time for each operation.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <returns>A dictionary where the key is the benchmark method name and the value is the elapsed time.</returns>
        public static async Task<Dictionary<string, TimeSpan>> RunAllBenchmarksAsync(this RepositoryBenchmarks benchmarks)
        {
            var results = new Dictionary<string, TimeSpan>();

            // Helper local function to measure a single benchmark method.
            async Task Measure(string name, Func<Task> func)
            {
                var sw = Stopwatch.StartNew();
                await func();
                sw.Stop();
                results[name] = sw.Elapsed;
            }

            await benchmarks.GlobalSetup();

            await Measure(nameof(benchmarks.GetByIdAsync), benchmarks.GetByIdAsync);
            await Measure(nameof(benchmarks.AddAsync), benchmarks.AddAsync);
            await Measure(nameof(benchmarks.UpdateAsync), benchmarks.UpdateAsync);
            await Measure(nameof(benchmarks.DeleteAsync), benchmarks.DeleteAsync);
            await Measure(nameof(benchmarks.AddRangeAsync_100_Entities), benchmarks.AddRangeAsync_100_Entities);
            await Measure(nameof(benchmarks.UpdateRangeAsync_100_Entities), benchmarks.UpdateRangeAsync_100_Entities);
            await Measure(nameof(benchmarks.DeleteRangeAsync_100_Entities), benchmarks.DeleteRangeAsync_100_Entities);
            await Measure(nameof(benchmarks.GetAllAsync), benchmarks.GetAllAsync);
            await Measure(nameof(benchmarks.GetByPredicateAsync), benchmarks.GetByPredicateAsync);
            await Measure(nameof(benchmarks.CountAsync), benchmarks.CountAsync);
            await Measure(nameof(benchmarks.AnyAsync), benchmarks.AnyAsync);
            await Measure(nameof(benchmarks.GetPagedAsync), benchmarks.GetPagedAsync);
            await Measure(nameof(benchmarks.GetWithOrdering), benchmarks.GetWithOrdering);
            await Measure(nameof(benchmarks.GetWithMultiplePredicates), benchmarks.GetWithMultiplePredicates);
            await Measure(nameof(benchmarks.GetWithComplexSpecification), benchmarks.GetWithComplexSpecification);
            await Measure(nameof(benchmarks.GetPagedPerformance), benchmarks.GetPagedPerformance);
            await Measure(nameof(benchmarks.FirstOrDefaultPerformance), benchmarks.FirstOrDefaultPerformance);

            await benchmarks.GlobalCleanup();

            return results;
        }

        /// <summary>
        /// Performs a simple warm‑up by invoking a few lightweight benchmark methods multiple times.
        /// This helps mitigate JIT and cold‑start effects before measuring real workloads.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="iterations">How many times each warm‑up method should be executed.</param>
        public static async Task WarmupAsync(this RepositoryBenchmarks benchmarks, int iterations = 5)
        {
            await benchmarks.GlobalSetup();

            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.GetByIdAsync();
                await benchmarks.CountAsync();
                await benchmarks.AnyAsync();
            }

            await benchmarks.GlobalCleanup();
        }

        /// <summary>
        /// Measures the execution time of an arbitrary asynchronous operation.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance (required for the extension method syntax).</param>
        /// <param name="operation">A delegate representing the operation to measure.</param>
        /// <returns>The elapsed <see cref="TimeSpan"/> of the operation.</returns>
        public static async Task<TimeSpan> MeasureAsync(this RepositoryBenchmarks benchmarks, Func<Task> operation)
        {
            var sw = Stopwatch.StartNew();
            await operation();
            sw.Stop();
            return sw.Elapsed;
        }
    }
}
