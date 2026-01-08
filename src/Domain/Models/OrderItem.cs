// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Represents an individual item within an order
/// </summary>
[Table("OrderItems")]
public class OrderItem : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("OrderId", IsNullable = false)]
    [Indexed]
    public int OrderId { get; set; }

    [Column("ProductId", IsNullable = false)]
    public int ProductId { get; set; }

    [Column("ProductName", IsNullable = false, MaxLength = 200)]
    public string ProductName { get; set; } = string.Empty;

    [Column("Quantity", IsNullable = false)]
    public int Quantity { get; set; }

    [Column("UnitPrice", IsNullable = false, Precision = 18, Scale = 2)]
    public decimal UnitPrice { get; set; }

    [Column("Discount", Precision = 18, Scale = 2)]
    public decimal Discount { get; set; }

    [Column("TaxAmount", Precision = 18, Scale = 2)]
    public decimal TaxAmount { get; set; }

    [Column("LineTotal", IsNullable = false, Precision = 18, Scale = 2)]
    public decimal LineTotal { get; set; }

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [NotMapped]
    public virtual Order? Order { get; set; }

    [NotMapped]
    public virtual Product? Product { get; set; }

    public OrderItem() { }

    public OrderItem(int orderId, int productId, string productName, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        CreatedDate = DateTime.UtcNow;
        CalculateLineTotal();
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (OrderId <= 0)
            errors.Add("Valid order ID is required");

        if (ProductId <= 0)
            errors.Add("Valid product ID is required");

        if (Quantity <= 0)
            errors.Add("Quantity must be at least 1");

        if (UnitPrice <= 0)
            errors.Add("Unit price must be greater than zero");

        if (string.IsNullOrWhiteSpace(ProductName))
            errors.Add("Product name is required");

        if (Discount < 0)
            errors.Add("Discount cannot be negative");

        if (Discount > UnitPrice * Quantity)
            errors.Add("Discount cannot exceed line subtotal");

        return errors.Count == 0;
    }

    public void CalculateLineTotal()
    {
        var subtotal = UnitPrice * Quantity;
        LineTotal = subtotal - Discount + TaxAmount;
    }

    public void ApplyDiscount(decimal amount)
    {
        if (amount < 0 || amount > UnitPrice * Quantity)
            throw new ArgumentException("Invalid discount amount", nameof(amount));
        Discount = amount;
        CalculateLineTotal();
    }

    public decimal GetSubtotal() => UnitPrice * Quantity;

    public decimal GetAfterDiscount() => GetSubtotal() - Discount;

    public decimal GetTotalWithTax() => GetAfterDiscount() + TaxAmount;

    public decimal GetTaxRate()
    {
        var subtotal = GetSubtotal();
        return subtotal > 0 ? TaxAmount / subtotal : 0;
    }
}
