# OrderItemExtensions

The `OrderItemExtensions` class provides a set of static extension methods for the `OrderItem` type. These methods encapsulate common business logic related to pricing, discount calculation, shipping eligibility, and receipt formatting, allowing callers to work with `OrderItem` instances in a consistent, reusable manner.

## API

### `GetEffectiveUnitPrice`

```csharp
public static decimal GetEffectiveUnitPrice(this OrderItem item)
```

**Purpose:**  
Calculates the actual unit price after applying any discounts. The effective unit price is the original unit price minus the discount amount (if any), but never below zero.

**Parameters:**  
- `item` – The `OrderItem` instance on which the method is called. Must not be `null`.

**Returns:**  
A `decimal` representing the effective unit price. The value is clamped to a minimum of zero.

**Throws:**  
- `ArgumentNullException` – if `item` is `null`.

---

### `QualifiesForFreeShipping`

```csharp
public static bool QualifiesForFreeShipping(this OrderItem item)
```

**Purpose:**  
Determines whether the order item qualifies for free shipping based on its total price (unit price × quantity) and any configured threshold.

**Parameters:**  
- `item` – The `OrderItem` instance on which the method is called. Must not be `null`.

**Returns:**  
`true` if the item’s total price meets or exceeds the free‑shipping threshold; otherwise `false`.

**Throws:**  
- `ArgumentNullException` – if `item` is `null`.

---

### `GetDiscountPercentage`

```csharp
public static decimal GetDiscountPercentage(this OrderItem item)
```

**Purpose:**  
Returns the discount percentage applied to the item, expressed as a value between 0 and 100.

**Parameters:**  
- `item` – The `OrderItem` instance on which the method is called. Must not be `null`.

**Returns:**  
A `decimal` representing the discount percentage (e.g., 15.0 for 15%). The value is clamped to the range [0, 100].

**Throws:**  
- `ArgumentNullException` – if `item` is `null`.

---

### `ToReceiptString`

```csharp
public static string ToReceiptString(this OrderItem item)
```

**Purpose:**  
Formats the order item as a human‑readable receipt line, typically including the product name, quantity, unit price, discount, and effective total.

**Parameters:**  
- `item` – The `OrderItem` instance on which the method is called. Must not be `null`.

**Returns:**  
A `string` containing the formatted receipt line.

**Throws:**  
- `ArgumentNullException` – if `item` is `null`.

## Usage

### Example 1: Calculating total and checking free shipping eligibility

```csharp
OrderItem item = new OrderItem
{
    ProductName = "Wireless Mouse",
    UnitPrice = 25.00m,
    Quantity = 3,
    Discount = 0.10m   // 10% discount
};

decimal effectivePrice = item.GetEffectiveUnitPrice();
decimal total = effectivePrice * item.Quantity;
bool freeShipping = item.QualifiesForFreeShipping();

Console.WriteLine($"Item total: {total:C}");
Console.WriteLine($"Free shipping: {freeShipping}");
```

### Example 2: Displaying a receipt line with discount percentage

```csharp
OrderItem item = new OrderItem
{
    ProductName = "USB-C Hub",
    UnitPrice = 45.00m,
    Quantity = 2,
    Discount = 0.15m   // 15% discount
};

decimal discountPct = item.GetDiscountPercentage();
string receiptLine = item.ToReceiptString();

Console.WriteLine($"Discount: {discountPct}%");
Console.WriteLine(receiptLine);
```

## Notes

- All methods throw `ArgumentNullException` when the `OrderItem` instance is `null`. Callers should guard against null references before invoking these extensions.
- `GetEffectiveUnitPrice` clamps the result to zero; a negative effective price is never returned.
- `GetDiscountPercentage` clamps the returned value to the range [0, 100]. If the underlying discount data is outside this range, it is adjusted accordingly.
- `QualifiesForFreeShipping` relies on the item’s total price; if the `UnitPrice` or `Quantity` are negative, the result may be unexpected (e.g., a negative total will never qualify).
- `ToReceiptString` assumes that `ProductName` is not `null` or empty; if it is, the output may be incomplete or contain placeholders.
- These extension methods are stateless and read‑only with respect to the `OrderItem` instance. They are thread‑safe as long as the `OrderItem` object is not mutated concurrently by other code. No shared mutable state is used internally.
