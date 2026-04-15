#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Represents a customer order entity
/// </summary>
[Table("Orders")]
public class sealed Order : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("OrderNumber", IsNullable = false, MaxLength = 50)]
    [Unique]
    [Indexed]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("UserId", IsNullable = false)]
    [Indexed]
    public int UserId { get; set; }

    [Column("OrderDate", IsNullable = false)]
    public DateTime OrderDate { get; set; }

    [Column("Status", IsNullable = false, MaxLength = 20)]
    public string Status { get; set; } = "Pending";

    [Column("TotalAmount", IsNullable = false, Precision = 18, Scale = 2)]
    public decimal TotalAmount { get; set; }

    [Column("TaxAmount", Precision = 18, Scale = 2)]
    public decimal TaxAmount { get; set; }

    [Column("ShippingAddress", IsNullable = false)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Column("BillingAddress")]
    public string? BillingAddress { get; set; }

    [Column("ShippingDate")]
    public DateTime? ShippingDate { get; set; }

    [Column("DeliveryDate")]
    public DateTime? DeliveryDate { get; set; }

    [Column("Notes")]
    public string? Notes { get; set; }

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [NotMapped]
    public virtual User? User { get; set; }

    [NotMapped]
    public virtual List<OrderItem> Items { get; set; } = [];

    public Order() { }

    public Order(int userId, string shippingAddress)
    {
        UserId = userId;
        ShippingAddress = shippingAddress;
        OrderNumber = GenerateOrderNumber();
        OrderDate = DateTime.UtcNow;
        CreatedDate = DateTime.UtcNow;
        Status = "Pending";
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(OrderNumber))
            errors.Add("Order number is required");

        if (UserId <= 0)
            errors.Add("Valid user ID is required");

        if (string.IsNullOrWhiteSpace(ShippingAddress) || ShippingAddress.Length < 10)
            errors.Add("Valid shipping address is required");

        if (Items.Count == 0)
            errors.Add("Order must contain at least one item");

        if (TotalAmount <= 0)
            errors.Add("Total amount must be greater than zero");

        if (!IsValidStatus(Status))
            errors.Add("Invalid order status");

        return errors.Count == 0;
    }

    public void AddItem(OrderItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        Items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(OrderItem item)
    {
        Items.Remove(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalAmount = Items.Sum(i => i.Quantity * i.UnitPrice);
        PreSave();
    }

    public void Ship(DateTime shipDate)
    {
        if (Status != "Confirmed")
            throw new InvalidOperationException("Only confirmed orders can be shipped");
        Status = "Shipped";
        ShippingDate = shipDate;
        PreSave();
    }

    public void MarkAsDelivered()
    {
        Status = "Delivered";
        DeliveryDate = DateTime.UtcNow;
        PreSave();
    }

    public void Cancel()
    {
        if (Status is "Shipped" or "Delivered")
            throw new InvalidOperationException("Cannot cancel a shipped or delivered order");
        Status = "Cancelled";
        PreSave();
    }

    private static string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

    private static bool IsValidStatus(string status) => status is "Pending" or "Confirmed" or "Shipped" or "Delivered" or "Cancelled";

    public decimal GetTaxableAmount() => TotalAmount - TaxAmount;
}
