#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq.Expressions;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class BatchUpsertOperationTests
{
    private static Mock<IDatabaseContext> BuildContextMock(
        List<Dictionary<string, object>>? queryResult = null)
    {
        var mock = new Mock<IDatabaseContext>();
        mock.Setup(c => c.GetDatabaseProvider()).Returns(Constants.DatabaseProvider.SqlServer);
        mock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(queryResult ?? [new Dictionary<string, object> { ["__action"] = "INSERT" }]);
        return mock;
    }

    [Fact]
    public async Task UpsertRangeAsync_EmptyList_ReturnsEmptyResults()
    {
        var contextMock = BuildContextMock();
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);

        var results = await upsert.UpsertRangeAsync([], p => p.Sku);

        results.Should().BeEmpty();
        contextMock.Verify(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }

    [Fact]
    public async Task UpsertAsync_NewEntity_ReturnsInsertedResult()
    {
        var row = new Dictionary<string, object> { ["__action"] = "INSERT" };
        var contextMock = BuildContextMock([row]);
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);

        var product = new Product("SKU001", "Widget A", 9.99m, 1);
        var result = await upsert.UpsertAsync(product, p => p.Sku);

        result.WasInserted.Should().BeTrue();
        result.Entity.Should().Be(product);
    }

    [Fact]
    public async Task UpsertAsync_ExistingEntity_ReturnsUpdatedResult()
    {
        var row = new Dictionary<string, object> { ["__action"] = "UPDATE" };
        var contextMock = BuildContextMock([row]);
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);

        var product = new Product("SKU001", "Widget A Updated", 12.99m, 1);
        var result = await upsert.UpsertAsync(product, p => p.Sku);

        result.WasInserted.Should().BeFalse();
        result.Entity.Should().Be(product);
    }

    [Fact]
    public async Task UpsertRangeAsync_InvalidBatchSize_ThrowsArgumentOutOfRange()
    {
        var contextMock = BuildContextMock();
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);
        var product = new Product("SKU001", "Widget A", 9.99m, 1);

        var act = () => upsert.UpsertRangeAsync([product], p => p.Sku, batchSize: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_InvalidEntity_ThrowsEntityValidationException()
    {
        var contextMock = BuildContextMock();
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);

        // Price <= 0 is invalid per Product.Validate
        var invalidProduct = new Product("SK", "X", -1m, 0);

        var act = () => upsert.UpsertRangeAsync([invalidProduct], p => p.Sku);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_MultipleBatches_ExecutesMultipleQueries()
    {
        var contextMock = BuildContextMock([new Dictionary<string, object> { ["__action"] = "INSERT" }]);
        contextMock.Setup(c => c.GetDatabaseProvider()).Returns(Constants.DatabaseProvider.SqlServer);
        var upsert = new BatchUpsertOperation<Product>(contextMock.Object);

        var products = Enumerable.Range(1, 5)
            .Select(i => new Product($"SKU{i:D3}", $"Widget {i}", 10m * i, 1))
            .ToList();

        // Use batchSize=2 → should trigger 3 MERGE calls
        await upsert.UpsertRangeAsync(products, p => p.Sku, batchSize: 2);

        contextMock.Verify(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Exactly(3));
    }
}
