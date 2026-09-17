# Domain Models

This document describes the core domain entities in the DotnetMicroOrm application, their properties, validation rules, and relationships.

## BaseEntity

The `BaseEntity` class is the abstract foundation for all domain entities, providing common functionality for equality comparison, validation, and lifecycle operations.

### Properties
- **Id** (implicit): Primary key inherited by derived classes
- **ModifiedDate**: Automatically updated during `PreSave()` if present

### Methods
- `Validate(out List<string> errors)`: Virtual method for entity-specific validation
- `PreSave()`: Sets ModifiedDate to current UTC time
- `PostLoad()`: Empty virtual method for post-load operations
- `Equals()` and `GetHashCode()`: Based on entity Id for comparison
- `ToString()`: Returns entity name and Id

## User

Represents a system user with authentication and profile information.

### Table Mapping
```csharp
[Table("Users")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| Username | string | No | MaxLength=50 | Login username |
| Email | string | No | MaxLength=100 | Email address |
| PasswordHash | string | No | - | Hashed password |
| FirstName | string? | Yes | MaxLength=50 | Given name |
| LastName | string? | Yes | MaxLength=50 | Family name |
| PhoneNumber | string? | Yes | MaxLength=20 | Contact number |
| IsActive | bool | No | Default=true | Account status |
| IsEmailVerified | bool | No | - | Email verification status |
| LastLoginDate | DateTime? | Yes | - | Last login timestamp |
| CreatedDate | DateTime | No | - | Account creation timestamp |
| ModifiedDate | DateTime? | Yes | - | Last modification timestamp |
| Version | int | No | - | Optimistic concurrency token |

### Navigation Properties
- **Orders**: List of Order entities (one-to-many)

### Validation Rules
- Username must be at least 3 characters
- Email must contain "@" symbol
- Password hash must be at least 32 characters
- First/Last name cannot exceed 50 characters if provided

### Methods
- `GetFullName()`: Returns combined first and last name
- `MarkAsEmailVerified()`: Sets email verification flag
- `UpdateLastLogin()`: Updates last login timestamp
- `Deactivate()`: Sets IsActive to false

## Product

Represents a sellable item in the product catalog.

### Table Mapping
```csharp
[Table("Products")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| Sku | string | No | MaxLength=50, Unique | Stock Keeping Unit |
| Name | string | No | MaxLength=200, Indexed | Product name |
| Description | string? | Yes | - | Detailed description |
| Price | decimal | No | Precision=18, Scale=2 | Sale price |
| CostPrice | decimal? | Yes | Precision=18, Scale=2 | Acquisition cost |
| CategoryId | int | No | - | Foreign key to Category |
| StockQuantity | int | No | - | Available inventory |
| IsActive | bool | No | Default=true | Product availability |
| CreatedDate | DateTime | No | - | Creation timestamp |
| ModifiedDate | DateTime? | Yes | - | Last modification timestamp |

### Navigation Properties
- **Category**: Reference to Category entity
- **OrderItems**: List of OrderItem entities (one-to-many)

### Validation Rules
- SKU must be at least 3 characters
- Name must be at least 2 characters
- Price must be greater than zero
- CostPrice cannot be negative if provided
- StockQuantity cannot be negative
- CategoryId must be positive

### Methods
- `IncreaseStock(int quantity)`: Adds to inventory
- `DecreaseStock(int quantity)`: Removes from inventory (throws if insufficient)
- `GetProfit()`: Returns profit margin (Price - CostPrice)
- `IsLowStock(int threshold = 10)`: Checks if stock is below threshold

## Order

Represents a customer purchase transaction.

### Table Mapping
```csharp
[Table("Orders")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| OrderNumber | string | No | MaxLength=50, Unique, Indexed | Human-readable order ID |
| UserId | int | No | Indexed | Foreign key to User |
| OrderDate | DateTime | No | - | When order was placed |
| Status | string | No | MaxLength=20 | Order state (Pending/Confirmed/Shipped/Delivered/Cancelled) |
| TotalAmount | decimal | No | Precision=18, Scale=2 | Sum of all line items |
| TaxAmount | decimal | Yes | Precision=18, Scale=2 | Applied taxes |
| ShippingAddress | string | No | - | Delivery address |
| BillingAddress | string? | Yes | - | Billing address |
| ShippingDate | DateTime? | Yes | - | When order shipped |
| DeliveryDate | DateTime? | Yes | - | When order delivered |
| Notes | string? | Yes | - | Additional comments |
| CreatedDate | DateTime | No | - | Record creation timestamp |
| ModifiedDate | DateTime? | Yes | - | Last modification timestamp |

### Navigation Properties
- **User**: Reference to User entity
- **Items**: List of OrderItem entities (one-to-many)

### Validation Rules
- OrderNumber is required
- UserId must be positive
- ShippingAddress must be at least 10 characters
- Must contain at least one item
- TotalAmount must be greater than zero
- Status must be valid (Pending/Confirmed/Shipped/Delivered/Cancelled)

### Methods
- `AddItem(OrderItem item)`: Adds item and recalculates totals
- `RemoveItem(OrderItem item)`: Removes item and recalculates totals
- `RecalculateTotals()`: Updates TotalAmount from line items
- `Ship(DateTime shipDate)`: Marks order as shipped
- `MarkAsDelivered()`: Marks order as delivered
- `Cancel()`: Cancels order (if not shipped/delivered)
- `GetTaxableAmount()`: Returns TotalAmount minus TaxAmount

## OrderItem

Represents an individual line item within an order.

### Table Mapping
```csharp
[Table("OrderItems")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| OrderId | int | No | Indexed | Foreign key to Order |
| ProductId | int | No | - | Foreign key to Product |
| ProductName | string | No | MaxLength=200 | Product name at time of order |
| Quantity | int | No | - | Number of units |
| UnitPrice | decimal | No | Precision=18, Scale=2 | Price per unit |
| Discount | decimal | Yes | Precision=18, Scale=2 | Discount applied |
| TaxAmount | decimal | Yes | Precision=18, Scale=2 | Taxes applied |
| LineTotal | decimal | No | Precision=18, Scale=2 | Final line amount |
| CreatedDate | DateTime | No | - | Creation timestamp |

### Navigation Properties
- **Order**: Reference to Order entity
- **Product**: Reference to Product entity

### Validation Rules
- OrderId must be positive
- ProductId must be positive
- Quantity must be at least 1
- UnitPrice must be greater than zero
- ProductName is required
- Discount cannot be negative
- Discount cannot exceed line subtotal (Quantity × UnitPrice)

### Methods
- `CalculateLineTotal()`: Computes LineTotal from Quantity, UnitPrice, Discount, TaxAmount
- `ApplyDiscount(decimal amount)`: Applies discount with validation
- `GetSubtotal()`: Returns Quantity × UnitPrice
- `GetAfterDiscount()`: Returns Subtotal minus Discount
- `GetTotalWithTax()`: Returns AfterDiscount plus TaxAmount
- `GetTaxRate()`: Returns TaxAmount divided by Subtotal (or 0)

## Category

Represents a product classification hierarchy.

### Table Mapping
```csharp
[Table("Categories")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| Name | string | No | MaxLength=100, Indexed | Category name |
| Slug | string | No | MaxLength=100, Unique | URL-friendly identifier |
| Description | string? | Yes | - | Detailed description |
| ParentCategoryId | int? | Yes | - | Self-referencing foreign key |
| DisplayOrder | int | No | - | Sort order for display |
| IsActive | bool | No | Default=true | Category visibility |
| CreatedDate | DateTime | No | - | Creation timestamp |

### Navigation Properties
- **Products**: List of Product entities (one-to-many)
- **ParentCategory**: Reference to parent Category
- **SubCategories**: List of child Category entities (one-to-many)

### Validation Rules
- Name must be at least 2 characters
- Slug must be at least 2 characters
- DisplayOrder cannot be negative

### Methods
- `MoveUp()`: Decreases DisplayOrder by 1 (if > 0)
- `MoveDown()`: Increases DisplayOrder by 1
- `GetBreadcrumb()`: Returns hierarchical path (e.g., "Electronics > Computers > Laptops")
- `GetProductCount()`: Returns number of associated products
- `Deactivate()`: Sets IsActive to false and recursively deactivates subcategories

## Inventory

Tracks stock levels and warehouse locations for products.

### Table Mapping
```csharp
[Table("Inventory")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| ProductId | int | No | Indexed | Foreign key to Product |
| WarehouseLocation | string | No | MaxLength=50 | Physical storage location |
| CurrentStock | int | No | - | Total units in stock |
| ReservedStock | int | No | - | Units allocated to orders |
| AvailableStock | int | No | Computed | CurrentStock minus ReservedStock |
| MinimumThreshold | int | No | Default=10 | Low stock alert level |
| LastRestockDate | DateTime? | Yes | - | Last inventory replenishment |
| LastCountDate | DateTime? | Yes | - | Last physical inventory count |
| CreatedDate | DateTime | No | - | Record creation timestamp |
| ModifiedDate | DateTime? | Yes | - | Last modification timestamp |

### Navigation Properties
- **Product**: Reference to Product entity

### Validation Rules
- ProductId must be positive
- WarehouseLocation must be at least 2 characters
- CurrentStock cannot be negative
- ReservedStock must be between 0 and CurrentStock
- MinimumThreshold cannot be negative

### Methods
- `Restock(int quantity, DateTime? restockDate = null)`: Adds to CurrentStock
- `Withdraw(int quantity)`: Removes from CurrentStock (throws if insufficient AvailableStock)
- `Reserve(int quantity)`: Allocates stock to pending orders
- `ReleaseReservation(int quantity)`: Releases previously reserved stock
- `PerformStockCount(int actualStock)`: Updates CurrentStock from physical count
- `IsLowStock()`: Returns true if AvailableStock ≤ MinimumThreshold
- `GetDaysLastRestocked()`: Days since last restock (or -1 if never)

## AuditLog

Records system changes and operations for compliance and debugging.

### Table Mapping
```csharp
[Table("AuditLogs")]
```

### Properties
| Property | Type | Nullable | Constraints | Description |
|----------|------|----------|-------------|-------------|
| Id | int | No | Primary Key | Unique identifier |
| EntityType | string | No | MaxLength=100, Indexed | Type of entity modified |
| EntityId | int | No | - | Identifier of modified entity |
| Action | string | No | MaxLength=20 | Operation performed (INSERT/UPDATE/DELETE/READ) |
| UserId | int? | Yes | - | User who performed action |
| Username | string? | Yes | MaxLength=50 | Username of actor |
| OldValues | string? | Yes | - | JSON state before change |
| NewValues | string? | Yes | - | JSON state after change |
| ChangedProperties | string? | Yes | - | List of modified properties |
| IPAddress | string? | Yes | MaxLength=45 | Origin IP address |
| UserAgent | string? | Yes | - | Client user agent string |
| Description | string? | Yes | - | Additional context |
| IsSuccessful | bool | No | Default=true | Operation success status |
| ErrorMessage | string? | Yes | - | Error details if failed |
| Timestamp | DateTime | No | Indexed | When action occurred |

### Validation Rules
- EntityType is required and ≤100 characters
- EntityId must be positive
- Action is required and must be valid (INSERT/UPDATE/DELETE/READ)

### Static Factory Methods
- `CreateInsert(string entityType, int entityId, string? newValues = null, int? userId = null, string? username = null)`
- `CreateUpdate(string entityType, int entityId, string? oldValues = null, string? newValues = null, string? changedProps = null, int? userId = null, string? username = null)`
- `CreateDelete(string entityType, int entityId, string? oldValues = null, int? userId = null, string? username = null)`

### Methods
- `MarkAsSuccess(string? description = null)`: Marks operation as successful
- `MarkAsFailure(string errorMessage, string? description = null)`: Marks operation as failed
- `SetIpAndUserAgent(string? ipAddress, string? userAgent)`: Sets client information

## Entity Relationships

### One-to-Many Relationships
- **User → Orders**: One user can place many orders
- **Category → Products**: One category can contain many products
- **Product → OrderItems**: One product can appear in many order line items
- **Order → OrderItems**: One order contains many line items
- **Category → SubCategories**: One category can have many subcategories (self-referencing)
- **Product → Inventory**: One product can have inventory records in multiple warehouses

### Many-to-One Relationships
- **Order → User**: Each order belongs to one user
- **OrderItem → Order**: Each line item belongs to one order
- **OrderItem → Product**: Each line item references one product
- **Product → Category**: Each product belongs to one category
- **SubCategory → Category**: Each subcategory has one parent category
- **Inventory → Product**: Each inventory record tracks one product

### Computed Properties
- **Inventory.AvailableStock**: Calculated as CurrentStock - ReservedStock

### Indexed Properties
For query performance, the following properties are indexed:
- User: None explicitly indexed beyond primary key
- Product: Sku (Unique), Name (Indexed)
- Order: OrderNumber (Unique, Indexed), UserId (Indexed)
- OrderItem: OrderId (Indexed), ProductId (Indexed)
- Category: Name (Indexed), Slug (Unique)
- Inventory: ProductId (Indexed)
- AuditLog: EntityType (Indexed), EntityId (Indexed), Timestamp (Indexed)

### Constraints Summary
- **Primary Keys**: All entities have integer Id primary keys
- **Foreign Keys**: Explicitly defined via attributes where applicable
- **Unique Constraints**: User.Username, Product.Sku, Order.OrderNumber, Category.Slug
- **Required Fields**: Most properties are non-nullable where business logic requires values
- **Validation**: Each entity implements Validate() method with business rules