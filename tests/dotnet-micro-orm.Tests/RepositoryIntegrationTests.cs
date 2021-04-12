#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class RepositoryIntegrationTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<Product> _repository;

    public RepositoryIntegrationTests()
    {
        _repository = new Repository<Product>(_contextMock.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new Repository<Product>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProductWithMatchingId()
    {
        var product = new Product("SKU001", "Widget A", 10.00m, 1) { Id = 1 };
        var products = new[] { product }.AsQueryable();

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([new Dictionary<string, object> { { "Id", 1 } }]);

        var result = await _repository.GetByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleProducts_ReturnsAllProducts()
    {
        var rows = new[]
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Sku", "SKU001" }, { "Name", "Widget A" }, { "Price", 10.00m } },
            new Dictionary<string, object> { { "Id", 2 }, { "Sku", "SKU002" }, { "Name", "Widget B" }, { "Price", 20.00m } }
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>()))
            .ReturnsAsync(rows);

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountAsync_WithData_ReturnsCorrectCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(5L);

        var result = await _repository.CountAsync();

        result.Should().Be(5);
    }

    [Fact]
    public async Task CountAsync_WithNoData_ReturnsZero()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(null);

        var result = await _repository.CountAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_WithValidProduct_InsertsSuccessfully()
    {
        var product = new Product("SKU123", "New Widget", 99.99m, 1);

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.AddAsync(product);

        result.Should().NotBeNull();
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        var product = new Product("XY", "Widget", 0m, 0);

        var act = async () => await _repository.AddAsync(product);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidProduct_UpdatesSuccessfully()
    {
        var product = new Product("SKU123", "Updated Widget", 99.99m, 1) { Id = 1 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(null);

        var result = await _repository.UpdateAsync(product);

        result.Should().NotBeNull();
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        var product = new Product("XY", "Widget", 0m, 0) { Id = 1 };

        var act = async () => await _repository.UpdateAsync(product);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentProduct_ThrowsOrmException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { Id = 999 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        var act = async () => await _repository.UpdateAsync(product);

        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { Id = 1 };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([new Dictionary<string, object> { { "Id", 1 } }]);

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([]);

        var result = await _repository.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithEntity_DeletesSuccessfully()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { Id = 1 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(product);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyList_ReturnsEmptyList()
    {
        var result = await _repository.AddRangeAsync([]);

        result.Should().BeEmpty();
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    [Fact]
    public async Task AddRangeAsync_WithValidProducts_InsertsAll()
    {
        var products = new List<Product>
        {
            new("SKU001", "Widget A", 10.00m, 1),
            new("SKU002", "Widget B", 20.00m, 1),
            new("SKU003", "Widget C", 30.00m, 1)
        };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.AddRangeAsync(products);

        result.Should().HaveCount(3);
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        var products = new List<Product>
        {
            new("SKU001", "Widget A", 10.00m, 1),
            new("XY", "Invalid", 0m, 0)
        };

        var act = async () => await _repository.AddRangeAsync(products);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task DeleteRangeAsync_WithValidEntities_DeletesAll()
    {
        var products = new List<Product>
        {
            new("SKU001", "Widget A", 10.00m, 1) { Id = 1 },
            new("SKU002", "Widget B", 20.00m, 1) { Id = 2 }
        };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteRangeAsync(products);

        result.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ReturnsTrue()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1L);

        var result = await _repository.ExistsAsync(p => p.Sku == "SKU001");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentEntity_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0L);

        var result = await _repository.ExistsAsync(p => p.Sku == "NONEXISTENT");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithValidParameters_ReturnsPaginatedResults()
    {
        var rows = new[]
        {
            new Dictionary<string, object> { { "Id", 1 } },
            new Dictionary<string, object> { { "Id", 2 } },
            new Dictionary<string, object> { { "Id", 3 } }
        };

        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(10L);

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>()))
            .ReturnsAsync(rows);

        var result = await _repository.GetPagedAsync(1, 3);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithExistingEntity_ReturnsEntity()
    {
        var rows = new[] { new Dictionary<string, object> { { "Id", 1 } } };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>()))
            .ReturnsAsync(rows);

        var result = await _repository.FirstOrDefaultAsync(p => p.Id == 1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithNonExistentEntity_ReturnsNull()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>()))
            .ReturnsAsync([]);

        var result = await _repository.FirstOrDefaultAsync(p => p.Id == 999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task QueryStreamAsync_WithValidQuery_YieldsResults()
    {
        var rows = new[] { new Dictionary<string, object> { { "Id", 1 } } }.ToAsyncEnumerable();

        _contextMock.Setup(c => c.ExecuteStreamAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .Returns(rows);

        var results = new List<Product>();
        await foreach (var item in _repository.QueryStreamAsync("SELECT * FROM Products"))
        {
            results.Add(item);
        }

        results.Should().HaveCount(1);
    }
}
