#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class UnitOfWorkIntegrationTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkIntegrationTests()
    {
        _unitOfWork = new UnitOfWork(_contextMock.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new UnitOfWork(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Repository_WithEntityType_ReturnsRepositoryInstance()
    {
        var repository = _unitOfWork.Repository<Product>();

        repository.Should().NotBeNull();
        repository.Should().BeOfType<Repository<Product>>();
    }

    [Fact]
    public void Repository_WithSameEntityType_ReturnsSameInstance()
    {
        var repo1 = _unitOfWork.Repository<Product>();
        var repo2 = _unitOfWork.Repository<Product>();

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Repository_WithDifferentEntityTypes_ReturnsDifferentInstances()
    {
        var productRepo = _unitOfWork.Repository<Product>();
        var userRepo = _unitOfWork.Repository<User>();

        productRepo.Should().NotBeSameAs(userRepo);
    }

    [Fact]
    public async Task BeginTransactionAsync_WithoutActiveTransaction_Succeeds()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        var result = await _unitOfWork.BeginTransactionAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithActiveTransaction_ThrowsOrmException()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();

        var act = async () => await _unitOfWork.BeginTransactionAsync();

        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithFailure_ReturnsFalse()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(false);

        var result = await _unitOfWork.BeginTransactionAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CommitAsync_WithoutActiveTransaction_ThrowsOrmException()
    {
        var act = async () => await _unitOfWork.CommitAsync();

        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task CommitAsync_WithActiveTransaction_Succeeds()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.CommitAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _unitOfWork.CommitAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CommitAsync_WithDatabaseFailure_RollsbackAndThrows()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.CommitAsync())
            .ThrowsAsync(new Exception("Database error"));

        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();

        var act = async () => await _unitOfWork.CommitAsync();

        await act.Should().ThrowAsync<Exception>();
        _contextMock.Verify(c => c.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackAsync_WithoutActiveTransaction_ReturnsTrue()
    {
        var result = await _unitOfWork.RollbackAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackAsync_WithActiveTransaction_Succeeds()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _unitOfWork.RollbackAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackAsync_WithDatabaseFailure_ThrowsOrmException()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.RollbackAsync())
            .ThrowsAsync(new Exception("Rollback failed"));

        await _unitOfWork.BeginTransactionAsync();

        var act = async () => await _unitOfWork.RollbackAsync();

        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutChanges_ReturnsZero()
    {
        var result = await _unitOfWork.SaveChangesAsync();

        result.Should().Be(0);
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        var act = () =>
        {
            var uow = new UnitOfWork(_contextMock.Object);
            uow.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task MultipleRepositories_MaintainIndependentState()
    {
        var productRepo = _unitOfWork.Repository<Product>();
        var userRepo = _unitOfWork.Repository<User>();

        var repo1Again = _unitOfWork.Repository<Product>();
        var repo2Again = _unitOfWork.Repository<User>();

        productRepo.Should().BeSameAs(repo1Again);
        userRepo.Should().BeSameAs(repo2Again);
    }

    [Fact]
    public async Task TransactionWorkflow_BeginCommitSequence()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.CommitAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _unitOfWork.CommitAsync();

        result.Should().BeTrue();
        _contextMock.Verify(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()), Times.Once);
        _contextMock.Verify(c => c.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task TransactionWorkflow_BeginRollbackSequence()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var result = await _unitOfWork.RollbackAsync();

        result.Should().BeTrue();
        _contextMock.Verify(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()), Times.Once);
        _contextMock.Verify(c => c.RollbackAsync(), Times.Once);
    }
}
