# BatchUpsertOperation

`BatchUpsertOperation<T>` provides a lightweight, asynchronous way to insert or update entities in a database using the dotnet‑micro‑orm library. It encapsulates the logic needed to determine the target table, map properties to columns, and execute the appropriate SQL MERGE/UPSERT statement for a single entity or a collection of entities.

## API

### BatchUpsertOperation()
Initializes a new instance of the `BatchUpsertOperation<T>` class. The instance is ready to be used after the generic type `T` has been supplied; no configuration parameters are required.

### Task<UpsertResult<T>> UpsertAsync(T entity, CancellationToken cancellationToken = default)
Asynchronously inserts `entity` if it does not exist, or updates it if it does, based on the primary key defined by the mapping.

- **Parameters**
  - `entity`: The instance of `T` to upsert. Must not be `null`.
  - `cancellationToken`: Optional token to observe while awaiting the operation.
- **Return Value**: A `Task` that completes with an `UpsertResult<T>` indicating whether the entity was inserted or updated and providing any generated keys.
- **Exceptions**
  - `ArgumentNullException` if `entity` is `null`.
  - `InvalidOperationException` if the type `T` lacks a defined primary key or table mapping.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.
  - Any exception thrown by the underlying data provider (e.g., `DbException`) is propagated.

### Task<List<UpsertResult<T>>> UpsertRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
Asynchronously upserts a sequence of entities in a single batch operation.

- **Parameters**
  - `entities`: The collection of `T` instances to upsert. Must not be `null` and must contain at least one element.
  - `cancellationToken`: Optional token to observe while awaiting the operation.
- **Return Value**: A `Task` that completes with a list of `UpsertResult<T>` corresponding to each input element, preserving order.
- **Exceptions**
  - `ArgumentNullException` if `entities` is `null`.
  - `ArgumentException` if `entities` contains any `null` element or is empty.
  - `InvalidOperationException` if the type `T` lacks a defined primary key or table mapping.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.
  - Any exception thrown by the underlying data provider is propagated.

### string GetTableName()
Returns the name of the database table associated with type `T` as determined by the ORM’s mapping conventions or attributes.

- **Parameters**: None.
- **Return Value**: The table name; never `null` or empty for a correctly mapped type.
- **Exceptions**: Throws `InvalidOperationException` if the table name cannot be resolved.

### string GetTableSchema()
Returns the schema name of the database table associated with type `T`. If no schema is explicitly configured, the default schema for the connection is returned.

- **Parameters**: None.
- **Return Value**: The schema name; may be an empty string when the default schema is used.
- **Exceptions**: Throws `InvalidOperationException` if the schema cannot be resolved.

### string PropertyName { get; }
Gets the name of the CLR property that corresponds to the column represented by the current mapping context (used internally by the builder). For the `BatchUpsertOperation` instance itself this property reflects the primary key property of `T`.

- **Return Value**: The property name; never `null`.
- **Exceptions**: None.

### string ColumnName { get; }
Gets the name of the database column that corresponds to the property represented by the current mapping context.

- **Return Value**: The column name; never `null`.
- **Exceptions**: None.

### bool IsPrimaryKey { get; }
Indicates whether the property represented by the current mapping context is part of the primary key for type `T`.

- **Return Value**: `true` if the property is a primary key column; otherwise `false`.
- **Exceptions**: None.

### Type PropertyType { get; }
Gets the CLR type of the property represented by the current mapping context.

- **Return Value**: The `Type` of the property.
- **Exceptions**: None.

## Usage

### Single entity upsert
```csharp
using var db = new MicroOrmDbContext(connectionString);
var operation = new BatchUpsertOperation<Product>();

var newProduct = new Product { Name = "Widget", Price = 9.99m };
UpsertResult<Product> result = await operation.UpsertAsync(newProduct);

if result.WasInserted
    Console.WriteLine($"Inserted new product with Id {result.Entity.Id}");
else
    Console.WriteLine($"Updated existing product Id {result.Entity.Id}");
```

### Batch upsert of multiple entities
```csharp
using var db = new MicroOrmDbContext(connectionString);
var operation = new BatchUpsertOperation<Order>();

var orders = new List<Order>
{
    new Order { CustomerId = 1, Amount = 100, OrderDate = DateTime.UtcNow },
    new Order { CustomerId = 2, Amount = 250, OrderDate = DateTime.UtcNow.AddDays(-1) },
    // … more orders
};

IOrder instances
};

List<UpsertResult<Order>> results = await operation.UpsertRangeAsync(orders);

int inserted = results.Count(r => r.WasInserted);
int updated  = results.Count(r => !r.WasInserted);
Console.WriteLine($"Upserted {orders.Count} orders: {inserted} inserted, {updated} updated.");
```

## Notes
- The `BatchUpsertOperation<T>` instance does **not** maintain any internal mutable state beyond the read‑only mapping information; therefore it is safe to reuse the same instance for multiple sequential operations.
- Concurrent calls to `UpsertAsync` or `UpsertRangeAsync` on the **same** instance are not thread‑safe because they share the same underlying command builder. For parallel execution, create separate instances per thread or synchronize access.
- If the generic type `T` is not properly mapped (missing table attribute, missing primary key, or mismatched property/column names), the methods will throw `InvalidOperationException` before attempting any database interaction.
- Passing `null` for a single entity or any element in the collection passed to `UpsertRangeAsync` results in an `ArgumentNullException`.
- Cancellation tokens are honored; if cancellation is requested before the operation starts, the method throws `OperationCanceledException` without accessing the database.
- The returned `UpsertResult<T>` contains the entity instance (potentially with generated key values populated) and a boolean flag indicating whether the operation was an insert or an update. Consumers should inspect this flag when post‑processing is required.
