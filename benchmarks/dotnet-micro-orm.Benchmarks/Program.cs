using BenchmarkDotNet.Running;
using DotnetMicroOrm.Benchmarks;

// Run all benchmarks
var summary = BenchmarkRunner.Run(
    typeof(RepositoryBenchmarks),
    typeof(BatchOperationsBenchmarks),
    typeof(ExpressionAndCachingBenchmarks),
    BenchmarkConfig.GetConfig()
);

Console.WriteLine("\n=== Benchmark Execution Complete ===");
Console.WriteLine($"Results saved to: {summary.ResultsDirectoryPath}");
Console.WriteLine("\nTo view results:");
Console.WriteLine("1. Open the benchmarks-results folder");
Console.WriteLine("2. Check the summary files for detailed metrics");
Console.WriteLine("3. Compare results across different runs");
