using System;
using System.Threading.Tasks;

#nullable enable

namespace DotnetMicroOrm.Benchmarks
{
    public static class AuditServiceBenchmarksExtensions
    {
        /// <summary>
        /// Creates a new AuditServiceBenchmarks instance with default configuration for benchmarking.
        /// </summary>
        /// <param name="service">The AuditServiceBenchmarks instance to extend.</param>
        /// <returns>A configured AuditServiceBenchmarks instance ready for benchmarking.</returns>
        public static AuditServiceBenchmarks WithDefaultConfig(this AuditServiceBenchmarks service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service;
        }

        /// <summary>
        /// Executes a warmup run for the audit service to stabilize measurements.
        /// This should be called before actual benchmark measurements to account for JIT compilation.
        /// </summary>
        /// <param name="service">The AuditServiceBenchmarks instance.</param>
        /// <returns>Task representing the warmup operation.</returns>
        public static async Task WarmupAsync(this AuditServiceBenchmarks service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            // Warmup by performing a single insert and update operation
            await service.GlobalSetup();
            await service.LogInsertAsync();
            await service.LogUpdateAsync();
            await service.GlobalCleanup();
        }

        /// <summary>
        /// Measures the performance of bulk insert operations by executing the specified number of insert operations.
        /// </summary>
        /// <param name="service">The AuditServiceBenchmarks instance.</param>
        /// <param name="operationCount">Number of insert operations to perform.</param>
        /// <returns>Task representing the benchmark operation.</returns>
        public static async Task MeasureBulkInsertAsync(this AuditServiceBenchmarks service, int operationCount = 1000)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (operationCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(operationCount), "Operation count must be positive.");
            }

            await service.GlobalSetup();

            for (int i = 0; i < operationCount; i++)
            {
                await service.LogInsertAsync();
            }

            await service.GlobalCleanup();
        }

        /// <summary>
        /// Measures the performance of bulk update operations by executing the specified number of update operations.
        /// </summary>
        /// <param name="service">The AuditServiceBenchmarks instance.</param>
        /// <param name="operationCount">Number of update operations to perform.</param>
        /// <returns>Task representing the benchmark operation.</returns>
        public static async Task MeasureBulkUpdateAsync(this AuditServiceBenchmarks service, int operationCount = 1000)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (operationCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(operationCount), "Operation count must be positive.");
            }

            await service.GlobalSetup();

            for (int i = 0; i < operationCount; i++)
            {
                await service.LogUpdateAsync();
            }

            await service.GlobalCleanup();
        }
    }
}