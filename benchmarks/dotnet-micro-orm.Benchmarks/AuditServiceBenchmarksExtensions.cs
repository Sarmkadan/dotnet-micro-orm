using System;
using System.Threading.Tasks;

#nullable enable

namespace DotnetMicroOrm.Benchmarks
{
	/// <summary>
	/// Provides extension methods for <see cref="AuditServiceBenchmarks"/> to simplify benchmark setup and execution.
	/// </summary>
	public static class AuditServiceBenchmarksExtensions
	{
		/// <summary>
		/// Creates a new <see cref="AuditServiceBenchmarks"/> instance with default configuration for benchmarking.
		/// </summary>
		/// <param name="service">The <see cref="AuditServiceBenchmarks"/> instance to extend.</param>
		/// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
		/// <returns>A configured <see cref="AuditServiceBenchmarks"/> instance ready for benchmarking.</returns>
		public static AuditServiceBenchmarks WithDefaultConfig(this AuditServiceBenchmarks service)
		{
			ArgumentNullException.ThrowIfNull(service);

			return service;
		}

		/// <summary>
		/// Executes a warmup run for the audit service to stabilize measurements.
		/// This should be called before actual benchmark measurements to account for JIT compilation.
		/// </summary>
		/// <param name="service">The <see cref="AuditServiceBenchmarks"/> instance.</param>
		/// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
		/// <returns>Task representing the warmup operation.</returns>
		public static async Task WarmupAsync(this AuditServiceBenchmarks service)
		{
			ArgumentNullException.ThrowIfNull(service);

			// Warmup by performing a single insert and update operation
			await service.GlobalSetup();
			await service.LogInsertAsync();
			await service.LogUpdateAsync();
			await service.GlobalCleanup();
		}

		/// <summary>
		/// Measures the performance of bulk insert operations by executing the specified number of insert operations.
		/// </summary>
		/// <param name="service">The <see cref="AuditServiceBenchmarks"/> instance.</param>
		/// <param name="operationCount">Number of insert operations to perform. Must be positive.</param>
		/// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="operationCount"/> is not positive.</exception>
		/// <returns>Task representing the benchmark operation.</returns>
		public static async Task MeasureBulkInsertAsync(this AuditServiceBenchmarks service, int operationCount = 1000)
		{
			ArgumentNullException.ThrowIfNull(service);

			if (operationCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(operationCount), operationCount, "Operation count must be positive.");
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
		/// <param name="service">The <see cref="AuditServiceBenchmarks"/> instance.</param>
		/// <param name="operationCount">Number of update operations to perform. Must be positive.</param>
		/// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="operationCount"/> is not positive.</exception>
		/// <returns>Task representing the benchmark operation.</returns>
		public static async Task MeasureBulkUpdateAsync(this AuditServiceBenchmarks service, int operationCount = 1000)
		{
			ArgumentNullException.ThrowIfNull(service);

			if (operationCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(operationCount), operationCount, "Operation count must be positive.");
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
