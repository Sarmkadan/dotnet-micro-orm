#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Migrations;
using FluentAssertions;
using Moq;
using Xunit;

/// <summary>
/// Tests for the MigrationRunnerExtensions class.
/// </summary>
public sealed class MigrationRunnerExtensionsTests
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

        // GetAppliedMigrationsAsync returns empty list by default
        mock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());
        mock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        return mock;
    }

    /// <summary>
    /// Builds a mock IMigration instance.
    /// </summary>
    /// <param name="version">The migration version.</param>
    /// <param name="description">The migration description.</param>
    /// <returns>A mock IMigration instance.</returns>
    private static Mock<IMigration> BuildMigrationMock(string version = "20240101_001", string description = "Test migration")
    {
        var mock = new Mock<IMigration>();
        mock.SetupGet(m => m.Version).Returns(version);
        mock.SetupGet(m => m.Description).Returns(description);
        mock.Setup(m => m.UpAsync(It.IsAny<IDatabaseContext>()))
            .Returns(Task.CompletedTask);
        mock.Setup(m => m.DownAsync(It.IsAny<IDatabaseContext>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Tests that GetLatestAppliedMigrationAsync returns null when no migrations are applied.
    /// </summary>
    [Fact]
    public async Task GetLatestAppliedMigrationAsync_NoAppliedMigrations_ReturnsNull()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, Array.Empty<IMigration>());

        // Act
        var result = await runner.GetLatestAppliedMigrationAsync();

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetLatestAppliedMigrationAsync returns the most recently applied migration.
    /// </summary>
    [Fact]
    public async Task GetLatestAppliedMigrationAsync_WithAppliedMigrations_ReturnsLatest()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migrations = new[]
        {
            BuildMigrationMock("20240101_001", "First migration").Object,
            BuildMigrationMock("20240102_002", "Second migration").Object,
            BuildMigrationMock("20240103_003", "Third migration").Object
        };

        var runner = new MigrationRunner(contextMock.Object, migrations);

        // Setup to return 3 applied migrations
        var appliedMigrations = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Id"] = 1,
                ["Version"] = "20240101_001",
                ["Description"] = "First migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            },
            new Dictionary<string, object>
            {
                ["Id"] = 2,
                ["Version"] = "20240102_002",
                ["Description"] = "Second migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            },
            new Dictionary<string, object>
            {
                ["Id"] = 3,
                ["Version"] = "20240103_003",
                ["Description"] = "Third migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            }
        };

        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("_MigrationHistory")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(appliedMigrations);

        // Act
        var result = await runner.GetLatestAppliedMigrationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be("20240103_003");
        result.Description.Should().Be("Third migration");
    }

    /// <summary>
    /// Tests that GetLatestAppliedMigrationAsync throws ArgumentNullException when runner is null.
    /// </summary>
    [Fact]
    public async Task GetLatestAppliedMigrationAsync_NullRunner_ThrowsArgumentNullException()
    {
        // Arrange
        MigrationRunner? runner = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => runner!.GetLatestAppliedMigrationAsync());
    }

    /// <summary>
    /// Tests that GetPendingMigrationsCountAsync returns 0 when no migrations are pending.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsCountAsync_NoPendingMigrations_ReturnsZero()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migrations = new[]
        {
            BuildMigrationMock("20240101_001", "First migration").Object,
            BuildMigrationMock("20240102_002", "Second migration").Object
        };

        var runner = new MigrationRunner(contextMock.Object, migrations);

        // Setup to return both migrations as applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT [Version]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["Version"] = "20240101_001" },
                new Dictionary<string, object> { ["Version"] = "20240102_002" }
            });

        // Act
        var result = await runner.GetPendingMigrationsCountAsync();

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetPendingMigrationsCountAsync returns correct count of pending migrations.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsCountAsync_WithPendingMigrations_ReturnsCorrectCount()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migrations = new[]
        {
            BuildMigrationMock("20240101_001", "First migration").Object,
            BuildMigrationMock("20240102_002", "Second migration").Object,
            BuildMigrationMock("20240103_003", "Third migration").Object,
            BuildMigrationMock("20240104_004", "Fourth migration").Object
        };

        var runner = new MigrationRunner(contextMock.Object, migrations);

        // Setup to return only first two migrations as applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT [Version]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["Version"] = "20240101_001" },
                new Dictionary<string, object> { ["Version"] = "20240102_002" }
            });

        // Act
        var result = await runner.GetPendingMigrationsCountAsync();

        // Assert
        result.Should().Be(2);
    }

    /// <summary>
    /// Tests that GetPendingMigrationsCountAsync returns 0 when no migrations exist.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsCountAsync_NoMigrations_ReturnsZero()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var runner = new MigrationRunner(contextMock.Object, Array.Empty<IMigration>());

        // Act
        var result = await runner.GetPendingMigrationsCountAsync();

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetPendingMigrationsCountAsync throws ArgumentNullException when runner is null.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsCountAsync_NullRunner_ThrowsArgumentNullException()
    {
        // Arrange
        IMigrationRunner? runner = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => runner!.GetPendingMigrationsCountAsync());
    }

    /// <summary>
    /// Tests that MigrateAndGetAppliedAsync applies all pending migrations and returns all applied migrations.
    /// </summary>
    [Fact]
    public async Task MigrateAndGetAppliedAsync_AppliesMigrationsAndReturnsAllApplied()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migrations = new[]
        {
            BuildMigrationMock("20240101_001", "First migration").Object,
            BuildMigrationMock("20240102_002", "Second migration").Object,
            BuildMigrationMock("20240103_003", "Third migration").Object
        };

        var runner = new MigrationRunner(contextMock.Object, migrations);

        // Setup to return no applied migrations initially
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT [Version]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        // Setup to return all migrations after MigrateAsync completes
        var appliedMigrations = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Id"] = 1,
                ["Version"] = "20240101_001",
                ["Description"] = "First migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            },
            new Dictionary<string, object>
            {
                ["Id"] = 2,
                ["Version"] = "20240102_002",
                ["Description"] = "Second migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            },
            new Dictionary<string, object>
            {
                ["Id"] = 3,
                ["Version"] = "20240103_003",
                ["Description"] = "Third migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            }
        };

        contextMock.SetupSequence(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("_MigrationHistory")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>()) // First call in MigrateAsync
            .ReturnsAsync(appliedMigrations); // Second call in GetAppliedMigrationsAsync after MigrateAsync

        // Act
        var result = await runner.MigrateAndGetAppliedAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Version.Should().Be("20240101_001");
        result[1].Version.Should().Be("20240102_002");
        result[2].Version.Should().Be("20240103_003");

        // Verify MigrateAsync was called (it calls EnsureHistoryTableAsync and ApplyMigrationAsync for each)
        contextMock.Verify(c => c.ExecuteNonQueryAsync(
            It.Is<string>(sql => sql.Contains("INSERT INTO")),
            It.IsAny<Dictionary<string, object>>()),
            Times.Exactly(3));
    }

    /// <summary>
    /// Tests that MigrateAndGetAppliedAsync throws ArgumentNullException when runner is null.
    /// </summary>
    [Fact]
    public async Task MigrateAndGetAppliedAsync_NullRunner_ThrowsArgumentNullException()
    {
        // Arrange
        MigrationRunner? runner = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => runner!.MigrateAndGetAppliedAsync());
    }

    /// <summary>
    /// Tests edge case with single migration.
    /// </summary>
    [Fact]
    public async Task GetLatestAppliedMigrationAsync_SingleMigration_ReturnsThatMigration()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migration = BuildMigrationMock("20240101_001", "Single migration").Object;
        var runner = new MigrationRunner(contextMock.Object, new[] { migration });

        var appliedMigrations = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Id"] = 1,
                ["Version"] = "20240101_001",
                ["Description"] = "Single migration",
                ["AppliedAt"] = DateTime.UtcNow,
                ["Success"] = true,
                ["ErrorMessage"] = null
            }
        };

        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("_MigrationHistory")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(appliedMigrations);

        // Act
        var result = await runner.GetLatestAppliedMigrationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be("20240101_001");
    }

    /// <summary>
    /// Tests edge case with many migrations.
    /// </summary>
    [Fact]
    public async Task GetPendingMigrationsCountAsync_ManyMigrations_ReturnsCorrectCount()
    {
        // Arrange
        var contextMock = BuildContextMock();
        var migrations = Enumerable.Range(1, 50)
            .Select(i => BuildMigrationMock($"202401{i:D2}_00{i}", $"Migration {i}").Object)
            .ToArray();

        var runner = new MigrationRunner(contextMock.Object, migrations);

        // Setup to return only first 10 as applied
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT [Version]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(Enumerable.Range(1, 10)
                .Select(i => new Dictionary<string, object> { ["Version"] = $"202401{i:D2}_00{i}" })
                .ToList());

        // Act
        var result = await runner.GetPendingMigrationsCountAsync();

        // Assert
        result.Should().Be(40);
    }
}
