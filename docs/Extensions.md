# Extensions

The `Extensions` class in the `dotnet-micro-orm` project provides a collection of utility methods designed to simplify common object-relational mapping (ORM) tasks. These methods support metadata retrieval, entity manipulation, query building, and data formatting, enabling developers to interact with database entities and queries in a more intuitive and type-safe manner.

## API

### GetTableName

**Purpose**: Retrieves the database table name associated with a given entity type.

**Parameters**:
- `Type entityType`: The type of the entity for which to retrieve the table name.

**Return Value**: `string` - The configured table name for the entity type.

**Exceptions**: None specified.

---

### GetTableSchema

**Purpose**: Retrieves the database schema name associated with a given entity type.

**Parameters**:
- `Type entityType`: The type of the entity for which to retrieve the schema name.

**Return Value**: `string` - The configured schema name for the entity type.

**Exceptions**: None specified.

---

### GetColumnName

**Purpose**: Retrieves the database column name mapped to a given property.

**Parameters**:
- `PropertyInfo property`: The property for which to retrieve the column name.

**Return Value**: `string` - The configured column name for the property.

**Exceptions**: None specified.

---

### GetMappedProperties

**Purpose**: Retrieves all properties of an entity type that are mapped to database columns.

**Parameters**:
- `Type entityType`: The entity type to inspect.

**Return Value**: `List<PropertyInfo>` - A list of properties with database column mappings.

**Exceptions**: None specified.

---

### GetPrimaryKeyProperty

**Purpose**: Retrieves the primary key property of an entity type.

**Parameters**:
- `Type entityType`: The entity type to inspect.

**Return Value**: `PropertyInfo?` - The primary key property, or `null` if none exists.

**Exceptions**: None specified.

---

### CloneWithNewId<T>

**Purpose**: Creates a clone of an entity with a newly generated identifier.

**Parameters**:
- `this T entity`: The entity to clone.

**Return Value**: `T` - A new instance of `T` with a fresh identifier.

**Exceptions**:
- `ArgumentNullException`: Thrown when `entity` is `null`.

---

### ToDictionary<T>

**Purpose**: Converts an entity instance into a dictionary of its mapped property values.

**Parameters**:
- `this T entity`: The entity to convert.

**Return Value**: `Dictionary<string, object>` - A dictionary where keys are column names and values are property values.

**Exceptions**: None specified.

---

### HasPropertyChanged<T>

**Purpose**: Determines whether a specific property of an entity has been modified.

**Parameters**:
- `this T entity`: The entity to check.
- `string propertyName`: The name of the property to evaluate.

**Return Value**: `bool` - `true` if the property value differs from its original state; otherwise, `false`.

**Exceptions**: None specified.

---

### GetMemberName<T, TProperty>

**Purpose**: Extracts the name of a property from an expression, avoiding hard-coded strings.

**Parameters**:
- `Expression<Func<T, TProperty>> expression`: The expression representing the property.

**Return Value**: `string` - The name of the property.

**Exceptions**: None specified.

---

### Paginate<T>

**Purpose**: Splits a query result into paginated segments.

**Parameters**:
- `IQueryable<T> query`: The query to paginate.
- `int pageNumber`: The 1-based page number.
- `int pageSize`: The number of items per page.

**Return Value**: `(List<T> Items, int TotalCount)` - A tuple containing the items for the requested page and the total item count.

**Exceptions**:
- `ArgumentOutOfRangeException`: Thrown when `pageNumber` or `pageSize` is less than 1.

---

### SafeWhere<T>

**Purpose**: Applies a filter to a query while safely handling `null` predicates.

**Parameters**:
- `this IQueryable<T> query`: The query to filter.
- `Expression<Func<T, bool>> predicate`: The filter condition (may be `null`).

**Return Value**: `IQueryable<T>` - The filtered query, or the original query if the predicate is `null`.

**Exceptions**: None specified.

---

### ApplySort<T, TKey>

**Purpose**: Applies sorting to a query based on a key selector.

**Parameters**:
- `this IQueryable<T> query`: The query to sort.
- `Expression<Func<T, TKey>> keySelector`: The property to sort by.
- `bool ascending`: `true` for ascending order; `false` for descending.

**Return Value**: `IQueryable<T>` - The sorted query.

**Exceptions**: None specified.

---

### ToJsonString<T>

**Purpose**: Serializes an object to a JSON string using default settings.

**Parameters**:
- `this T obj`: The object to serialize.

**Return Value**: `string` - The JSON representation of the object.

**Exceptions**: None specified.

---

### IsNullOrEmpty

**Purpose**: Checks if a string is either `null` or empty.

**Parameters**:
- `this string value`: The string to evaluate.

**Return Value**: `bool` - `true` if the string is `null` or has zero length; otherwise, `false`.

**Exceptions**: None specified.

---

### GetAgeInYears

**Purpose**: Calculates the age in years from a given date.

**Parameters**:
- `this DateTime dateOfBirth`: The birth date to evaluate.

**Return Value**: `int` - The number of full years since the birth date.

**Exceptions**: None specified.

---

### FormatCurrency

**Purpose**: Formats a decimal value as a currency string using a specified culture.

**Parameters**:
- `this decimal value`: The numeric value to format.
- `string cultureCode`: The culture code (e.g., `"en-US"`).

**Return Value**: `string` - The formatted currency string.

**Exceptions**:
- `CultureNotFoundException`: Thrown when the specified culture is not supported.

---

## Usage

```csharp
// Example 1: Cloning an entity with a new ID
var originalUser = new User { Id = 1, Name = "Alice" };
var clonedUser = originalUser.CloneWithNewId();
Console.WriteLine($"Original ID: {originalUser.Id}, Cloned ID: {clonedUser.Id}");
// Output: Original ID: 1, Cloned ID: 0 (assuming BaseEntity resets ID on clone)
```

```csharp
// Example 2: Paginating a query
var usersQuery = dbContext.Users.AsQueryable();
var (users, total) = usersQuery.Paginate(2, 10); // Page 2, 10 items per page
Console.WriteLine($"Total users: {total}, Current page items: {users.Count}");
```

---

## Notes

- `GetPrimaryKeyProperty` returns `null` for entity types without an explicitly configured primary key.
- `CloneWithNewId<T>` requires the entity type to inherit from `BaseEntity` and provide a parameterless constructor. It does not perform deep cloning of complex properties.
- `SafeWhere<T>` allows chaining without null checks, returning the unmodified query when the predicate is `null`.
- `ApplySort<T, TKey>` uses standard LINQ ordering; null values in the sort key are handled according to the underlying provider's rules.
- All methods are static and stateless, making them thread-safe under normal usage conditions.
- `FormatCurrency` relies on `CultureInfo` resolution; invalid culture codes will throw at runtime.
- `GetAgeInYears` calculates age based on the current date and does not account for time zones or calendar systems.
