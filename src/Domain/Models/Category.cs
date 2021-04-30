#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Represents a product category entity
/// </summary>
[Table("Categories")]
public class Category : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Name", IsNullable = false, MaxLength = 100)]
    [Indexed]
    public string Name { get; set; } = string.Empty;

    [Column("Slug", IsNullable = false, MaxLength = 100)]
    [Unique]
    public string Slug { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("ParentCategoryId")]
    public int? ParentCategoryId { get; set; }

    [Column("DisplayOrder", IsNullable = false)]
    public int DisplayOrder { get; set; }

    [Column("IsActive", IsNullable = false)]
    public bool IsActive { get; set; } = true;

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [NotMapped]
    public virtual List<Product> Products { get; set; } = [];

    [NotMapped]
    public virtual Category? ParentCategory { get; set; }

    [NotMapped]
    public virtual List<Category> SubCategories { get; set; } = [];

    public Category() { }

    public Category(string name, string slug)
    {
        Name = name;
        Slug = slug;
        CreatedDate = DateTime.UtcNow;
    }

    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
            errors.Add("Category name is required and must be at least 2 characters");

        if (string.IsNullOrWhiteSpace(Slug) || Slug.Length < 2)
            errors.Add("Category slug is required and must be at least 2 characters");

        if (DisplayOrder < 0)
            errors.Add("Display order cannot be negative");

        return errors.Count == 0;
    }

    public void MoveUp()
    {
        if (DisplayOrder > 0)
            DisplayOrder--;
    }

    public void MoveDown()
    {
        DisplayOrder++;
    }

    public string GetBreadcrumb() => BuildBreadcrumb();

    private string BuildBreadcrumb(string current = "")
    {
        var path = string.IsNullOrEmpty(current) ? Name : $"{Name} > {current}";
        return ParentCategory is not null ? ParentCategory.BuildBreadcrumb(path) : path;
    }

    public int GetProductCount() => Products.Count;

    public void Deactivate()
    {
        IsActive = false;
        SubCategories.ForEach(c => c.Deactivate());
    }
}
