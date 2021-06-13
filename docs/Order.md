# Order

Represents a customer order in an e-commerce system, tracking order details, financial information, shipping status, and associated line items. The type integrates with a micro-ORM for persistence and supports validation and item management.

## API

### Properties

#### `public int Id`
Unique identifier for the order. Assigned by the persistence layer on creation.

#### `public string OrderNumber`
Human-readable order reference (e.g., "ORD-2024-00123"). Must be non-null and non-empty.

#### `public int UserId`
Foreign key referencing the customer who placed the order. Must correspond to an existing `User` record.

#### `public DateTime OrderDate`
Timestamp when the order was created. Set automatically by the system; cannot be modified after creation.

#### `public string Status`
Current state of the order (e.g., "Pending", "Shipped", "Delivered", "Cancelled"). Valid values enforced by business logic.

#### `public decimal TotalAmount`
Sum of all line item totals plus tax. Automatically calculated from `Items` and `TaxAmount`; updates when items are added or modified.

#### `public decimal TaxAmount`
Total tax applied to the order. Must be non-negative. Automatically recalculated when `TotalAmount` changes.

#### `public string ShippingAddress`
Mandatory delivery address for the order. Must be a valid, non-empty string.

#### `public string? BillingAddress`
Optional billing address. If omitted, defaults to `ShippingAddress`.

#### `public DateTime? ShippingDate`
Timestamp when the order was shipped. Set when status transitions to "Shipped".

#### `public DateTime? DeliveryDate`
Timestamp when the order was delivered. Set when status transitions to "Delivered".

#### `public string? Notes`
Optional internal or customer-facing notes about the order.

#### `public DateTime CreatedDate`
Timestamp when the order was first persisted. Set automatically; immutable.

#### `public DateTime? ModifiedDate`
Timestamp when the order was last updated. Updated automatically on changes to mutable fields.

#### `public virtual User? User`
Navigation property to the customer who placed the order. Lazy-loaded via ORM.

#### `public virtual List<OrderItem> Items`
Collection of line items in the order. Managed via `AddItem`; updates `TotalAmount` and `TaxAmount` on changes.

### Constructors

#### `public Order()`
Initializes a new, empty order with default values for all fields. `OrderDate` and `CreatedDate` are set to `DateTime.UtcNow`.

#### `public Order(string orderNumber, int userId, string shippingAddress)`
Initializes a new order with required fields. Sets `OrderNumber`, `UserId`, `ShippingAddress`, and initializes `Status` to "Pending". `OrderDate`, `CreatedDate`, and `TotalAmount` are populated automatically.

### Methods

#### `public override bool Validate()`
Validates the order state. Returns `true` if all required fields are populated, `Status` is valid, `TotalAmount` and `TaxAmount` are non-negative, and `OrderDate` is not in the future. Throws `InvalidOperationException` if `OrderNumber` or `ShippingAddress` are null or empty.

#### `public void AddItem(OrderItem item)`
Adds a line item to the order. Updates `TotalAmount` and `TaxAmount` accordingly. Throws `ArgumentNullException` if `item` is null. Throws `InvalidOperationException` if the item's `ProductId` already exists in `Items`.
