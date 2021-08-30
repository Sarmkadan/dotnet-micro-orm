#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
/// <summary>
/// Tests for the Product model.
/// </summary>
using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class ProductModelTests
{
    /// <summary>
    /// Tests that a valid product returns true when validated.
    /// </summary>
    [Fact]
    public void Validate_WithValidProduct_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a product with an empty SKU returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithEmptySku_ReturnsFalseWithError()
    {
        var product = new Product("", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU"));
    }

    /// <summary>
    /// Tests that a product with a short SKU returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithShortSku_ReturnsFalseWithError()
    {
        var product = new Product("AB", "Widget", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU") && e.Contains("3 characters"));
    }

    /// <summary>
    /// Tests that a product with a null SKU returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithNullSku_ReturnsFalseWithError()
    {
        var product = new Product { Sku = null!, Name = "Widget", Price = 99.99m, CategoryId = 1 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SKU"));
    }

    /// <summary>
    /// Tests that a product with an empty name returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyName_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("name"));
    }

    /// <summary>
    /// Tests that a product with a short name returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithShortName_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "A", 99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("name") && e.Contains("2 characters"));
    }

    /// <summary>
    /// Tests that a product with a zero price returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithZeroPrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 0m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Price"));
    }

    /// <summary>
    /// Tests that a product with a negative price returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithNegativePrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", -99.99m, 1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Price"));
    }

    /// <summary>
    /// Tests that a product with a negative cost price returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeCostPrice_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { CostPrice = -10m };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Cost price"));
    }

    /// <summary>
    /// Tests that a product with a negative stock quantity returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeStockQuantity_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = -5 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Stock quantity"));
    }

    /// <summary>
    /// Tests that a product with a zero category ID returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithZeroCategoryId_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 0);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("category"));
    }

    /// <summary>
    /// Tests that a product with a negative category ID returns false with an error when validated.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeCategoryId_ReturnsFalseWithError()
    {
        var product = new Product("SKU123", "Widget", 99.99m, -1);

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("category"));
    }

    /// <summary>
    /// Tests that a product with multiple validation errors returns all errors.
    /// </summary>
    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        var product = new Product { Sku = "X", Name = "", Price = -10m, CategoryId = 0, StockQuantity = -5 };

        var isValid = product.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    /// <summary>
    /// Tests that increasing the stock quantity with a positive quantity increases the stock correctly.
    /// </summary>
    [Fact]
    public void IncreaseStock_WithPositiveQuantity_IncreasesStockCorrectly()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        product.IncreaseStock(5);

        product.StockQuantity.Should().Be(15);
    }

    /// <summary>
    /// Tests that increasing the stock quantity with a zero quantity throws an ArgumentException.
    /// </summary>
    [Fact]
    public void IncreaseStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.IncreaseStock(0);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that increasing the stock quantity with a negative quantity throws an ArgumentException.
    /// </summary>
    [Fact]
    public void IncreaseStock_WithNegativeQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.IncreaseStock(-5);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that decreasing the stock quantity with a valid quantity decreases the stock correctly.
    /// </summary>
    [Fact]
    public void DecreaseStock_WithValidQuantity_DecreasesStockCorrectly()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 20 };

        product.DecreaseStock(5);

        product.StockQuantity.Should().Be(15);
    }

    /// <summary>
    /// Tests that decreasing the stock quantity with a zero quantity throws an ArgumentException.
    /// </summary>
    [Fact]
    public void DecreaseStock_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.DecreaseStock(0);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that decreasing the stock quantity with a negative quantity throws an ArgumentException.
    /// </summary>
    [Fact]
    public void DecreaseStock_WithNegativeQuantity_ThrowsArgumentException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var act = () => product.DecreaseStock(-5);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that decreasing the stock quantity with more than the available quantity throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void DecreaseStock_WithMoreThanAvailable_ThrowsInvalidOperationException()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var act = () => product.DecreaseStock(10);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient stock*");
    }

    /// <summary>
    /// Tests that decreasing the stock quantity with exactly the available quantity decreases to zero.
    /// </summary>
    [Fact]
    public void DecreaseStock_WithExactlyAvailableQuantity_DecreasesToZero()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        product.DecreaseStock(10);

        product.StockQuantity.Should().Be(0);
    }

    /// <summary>
    /// Tests that getting the profit with a cost price calculates the profit correctly.
    /// </summary>
    [Fact]
    public void GetProfit_WithCostPrice_CalculatesProfit()
    {
        var product = new Product("SKU123", "Widget", 100m, 1) { CostPrice = 60m };

        var profit = product.GetProfit();

        profit.Should().Be(40m);
    }

    /// <summary>
    /// Tests that getting the profit without a cost price returns the sell price.
    /// </summary>
    [Fact]
    public void GetProfit_WithoutCostPrice_ReturnsSellPrice()
    {
        var product = new Product("SKU123", "Widget", 100m, 1);

        var profit = product.GetProfit();

        profit.Should().Be(100m);
    }

    /// <summary>
    /// Tests that getting the profit with a zero cost price returns the sell price.
    /// </summary>
    [Fact]
    public void GetProfit_WithZeroCostPrice_ReturnsSellPrice()
    {
        var product = new Product("SKU123", "Widget", 100m, 1) { CostPrice = 0m };

        var profit = product.GetProfit();

        profit.Should().Be(100m);
    }

    /// <summary>
    /// Tests that checking if the stock is low with a stock below the threshold returns true.
    /// </summary>
    /// <param name="threshold">The threshold to check against.</param>
    [Fact]
    public void IsLowStock_WithStockBelowThreshold_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    /// <summary>
    /// Tests that checking if the stock is low with a stock equal to the threshold returns true.
    /// </summary>
    [Fact]
    public void IsLowStock_WithStockEqualToThreshold_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 10 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    /// <summary>
    /// Tests that checking if the stock is low with a stock above the threshold returns false.
    /// </summary>
    [Fact]
    public void IsLowStock_WithStockAboveThreshold_ReturnsFalse()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 15 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeFalse();
    }

    /// <summary>
    /// Tests that checking if the stock is low with the default threshold uses 10 as the default.
    /// </summary>
    [Fact]
    public void IsLowStock_WithDefaultThreshold_Uses10AsDefault()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 5 };

        var isLow = product.IsLowStock();

        isLow.Should().BeTrue();
    }

    /// <summary>
    /// Tests that checking if the stock is low with a zero stock returns true.
    /// </summary>
    [Fact]
    public void IsLowStock_WithZeroStock_ReturnsTrue()
    {
        var product = new Product("SKU123", "Widget", 99.99m, 1) { StockQuantity = 0 };

        var isLow = product.IsLowStock(10);

        isLow.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the constructor with parameters initializes the fields correctly.
    /// </summary>
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
