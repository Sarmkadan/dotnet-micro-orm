#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Specifies the database table name for the entity
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class sealed TableAttribute : Attribute
{
    public string Name { get; }
    public string Schema { get; set; } = Constants.OrmConstants.DefaultSchema;

    public TableAttribute(string name) => Name = name;
}

/// <summary>
/// Specifies column mapping for a property
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed ColumnAttribute : Attribute
{
    public string Name { get; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; } = true;
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string? ColumnType { get; set; }

    public ColumnAttribute(string name) => Name = name;

    public ColumnAttribute(string name, bool isnullable) : this(name) => IsNullable = isnullable;
}

/// <summary>
/// Indicates the property should not be mapped to a database column
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed NotMappedAttribute : Attribute { }

/// <summary>
/// Specifies a foreign key relationship
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed ForeignKeyAttribute : Attribute
{
    public string ReferencedTable { get; }
    public string ReferencedColumn { get; }

    public ForeignKeyAttribute(string referencedTable, string referencedColumn)
    {
        ReferencedTable = referencedTable;
        ReferencedColumn = referencedColumn;
    }
}

/// <summary>
/// Marks a property as a unique constraint
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed UniqueAttribute : Attribute
{
    public string? Name { get; set; }
}

/// <summary>
/// Marks a property as indexed
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed IndexedAttribute : Attribute
{
    public string? Name { get; set; }
    public bool IsUnique { get; set; }
}

/// <summary>
/// Marks a property as a computed/generated column
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class sealed ComputedAttribute : Attribute
{
    public string? ComputationExpression { get; set; }
}
