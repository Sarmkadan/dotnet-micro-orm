# ReflectionHelper

Provides cached reflection utilities for property access, attribute retrieval, type inspection, and instance creation. Designed to minimize reflection overhead in high-frequency ORM operations such as entity mapping, change tracking, and query building.

## API

### GetProperties
```csharp
public static PropertyInfo[] GetProperties(Type type)
```
Returns all public instance properties for `type`, using an internal cache.  
**Parameters**: `type` — The type to inspect.  
**Returns**: Array of `PropertyInfo`; never null.  
**Throws**: `ArgumentNullException` if `type` is null.

---

### GetProperty
```csharp
public static PropertyInfo? GetProperty(Type type, string name)
```
Retrieves a single public instance property by name, case-sensitive.  
**Parameters**: `type` — The declaring type. `name` — Property name.  
**Returns**: `PropertyInfo` if found; otherwise `null`.  
**Throws**: `ArgumentNullException` if `type` or `name` is null.

---

### GetPropertyValue
```csharp
public static object? GetPropertyValue(object instance, PropertyInfo property)
```
Reads the value of `property` from `instance`.  
**Parameters**: `instance` — Target object. `property` — Property to read.  
**Returns**: Property value, or `null` if the property returns null.  
**Throws**: `ArgumentNullException` if `instance` or `property` is null. `TargetException` if `instance` is not compatible with `property.DeclaringType`.

---

### SetPropertyValue
```csharp
public static void SetPropertyValue(object instance, PropertyInfo property, object? value)
```
Writes `value` to `property` on `instance`.  
**Parameters**: `instance` — Target object. `property` — Property to write. `value` — Value to assign (must be assignable to property type).  
**Throws**: `ArgumentNullException` if `instance` or `property` is null. `ArgumentException` if `value` is not compatible. `TargetException` if `instance` is not compatible.

---

### IsNullableType
```csharp
public static bool IsNullableType(Type type)
```
Determines whether `type` is a nullable value type (`Nullable<T>`).  
**Parameters**: `type` — Type to test.  
**Returns**: `true` if `type` is `Nullable<T>`; otherwise `false`.  
**Throws**: `ArgumentNullException` if `type` is null.

---

### IsSimpleType
```csharp
public static bool IsSimpleType(Type type)
```
Checks if `type` is a primitive, enum, string, decimal, DateTime, DateTimeOffset, TimeSpan, Guid, or nullable variant thereof. Used to decide whether a property requires complex mapping.  
**Parameters**: `type` — Type to test.  
**Returns**: `true` for simple types; `false` for classes, collections, etc.  
**Throws**: `ArgumentNullException` if `type` is null.

---

### GetAttributes\<T\>
```csharp
public static T[] GetAttributes<T>(MemberInfo member, bool inherit = true) where T : Attribute
```
Retrieves all attributes of type `T` applied to `member`.  
**Parameters**: `member` — Target member. `inherit` — Search inheritance chain.  
**Returns**: Array of matching attributes; empty if none.  
**Throws**: `ArgumentNullException` if `member` is null.

---

### GetAttribute\<T\>
```csharp
public static T? GetAttribute<T>(MemberInfo member, bool inherit = true) where T : Attribute
```
Retrieves the first attribute of type `T` on `member`.  
**Parameters**: `member` — Target member. `inherit` — Search inheritance chain.  
**Returns**: The attribute instance, or `null` if not found.  
**Throws**: `ArgumentNullException` if `member` is null.

---

### ImplementsGenericInterface
```csharp
public static bool ImplementsGenericInterface(Type type, Type genericInterface)
```
Tests whether `type` implements a constructed version of `genericInterface` (e.g., `IEnumerable<T>`).  
**Parameters**: `type` — Type to test. `genericInterface` — Open generic interface definition.  
**Returns**: `true` if `type` implements at least one closed generic variant; otherwise `false`.  
**Throws**: `ArgumentNullException` if either parameter is null. `ArgumentException` if `genericInterface` is not a generic type definition.

