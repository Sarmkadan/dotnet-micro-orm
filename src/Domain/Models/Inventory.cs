#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Tracks inventory movements and stock levels for products
/// </summary>
[Table("Inventory")]
public class sealed Inventory : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("ProductId", IsNullable = false)]
    [Indexed]
    public int ProductId { get; set; }

    [Column("WarehouseLocation", IsNullable = false, MaxLength = 50)]
    public string WarehouseLocation { get; set; } = string.Empty;

    [Column("CurrentStock", IsNullable = false)]
    public int CurrentStock { get; set; }

    [Column("ReservedStock", IsNullable = false)]
    public int ReservedStock { get; set; }

    [Column("AvailableStock", IsNullable = false)]
    [Computed]
    public int AvailableStock => CurrentStock - ReservedStock;

    [Column("MinimumThreshold", IsNullable = false)]
    public int MinimumThreshold { get; set; } = 10;

    [Column("LastRestockDate")]
    public DateTime? LastRestockDate { get; set; }

    [Column("LastCountDate")]
    public DateTime? LastCountDate { get; set; }

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [NotMapped]
    public virtual Product? Product { get; set; }

    public Inventory() { }

    public Inventory(int productId, string warehouseLocation, int currentStock)
    {
        ProductId = productId;
        WarehouseLocation = warehouseLocation;
        CurrentStock = currentStock;
        CreatedDate = DateTime.UtcNow;
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (ProductId <= 0)
            errors.Add("Valid product ID is required");

        if (string.IsNullOrWhiteSpace(WarehouseLocation) || WarehouseLocation.Length < 2)
            errors.Add("Warehouse location is required");

        if (CurrentStock < 0)
            errors.Add("Current stock cannot be negative");

        if (ReservedStock < 0 || ReservedStock > CurrentStock)
            errors.Add("Reserved stock must be between 0 and current stock");

        if (MinimumThreshold < 0)
            errors.Add("Minimum threshold cannot be negative");

        return errors.Count == 0;
    }

    public void Restock(int quantity, DateTime? restockDate = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        CurrentStock += quantity;
        LastRestockDate = restockDate ?? DateTime.UtcNow;
        PreSave();
    }

    public void Withdraw(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (AvailableStock < quantity)
            throw new InvalidOperationException($"Insufficient available stock. Available: {AvailableStock}, Requested: {quantity}");
        CurrentStock -= quantity;
        PreSave();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (AvailableStock < quantity)
            throw new InvalidOperationException("Insufficient available stock to reserve");
        ReservedStock += quantity;
        PreSave();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (ReservedStock < quantity)
            throw new InvalidOperationException("Cannot release more than reserved");
        ReservedStock -= quantity;
        PreSave();
    }

    public void PerformStockCount(int actualStock)
    {
        CurrentStock = actualStock;
        LastCountDate = DateTime.UtcNow;
        PreSave();
    }

    public bool IsLowStock() => AvailableStock <= MinimumThreshold;

    public int GetDaysLastRestocked() => LastRestockDate.HasValue ? (int)(DateTime.UtcNow - LastRestockDate.Value).TotalDays : -1;
}
