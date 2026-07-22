#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Regression tests for concurrency token handling.
/// </summary>
public sealed class ConcurrencyTokenRegressionTests
{
    private readonly Mock<IDatabaseContext> _contextMock = new();
    private readonly Repository<ProductWithConcurrencyToken> _repository;

    public ConcurrencyTokenRegressionTests()
    {
        _repository = new Repository<ProductWithConcurrencyToken>(_contextMock.Object);
    }

    /// <summary>
    /// Regression test for conflicting-update path: ensures ConcurrencyException is thrown when 0 rows are affected due to version mismatch.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenConflictingUpdateOccurs_ThrowsConcurrencyException()
    {
        // Arrange
        var product = new ProductWithConcurrencyToken
        {
            Id = 1,
            Sku = "TEST123",
            Name = "Test Product",
            Price = 100,
            CategoryId = 1,
            StockQuantity = 50,
            Version = 1
        };

        // Mock: 0 rows affected (simulates a concurrent update that changed the version in the DB)
        _contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(0);

        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>()
            .WithMessage("*Concurrency conflict detected*");
    }
}
