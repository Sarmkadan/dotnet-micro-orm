#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Tests for concurrency token handling in optimistic concurrency control.
/// </summary>
public sealed class ConcurrencyTokenTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<ProductWithConcurrencyToken> _repository;

    public ConcurrencyTokenTests()
    {
        _repository = new Repository<ProductWithConcurrencyToken>(_contextMock.Object);
    }

    /// <summary>
    /// Tests that UpdateAsync includes the concurrency token in both WHERE and SET clauses.
    /// Verifies that the SQL UPDATE statement includes the concurrency token column in SET clause
    /// to increment it, ensuring proper optimistic concurrency control.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithConcurrencyToken_IncludesTokenInWhereAndSetClauses()
    {
        // Arrange
        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Setup mock to return 1 row affected for update, and new version value
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        _contextMock.Setup(c => c.ExecuteScalarAsync(
                It.Is<string>(sql => sql.Contains("[Version]") && sql.Contains("WHERE [Id]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(2); // New version value

        // Act
        await _repository.UpdateAsync(product);

        // Assert
        // Verify ExecuteNonQueryAsync was called - the SQL should include:
        // 1. The concurrency token in the SET clause with an increment operation: [Version] = @Version + 1
        // 2. The concurrency token in the WHERE clause for conflict detection: [Version] = @OriginalConcurrencyValue
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(
            It.Is<string>(sql =>
                sql.Contains("[Version] = @Version + 1") && // SET clause: increments the value
                sql.Contains("[Version] = @OriginalConcurrencyValue") // WHERE clause: checks original value
            ),
            It.IsAny<Dictionary<string, object>>()),
            Times.Exactly(1));
    }

    /// <summary>
    /// Tests that UpdateAsync throws ConcurrencyException when the concurrency token doesn't match.
    /// Verifies that optimistic concurrency control properly detects conflicts.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithConcurrencyTokenMismatch_ThrowsConcurrencyException()
    {
        // Arrange
        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Mock the database to return 0 rows affected (simulating another user updated the record)
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>()
            .WithMessage("*Concurrency conflict detected*");
    }

    /// <summary>
    /// Tests that UpdateAsync updates the entity's concurrency token value after successful update.
    /// Verifies that the entity's concurrency token is bumped to the new database value.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_AfterSuccessfulUpdate_UpdatesEntityConcurrencyToken()
    {
        // Arrange
        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Mock the database to return 1 row affected
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        // Mock getting the new concurrency token value from database
        _contextMock.Setup(c => c.ExecuteScalarAsync(
                It.Is<string>(sql => sql.Contains("[Version]") && sql.Contains("WHERE [Id]")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(2); // New version value

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Version.Should().Be(2); // Entity's version should be updated to the new database value

        // Verify GetConcurrencyTokenValueAsync was called to fetch the new value
        _contextMock.Verify(c => c.ExecuteScalarAsync(
            It.Is<string>(sql => sql.Contains("[Version]") && sql.Contains("WHERE [Id]")),
            It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync handles byte[] concurrency tokens correctly (doesn't try to update entity value).
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithByteArrayConcurrencyToken_DoesNotUpdateEntityValue()
    {
        // Arrange
        var mockContext = new Mock<IDatabaseContext>();
        var repository = new Repository<EntityWithByteArrayConcurrencyToken>(mockContext.Object);

        var entity = new EntityWithByteArrayConcurrencyToken
        {
            Id = 1,
            Name = "Test Entity",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        mockContext.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        // Act
        var result = await repository.UpdateAsync(entity);

        // Assert
        result.Should().NotBeNull();
        result.RowVersion.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
    }

    /// <summary>
    /// Tests that DeleteAsync also honors concurrency tokens.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithConcurrencyToken_HonorsTokenInWhereClause()
    {
        // Arrange
        var mockContext = new Mock<IDatabaseContext>();
        var repository = new Repository<ProductWithConcurrencyToken>(mockContext.Object);

        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Mock getting the entity for delete
        mockContext.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT *")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new()
                {
                    { "Id", 1 },
                    { "Sku", "TEST123" },
                    { "Name", "Test Product" },
                    { "Price", 100m },
                    { "CategoryId", 1 },
                    { "StockQuantity", 50 },
                    { "Version", 1 }
                }
            });

        // Mock the delete operation to return 1 row affected
        mockContext.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        // Act
        var result = await repository.DeleteAsync(product);

        // Assert
        result.Should().BeTrue();

        // Verify the SQL query includes the concurrency token in WHERE clause
        mockContext.Verify(c => c.ExecuteNonQueryAsync(
            It.Is<string>(sql =>
                sql.Contains("[Version] = @OriginalConcurrencyValue") // WHERE clause with version check
            ),
            It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync throws ConcurrencyException when concurrency token doesn't match.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithConcurrencyTokenMismatch_ThrowsConcurrencyException()
    {
        // Arrange
        var mockContext = new Mock<IDatabaseContext>();
        var repository = new Repository<ProductWithConcurrencyToken>(mockContext.Object);

        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Mock getting the entity for delete
        mockContext.Setup(c => c.ExecuteQueryAsync(
                It.Is<string>(sql => sql.Contains("SELECT *")),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new()
                {
                    { "Id", 1 },
                    { "Sku", "TEST123" },
                    { "Name", "Test Product" },
                    { "Price", 100m },
                    { "CategoryId", 1 },
                    { "StockQuantity", 50 },
                    { "Version", 1 }
                }
            });

        // Mock the delete operation to return 0 rows affected (simulating version mismatch)
        mockContext.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        // Act
        Func<Task> act = async () => await repository.DeleteAsync(product);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>();
    }
}

/// <summary>
/// Test entity with a concurrency token using int Version column.
/// </summary>
[Table("Products")]
public class ProductWithConcurrencyToken : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Sku", IsNullable = false, MaxLength = 50)]
    [Unique]
    public string Sku { get; set; } = string.Empty;

    [Column("Name", IsNullable = false, MaxLength = 200)]
    public string Name { get; set; } = string.Empty;

    [Column("Price", IsNullable = false, Precision = 18, Scale = 2)]
    public decimal Price { get; set; }

    [Column("CategoryId", IsNullable = false)]
    public int CategoryId { get; set; }

    [Column("StockQuantity", IsNullable = false)]
    public int StockQuantity { get; set; }

    [Column("Version", IsNullable = false)]
    [ConcurrencyToken]
    public int Version { get; set; }
}

/// <summary>
/// Test entity with a concurrency token using byte[] RowVersion column.
/// </summary>
[Table("Entities")]
public class EntityWithByteArrayConcurrencyToken : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("RowVersion")]
    [ConcurrencyToken]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
