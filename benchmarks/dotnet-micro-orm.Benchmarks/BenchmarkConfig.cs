using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Configuration for BenchmarkDotNet benchmarks
/// </summary>
public class BenchmarkConfig
{
    public static IConfig GetConfig()
    {
        return DefaultConfig.Instance
            .AddJob(Job.Default
                .WithId("Benchmark")
                .WithIterationCount(10)
                .WithWarmupCount(3)
                .AsDefault())
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddDiagnoser(new DisassemblyDiagnoser(new DisassemblyDiagnoserConfig(maxDepth: 3)))
            .KeepBenchmarkFiles(false)
            .WithArtifactsPath("benchmarks-results");
    }
}
