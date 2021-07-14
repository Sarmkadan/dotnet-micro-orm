# CategoryModelTests

Provides unit tests for the `CategoryModel` class in the `dotnet-micro-orm` project, verifying construction, validation, manipulation, and helper methods.

## API

### Constructor_WithParameters_CreatesValidCategory
- **Purpose**: Confirms that constructing a `CategoryModel` with supplied parameters results in a fully initialized object with expected property values.
- **Parameters**: None.
- **Return Value**: None (the method returns `void`).
- **Throws**: May throw an exception from the test framework (e.g., `AssertFailedException`) if the constructed object does not match the expected state.

### Validate_WithValidCategory_ReturnsTrue
- **Purpose**: Verifies that the `Validate` method returns `true` when the category instance contains a valid name, slug, and display order.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `false`.

### Validate_WithEmptyName_ReturnsFalse
- **Purpose**: Ensures that validation fails (`false`) when the category’s `Name` property is empty or whitespace.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `true`.

### Validate_WithShortName_ReturnsFalse
- **Purpose**: Ensures that validation fails when the category’s `Name` is shorter than the minimum allowed length.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `true`.

### Validate_WithEmptySlug_ReturnsFalse
- **Purpose**: Ensures that validation fails when the category’s `Slug` property is empty or whitespace.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `true`.

### Validate_WithShortSlug_ReturnsFalse
- **Purpose**: Ensures that validation fails when the category’s `Slug` is shorter than the minimum allowed length.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `true`.

### Validate_WithNegativeDisplayOrder_ReturnsFalse
- **Purpose**: Ensures that validation fails when the category’s `DisplayOrder` is negative.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if validation incorrectly returns `true`.

### MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder
- **Purpose**: Confirms that calling `MoveUp` on a category with a `DisplayOrder` greater than zero decrements the order by one.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if the order is not decremented as expected.

### MoveUp_WithDisplayOrderZero_StaysZero
- **Purpose**: Confirms that calling `MoveUp` on a category whose `DisplayOrder` is zero leaves the order unchanged.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if the order changes unexpectedly.

### MoveDown_IncrementsDisplayOrder
- **Purpose**: Confirms that calling `MoveDown` on a category increments its `DisplayOrder` by one.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if the order is not incremented as expected.

### GetBreadcrumb_WithParentCategory_ReturnsFullPath
- **Purpose**: Verifies that `GetBreadcrumb` returns a hierarchical path string when the category has a parent chain.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if the returned breadcrumb does not match the expected path.

### GetBreadcrumb_WithoutParentCategory_ReturnsCategoryName
- **Purpose**: Verifies that `GetBreadcrumb` returns just the category’s name when it has no parent.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if the returned breadcrumb is incorrect.

### GetProductCount_WithNoProducts_ReturnsZero
- **Purpose**: Ensures that `GetProductCount` returns zero for a category that has no associated products.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if a non‑zero count is returned.

### Deactivate_SetsIsActiveToFalse
- **Purpose**: Confirms that invoking `Deactivate` sets the `IsActive` property to `false`.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if `IsActive` remains `true`.

### Deactivate_WithSubCategories_DeactivatesAll
- **Purpose**: Confirms that calling `Deactivate` on a category recursively sets `IsActive` to `false` on the category and all of its sub‑categories.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: May throw an exception from the test framework if any sub‑category remains active.

## Usage

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetMicroOrm.Models; // adjust namespace as needed

[TestClass]
public class CategoryModelTests
{
    [TestMethod]
    public void Validate_WithValidCategory_ReturnsTrue()
    {
        // Arrange
        var category = new CategoryModel { Name = "Electronics", Slug = "electronics", DisplayOrder = 1 };

        // Act
        bool isValid = category.Validate();

        // Assert
        Assert.IsTrue(isValid);
    }
}
```

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetMicroOrm.Models;

[TestClass]
public class CategoryModelTests
{
    [TestMethod]
    public void MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder()
    {
        // Arrange
        var category = new CategoryModel { Name = "Books", Slug = "books", DisplayOrder = 3 };

        // Act
        category.MoveUp();

        // Assert
        Assert.AreEqual(2, category.DisplayOrder);
    }
}
```

## Notes

- Each test method operates on a newly instantiated `CategoryModel`; there is no shared mutable state, so the tests are inherently thread‑safe when executed in parallel by a test runner.
- The methods do not accept parameters; all test data is created inside the method body.
- If a test fails, the testing framework will surface an exception (e.g., `AssertFailedException`) indicating which assertion did not hold.
- Edge cases such as whitespace‑only strings, boundary lengths for name and slug, and zero or negative display order are covered by the respective validation tests.
- The `Deactivate_WithSubCategories_DeactivatesAll` test assumes that the `CategoryModel` implementation correctly propagates the deactivation call to child categories; behavior may differ if the model does not maintain a sub‑category collection.