---

### CreateInstance
```csharp
public static object? CreateInstance(Type type, params object?[] args)
```
Creates an instance of `type` using the constructor that best matches `args`.  
**Parameters**: `type` — Type to instantiate. `args` — Constructor arguments.  
**Returns**: New instance, or `null` if `type` is abstract or interface.  
**Throws**: `ArgumentNullException` if `type` is null. `MissingMethodException` if no matching constructor. `TargetInvocationException` if constructor throws.

---

### CreateInstance\<T\>
```csharp
public static T? CreateInstance<T>(params object?[] args) where T : class
```
Generic overload of `CreateInstance`.  
**Parameters**: `args` — Constructor arguments.  
**Returns**: New instance of `T`, or `null` if `T` is abstract/interface.  
**Throws**: `MissingMethodException` if no matching constructor. `TargetInvocationException` if constructor throws.

---

### GetGenericArguments
```csharp
public static Type[] GetGenericArguments(Type type)
```
Returns the generic type arguments of a constructed generic type (e.g., `List<int>` → `[int]`).  
**Parameters**: `type` — Constructed generic type.  
**Returns**: Array of type arguments; empty if `type` is not generic or is an open generic definition.  
**Throws**: `ArgumentNullException` if `type` is null.

---

### GetUnderlyingType
```csharp
public static Type? GetUnderlyingType(Type type)
```
Returns the underlying type of a nullable type, or the element type of an array, or the generic argument of `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`. Returns `null` for non-matching types.  
**Parameters**: `type` — Type to inspect.  
**Returns**: Underlying/element type, or `null`.  
**Throws**: `ArgumentNullException` if `type` is null.

---

### ClearCache
```csharp
public static void ClearCache()
```
Clears all internal reflection caches (property arrays, attribute lookups, constructor delegates). Call after dynamic assembly loading or in test teardown.  
**Throws**: None.

## Usage

### Mapping entity properties to database columns
```csharp
var properties = ReflectionHelper.GetProperties(typeof(Order));
foreach (var prop in properties)
{
    var columnAttr = ReflectionHelper.GetAttribute<ColumnAttribute>(prop);
    var columnName = columnAttr?.Name ?? prop.Name;
    var isNullable = ReflectionHelper.IsNullableType(prop.PropertyType);
    // build column mapping
}
```

### Creating entity instances from query results
```csharp
public TEntity Materialize<TEntity>(IDataReader reader) where TEntity : class
{
    var entity = ReflectionHelper.CreateInstance<TEntity>();
    var props = ReflectionHelper.GetProperties(typeof(TEntity));
    foreach (var prop in props)
    {
        if (!prop.CanWrite) continue;
        var ordinal = reader.GetOrdinal(prop.Name);
        if (!reader.IsDBNull(ordinal))
        {
            var value = reader.GetValue(ordinal);
            ReflectionHelper.SetPropertyValue(entity, prop, value);
        }
    }
    return entity;
}
```

## Notes

- All `GetProperties`, `GetAttributes`, and `GetAttribute` results are cached per `Type`/`MemberInfo`. `ClearCache` invalidates all caches globally; not thread-safe with concurrent readers — call only during quiescent periods (e.g., app startup, test teardown).
- `CreateInstance` caches constructor delegates per signature; first call for a given signature incurs expression compilation cost.
- `IsSimpleType` treats `decimal`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, and their nullable forms as simple. Extend via fork if domain types require different classification.
- `GetUnderlyingType` returns `null` for `IEnumerable` (non-generic), `ArrayList`, or custom collections not implementing generic collection interfaces.
- Thread safety: read-only methods (`GetProperties`, `GetProperty`, `IsNullableType`, `IsSimpleType`, `GetAttributes`, `GetAttribute`, `ImplementsGenericInterface`, `GetGenericArguments`, `GetUnderlyingType`) are safe for concurrent use. `ClearCache` is not synchronized; avoid concurrent calls with readers.
