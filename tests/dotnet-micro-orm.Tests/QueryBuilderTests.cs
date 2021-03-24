#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Moq;

namespace DotnetMicroOrm.Tests;

public class sealed QueryBuilderTests
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
}
