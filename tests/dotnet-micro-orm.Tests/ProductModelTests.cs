#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class ProductModelTests
{
    [Fact]
    public void Validate_WithValidProduct_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptySku_ReturnsFalseWithError()
    {
        var product = new Product("", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU"));
    }

    [Fact]
    public void Validate_WithShortSku_ReturnsFalseWithError()
    {
        var product = new Product("AB", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU") && e.Contains("3 characters"));
    }

    [Fact]
    public void Validate_WithNullSku_ReturnsFalseWithError()
    {
        var product = new Product { Sku = null!, Name = "Widget", Price = 99.99m, CategoryId = 1 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU"));
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("name"));
    }

    [Fact]
    public void Validate_WithShortName_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "A", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("name") && e.Contains("2 characters"));
    }

    [Fact]
    public void Validate_WithZeroPrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 0m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Price"));
    }

    [Fact]
    public void Validate_WithNegativePrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", -99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Price"));
    }

    [Fact]
    public void Validate_WithNegativeCostPrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { CostPrice = -10m };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Cost price"));
    }

    [Fact]
    public void Validate_WithNegativeStockQuantity_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = -5 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Stock quantity"));
    }

    [Fact]
    public void Validate_WithZeroCategoryId_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 0);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("category"));
    }

    [Fact]
    public void Validate_WithNegativeCategoryId_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, -1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("category"));
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        var product = new Product { Sku = "X", Name = "", Price = -10m, CategoryId = 0, StockQuantity = -5 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void IncreaseStock_WithPositiveQuantity_IncreasesStockCorrectly()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        product.IncreaseStock(5);

        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void IncreaseStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.IncreaseStock(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IncreaseStock_WithNegativeQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.IncreaseStock(-5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecreaseStock_WithValidQuantity_DecreasesStockCorrectly()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 20 };

        product.DecreaseStock(5);

        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void DecreaseStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.DecreaseStock(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecreaseStock_WithNegativeQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.DecreaseStock(-5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DecreaseStock_WithMoreThanAvailable_ThrowsInvalidOperationException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var act = () => product.DecreaseStock(10);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient stock*");
    }

    [Fact]
    public void DecreaseStock_WithExactlyAvailableQuantity_DecreasesToZero()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        product.DecreaseStock(10);

        product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void GetProfit_WithCostPrice_CalculatesProfit()
    {
        var product = new Product("SKU123", "Widget", 100m, 1) { CostPrice = 60m };

        var profit = product.GetProfit();

        profit.Should().Be(40m);
    }

    [Fact]
    public void GetProfit_WithoutCostPrice_ReturnsSellPrice()
    {
        var product = new Product("SKU123", "Widget", 100m, 1);

        var profit = product.GetProfit();

        profit.Should().Be(100m);
    }

    [Fact]
    public void GetProfit_WithZeroCostPrice_ReturnsSellPrice()
    {
        var product = new Product("SKU123", "Widget", 100m, 1) { CostPrice = 0m };

        var profit = product.GetProfit();

        profit.Should().Be(100m);
    }

    [Fact]
    public void IsLowStock_WithStockBelowThreshold_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WithStockEqualToThreshold_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WithStockAboveThreshold_ReturnsFalse()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 15 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeFalse();
    }

    [Fact]
    public void IsLowStock_WithDefaultThreshold_Uses10AsDefault()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var isLow = product.IsLowStock();

        isLow.Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WithZeroStock_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 0 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithParameters_InitializesFieldsCorrectly()
    {
        var product = new Product("SKU999", "Test Product", 49.99m, 5);

        product.Sku.Should().Be("SKU999");
        product.Name.Should().Be("Test Product");
        product.Price.Should().Be(49.99m);
        product.CategoryId.Should().Be(5);
        product.IsActive.Should().BeTrue();
        product.CreatedDate.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}
