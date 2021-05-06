#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class BatchOperationsTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<Product> _repository;

    public BatchOperationsTests()
    {
        _repository = new Repository<Product>(_contextMock.Object);
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = await _repository.AddRangeAsync(new List<Product>());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRangeAsync_WithValidProducts_AddsAllSuccessfully()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("BATCH001", "Batch Product 1", 19.99m, 1) { Id = 1 },
            new Product("BATCH002", "Batch Product 2", 29.99m, 2) { Id = 2 },
            new Product("BATCH003", "Batch Product 3", 39.99m, 1) { Id = 3 }
        };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(3);

        // Act
        var result = await _repository.AddRangeAsync(products);

        // Assert
        result.Should().HaveCount(3);
        result[0].Sku.Should().Be("BATCH001");
        result[1].Sku.Should().Be("BATCH002");
        result[2].Sku.Should().Be("BATCH003");
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("VALID001", "Valid Product", 19.99m, 1),
            new Product("INVALID001", "", 29.99m, 2) // Invalid name (empty)
        };

        // Act
        var act = () => _repository.AddRangeAsync(products);

        // Assert
        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_WithEmptyList_ReturnsZero()
    {
        // Arrange & Act
        var result = await _repository.DeleteRangeAsync(new List<Product>());

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRangeAsync_WithMultipleProducts_DeletesAllSuccessfully()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("DELETE001", "Delete Product 1", 10.99m, 1) { Id = 1 },
            new Product("DELETE002", "Delete Product 2", 20.99m, 2) { Id = 2 }
        };

        _contextMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1) // First delete succeeds
            .ReturnsAsync(1); // Second delete succeeds

        // Act
        var result = await _repository.DeleteRangeAsync(products);

        // Assert
        result.Should().Be(2);
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteRangeAsync_WithSomeNonExistentProducts_ReturnsPartialCount()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("EXIST001", "Exist Product", 10.99m, 1) { Id = 1 },
            new Product("NONEXIST001", "NonExist Product", 20.99m, 2) { Id = 2 }
        };

        _contextMock.SetupSequence(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1) // First delete succeeds
            .ReturnsAsync(0); // Second delete fails (not found)

        // Act
        var result = await _repository.DeleteRangeAsync(products);

        // Assert
        result.Should().Be(1); // Only one was actually deleted
    }
}

public sealed class BatchUpsertOperationTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly BatchUpsertOperation<Product> _batchUpsert;

    public BatchUpsertOperationTests()
    {
        _batchUpsert = new BatchUpsertOperation<Product>(_contextMock.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new BatchUpsertOperation<Product>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public async Task UpsertAsync_WithSingleProductBySku_ReturnsInsertResult()
    {
        // Arrange
        var product = new Product("UPSERT001", "Upsert Product", 24.99m, 1);

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new()
                {
                    { "Id", 1 },
                    { "__action", "INSERT" }
                }
            });

        // Act
        var result = await _batchUpsert.UpsertAsync(product, p => p.Sku);

        // Assert
        result.Should().NotBeNull();
        result.Entity.Sku.Should().Be("UPSERT001");
        result.WasInserted.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertAsync_WithSingleProductById_ReturnsUpdateResult()
    {
        // Arrange
        var product = new Product("UPSERT002", "Upsert Product 2", 29.99m, 2) { Id = 5 };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new()
                {
                    { "Id", 5 },
                    { "__action", "UPDATE" }
                }
            });

        // Act
        var result = await _batchUpsert.UpsertAsync(product, p => p.Id);

        // Assert
        result.Should().NotBeNull();
        result.Entity.Id.Should().Be(5);
        result.WasInserted.Should().BeFalse();
    }

    [Fact]
    public async Task UpsertAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        // Arrange
        var product = new Product("BAD", "", 29.99m, 1); // Invalid name

        // Act
        var act = () => _batchUpsert.UpsertAsync(product, p => p.Sku);

        // Assert
        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithMultipleProducts_ReturnsMultipleResults()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("MULTI001", "Multi Product 1", 19.99m, 1),
            new Product("MULTI002", "Multi Product 2", 29.99m, 2),
            new Product("MULTI003", "Multi Product 3", 39.99m, 1)
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } },
                new() { { "Id", 2 }, { "__action", "UPDATE" } },
                new() { { "Id", 3 }, { "__action", "INSERT" } }
            });

        // Act
        var results = await _batchUpsert.UpsertRangeAsync(products, p => p.Sku);

        // Assert
        results.Should().HaveCount(3);
        results[0].WasInserted.Should().BeTrue();
        results[1].WasInserted.Should().BeFalse();
        results[2].WasInserted.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithBatchSize_SplitsIntoBatches()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("BATCH1", "Batch 1", 10.00m, 1),
            new Product("BATCH2", "Batch 2", 20.00m, 1),
            new Product("BATCH3", "Batch 3", 30.00m, 1),
            new Product("BATCH4", "Batch 4", 40.00m, 1),
            new Product("BATCH5", "Batch 5", 50.00m, 1)
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } },
                new() { { "Id", 2 }, { "__action", "INSERT" } },
                new() { { "Id", 3 }, { "__action", "INSERT" } },
                new() { { "Id", 4 }, { "__action", "INSERT" } },
                new() { { "Id", 5 }, { "__action", "INSERT" } }
            });

        // Act
        var results = await _batchUpsert.UpsertRangeAsync(products, p => p.Sku, batchSize: 2);

        // Assert
        results.Should().HaveCount(5);
        _contextMock.Verify(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Exactly(3)); // 3 batches: 2, 2, 1
    }

    [Fact]
    public async Task UpsertRangeAsync_WithCompositeKey_ReturnsCorrectResults()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("COMP001", "Composite Product 1", 15.99m, 1),
            new Product("COMP002", "Composite Product 2", 25.99m, 2)
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } },
                new() { { "Id", 2 }, { "__action", "INSERT" } }
            });

        // Act
        var results = await _batchUpsert.UpsertRangeAsync(products, p => new { p.Sku, p.CategoryId });

        // Assert
        results.Should().HaveCount(2);
        results[0].WasInserted.Should().BeTrue();
        results[1].WasInserted.Should().BeTrue();
    }
}