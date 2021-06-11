# Category

The `Category` type represents a hierarchical grouping of products within the dotnet‑micro‑orm domain model. It stores identifying information, display metadata, and navigation properties that enable traversal of the category tree and access to associated products.

## API

### Id  
`public int Id`  
Gets or sets the unique identifier for the category. This value is typically assigned by the data store and should not be modified after persistence.

### Name  
`public string Name`  
Gets or sets the human‑readable name of the category. Required for display purposes; should not be null or empty when the category is saved.

### Slug  
`public string Slug`  
Gets or sets a URL‑friendly identifier derived from the name. Used in routing; must be unique within the same parent hierarchy.

### Description  
`public string? Description`  
Gets or sets an optional detailed description of the category. May be null.

### ParentCategoryId  
`public int? ParentCategoryId`  
Gets or sets the identifier of the parent category. Null indicates a top‑level category.

### DisplayOrder  
`public int DisplayOrder`  
Gets or sets the ordinal position used to sort categories among siblings. Lower values appear first.

### IsActive  
`public bool IsActive`  
Gets or sets a flag indicating whether the category is active and visible to users. Inactive categories are typically excluded from queries.

### CreatedDate  
`public DateTime CreatedDate`  
Gets the date and time when the category instance was instantiated. Set automatically by the constructor; should not be altered manually.

### Products  
`public virtual List<Product> Products`  
Gets or sets the collection of products associated with this category. The property is virtual to enable lazy‑loading proxies; the list may be empty.

### ParentCategory  
`public virtual Category? ParentCategory`  
Gets or sets the parent category navigation property. Null when the category has no parent. Virtual to support change‑tracking proxies.

### SubCategories  
`public virtual List<Category> SubCategories`  
Gets or sets the collection of direct child categories. Virtual for lazy‑loading; may be empty.

### Category()  
`public Category()`  
Parameterless constructor that creates a new `Category` instance with default values. `CreatedDate` is set to the current UTC time; other properties retain their CLR defaults.

### Category  
`public Category`  
Overloaded constructor that initializes a new `Category` instance. The exact parameter list is not exposed in the public surface; callers should use the appropriate overload to set required fields such as `Name` and `Slug`. After construction, the instance is ready for persistence.

### Validate  
`public override bool Validate`  
Performs validation of the category’s state. Returns `true` if all required fields (`Name`, `Slug`, etc.) meet business rules; otherwise returns `false`. The method does not throw exceptions for validation failures; however, it may throw an `InvalidOperationException` if an unexpected internal error occurs during validation.

### MoveUp  
`public void MoveUp`  
Attempts to move the category one position higher among its siblings by decrementing `DisplayOrder`. If the category is already at the top of its sibling list or has no parent, the method throws an `InvalidOperationException`.

### MoveDown  
`public void MoveDown`  
Attempts to move the category one position lower among its siblings by incrementing `DisplayOrder`. If the category is already at the bottom of its sibling list or has no parent, the method throws an `InvalidOperationException`.

### GetBreadcrumb  
`public string GetBreadcrumb`  
Constructs a hierarchical breadcrumb string representing the path from the root category to this category, using each category’s `Name` separated by the “ > ” delimiter. Returns a plain string; if the category has no parent, returns only its own `Name`. The method does not throw.

### GetProductCount  
`public int GetProductCount`  
Returns the number of products currently associated with the category by evaluating the `Products` collection. Returns zero if the collection is null or empty. No exceptions are thrown under normal circumstances.

### Deactivate  
`public void Deactivate`  
Sets the `IsActive` property to `false`, effectively hiding the category from active queries. If the category is already inactive, the method does nothing and does not throw.

## Usage

```csharp
// Creating a new top‑level category and persisting it.
var root = new Category
{
    Name = "Electronics",
    Slug = "electronics",
    DisplayOrder = 1,
    IsActive = true
};
// Assume `context` is an EF Core DbContext or similar.
context.Categories.Add(root);
await context.SaveChangesAsync();

// Adding a subcategory, moving it, and retrieving a breadcrumb.
var sub = new Category
{
    Name = "Laptops",
    Slug = "laptops",
    ParentCategoryId = root.Id,
    DisplayOrder = 1,
    IsActive = true
};
context.Categories.Add(sub);
await context.SaveChangesAsync();

// Move the subcategory down one position (if another sibling exists).
sub.MoveDown();
await context.SaveChangesAsync();

// Get breadcrumb for the subcategory.
string breadcrumb = sub.GetBreadcrumb; // "Electronics > Laptops"
```

```csharp
// Deactivating a category and validating its state.
var category = await context.Categories.FindAsync(42);
if (category != null)
{
    category.Deactivate();
    if (!category.Validate())
    {
        // Handle validation failure – e.g., log missing required fields.
    }
    await context.SaveChangesAsync();
}
```

## Notes

- The `Description` and `ParentCategoryId` properties are nullable; code that consumes these members should guard against `null` values when they are semantically required.
- Setting `ParentCategoryId` does not automatically update the `ParentCategory` navigation property; both sides of the relationship must be managed consistently to avoid inconsistent state.
- `MoveUp` and `MoveDown` rely on the `DisplayOrder` values of sibling categories. If siblings share the same `DisplayOrder`, the outcome is undefined and may require additional tie‑breaking logic.
- The virtual navigation properties (`Products`, `ParentCategory`, `SubCategories`) are intended for use with an ORM that supports lazy‑loading or change‑tracking proxies. Direct instantiation of `Category` outside of an ORM context will leave these properties as `null` or empty lists unless explicitly initialized.
- The class is not thread‑safe. Concurrent modifications to the same `Category` instance from multiple threads may result in race conditions, particularly on mutable properties such as `DisplayOrder`, `IsActive`, and the collection properties. External synchronization is required when shared access is needed.
