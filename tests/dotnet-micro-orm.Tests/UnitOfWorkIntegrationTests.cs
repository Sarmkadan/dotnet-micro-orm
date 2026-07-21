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
    public async Task Dispose_CleansUpResources()
    {
        var uow = new UnitOfWork(_contextMock.Object);
        await uow.DisposeAsync();
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
    public async Task ExceptionInsideTransactionScope_RollsBackChanges()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);

        // Simulate an exception during commit
        _contextMock.Setup(c => c.CommitAsync())
            .ThrowsAsync(new Exception("Database commit failed"));

        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();

        // Act & Assert - commit should throw and rollback
        var act = async () => await _unitOfWork.CommitAsync();
        await act.Should().ThrowAsync<Exception>();

        // Verify rollback was called
        _contextMock.Verify(c => c.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task CommitAfterDisposeAsync_ThrowsOrmException()
    {
        var uow = new UnitOfWork(_contextMock.Object);
        await uow.DisposeAsync();

        var act = async () => await uow.CommitAsync();
        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task RollbackAfterDisposeAsync_DoesNotThrow()
    {
        var uow = new UnitOfWork(_contextMock.Object);
        await uow.DisposeAsync();

        var act = async () => await uow.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DoubleCommit_SecondCommitThrowsOrmException()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.CommitAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var firstCommitResult = await _unitOfWork.CommitAsync();
        firstCommitResult.Should().BeTrue();

        // Second commit should throw because no active transaction
        var act = async () => await _unitOfWork.CommitAsync();
        await act.Should().ThrowAsync<OrmException>();
    }

    [Fact]
    public async Task DoubleRollback_SecondRollbackDoesNotThrow()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();
        var firstRollbackResult = await _unitOfWork.RollbackAsync();
        firstRollbackResult.Should().BeTrue();

        // Second rollback should not throw because transaction is no longer active
        var act = async () => await _unitOfWork.RollbackAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExceptionDuringCommit_CallsRollbackAndThrows()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.CommitAsync())
            .ThrowsAsync(new InvalidOperationException("Invalid operation during commit"));
        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();

        // Act & Assert
        var act = async () => await _unitOfWork.CommitAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify rollback was called despite commit failure
        _contextMock.Verify(c => c.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WithActiveTransaction_CallsRollback()
    {
        _contextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<TransactionIsolationLevel>()))
            .ReturnsAsync(true);
        _contextMock.Setup(c => c.RollbackAsync())
            .ReturnsAsync(true);

        await _unitOfWork.BeginTransactionAsync();

        await _unitOfWork.DisposeAsync();

        // Verify rollback was called during disposal
        _contextMock.Verify(c => c.RollbackAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_MultipleTimes_DoesNotThrow()
    {
        var uow = new UnitOfWork(_contextMock.Object);
        await uow.DisposeAsync();
        await uow.DisposeAsync(); // Second dispose should not throw
    }

    [Fact]
    public async Task SaveChangesAsync_WithChanges_ClearsChangeSet()
    {
        // Manually add to change set to simulate changes being tracked
        var changeTrackingField = typeof(UnitOfWork).GetField("_changeSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var changeSet = (List<BaseEntity>)changeTrackingField!.GetValue(_unitOfWork)!;
        changeSet.Add(new Product("TEST-002", "Test Product 2", 15.99m, 1));

        var result = await _unitOfWork.SaveChangesAsync();
        result.Should().Be(1);
        _unitOfWork.HasChanges().Should().BeFalse();
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
