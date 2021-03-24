#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Represents a product entity in the catalog
/// </summary>
[Table("Products")]
public class sealed Product : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Sku", IsNullable = false, MaxLength = 50)]
    [Unique]
    public string Sku { get; set; } = string.Empty;

    [Column("Name", IsNullable = false, MaxLength = 200)]
    [Indexed]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("Price", IsNullable = false, Precision = 18, Scale = 2)]
    public decimal Price { get; set; }

    [Column("CostPrice", Precision = 18, Scale = 2)]
    public decimal? CostPrice { get; set; }

    [Column("CategoryId", IsNullable = false)]
    public int CategoryId { get; set; }

    [Column("StockQuantity", IsNullable = false)]
    public int StockQuantity { get; set; }

    [Column("IsActive", IsNullable = false)]
    public bool IsActive { get; set; } = true;

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [NotMapped]
    public virtual Category? Category { get; set; }

    [NotMapped]
    public virtual List<OrderItem> OrderItems { get; set; } = [];

    public Product() { }

    public Product(string sku, string name, decimal price, int categoryId)
    {
        Sku = sku;
        Name = name;
        Price = price;
        CategoryId = categoryId;
        CreatedDate = DateTime.UtcNow;
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(Sku) || Sku.Length < 3)
            errors.Add("SKU must be at least 3 characters long");

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
            errors.Add("Product name is required and must be at least 2 characters");

        if (Price <= 0)
            errors.Add("Price must be greater than zero");

        if (CostPrice.HasValue && CostPrice.Value < 0)
            errors.Add("Cost price cannot be negative");

        if (StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative");

        if (CategoryId <= 0)
            errors.Add("Valid category ID is required");

        return errors.Count == 0;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        StockQuantity += quantity;
        PreSave();
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock available");
        StockQuantity -= quantity;
        PreSave();
    }

    public decimal GetProfit()
    {
        if (CostPrice is null || CostPrice == 0)
            return Price;
        return Price - CostPrice.Value;
    }

    public bool IsLowStock(int threshold = 10) => StockQuantity <= threshold;
}
