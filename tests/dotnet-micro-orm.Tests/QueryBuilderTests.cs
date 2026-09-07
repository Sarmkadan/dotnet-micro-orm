#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class QueryBuilderTests
{
    private readonly Mock<IRepository<Product>> _repositoryMock = new();

    [Fact]
    public void Take_ZeroCount_ThrowsArgumentException()
    {
        var builder = new QueryBuilder<Product>(_repositoryMock.Object);

        var act = () => builder.Take(0);

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void Skip_NegativeCount_ThrowsArgumentException()
    {
        var builder = new QueryBuilder<Product>(_repositoryMock.Object);

        var act = () => builder.Skip(-1);

        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Fact]
    public void Take_NegativeCount_ThrowsArgumentException()
    {
        var builder = new QueryBuilder<Product>(_repositoryMock.Object);

        var act = () => builder.Take(-1);

        act.Should().Throw<ArgumentException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public async Task ToListAsync_WithTwoWhereCalls_AppliesBothFilters()
    {
        SetupProducts(
            new("SKU001", "Budget Active", 10.00m, 1) { Id = 1, IsActive = true },
            new("SKU002", "Premium Active", 30.00m, 1) { Id = 2, IsActive = true },
            new("SKU003", "Premium Inactive", 40.00m, 1) { Id = 3, IsActive = false });

        var result = await new QueryBuilder<Product>(_repositoryMock.Object)
            .Where(product => product.IsActive)
            .Where(product => product.Price >= 20.00m)
            .ToListAsync();

        result.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact]
    public async Task ToListAsync_WithThreeWhereCalls_AppliesAllFilters()
    {
        SetupProducts(
            new("SKU001", "Matching Product", 25.00m, 2) { Id = 1, IsActive = true },
            new("SKU002", "Wrong Category", 25.00m, 1) { Id = 2, IsActive = true },
            new("SKU003", "Too Cheap", 15.00m, 2) { Id = 3, IsActive = true },
            new("SKU004", "Inactive Product", 25.00m, 2) { Id = 4, IsActive = false });

        var result = await new QueryBuilder<Product>(_repositoryMock.Object)
            .Where(product => product.IsActive)
            .Where(product => product.CategoryId == 2)
            .Where(product => product.Price >= 20.00m)
            .ToListAsync();

        result.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public async Task ToListAsync_WithDifferentlyNamedWhereParameters_CombinesPredicates()
    {
        SetupProducts(
            new("SKU001", "Matching Product", 25.00m, 1) { Id = 1, IsActive = true },
            new("SKU002", "Inactive Product", 25.00m, 1) { Id = 2, IsActive = false },
            new("SKU003", "Too Cheap", 5.00m, 1) { Id = 3, IsActive = true });

        var result = await new QueryBuilder<Product>(_repositoryMock.Object)
            .Where(product => product.IsActive)
            .Where(item => item.Price > 10.00m)
            .ToListAsync();

        result.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public async Task ToListAsync_OrderByThenOrderByDescending_UsesLastOrdering()
    {
        SetupProducts(
            new("SKU001", "First", 30.00m, 1) { Id = 1 },
            new("SKU002", "Second", 10.00m, 1) { Id = 2 },
            new("SKU003", "Third", 20.00m, 1) { Id = 3 });

        var result = await new QueryBuilder<Product>(_repositoryMock.Object)
            .OrderBy(product => product.Price)
            .OrderByDescending(product => product.Id)
            .ToListAsync();

        result.Select(product => product.Id).Should().Equal(3, 2, 1);
    }

    [Fact]
    public async Task ToListAsync_WithSkipAndTake_ReturnsRequestedWindow()
    {
        SetupProducts(
            new("SKU001", "First", 10.00m, 1) { Id = 1 },
            new("SKU002", "Second", 20.00m, 1) { Id = 2 },
            new("SKU003", "Third", 30.00m, 1) { Id = 3 },
            new("SKU004", "Fourth", 40.00m, 1) { Id = 4 },
            new("SKU005", "Fifth", 50.00m, 1) { Id = 5 });

        var result = await new QueryBuilder<Product>(_repositoryMock.Object)
            .OrderBy(product => product.Id)
            .Skip(1)
            .Take(3)
            .ToListAsync();

        result.Select(product => product.Id).Should().Equal(2, 3, 4);
    }

    [Fact]
    public async Task ToListAsync_WithActiveFilter_ReturnsOnlyActiveProducts()
    {
        var products = new List<Product>
        {
            new("SKU001", "Widget A", 10.00m, 1) { Id = 1, IsActive = true },
            new("SKU002", "Widget B", 20.00m, 1) { Id = 2, IsActive = false },
            new("SKU003", "Widget C", 30.00m, 1) { Id = 3, IsActive = true }
        }.AsQueryable();

        _repositoryMock.Setup(r => r.Query()).Returns(products);

        var builder = new QueryBuilder<Product>(_repositoryMock.Object);
        var result = await builder.Where(p => p.IsActive).ToListAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
        _repositoryMock.Verify(r => r.Query(), Times.Once);
    }

    [Fact]
    public void Validate_NegativePrice_ContainsPriceError()
    {
        var product = new Product("SKU001", "Widget A", -5.00m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Price"));
    }

    private void SetupProducts(params Product[] products) =>
        _repositoryMock.Setup(repository => repository.Query()).Returns(products.AsQueryable());
}
