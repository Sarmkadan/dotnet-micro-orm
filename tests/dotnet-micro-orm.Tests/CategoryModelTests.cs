#nullable enable

using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class CategoryModelTests
{
    [Fact]
    public void Constructor_WithParameters_CreatesValidCategory()
    {
        // Arrange
        var name = "Electronics";
        var slug = "electronics";

        // Act
        var category = new Category(name, slug);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be(name);
        category.Slug.Should().Be(slug);
        category.IsActive.Should().BeTrue();
        category.DisplayOrder.Should().Be(0);
        category.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Validate_WithValidCategory_ReturnsTrue()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            DisplayOrder = 1,
            ParentCategoryId = null
        };

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var category = new Category("", "books");

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Category name is required and must be at least 2 characters");
    }

    [Fact]
    public void Validate_WithShortName_ReturnsFalse()
    {
        // Arrange
        var category = new Category("B", "books");

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Category name is required and must be at least 2 characters");
    }

    [Fact]
    public void Validate_WithEmptySlug_ReturnsFalse()
    {
        // Arrange
        var category = new Category("Books", "");

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Category slug is required and must be at least 2 characters");
    }

    [Fact]
    public void Validate_WithShortSlug_ReturnsFalse()
    {
        // Arrange
        var category = new Category("Books", "b");

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Category slug is required and must be at least 2 characters");
    }

    [Fact]
    public void Validate_WithNegativeDisplayOrder_ReturnsFalse()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            DisplayOrder = -1
        };

        // Act
        var isValid = category.Validate(out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Display order cannot be negative");
    }

    [Fact]
    public void MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            DisplayOrder = 5
        };

        // Act
        category.MoveUp();

        // Assert
        category.DisplayOrder.Should().Be(4);
    }

    [Fact]
    public void MoveUp_WithDisplayOrderZero_StaysZero()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            DisplayOrder = 0
        };

        // Act
        category.MoveUp();

        // Assert
        category.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void MoveDown_IncrementsDisplayOrder()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            DisplayOrder = 3
        };

        // Act
        category.MoveDown();

        // Assert
        category.DisplayOrder.Should().Be(4);
    }

    [Fact]
    public void GetBreadcrumb_WithParentCategory_ReturnsFullPath()
    {
        // Arrange
        var parent = new Category("Electronics", "electronics")
        {
            Id = 1
        };
        var child = new Category("Laptops", "laptops")
        {
            Id = 2,
            ParentCategory = parent
        };

        // Act
        var breadcrumb = child.GetBreadcrumb();

        // Assert
        breadcrumb.Should().Be("Electronics > Laptops");
    }

    [Fact]
    public void GetBreadcrumb_WithoutParentCategory_ReturnsCategoryName()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            Id = 1
        };

        // Act
        var breadcrumb = category.GetBreadcrumb();

        // Assert
        breadcrumb.Should().Be("Books");
    }

    [Fact]
    public void GetProductCount_WithNoProducts_ReturnsZero()
    {
        // Arrange
        var category = new Category("Books", "books");

        // Act
        var count = category.GetProductCount();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var category = new Category("Books", "books")
        {
            IsActive = true
        };

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WithSubCategories_DeactivatesAll()
    {
        // Arrange
        var parent = new Category("Electronics", "electronics")
        {
            IsActive = true
        };
        var child = new Category("Laptops", "laptops")
        {
            IsActive = true,
            ParentCategory = parent
        };
        parent.SubCategories.Add(child);

        // Act
        parent.Deactivate();

        // Assert
        parent.IsActive.Should().BeFalse();
        child.IsActive.Should().BeFalse();
    }
}