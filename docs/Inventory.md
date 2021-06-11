# Inventory

Represents a stock inventory record for a product in a specific warehouse location, tracking quantities, thresholds, and stock operations.

## API

### `public int Id`
Unique identifier for the inventory record. Set by persistence; do not modify directly.

### `public int ProductId`
Identifier of the associated product. Must reference an existing product; enforced by validation.

### `public string WarehouseLocation`
Physical or logical location where the stock is stored. Non-null and non-empty; validated on construction.

### `public int CurrentStock`
Current available quantity of the product in this location. Must be non-negative; validated on construction.

### `public int ReservedStock`
Quantity currently reserved for pending orders. Must be non-negative and not exceed `CurrentStock`; validated on construction.

### `public int MinimumThreshold`
Minimum stock level that triggers low-stock alerts. Must be non-negative; validated on construction.

### `public DateTime? LastRestockDate`
Timestamp of the last restock operation. Updated by `Restock`; null if never restocked.

### `public DateTime? LastCountDate`
Timestamp of the last stock count operation. Updated by `PerformStockCount`; null if never counted.

### `public DateTime CreatedDate`
Timestamp when the inventory record was created. Set by persistence; immutable.

### `public DateTime? ModifiedDate`
Timestamp when the inventory record was last modified. Updated on relevant operations; null if never modified.

### `public virtual Product? Product`
Navigation property to the associated product. Loaded lazily or eagerly depending on ORM configuration.

### `public Inventory()`
Default constructor. Initializes `CreatedDate` to `DateTime.UtcNow` and sets `ReservedStock` and `CurrentStock` to zero.

### `public Inventory(int productId, string warehouseLocation, int currentStock, int minimumThreshold)`
Parameterized constructor. Validates inputs and initializes `ProductId`, `WarehouseLocation`, `CurrentStock`, `MinimumThreshold`, `ReservedStock`, and `CreatedDate`.

### `public override bool Validate()`
Validates the inventory record. Checks that `ProductId` references an existing product, `WarehouseLocation` is non-empty, `CurrentStock` and `ReservedStock` are non-negative, and `MinimumThreshold` is non-negative. Returns `true` if valid; otherwise, `false`.

### `public void Restock(int quantity)`
Increases `CurrentStock` by the specified `quantity`. Validates that `quantity` is positive; throws `ArgumentOutOfRangeException` otherwise. Updates `LastRestockDate` to `DateTime.UtcNow`.

### `public void Withdraw(int quantity)`
Decreases `CurrentStock` by the specified `quantity`. Validates that `quantity` is positive and does not exceed `CurrentStock`; throws `InvalidOperationException` otherwise. Updates `ModifiedDate` to `DateTime.UtcNow`.

### `public void Reserve(int quantity)`
Increases `ReservedStock` by the specified `quantity`. Validates that `quantity` is positive and does not exceed `CurrentStock - ReservedStock`; throws `InvalidOperationException` otherwise. Updates `ModifiedDate` to `DateTime.UtcNow`.

### `public void ReleaseReservation(int quantity)`
Decreases `ReservedStock` by the specified `quantity`. Validates that `quantity` is positive and does not exceed `ReservedStock`; throws `InvalidOperationException` otherwise. Updates `ModifiedDate` to `DateTime.UtcNow`.

### `public void PerformStockCount(int countedStock)`
Updates `CurrentStock` to the specified `countedStock`. Validates that `countedStock` is non-negative; throws `ArgumentOutOfRangeException` otherwise. Updates `LastCountDate` and `ModifiedDate` to `DateTime.UtcNow`.

### `public bool IsLowStock()`
Returns `true` if `CurrentStock` is less than or equal to `MinimumThreshold`; otherwise, `false`.

## Usage
