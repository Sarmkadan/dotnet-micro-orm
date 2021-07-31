using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DotnetMicroOrm.Benchmarks
{
    /// <summary>
    /// Extension methods that make it easier to run and compare the various benchmark
    /// operations defined in <see cref="ComparisonBenchmarks"/>.
    /// </summary>
    public static class ComparisonBenchmarksExtensions
    {
        /// <summary>
        /// Executes every public benchmark method on <see cref="ComparisonBenchmarks"/>
        /// sequentially and returns the elapsed time for each.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>
        /// A dictionary where the key is the benchmark method name and the value is the
        /// time it took to complete.
        /// </returns>
        public static async Task<Dictionary<string, TimeSpan>> RunAllBenchmarksAsync(this ComparisonBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            var results = new Dictionary<string, TimeSpan>();

            var methods = new (string Name, Func<Task> Action)[]
            {
                ("RawADO_GetById", benchmarks.RawADO_GetById),
                ("RawADO_GetAll", benchmarks.RawADO_GetAll),
                ("RawADO_Add", benchmarks.RawADO_Add),
                ("RawADO_AddRange", benchmarks.RawADO_AddRange),
                ("DotnetMicroOrm_GetById", benchmarks.DotnetMicroOrm_GetById),
                ("DotnetMicroOrm_GetAll", benchmarks.DotnetMicroOrm_GetAll),
                ("DotnetMicroOrm_Add", benchmarks.DotnetMicroOrm_Add),
                ("DotnetMicroOrm_AddRange", benchmarks.DotnetMicroOrm_AddRange),
                ("Query_GetByValueGreaterThan", benchmarks.Query_GetByValueGreaterThan),
                ("Query_GetWithOrdering", benchmarks.Query_GetWithOrdering),
                ("Query_Count", benchmarks.Query_Count),
                ("Query_Any", benchmarks.Query_Any),
                ("BatchInsert_100_Entities", benchmarks.BatchInsert_100_Entities),
                ("BatchUpdate_100_Entities", benchmarks.BatchUpdate_100_Entities),
                ("BatchDelete_100_Entities", benchmarks.BatchDelete_100_Entities)
            };

            foreach (var (name, action) in methods)
            {
                var sw = Stopwatch.StartNew();
                await action();
                sw.Stop();

                results[name] = sw.Elapsed;
            }

            return results;
        }

        /// <summary>
        /// Measures and returns the elapsed time for the raw ADO.NET Get‑by‑Id
        /// operation versus the DotnetMicroOrm Get‑by‑Id operation.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>A tuple containing the elapsed times for raw ADO.NET and DotnetMicroOrm operations.</returns>
        public static async Task<(TimeSpan Raw, TimeSpan Orm)> CompareGetByIdAsync(this ComparisonBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            var swRaw = Stopwatch.StartNew();
            await benchmarks.RawADO_GetById();
            swRaw.Stop();

            var swOrm = Stopwatch.StartNew();
            await benchmarks.DotnetMicroOrm_GetById();
            swOrm.Stop();

            return (swRaw.Elapsed, swOrm.Elapsed);
        }

        /// <summary>
        /// Measures and returns the elapsed time for adding a range of entities using
        /// raw ADO.NET versus using DotnetMicroOrm.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <returns>A tuple containing the elapsed times for raw ADO.NET and DotnetMicroOrm operations.</returns>
        public static async Task<(TimeSpan Raw, TimeSpan Orm)> CompareAddRangeAsync(this ComparisonBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            var swRaw = Stopwatch.StartNew();
            await benchmarks.RawADO_AddRange();
            swRaw.Stop();

            var swOrm = Stopwatch.StartNew();
            await benchmarks.DotnetMicroOrm_AddRange();
            swOrm.Stop();

            return (swRaw.Elapsed, swOrm.Elapsed);
        }
    }
}
