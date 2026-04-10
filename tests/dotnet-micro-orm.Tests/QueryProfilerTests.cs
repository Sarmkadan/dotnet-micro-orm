#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Profiling;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class QueryProfilerTests
{
    [Fact]
    public async Task ProfileAsync_SuccessfulOperation_RecordsProfileWithCorrectDuration()
    {
        var profiler = new QueryProfiler();
        const string sql = "SELECT * FROM [dbo].[Products]";

        await profiler.ProfileAsync(sql, async () =>
        {
            await Task.Delay(5);
            return 1;
        });

        var profiles = profiler.GetProfiles();

        profiles.Should().ContainSingle();
        profiles[0].Query.Should().Be(sql);
        profiles[0].Succeeded.Should().BeTrue();
        profiles[0].ErrorMessage.Should().BeNull();
        profiles[0].Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ProfileAsync_WhenDisabled_DoesNotRecordProfiles()
    {
        var profiler = new QueryProfiler { IsEnabled = false };

        await profiler.ProfileAsync("SELECT 1", () => Task.FromResult(42));

        profiler.GetProfiles().Should().BeEmpty();
    }

    [Fact]
    public async Task ProfileAsync_FailingOperation_RecordsFailedProfileAndRethrows()
    {
        var profiler = new QueryProfiler();

        var act = () => profiler.ProfileAsync<int>("SELECT bad", () =>
            throw new InvalidOperationException("forced failure"));

        await act.Should().ThrowAsync<InvalidOperationException>();

        var profiles = profiler.GetProfiles();
        profiles.Should().ContainSingle();
        profiles[0].Succeeded.Should().BeFalse();
        profiles[0].ErrorMessage.Should().Contain("forced failure");
    }

    [Fact]
    public async Task GetSummary_MultipleQueries_ReturnsCorrectAggregates()
    {
        var profiler = new QueryProfiler();

        await profiler.ProfileAsync("Q1", () => Task.FromResult(1));
        await profiler.ProfileAsync("Q2", () => Task.FromResult(2));

        var summary = profiler.GetSummary();

        summary.TotalQueries.Should().Be(2);
        summary.FailedQueries.Should().Be(0);
        summary.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
        summary.SlowestQuery.Should().NotBeNull();
    }

    [Fact]
    public async Task Clear_RemovesAllProfiles()
    {
        var profiler = new QueryProfiler();
        await profiler.ProfileAsync("SELECT 1", () => Task.FromResult(0));

        profiler.Clear();

        profiler.GetProfiles().Should().BeEmpty();
        profiler.GetSummary().TotalQueries.Should().Be(0);
    }

    [Fact]
    public async Task Constructor_MaxProfilesExceeded_EvictsOldEntries()
    {
        var profiler = new QueryProfiler(maxProfiles: 3);

        for (int i = 0; i < 5; i++)
            await profiler.ProfileAsync($"Q{i}", () => Task.FromResult(i));

        profiler.GetProfiles().Should().HaveCount(3);
    }
}
