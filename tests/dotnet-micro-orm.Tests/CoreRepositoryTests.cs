#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Repository{T}"/> class, which provides data access operations for domain entities.
/// Tests verify correct behavior of CRUD operations, filtering, paging, and validation through mocked database interactions.
/// </summary>
public sealed class CoreRepositoryTests
{
	/// <summary>
	/// Mock implementation of <see cref="IDatabaseContext"/> used to simulate database operations without requiring a real database connection.
	/// </summary>
	private readonly Mock<IDatabaseContext> _contextMock = new();

	/// <summary>
	/// Instance of <see cref="Repository{Product}"/> under test, initialized with the mocked database context.
	/// </summary>
	private readonly Repository<Product> _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="CoreRepositoryTests"/> class.
	/// Sets up the test fixture with a mocked database context and creates a repository instance for testing.
	/// </summary>
	public CoreRepositoryTests()
	{
		_repository = new Repository<Product>(_contextMock.Object);
	}

	/// <summary>
	/// Tests that the repository constructor throws an <see cref="ArgumentNullException"/> when provided with a null database context.
	/// </summary>
	[Fact]
	public void Constructor_WithNullContext_ThrowsArgumentNullException()
	{
		var act = () => new Repository<Product>(null!);

		act.Should().Throw<ArgumentNullException>().WithParameterName("context");
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetByIdAsync"/> returns a product when the ID exists in the database.
	/// </summary>
	[Fact]
	public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
	{
		// Arrange
		var product = new Product("SKU001", "Test Product", 29.99m, 1) { Id = 1 };
		var productData = new List<Dictionary<string, object>>
		{
			new()
			{
				{ "Id", 1 },
				{ "Sku", "SKU001" },
				{ "Name", "Test Product" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 29.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 1 },
				{ "StockQuantity", 100 },
				{ "IsActive", true },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			}
		};

		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(productData);

		// Act
		var result = await _repository.GetByIdAsync(1);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(1);
		result.Sku.Should().Be("SKU001");
		result.Name.Should().Be("Test Product");
		result.Price.Should().Be(29.99m);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetByIdAsync"/> returns null when the ID does not exist in the database.
	/// </summary>
	[Fact]
	public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(new List<Dictionary<string, object>>());

		// Act
		var result = await _repository.GetByIdAsync(999);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.AddAsync"/> successfully adds a valid product to the database.
	/// </summary>
	[Fact]
	public async Task AddAsync_WithValidProduct_AddsSuccessfully()
	{
		// Arrange
		var product = new Product("NEW001", "New Product", 19.99m, 2);

		_contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1);

		// Act
		var result = await _repository.AddAsync(product);

		// Assert
		result.Should().NotBeNull();
		result.Sku.Should().Be("NEW001");
		result.Name.Should().Be("New Product");
		result.Price.Should().Be(19.99m);
		_contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.AddAsync"/> throws an <see cref="EntityValidationException"/> when the SKU is invalid.
	/// </summary>
	[Fact]
	public async Task AddAsync_WithInvalidSku_ThrowsEntityValidationException()
	{
		// Arrange
		var product = new Product("AB", "Test Product", 19.99m, 1); // SKU too short

		// Act
		var act = () => _repository.AddAsync(product);

		// Assert
		await act.Should().ThrowAsync<EntityValidationException>();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.AddAsync"/> throws an <see cref="EntityValidationException"/> when the price is invalid.
	/// </summary>
	[Fact]
	public async Task AddAsync_WithInvalidPrice_ThrowsEntityValidationException()
	{
		// Arrange
		var product = new Product("VALID001", "Test Product", 0m, 1); // Price not positive

		// Act
		var act = () => _repository.AddAsync(product);

		// Assert
		await act.Should().ThrowAsync<EntityValidationException>();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.UpdateAsync"/> successfully updates an existing product in the database.
	/// </summary>
	[Fact]
	public async Task UpdateAsync_WithValidProduct_UpdatesSuccessfully()
	{
		// Arrange
		var product = new Product("UPD001", "Updated Product", 24.99m, 3) { Id = 1 };

		_contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1);

		// Act
		var result = await _repository.UpdateAsync(product);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(1);
		result.Name.Should().Be("Updated Product");
		result.Price.Should().Be(24.99m);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.UpdateAsync"/> throws an <see cref="EntityValidationException"/> when the product is invalid.
	/// </summary>
	[Fact]
	public async Task UpdateAsync_WithInvalidProduct_ThrowsEntityValidationException()
	{
		// Arrange
		var product = new Product { Id = 1, Sku = "", Name = "Test", Price = 10m, CategoryId = 1 }; // Empty SKU

		// Act
		var act = () => _repository.UpdateAsync(product);

		// Assert
		await act.Should().ThrowAsync<EntityValidationException>();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.DeleteAsync"/> successfully deletes a product with an existing ID.
	/// </summary>
	[Fact]
	public async Task DeleteAsync_WithExistingId_DeletesSuccessfully()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1);

		// Act
		var result = await _repository.DeleteAsync(1);

		// Assert
		result.Should().BeTrue();
		_contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.DeleteAsync"/> returns false when attempting to delete a non-existing product.
	/// </summary>
	[Fact]
	public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0);

		// Act
		var result = await _repository.DeleteAsync(999);

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetAsync"/> returns filtered products matching the predicate.
	/// </summary>
	[Fact]
	public async Task GetAsync_WithMatchingPredicate_ReturnsFilteredProducts()
	{
		// Arrange
		var products = new List<Dictionary<string, object>>
		{
			new()
			{
				{ "Id", 1 },
				{ "Sku", "ACTIVE001" },
				{ "Name", "Active Product" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 15.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 1 },
				{ "StockQuantity", 50 },
				{ "IsActive", true },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			},
			new()
			{
				{ "Id", 2 },
				{ "Sku", "INACTIVE001" },
				{ "Name", "Inactive Product" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 25.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 2 },
				{ "StockQuantity", 30 },
				{ "IsActive", false },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			}
		};

		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _repository.GetAsync(p => p.IsActive);

		// Assert
		result.Should().HaveCount(1);
		result[0].Sku.Should().Be("ACTIVE001");
		result[0].IsActive.Should().BeTrue();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetAsync"/> returns an empty list when no products match the predicate.
	/// </summary>
	[Fact]
	public async Task GetAsync_WithNoMatchingPredicate_ReturnsEmptyList()
	{
		// Arrange
		var products = new List<Dictionary<string, object>>
		{
			new()
			{
				{ "Id", 1 },
				{ "Sku", "PROD001" },
				{ "Name", "Product 1" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 10.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 1 },
				{ "StockQuantity", 20 },
				{ "IsActive", false },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			}
		};

		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _repository.GetAsync(p => p.IsActive);

		// Assert
		result.Should().BeEmpty();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.CountAsync"/> returns the correct count of products matching the predicate.
	/// </summary>
	[Fact]
	public async Task CountAsync_WithPredicate_ReturnsCorrectCount()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(7L);

		// Act
		var result = await _repository.CountAsync(p => p.IsActive);

		// Assert
		result.Should().Be(7);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.CountAsync"/> returns the total count when no predicate is provided.
	/// </summary>
	[Fact]
	public async Task CountAsync_WithNullPredicate_ReturnsTotalCount()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(15L);

		// Act
		var result = await _repository.CountAsync();

		// Assert
		result.Should().Be(15);
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.ExistsAsync"/> returns true when a product with the specified criteria exists.
	/// </summary>
	[Fact]
	public async Task ExistsAsync_WithExistingProduct_ReturnsTrue()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(1L);

		// Act
		var result = await _repository.ExistsAsync(p => p.Sku == "EXIST001");

		// Assert
		result.Should().BeTrue();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.ExistsAsync"/> returns false when no product matches the specified criteria.
	/// </summary>
	[Fact]
	public async Task ExistsAsync_WithNonExistingProduct_ReturnsFalse()
	{
		// Arrange
		_contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(0L);

		// Act
		var result = await _repository.ExistsAsync(p => p.Sku == "NONEXIST");

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetPagedAsync"/> returns the correct paged results.
	/// </summary>
	[Fact]
	public async Task GetPagedAsync_WithValidParameters_ReturnsPagedResults()
	{
		// Arrange
		var products = new List<Dictionary<string, object>>
		{
			new()
			{
				{ "Id", 11 },
				{ "Sku", "PAGE001" },
				{ "Name", "Page Product 1" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 12.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 1 },
				{ "StockQuantity", 25 },
				{ "IsActive", true },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			},
			new()
			{
				{ "Id", 12 },
				{ "Sku", "PAGE002" },
				{ "Name", "Page Product 2" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 18.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 2 },
				{ "StockQuantity", 15 },
				{ "IsActive", true },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			}
		};

		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(products);

		// Act
		var result = await _repository.GetPagedAsync(2, 2); // Page 2, 2 items per page

		// Assert
		result.Should().HaveCount(2);
		result[0].Sku.Should().Be("PAGE001");
		result[1].Sku.Should().Be("PAGE002");
	}

	/// <summary>
	/// Tests that <see cref="Repository{Product}.GetPagedWithCountAsync"/> returns paged results along with the total count.
	/// </summary>
	[Fact]
	public async Task GetPagedWithCountAsync_WithValidParameters_ReturnsPagedResultsWithTotalCount()
	{
		// Arrange
		var products = new List<Dictionary<string, object>>
		{
			new()
			{
				{ "Id", 21 },
				{ "Sku", "PAGECOUNT001" },
				{ "Name", "Page Count Product 1" },
				{ "Description", (object?)DBNull.Value },
				{ "Price", 22.99m },
				{ "CostPrice", (object?)DBNull.Value },
				{ "CategoryId", 1 },
				{ "StockQuantity", 35 },
				{ "IsActive", true },
				{ "CreatedDate", DateTime.UtcNow },
				{ "ModifiedDate", (object?)DBNull.Value }
			}
		};

		_contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(25L); // Total count

		_contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
			.ReturnsAsync(products);

		// Act
		var (items, totalCount) = await _repository.GetPagedWithCountAsync(3, 5); // Page 3, 5 items per page

		// Assert
		items.Should().HaveCount(1);
		items[0].Sku.Should().Be("PAGECOUNT001");
		totalCount.Should().Be(25);
	}
}