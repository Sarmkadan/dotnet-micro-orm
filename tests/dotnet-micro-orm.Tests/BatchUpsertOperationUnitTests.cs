#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class BatchUpsertOperationUnitTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly BatchUpsertOperation<User> _batchUpsert;

    public BatchUpsertOperationUnitTests()
    {
        _batchUpsert = new BatchUpsertOperation<User>(_contextMock.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new BatchUpsertOperation<User>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public async Task UpsertAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        var act = () => _batchUpsert.UpsertAsync(null!, u => u.Id);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("entity");
    }

    [Fact]
    public async Task UpsertAsync_WithNullKeySelector_ThrowsArgumentNullException()
    {
        var user = new User("test", "test@example.com", "hashedpassword1234567890");
        var act = () => _batchUpsert.UpsertAsync(user, null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("keySelector");
    }

    [Fact]
    public async Task UpsertAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var user = new User { Username = "", Email = "test@example.com", PasswordHash = "hash" };
        var act = () => _batchUpsert.UpsertAsync(user, u => u.Id);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpsertAsync_WithValidEntity_ReturnsUpsertResult()
    {
        var user = new User("test", "test@example.com", "hashedpassword1234567890");

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } }
            });

        var result = await _batchUpsert.UpsertAsync(user, u => u.Id);

        result.Should().NotBeNull();
        result.Entity.Id.Should().BeGreaterThan(0);
        result.WasInserted.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithNullEntities_ThrowsArgumentNullException()
    {
        var act = () => _batchUpsert.UpsertRangeAsync(null!, u => u.Id);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("entities");
    }

    [Fact]
    public async Task UpsertRangeAsync_WithNullKeySelector_ThrowsArgumentNullException()
    {
        var users = new List<User> { new User("test", "test@example.com", "hashedpassword1234567890") };
        var act = () => _batchUpsert.UpsertRangeAsync(users, null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("keySelector");
    }

    [Fact]
    public async Task UpsertRangeAsync_WithInvalidBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var users = new List<User> { new User("test", "test@example.com", "hashedpassword1234567890") };
        var act = () => _batchUpsert.UpsertRangeAsync(users, u => u.Id, 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithTooLargeBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var users = new List<User> { new User("test", "test@example.com", "hashedpassword1234567890") };
        var act = () => _batchUpsert.UpsertRangeAsync(users, u => u.Id, 10001);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithEmptyList_ReturnsEmptyList()
    {
        var result = await _batchUpsert.UpsertRangeAsync(new List<User>(), u => u.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithInvalidEntity_ThrowsEntityValidationException()
    {
        var users = new List<User> { new User { Username = "", Email = "test@example.com", PasswordHash = "hash" } };
        var act = () => _batchUpsert.UpsertRangeAsync(users, u => u.Id);

        await act.Should().ThrowAsync<EntityValidationException>();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithValidEntities_ReturnsMultipleResults()
    {
        var users = new List<User>
        {
            new User("user1", "user1@example.com", "hashedpassword1234567890"),
            new User("user2", "user2@example.com", "hashedpassword1234567890")
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } },
                new() { { "Id", 2 }, { "__action", "INSERT" } }
            });

        var results = await _batchUpsert.UpsertRangeAsync(users, u => u.Id);

        results.Should().HaveCount(2);
        results[0].WasInserted.Should().BeTrue();
        results[1].WasInserted.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertRangeAsync_WithBatchSize_ReturnsCorrectNumberOfResults()
    {
        var users = new List<User>
        {
            new User("user1", "user1@example.com", "hashedpassword1234567890"),
            new User("user2", "user2@example.com", "hashedpassword1234567890"),
            new User("user3", "user3@example.com", "hashedpassword1234567890")
        };

        _contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } },
                new() { { "Id", 2 }, { "__action", "INSERT" } },
                new() { { "Id", 3 }, { "__action", "INSERT" } }
            });

        var results = await _batchUpsert.UpsertRangeAsync(users, u => u.Id, batchSize: 2);

        results.Should().HaveCount(3);
    }

    [Fact]
    public void GetTableName_ReturnsCorrectTableName()
    {
        var tableName = _batchUpsert.GetTableName();

        tableName.Should().Be("Users");
    }

    [Fact]
    public void GetTableSchema_ReturnsCorrectSchema()
    {
        var schema = _batchUpsert.GetTableSchema();

        schema.Should().Be("dbo");
    }
}
