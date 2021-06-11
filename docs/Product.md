# Product
The `Product` class represents a sellable item in the domain model, encapsulating its identifying data, pricing, inventory state, and relationships to categories and order items. It is intended to be used as an entity within the ORM‑based data layer of the application.

## API
### Id (`int`)
**Purpose:** Unique identifier for the product.  
**Parameters:** None.  
**Return value:** The current identifier.  
**Throws:** Setting the property does not throw exceptions; however, assigning a negative value may violate domain invariants and is not validated by the property itself.

### Sku (`string`)
**Purpose:** Stock‑keeping unit, a human‑readable code used to identify the product.  
**Parameters:** None.  
**Return value:** The current SKU string.  
**Throws:** Assigning `null` throws `ArgumentNullException` because the property is non‑nullable.

### Name (`string`)
**Purpose:** Descriptive name of the product.  
**Parameters:** None.  
**Return value:** The current name.  
**Throws:** Assigning `null` throws `ArgumentNullException`.

### Description (`string?`)
**Purpose:** Optional detailed description of the product.  
**Parameters:** None.  
**Return value:** The description or `null` if none is set.  
**Throws:** No exceptions are thrown by the getter or setter.

### Price (`decimal`)
**Purpose:** Sale price of the product.  
**Parameters:** None.  
**Return value:** The current price.  
**Throws:** Setting a negative value does not throw an exception but may be considered invalid by business logic.

### CostPrice (`decimal?`)
**Purpose:** Optional cost incurred to acquire or produce the product.  
**Parameters:** None.  
**Return value:** The cost price or `null` if unknown.  
**Throws:** No exceptions are thrown by the getter or setter.

### CategoryId (`int`)
**Purpose:** Foreign key referencing the associated category.  
**Parameters:** None.  
**Return value:** The identifier of the category.  
**Throws:** Setting the property does not throw exceptions; invalid foreign‑key values will cause persistence errors when saved.

### StockQuantity (`int`)
**Purpose:** Number of units currently in stock.  
**Parameters:** None.  
**Return value:** The current stock count.  
**Throws:** Assigning a negative value does not throw an exception but may violate inventory constraints.

### IsActive (`bool`)
**Purpose:** Indicates whether the product is available for sale.  
**Parameters:** None.  
**Return value:** `true` if active; otherwise `false`.  
**Throws:** No exceptions are thrown by the getter or setter.

### CreatedDate (`DateTime`)
**Purpose:** Timestamp when the product instance was first created.  
**Parameters:** None.  
**Return value:** The creation date and time.  
**Throws:** The property does not throw exceptions; setting it to a value far in the past or future is allowed but may be nonsensical.

### ModifiedDate (`DateTime?`)
**Purpose:** Timestamp of the most recent modification to the product, if any.  
**Parameters:** None.  
**Return value:** The modification date and time, or `null` if never modified.  
**Throws:** No exceptions are thrown by the getter or setter.

### Category (`virtual Category?`)
**Purpose:** Navigation property to the category to which the product belongs. May be `null` if the relationship is not loaded.  
**Parameters:** None.  
**Return value:** The associated `Category` instance or `null`.  
**Throws:** No exceptions are thrown by the getter or setter.

### OrderItems (`virtual List<OrderItem>`)
**Purpose:** Collection of order items that reference this product. May be empty if no orders exist.  
**Parameters:** None.  
**Return value:** A list of `OrderItem` objects.  
**Throws:** No exceptions are thrown by the getter or setter; however, assigning `null` is not permitted because the property type is non‑nullable list.

### Product ()
**Purpose:** Parameterless constructor that creates a new `Product` instance with default values.  
**Parameters:** None.  
**Return value:** A new `Product` object.  
**Throws:** Does not throw exceptions.

### Product (…*)
**Purpose:** Constructor that initializes a new `Product` instance with supplied values (the exact parameter list is not visible in the provided signature).  
**Parameters:** As defined by the constructor signature (not specified here).  
**Return value:** A new `Product` object populated with the given arguments.  
**Throws:** May throw `ArgumentException` or derived exceptions if any argument fails validation; consult the constructor’s XML documentation for specifics.

### Validate ()
**Purpose:** Performs validation of the product’s data integrity (e.g., required fields, value ranges).  
**Parameters:** None.  
**Return value:** `true` if the product passes all validation rules; otherwise `false`.  
**Throws:** Does not throw exceptions; validation failures are indicated by the return value.

### IncreaseStock ()
**Purpose:** Increments the product’s stock quantity.  
**Parameters:** None (the method assumes a default increment amount as defined by the implementation).  
**Return value:** None.  
**Throws:** May throw `InvalidOperationException` if the resulting stock quantity would exceed business‑defined limits.

### DecreaseStock ()
**Purpose:** Decrements the product’s stock quantity.  
**Parameters:** None (the method assumes a default decrement amount as defined by the implementation).  
**Return value:** None.  
**Throws:** May throw `InvalidOperationException` if the resulting stock quantity would be negative.

### GetProfit ()
**Purpose:** Calculates the profit per unit based on the sale price and cost price.  
**Parameters:** None.  
**Return value:** The profit as a `decimal`; returns `Price - CostPrice` if `CostPrice` has a value, otherwise returns `Price`.  
**Throws:** Does not throw exceptions.

### IsLowStock ()
**Purpose:** Determines whether the current stock quantity falls below a low‑stock threshold.  
**Parameters:** None.  
**Return value:** `true` if stock is considered low; otherwise `false`.  
**Throws:** Does not throw exceptions.

## Usage
```csharp
// Example 1: Creating a product and adjusting inventory
var product = new Product
{
    Sku = "ABC-123",
    Name = "Widget",
    Price = 19.99m,
    CostPrice = 12.00m,
    CategoryId = 5,
    StockQuantity = 100,
    IsActive = true,
    CreatedDate = DateTime.UtcNow
};

product.IncreaseStock();   // Assume increases by a default amount
product.DecreaseStock();   // Assume decreases by a default amount

Console.WriteLine($"Profit per unit: {product.GetProfit():C}");
Console.WriteLine($"Is low stock? {product.IsLowStock}");
```
```csharp
// Example 2: Validating a product before persisting
var product = new Product
{
    Sku = "XYZ-789",
    Name = "Gadget",
    Price = 0m, // Invalid price for demonstration
    CreatedDate = DateTime.UtcNow
};

if (!product.Validate())
{
    // Handle validation failure – e.g., log errors or notify user
    Console.WriteLine("Product validation failed.");
}
else
{
    // Proceed to save the product via the repository/context
    // repository.Add(product);
    // await repository.SaveChangesAsync();
}
```
## Notes
- The `Description`, `CostPrice`, and `ModifiedDate` properties are nullable; consuming code should check for `null` before relying on their values.
- Navigation properties `Category` and `OrderItems` are `virtual` to enable lazy‑loading proxies; they may return `null` if the related data has not been loaded.
- The class does not contain any internal synchronization mechanisms; concurrent access from multiple threads requires external locking or reliance on the surrounding unit‑of‑work/context to ensure thread safety.
- Setters for non‑nullable string properties (`Sku`, `Name`) will throw `ArgumentNullException` if `null` is assigned; other property setters do not perform validation and will accept any value permitted by their type.
- The `Validate` method’s implementation is not shown; developers should refer to its XML documentation or source code to understand which rules are evaluated.
- Stock adjustment methods (`IncreaseStock`, `DecreaseStock`) modify `StockQuantity` directly; callers should verify that the resulting quantity remains within acceptable bounds to avoid negative inventory.
