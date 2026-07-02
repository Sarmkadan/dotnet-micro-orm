#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class RepositoryTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        _repository = new Repository<User>(_contextMock.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new Repository<User>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

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

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

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

    [Fact]
    public async Task GetAllAsync_WithNoEntities_ReturnsEmptyList()
    {
        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>());

        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

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

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsCorrectCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(5L);

        var result = await _repository.CountAsync(u => u.IsActive);

        result.Should().Be(5);
    }

    [Fact]
    public async Task CountAsync_WithNullPredicate_ReturnsTotalCount()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(10L);

        var result = await _repository.CountAsync();

        result.Should().Be(10);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingEntity_ReturnsTrue()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1L);

        var result = await _repository.ExistsAsync(u => u.Username == "admin");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingEntity_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0L);

        var result = await _repository.ExistsAsync(u => u.Username == "nonexistent");

        result.Should().BeFalse();
    }

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

    [Fact]
    public async Task AddAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var user = new User { Username = "", Email = "test@example.com", PasswordHash = "hash" };

        var act = () => _repository.AddAsync(user);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

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

    [Fact]
    public async Task UpdateAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var user = new User { Id = 1, Username = "", Email = "test@example.com", PasswordHash = "hash" };

        var act = () => _repository.UpdateAsync(user);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(1);

        result.Should().BeTrue();
        _contextMock.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        var result = await _repository.DeleteAsync(999);

        result.Should().BeFalse();
    }
}
