#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Repository{T}"/> class.
/// Tests repository operations against mocked database context to verify correct behavior.
/// </summary>
public sealed class RepositoryTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<User> _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryTests"/> class.
    /// </summary>
    public RepositoryTests()
    {
        _repository = new Repository<User>(_contextMock.Object);
    }

    /// <summary>
    /// Tests that the repository constructor throws an <see cref="ArgumentNullException"/> when provided with a null database context.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new Repository<User>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetByIdAsync"/> returns the expected entity when querying with an existing ID.
    /// Verifies that the repository correctly maps database results to the entity model.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
    {
        var user = new User("testuser", "test@example.com", "hashedpassword1234567890") { Id = 1 };
        var users = new[] { user }.AsQueryable();

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "Username", "testuser" }, { "Email", "test@example.com" },
                        { "PasswordHash", "hashedpassword1234567890" }, { "IsActive", true },
                        { "IsEmailVerified", false }, { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } }
            });

        var result = await _repository.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Username.Should().Be("testuser");
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetByIdAsync"/> returns null when querying with a non-existing ID.
    /// Verifies proper handling of missing entities in the repository.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetAllAsync"/> returns all entities when multiple entities exist in the database.
    /// Verifies that the repository correctly retrieves and maps all records.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ReturnsAllEntities()
    {
        var users = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Username", "user1" }, { "Email", "user1@example.com" },
                    { "PasswordHash", "hash1" }, { "IsActive", true }, { "IsEmailVerified", false },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } },
            new() { { "Id", 2 }, { "Username", "user2" }, { "Email", "user2@example.com" },
                    { "PasswordHash", "hash2" }, { "IsActive", true }, { "IsEmailVerified", true },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } }
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(users);

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetAllAsync"/> returns an empty list when no entities exist in the database.
    /// Verifies proper handling of empty result sets.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithNoEntities_ReturnsEmptyList()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetAsync"/> returns filtered entities when a predicate matches existing entities.
    /// Verifies that the repository correctly applies the predicate filter to database queries.
    /// </summary>
    [Fact]
    public async Task GetAsync_WithMatchingPredicate_ReturnsFilteredEntities()
    {
        var users = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Username", "admin" }, { "Email", "admin@example.com" },
                    { "PasswordHash", "hash1" }, { "IsActive", true }, { "IsEmailVerified", true },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } },
            new() { { "Id", 2 }, { "Username", "user1" }, { "Email", "user1@example.com" },
                    { "PasswordHash", "hash2" }, { "IsActive", true }, { "IsEmailVerified", false },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } }
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(users);

        var result = await _repository.GetAsync(u => u.IsEmailVerified);

        result.Should().HaveCount(1);
        result[0].Username.Should().Be("admin");
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.GetAsync"/> returns an empty list when no entities match the specified predicate.
    /// Verifies proper filtering behavior when no records satisfy the criteria.
    /// </summary>
    [Fact]
    public async Task GetAsync_WithNoMatchingPredicate_ReturnsEmptyList()
    {
        var users = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Username", "user1" }, { "Email", "user1@example.com" },
                    { "PasswordHash", "hash1" }, { "IsActive", true }, { "IsEmailVerified", false },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } },
            new() { { "Id", 2 }, { "Username", "user2" }, { "Email", "user2@example.com" },
                    { "PasswordHash", "hash2" }, { "IsActive", true }, { "IsEmailVerified", false },
                    { "Version", 1 }, { "CreatedDate", DateTime.UtcNow } }
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(users);

        var result = await _repository.GetAsync(u => u.IsEmailVerified);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.CountAsync"/> returns the correct count when filtering with a predicate.
    /// Verifies that the repository correctly counts entities matching the specified criteria.
    /// </summary>
    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsCorrectCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(5L);

        var result = await _repository.CountAsync(u => u.IsActive);

        result.Should().Be(5);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.CountAsync"/> returns the total count when no predicate is provided.
    /// Verifies that the repository correctly counts all entities in the database when no filter is applied.
    /// </summary>
    [Fact]
    public async Task CountAsync_WithNullPredicate_ReturnsTotalCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(10L);

        var result = await _repository.CountAsync();

        result.Should().Be(10);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.ExistsAsync"/> returns true when an entity matching the predicate exists.
    /// Verifies that the repository correctly identifies existing entities in the database.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ReturnsTrue()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1L);

        var result = await _repository.ExistsAsync(u => u.Username == "admin");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.ExistsAsync"/> returns false when no entity matches the predicate.
    /// Verifies that the repository correctly identifies non-existing entities in the database.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0L);

        var result = await _repository.ExistsAsync(u => u.Username == "nonexistent");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.AddAsync"/> successfully adds a valid entity to the database.
    /// Verifies that the repository correctly persists new entities and returns the added entity with an assigned ID.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithValidEntity_AddsSuccessfully()
    {
        var user = new User("newuser", "new@example.com", "hashedpassword1234567890");

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.AddAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.AddAsync"/> throws an <see cref="EntityValidationException"/> when adding an invalid entity.
    /// Verifies that the repository validates entity data before attempting to persist it to the database.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var user = new User { Username = "", Email = "test@example.com", PasswordHash = "hash" };

        var act = () => _repository.AddAsync(user);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.UpdateAsync"/> successfully updates an existing valid entity in the database.
    /// Verifies that the repository correctly persists changes to existing entities and returns the updated entity.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithValidEntity_UpdatesSuccessfully()
    {
        var user = new User("updateduser", "updated@example.com", "hashedpassword1234567890") { Id = 1 };

        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.UpdateAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.UpdateAsync"/> throws an <see cref="EntityValidationException"/> when updating an invalid entity.
    /// Verifies that the repository validates entity data before attempting to update it in the database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var user = new User { Id = 1, Username = "", Email = "test@example.com", PasswordHash = "hash" };

        var act = () => _repository.UpdateAsync(user);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.DeleteAsync"/> successfully deletes an entity with a valid ID from the database.
    /// Verifies that the repository correctly removes entities and returns true to indicate successful deletion.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(1);

        result.Should().BeTrue();
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="Repository{T}.DeleteAsync"/> returns false when attempting to delete an entity with a non-existing ID.
    /// Verifies that the repository correctly handles attempts to delete non-existent entities.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        var result = await _repository.DeleteAsync(999);

        result.Should().BeFalse();
    }
}
