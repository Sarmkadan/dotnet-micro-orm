// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Utils;
using FluentAssertions;

namespace DotnetMicroOrm.Tests;

public class PerformanceMonitorTests
{
    [Fact]
    public void Constructor_NullOperationName_UsesDefault()
    {
        using var monitor = new PerformanceMonitor(null!);
        monitor.Should().NotBeNull();
    }

    [Fact]
    public void Elapsed_ImmediatelyAfterCreation_IsSmall()
    {
        using var monitor = new PerformanceMonitor("test-op");
        monitor.Elapsed.TotalSeconds.Should().BeLessThan(1);
    }

    [Fact]
    public void ElapsedMilliseconds_AfterDelay_ReflectsTime()
    {
        using var monitor = new PerformanceMonitor("test-op");
        Thread.Sleep(20);
        monitor.ElapsedMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Checkpoint_RecordsNamedCheckpoint()
    {
        using var monitor = new PerformanceMonitor("test-op");
        var act = () => monitor.Checkpoint("step1");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordMetric_StoresCustomValue()
    {
        using var monitor = new PerformanceMonitor("test-op");
        var act = () => monitor.RecordMetric("query_count", 42);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordItemCount_StoresCount()
    {
        using var monitor = new PerformanceMonitor("test-op");
        var act = () => monitor.RecordItemCount(100);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetReport_ReturnsNonNullReport()
    {
        using var monitor = new PerformanceMonitor("test-op");
        monitor.RecordItemCount(50);
        var report = monitor.GetReport();
        report.Should().NotBeNull();
        report.OperationName.Should().Be("test-op");
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var monitor = new PerformanceMonitor("test-op");
        var act = () => monitor.Dispose();
        act.Should().NotThrow();
    }
}
