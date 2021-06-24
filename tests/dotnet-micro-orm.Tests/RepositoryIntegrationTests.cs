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

/// <summary>
/// Provides integration tests for the <see cref="Repository{TEntity}"/> class.
/// Tests verify that repository operations work correctly with mocked database context,
/// ensuring proper interaction patterns and exception handling for various scenarios.
/// </summary>
public sealed class RepositoryIntegrationTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<Product> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryIntegrationTests"/> class.
    /// Sets up the mock database context and repository for testing.
    /// </summary>
    public RepositoryIntegrationTests()
    {
        _repository = new Repository<Product>(_contextMock.Object);
    }

    /// <summary>
    /// Tests that the repository constructor throws an <see cref="ArgumentNullException"/> when null context is provided.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new Repository<Product>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.GetByIdAsync"/> returns a product with matching ID when valid ID is provided.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.GetAllAsync"/> returns all products when multiple products exist in the database.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithMultipleProducts_ReturnsAllProducts()
    {
        var rows = new[]
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Sku", "SKU001" }, { "Name", "Widget A" }, { "Price", 10.00m } },
            new Dictionary<string, object> { { "Id", 2 }, { "Sku", "SKU002" }, { "Name", "Widget B" }, { "Price", 20.00m } }
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(rows.ToList());

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.CountAsync"/> returns the correct count when data exists in the database.
    /// </summary>
    [Fact]
    public async Task CountAsync_WithData_ReturnsCorrectCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(5L);

        var result = await _repository.CountAsync();

        result.Should().Be(5);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.CountAsync"/> returns zero when no data exists in the database.
    /// </summary>
    [Fact]
    public async Task CountAsync_WithNoData_ReturnsZero()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync((object?)null);

        var result = await _repository.CountAsync();

        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.AddAsync"/> successfully inserts a valid product into the database.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.AddAsync"/> throws <see cref="EntityValidationException"/> when invalid product is provided.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        var product = new Product("XY", "Widget", 0m, 0);

        var act = async () => await _repository.AddAsync(product);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.UpdateAsync"/> successfully updates a valid product in the database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithValidProduct_UpdatesSuccessfully()
    {
        var product = new Product("SKU123", "Updated Widget", 99.99m, 1) { Id = 1 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync((object?)null);

        var result = await _repository.UpdateAsync(product);

        result.Should().NotBeNull();
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.UpdateAsync"/> throws <see cref="EntityValidationException"/> when invalid product is provided.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithInvalidProduct_ThrowsEntityValidationException()
    {
        var product = new Product("XY", "Widget", 0m, 0) { Id = 1 };

        var act = async () => await _repository.UpdateAsync(product);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.UpdateAsync"/> throws <see cref="OrmException"/> when attempting to update a non-existent product.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithNonExistentProduct_ThrowsOrmException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { Id = 999 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        var act = async () => await _repository.UpdateAsync(product);

        await act.Should().ThrowAsync<OrmException>();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.DeleteAsync(int)"/> successfully deletes a product by ID when the product exists.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.DeleteAsync(int)"/> returns false when attempting to delete a non-existent product by ID.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([]);

        var result = await _repository.DeleteAsync(999);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.DeleteAsync(TEntity)"/> successfully deletes a product entity when it exists in the database.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithEntity_DeletesSuccessfully()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { Id = 1 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(product);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.AddRangeAsync"/> returns an empty list when an empty list of products is provided.
    /// </summary>
    [Fact]
    public async Task AddRangeAsync_WithEmptyList_ReturnsEmptyList()
    {
        var result = await _repository.AddRangeAsync([]);

        result.Should().BeEmpty();
        _contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.AddRangeAsync"/> successfully inserts all valid products when a list of products is provided.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.AddRangeAsync"/> throws <see cref="EntityValidationException"/> when an invalid product is included in the list.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.DeleteRangeAsync"/> successfully deletes all provided product entities from the database.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.ExistsAsync"/> returns true when an entity matching the predicate exists in the database.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ReturnsTrue()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1L);

        var result = await _repository.ExistsAsync(p => p.Sku == "SKU001");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.ExistsAsync"/> returns false when no entity matching the predicate exists in the database.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WithNonExistentEntity_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0L);

        var result = await _repository.ExistsAsync(p => p.Sku == "NONEXISTENT");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.GetPagedAsync"/> returns paginated results when valid page number and page size parameters are provided.
    /// </summary>
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

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(rows.ToList());

        var result = await _repository.GetPagedAsync(1, 3);

        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.FirstOrDefaultAsync"/> returns the entity when it exists in the database matching the predicate.
    /// </summary>
    [Fact]
    public async Task FirstOrDefaultAsync_WithExistingEntity_ReturnsEntity()
    {
        var rows = new[] { new Dictionary<string, object> { { "Id", 1 } } };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(rows.ToList());

        var result = await _repository.FirstOrDefaultAsync(p => p.Id == 1);

        result.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.FirstOrDefaultAsync"/> returns null when no entity matching the predicate exists in the database.
    /// </summary>
    [Fact]
    public async Task FirstOrDefaultAsync_WithNonExistentEntity_ReturnsNull()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync([]);

        var result = await _repository.FirstOrDefaultAsync(p => p.Id == 999);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="Repository{TEntity}.QueryStreamAsync"/> yields results when a valid SQL query is provided.
    /// </summary>
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
