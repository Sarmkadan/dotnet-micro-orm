#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Exceptions;
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

    /// <summary>
    /// Tests that a failing migration halts the run and records the failed migration.
    /// </summary>
    [Fact]
    public async Task MigrateAsync_FailingMigration_HaltsRunAndRecordsFailure()
    {
        var appliedVersions = new List<Dictionary<string, object>>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240102_001");
        m2.SetupGet(m => m.Description).Returns("Second");
        m2.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .ThrowsAsync(new InvalidOperationException("Migration failed"));

        var contextMock = BuildContextMock();

        // Setup to track applied versions in history table - initially empty
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(appliedVersions);
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            null))
            .ReturnsAsync(appliedVersions);

        var runner = new MigrationRunner(contextMock.Object, [m1.Object, m2.Object]);

        // Act & Assert
        var act = async () => await runner.MigrateAsync();
        await act.Should().ThrowAsync<OrmException>();

        // Verify history table was called to record m1 as success and m2 as failure
        contextMock.Verify(c => c.ExecuteNonQueryAsync(
            It.Is<string>(s => s.Contains("_MigrationHistory") && s.Contains("@Version") && s.Contains("@Success")),
            It.Is<Dictionary<string, object>>(d =>
                d["Version"].ToString() == "20240101_001" &&
                Convert.ToBoolean(d["Success"]) == true)),
            Times.Once);

        contextMock.Verify(c => c.ExecuteNonQueryAsync(
            It.Is<string>(s => s.Contains("_MigrationHistory") && s.Contains("@Version") && s.Contains("@Success")),
            It.Is<Dictionary<string, object>>(d =>
                d["Version"].ToString() == "20240102_001" &&
                Convert.ToBoolean(d["Success"]) == false &&
                d["ErrorMessage"].ToString().Contains("Migration failed"))),
            Times.Once);
    }

    /// <summary>
    /// Tests that duplicate versions are handled correctly - only one migration with each version is applied.
    /// </summary>
    [Fact]
    public async Task MigrationRunner_DuplicateVersions_OnlyOneAppliedPerVersion()
    {
        var applied = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240101_001"); // Duplicate version
        m2.SetupGet(m => m.Description).Returns("Duplicate");

        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, [m1.Object, m2.Object]);

        await runner.MigrateAsync();

        // Only one migration should be applied since they have the same version
        applied.Should().Equal("20240101_001");
        applied.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that RollbackToAsync reverts migrations in correct order.
    /// </summary>
    [Fact]
    public async Task RollbackToAsync_RevertsMigrationsInDescendingOrder()
    {
        var rolledBack = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.DownAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => rolledBack.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("20240102_001");
        m2.SetupGet(m => m.Description).Returns("Second");
        m2.Setup(m => m.DownAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => rolledBack.Add("20240102_001"))
            .Returns(Task.CompletedTask);

        var m3 = new Mock<IMigration>();
        m3.SetupGet(m => m.Version).Returns("20240103_001");
        m3.SetupGet(m => m.Description).Returns("Third");
        m3.Setup(m => m.DownAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => rolledBack.Add("20240103_001"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();

        // Setup to return all three versions as applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([
                new Dictionary<string, object> { ["Version"] = "20240101_001" },
                new Dictionary<string, object> { ["Version"] = "20240102_001" },
                new Dictionary<string, object> { ["Version"] = "20240103_001" }
            ]);
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            null))
            .ReturnsAsync([
                new Dictionary<string, object> { ["Version"] = "20240101_001" },
                new Dictionary<string, object> { ["Version"] = "20240102_001" },
                new Dictionary<string, object> { ["Version"] = "20240103_001" }
            ]);

        // Setup to delete history records
        contextMock.Setup(c => c.ExecuteNonQueryAsync(
            It.Is<string>(s => s.Contains("DELETE FROM") && s.Contains("_MigrationHistory")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var runner = new MigrationRunner(contextMock.Object, [m1.Object, m2.Object, m3.Object]);

        await runner.RollbackToAsync("20240101_001");

        // Should rollback m3 and m2, but not m1 (target version)
        rolledBack.Should().Equal("20240103_001", "20240102_001");
    }

    /// <summary>
    /// Tests that GetAppliedMigrationsAsync returns correct migration records.
    /// </summary>
    [Fact]
    public async Task GetAppliedMigrationsAsync_ReturnsCorrectRecords()
    {
        var contextMock = BuildContextMock();

        // Setup to return migration history records
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Id]") && s.Contains("_MigrationHistory") && s.Contains("ORDER BY [Version]")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([
                new Dictionary<string, object> {
                    ["Id"] = 1,
                    ["Version"] = "20240101_001",
                    ["Description"] = "First migration",
                    ["AppliedAt"] = DateTime.UtcNow.AddMinutes(-10),
                    ["Success"] = true,
                    ["ErrorMessage"] = DBNull.Value
                },
                new Dictionary<string, object> {
                    ["Id"] = 2,
                    ["Version"] = "20240102_001",
                    ["Description"] = "Second migration",
                    ["AppliedAt"] = DateTime.UtcNow.AddMinutes(-5),
                    ["Success"] = true,
                    ["ErrorMessage"] = DBNull.Value
                },
                new Dictionary<string, object> {
                    ["Id"] = 3,
                    ["Version"] = "20240103_001",
                    ["Description"] = "Failed migration",
                    ["AppliedAt"] = DateTime.UtcNow,
                    ["Success"] = false,
                    ["ErrorMessage"] = "Migration failed with error"
                }
            ]);
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Id]") && s.Contains("_MigrationHistory") && s.Contains("ORDER BY [Version]")),
            null))
            .ReturnsAsync([
                new Dictionary<string, object> {
                    ["Id"] = 1,
                    ["Version"] = "20240101_001",
                    ["Description"] = "First migration",
                    ["AppliedAt"] = DateTime.UtcNow.AddMinutes(-10),
                    ["Success"] = true,
                    ["ErrorMessage"] = DBNull.Value
                },
                new Dictionary<string, object> {
                    ["Id"] = 2,
                    ["Version"] = "20240102_001",
                    ["Description"] = "Second migration",
                    ["AppliedAt"] = DateTime.UtcNow.AddMinutes(-5),
                    ["Success"] = true,
                    ["ErrorMessage"] = DBNull.Value
                },
                new Dictionary<string, object> {
                    ["Id"] = 3,
                    ["Version"] = "20240103_001",
                    ["Description"] = "Failed migration",
                    ["AppliedAt"] = DateTime.UtcNow,
                    ["Success"] = false,
                    ["ErrorMessage"] = "Migration failed with error"
                }
            ]);

        var runner = new MigrationRunner(contextMock.Object, []);
        var appliedMigrations = await runner.GetAppliedMigrationsAsync();

        appliedMigrations.Should().HaveCount(3);
        appliedMigrations[0].Version.Should().Be("20240101_001");
        appliedMigrations[0].Description.Should().Be("First migration");
        appliedMigrations[0].Success.Should().BeTrue();
        appliedMigrations[0].ErrorMessage.Should().BeNull();

        appliedMigrations[1].Version.Should().Be("20240102_001");
        appliedMigrations[1].Description.Should().Be("Second migration");
        appliedMigrations[1].Success.Should().BeTrue();
        appliedMigrations[1].ErrorMessage.Should().BeNull();

        appliedMigrations[2].Version.Should().Be("20240103_001");
        appliedMigrations[2].Description.Should().Be("Failed migration");
        appliedMigrations[2].Success.Should().BeFalse();
        appliedMigrations[2].ErrorMessage.Should().Be("Migration failed with error");
    }

    /// <summary>
    /// Tests that MigrateToAsync with target version that's already applied does nothing.
    /// </summary>
    [Fact]
    public async Task MigrateToAsync_TargetAlreadyApplied_DoesNothing()
    {
        var applied = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();

        // Return m1 as already applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            null))
            .ReturnsAsync([new Dictionary<string, object> { ["Version"] = "20240101_001" }]);

        var runner = new MigrationRunner(contextMock.Object, [m1.Object]);

        await runner.MigrateToAsync("20240101_001");

        applied.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that RollbackToAsync with target version that's not applied does nothing.
    /// </summary>
    [Fact]
    public async Task RollbackToAsync_TargetNotApplied_DoesNothing()
    {
        var rolledBack = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("First");
        m1.Setup(m => m.DownAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => rolledBack.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();

        // Return no applied versions
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([]);
        contextMock.Setup(c => c.ExecuteQueryAsync(
            It.Is<string>(s => s.Contains("SELECT [Version]") && s.Contains("[Success] = 1")),
            null))
            .ReturnsAsync([]);

        var runner = new MigrationRunner(contextMock.Object, [m1.Object]);

        await runner.RollbackToAsync("20240102_001");

        rolledBack.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that migrations are sorted correctly with different version formats.
    /// </summary>
    [Fact]
    public async Task MigrationRunner_VersionSorting_HandlesDifferentFormats()
    {
        var applied = new List<string>();

        var m1 = new Mock<IMigration>();
        m1.SetupGet(m => m.Version).Returns("20240101_001");
        m1.SetupGet(m => m.Description).Returns("Date format");
        m1.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("20240101_001"))
            .Returns(Task.CompletedTask);

        var m2 = new Mock<IMigration>();
        m2.SetupGet(m => m.Version).Returns("2024-01-02_002");
        m2.SetupGet(m => m.Description).Returns("ISO format");
        m2.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("2024-01-02_002"))
            .Returns(Task.CompletedTask);

        var m3 = new Mock<IMigration>();
        m3.SetupGet(m => m.Version).Returns("2.0.0");
        m3.SetupGet(m => m.Description).Returns("Semantic version");
        m3.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Callback(() => applied.Add("2.0.0"))
            .Returns(Task.CompletedTask);

        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, [m3.Object, m1.Object, m2.Object]);

        await runner.MigrateAsync();

        // Should be sorted by string comparison: 2.0.0 < 2024-01-02_002 < 20240101_001
        applied.Should().Equal("2.0.0", "2024-01-02_002", "20240101_001");
    }
}
