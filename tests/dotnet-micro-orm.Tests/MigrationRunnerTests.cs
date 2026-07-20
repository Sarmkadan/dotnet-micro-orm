#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Migrations;
using FluentAssertions;
using Moq;
using Xunit;

/// <summary>
/// Tests for the MigrationRunner class.
/// </summary>
public sealed class MigrationRunnerTests
{
    /// <summary>
    /// Builds a mock IDatabaseContext instance.
    /// </summary>
    /// <returns>A mock IDatabaseContext instance.</returns>
    private static Mock<IDatabaseContext> BuildContextMock()
    {
        var mock = new Mock<IDatabaseContext>();

        // EnsureHistoryTable – DDL always succeeds
        mock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);
        mock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), null))
            .ReturnsAsync(1);

        // GetAppliedVersionsAsync returns empty set by default
        mock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([]);
        mock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), null))
            .ReturnsAsync([]);

        return mock;
    }

    /// <summary>
    /// Tests that MigrateAsync applies pending migrations in version order.
    /// </summary>
    [Fact]
    public async Task MigrateAsync_PendingMigrations_AppliesThemInVersionOrder()
    {
        var applied = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240102_001");
        m2.SetupGet(m => m.Description).Returns("Second");
        m2.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240102_001"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, [m2.Object, m1.Object]);

        await runner.MigrateAsync();

        applied.Should().Equal("20240101_001", "20240102_001");
    }

    /// <summary>
    /// Tests that GetPendingMigrationsAsync excludes already applied migrations.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsAsync_AlreadyApplied_ExcludesThem()
    {
        var contextMock = BuildContextMock();

        // Return one already-applied version
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(s => s.Contains("SELECT [Version]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);

        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(s => s.Contains("SELECT [Version]")), null))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("Already applied");

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240103_001");
        m2.SetupGet(m => m.Description).Returns("Pending");

        var runner = new MigrationRunner(contextMock.Object, [m1.Object, m2.Object]);

        var pending = await runner.GetPendingMigrationsAsync();

        pending.Should().ContainSingle();
        pending[0].Should().Be("20240103_001");
    }

    /// <summary>
    /// Tests that GetPendingMigrationsAsync returns only pending migrations in version order.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsAsync_ReturnsPendingMigrations()
    {
        var contextMock = BuildContextMock();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        
        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240102_001");

        var m3 = new Mock<IMigration>();
        m3.SetupGet(m => m.Version).Returns("20240103_001");

        // Assume only m1 is applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(s => s.Contains("SELECT [Version]")), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);
        
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(s => s.Contains("SELECT [Version]")), null))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);

        var runner = new MigrationRunner(contextMock.Object, [m3.Object, m2.Object, m1.Object]);

        var pending = await runner.GetPendingMigrationsAsync();

        pending.Should().HaveCount(2);
        pending.Should().Equal("20240102_001", "20240103_001");
    }

    /// <summary>
    /// Tests that MigrateToAsync limits application to the target version.
    /// </summary>
    [Fact]
    public async Task MigrateToAsync_LimitsApplicationToTargetVersion()
    {
        var applied = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240102_001");
        m2.SetupGet(m => m.Description).Returns("Second");
        m2.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240102_001"))
            .Returns(Task.CompletedTask);

        var m3 = new Mock<IMigration>();
        m3.SetupGet(m => m.Version).Returns("20240103_001");
        m3.SetupGet(m => m.Description).Returns("Third");
        m3.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240103_001"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, [m1.Object, m2.Object, m3.Object]);

        await runner.MigrateToAsync("20240102_001");

        applied.Should().Equal("20240101_001", "20240102_001");
        applied.Should().NotContain("20240103_001");
    }
}
