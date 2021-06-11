# OrderItem

Represents a single line item within an order, storing product details, quantities, pricing, and calculated totals. It also provides navigation properties to the parent `Order` and related `Product` entities.

## API

### Id  
`public int Id`  
The primary key of the order item. Set by the data store; modifying it after persistence may cause inconsistencies.

### OrderId  
`public int OrderId`  
Foreign key referencing the `Order` to which this item belongs. Should match the `Id` of the associated `Order` instance.

### ProductId  
`public int ProductId`  
Foreign key referencing the `Product` represented by this line item.

### ProductName  
`public string ProductName`  
Denormalized name of the product at the time the order was placed. Useful for display when the product record may change.

### Quantity  
`public int Quantity`  
Number of units of the product ordered. Expected to be a non‑negative integer.

### UnitPrice  
`public decimal UnitPrice`  
Price per unit before discounts or taxes. Should be non‑negative.

### Discount  
`public decimal Discount`  
Discount amount applied to the line item (in currency units). Typically non‑negative and not exceeding `UnitPrice * Quantity`.

### TaxAmount  
`public decimal TaxAmount`  
Tax amount applied to the line item after discounts. Should be non‑negative.

### LineTotal  
`public decimal LineTotal`  
Calculated total for the line item, reflecting quantity, unit price, discount, and tax. Updated by `CalculateLineTotal`.

### CreatedDate  
`public DateTime CreatedDate`  
Timestamp indicating when the order item was created. Set once at initialization and not intended to change.

### Order  
`public virtual Order? Order`  
Navigation property to the parent order. May be `null` if the relationship is not loaded or the item is detached.

### Product  
`public virtual Product? Product`  
Navigation property to the product referenced by this line item. May be `null` if the relationship is not loaded.

### OrderItem()  
`public OrderItem()`  
Parameterless constructor that creates an empty `OrderItem`. All mutable fields are initialized to their default CLR values (0 for numerics, `DateTime.MinValue` for `CreatedDate`, `null` for references). Intended for use by ORMs or deserialization.

### OrderItem  
`public OrderItem(...)`  
Overloaded constructor that accepts arguments to initialize the item’s state (signature not shown in the public member list). It sets the supplied properties and leaves others at default values. Throws `ArgumentException` if any required argument is invalid (e.g., negative quantity).

### Validate  
`public override bool Validate()`  
Evaluates the consistency of the order item’s data. Returns `true` when all of the following hold: `Quantity >= 0`, `UnitPrice >= 0`, `Discount >= 0`, `TaxAmount >= 0`, `Discount <= UnitPrice * Quantity`, and both foreign keys reference existing entities when the relationships are loaded. Returns `false` otherwise. Does not throw exceptions under normal operation; invalid state results in a `false` return.

### CalculateLineTotal  
`public void CalculateLineTotal()`  
Recomputes `LineTotal` as `(Quantity * UnitPrice) - Discount + TaxAmount`. Called automatically by the ORM after changes to the constituent fields, but can be invoked manually to ensure the total reflects current values. Throws `InvalidOperationException` if any of the source fields contain `null`‑incompatible values (e.g., `UnitPrice` is `NaN` – not possible for `decimal` but guarded against misuse).

### ApplyDiscount  
`public void ApplyDiscount()`  
Applies the stored `Discount` amount to the line item by adjusting `LineTotal` through a call to `CalculateLineTotal`. Intended for scenarios where the discount is expressed as a fixed amount rather than a percentage. Does not modify `UnitPrice` or `Quantity`. Throws `InvalidOperationException` if `Discount` is negative or exceeds the pre‑discount subtotal.

### GetSubtotal  
`public decimal GetSubtotal()`  
Returns the product of `Quantity` and `UnitPrice` (i.e., the amount before discounts and taxes). No parameters; never throws.

### GetAfterDiscount  
`public decimal GetAfterDiscount()`  
Returns the subtotal minus the `Discount` amount. Guarantees a non‑negative result; if `Discount` exceeds the subtotal, returns zero. No parameters; never throws.

### GetTotalWithTax  
`public decimal GetTotalWithTax()`  
Returns the after‑discount amount plus `TaxAmount`. No parameters; never throws.

## Usage

```csharp
using var ctx = new MicroOrmContext();

// Create a new order item and persist it.
var item = new OrderItem
{
    OrderId = 42,
    ProductId = 7,
    ProductName = "Wireless Mouse",
    Quantity = 3,
    UnitPrice = 29.99m,
    Discount = 5.00m,
    TaxAmount = 4.50m,
    CreatedDate = DateTime.UtcNow
};

ctx.OrderItems.Add(item);
ctx.SaveChanges(); // Id is populated by the store

// Ensure the calculated total matches expectations.
item.CalculateLineTotal();
Console.WriteLine(item.LineTotal); // Expected: (3 * 29.99) - 5 + 4.50 = 94.47
```

```csharp
// Retrieve an existing item and apply a promotional discount.
var item = ctx.OrderItems.Find(123);
if (item != null)
{
    // Apply a store‑wide 10 % discount as a fixed amount.
    item.Discount = Math.Round(item.GetSubtotal() * 0.10m, 2);
    item.ApplyDiscount(); // Recalculates LineTotal with the new discount.
    Console.WriteLine(item.GetTotalWithTax()); // Shows final price including tax.
    ctx.SaveChanges();
}
```

## Notes

- The class is **not thread‑safe**; concurrent modifications to the same instance from multiple threads may lead to inconsistent state, especially for the calculated properties (`LineTotal`, results of the getter methods). External synchronization is required when sharing an instance across threads.
- Navigation properties (`Order`, `Product`) are `virtual` to enable lazy‑loading proxies; accessing them after the associated context has been disposed will throw an `ObjectDisposedException` if lazy loading is enabled.
- `Validate` does not throw; callers should inspect the return value and handle invalid items according to business logic (e.g., prevent saving or raise a domain event).
- Setters for the scalar properties perform no range validation; it is the caller’s responsibility to ensure values such as `Quantity`, `UnitPrice`, `Discount`, and `TaxAmount` are sensible before persisting.
- The `ApplyDiscount` method assumes the `Discount` field represents a fixed currency amount. If a percentage‑based discount is needed, compute the amount beforehand and assign it to `Discount` before calling this method.
