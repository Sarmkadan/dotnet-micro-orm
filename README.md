// entire file content ...
// ... goes in between

## FormatterFactory

The `FormatterFactory` class is a centralized factory for creating output formatters based on requested formats. It provides a way to instantiate formatters with consistent configuration and supports registration of custom formatter implementations.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Formatters;

class Program
{
    static void Main()
    {
        var factory = new FormatterFactory();
        var jsonFormatter = factory.GetFormatter(OutputFormat.Json);
        var csvFormatter = factory.GetFormatter(OutputFormat.Csv);

        Console.WriteLine(jsonFormatter.Format(new { Name = "John", Age = 30 }));
        Console.WriteLine(csvFormatter.Format(new { Name = "Jane", Age = 25 }));
    }
}
```

// ... goes in between

## CategoryModelTests

`CategoryModelTests` is a collection of unit tests that verify the behavior of the `Category` domain model. The suite checks construction, validation rules, ordering operations, breadcrumb generation, product counting, and deactivation logic, ensuring the model behaves correctly in a variety of scenarios.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Domain.Models;

// The test class lives in the global namespace (no explicit namespace)
// and its public methods are the individual test cases.
var tests = new CategoryModelTests();

// Example: creating a valid category (mirrors the Constructor test)
var category = new Category("Books", "books")
{
    DisplayOrder = 1,
    ParentCategoryId = null,
    IsActive = true
};

// Example: validating a correct category (mirrors Validate_WithValidCategory_ReturnsTrue)
bool isValid = category.Validate(out var validationErrors);
// isValid == true, validationErrors is empty

// Example: moving a category up (mirrors MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder)
category.DisplayOrder = 5;
tests.MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder(); // internally calls category.MoveUp()
Console.WriteLine($"DisplayOrder after MoveUp: {category.DisplayOrder}");

// Example: generating a breadcrumb (mirrors GetBreadcrumb_WithParentCategory_ReturnsFullPath)
var parent = new Category("Electronics", "electronics") { Id = 1 };
var child = new Category("Laptops", "laptops") { Id = 2, ParentCategory = parent };
string breadcrumb = child.GetBreadcrumb();
Console.WriteLine($"Breadcrumb: {breadcrumb}");
```

The example demonstrates how the public members of `Category` are exercised by the test methods, providing a quick reference for developers who need to understand the expected behavior of the model.