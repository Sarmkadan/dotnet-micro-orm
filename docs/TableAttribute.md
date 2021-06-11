# TableAttribute

`TableAttribute` is a data annotation used in `dotnet-micro-orm` to decorate classes that represent database tables. It maps a C# class to a specific database table, allowing configuration of the table name, schema, and other metadata. This attribute is essential for defining the relationship between object-oriented code and relational database structures.

## API

### `public string Name`
Gets or sets the name of the table in the database. If not specified, the class name is used as the default table name.

### `public string Schema`
Gets or sets the schema of the table in the database. If not specified, the default schema of the database connection is used.

### `public TableAttribute()`
Initializes a new instance of the `TableAttribute` class with default values. The table name defaults to the class name, and the schema defaults to `null`.

### `public TableAttribute(string name)`
Initializes a new instance of the `TableAttribute` class with the specified table name. The schema defaults to `null`.

### `public bool IsPrimaryKey`
Gets or sets a value indicating whether the column is part of the primary key. Defaults to `false`.

### `public bool IsNullable`
Gets or sets a value indicating whether the column allows `NULL` values in the database. Defaults to `true`.

### `public int MaxLength`
Gets or sets the maximum length of the column data. Used for string or binary data types. Defaults to `0`, indicating no limit.

### `public int Precision`
Gets or sets the precision of the column for numeric data types. Defaults to `0`, indicating no precision is specified.

### `public int Scale`
Gets or sets the scale of the column for numeric data types. Defaults to `0`, indicating no scale is specified.

### `public string? DefaultValue`
Gets or sets the default value of the column in the database. Defaults to `null`, indicating no default value is set.

### `public bool IsAutoIncrement`
Gets or sets a value indicating whether the column is auto-incremented by the database. Defaults to `false`.

### `public string? ColumnType`
Gets or sets the explicit column type to use in the database. If specified, overrides the default type inferred by the ORM. Defaults to `null`.

### `public ColumnAttribute(string name, bool isnullable)`
Initializes a new instance of the `ColumnAttribute` class with the specified column name and nullability. This constructor is used to decorate properties in a class mapped to a table.

### `public sealed class NotMappedAttribute : Attribute`
A marker attribute indicating that a property should not be mapped to a database column. This attribute is sealed and cannot be inherited.

### `public string ReferencedTable`
Gets or sets the name of the referenced table in a foreign key relationship.

### `public string ReferencedColumn`
Gets or sets the name of the referenced column in a foreign key relationship.

### `public ForeignKeyAttribute`
A marker attribute indicating that a property represents a foreign key relationship. This attribute is used to define relationships between tables.

### `public string? Name`
Gets or sets the name of the foreign key constraint. If not specified, the ORM generates a default name.

## Usage

### Example 1: Basic Table Mapping
