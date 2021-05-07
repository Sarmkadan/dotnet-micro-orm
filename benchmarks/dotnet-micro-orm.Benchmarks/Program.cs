using BenchmarkDotNet.Running;
using DotnetMicroOrm.Benchmarks;

// Run all benchmarks
var summaries = BenchmarkRunner.Run(
    new[]
    {
        typeof(RepositoryBenchmarks),
        typeof(BatchOperationsBenchmarks),
        typeof(ExpressionAndCachingBenchmarks)
    },
    BenchmarkConfig.GetConfig()
);

Console.WriteLine("\n=== Benchmark Execution Complete ===");
foreach (var summary in summaries)
{
    Console.WriteLine($"Results saved to: {summary.ResultsDirectoryPath}");
}
Console.WriteLine("\nTo view results:");
Console.WriteLine("1. Open the benchmarks-results folder");
Console.WriteLine("2. Check the summary files for detailed metrics");
Console.WriteLine("3. Compare results across different runs");
