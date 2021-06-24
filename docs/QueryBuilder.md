# QueryBuilder

A fluent API helper for constructing and executing LINQ-like queries against micro-ORM data contexts. It enables deferred execution of `SELECT`, `WHERE`, `ORDER BY`, `SKIP`, `TAKE`, and navigation property inclusion (`Include`) operations, with async result materialization methods.

## API

### `public QueryBuilder`
The entry point for constructing queries. Typically instantiated via a micro-ORM context or repository.

### `public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)`
Adds a `WHERE` clause to the query using the supplied predicate expression.
- **Parameters**: `predicate` – A boolean expression tree defining the filter.
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentNullException` if `predicate` is `null`.

### `public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)`
Adds an `ORDER BY` clause using the supplied ascending key selector.
- **Type parameters**: `TKey` – The type of the key used for ordering.
- **Parameters**: `keySelector` – An expression selecting the ordering key.
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentNullException` if `keySelector` is `null`.

### `public IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)`
Adds an `ORDER BY ... DESC` clause using the supplied descending key selector.
- **Type parameters**: `TKey` – The type of the key used for ordering.
- **Parameters**: `keySelector` – An expression selecting the ordering key.
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentNullException` if `keySelector` is `null`.

### `public IQueryBuilder<T> Take(int count)`
Limits the number of rows returned to `count`.
- **Parameters**: `count` – The maximum number of rows to return (must be ≥ 0).
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentOutOfRangeException` if `count` < 0.

### `public IQueryBuilder<T> Skip(int count)`
Skips the first `count` rows before returning results.
- **Parameters**: `count` – The number of rows to skip (must be ≥ 0).
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentOutOfRangeException` if `count` < 0.

### `public IQueryBuilder<T> Include<TProperty>(Expression<Func<T, TProperty>> navigationPath)`
Includes a navigation property or sub-path in the result set to avoid lazy-loading overhead.
- **Type parameters**: `TProperty` – The type of the included navigation property.
- **Parameters**: `navigationPath` – An expression identifying the navigation property to include.
- **Return value**: The same `IQueryBuilder<T>` instance for chaining.
- **Throws**: `ArgumentNullException` if `navigationPath` is `null`.

### `public async Task<List<T>> ToListAsync()`
Executes the composed query and materializes the results as a list.
- **Return value**: A `Task<List<T>>` resolving to the query results.
- **Throws**: `InvalidOperationException` if the query is malformed or the connection is unavailable.

### `public async Task<T?> FirstOrDefaultAsync()`
Executes the composed query and returns the first result or `default(T)` if none exist.
- **Return value**: A `Task<T?>` resolving to the first element or `null`.
- **Throws**: `InvalidOperationException` if the query is malformed or the connection is unavailable.

### `public async Task<int> CountAsync()`
Executes the composed query and returns the number of matching rows.
- **Return value**: A `Task<int>` resolving to the row count.
- **Throws**: `InvalidOperationException` if the query is malformed or the connection is unavailable.

## Usage

```csharp
// Example 1: Basic filtering and pagination
var activeUsers = await db.Query<User>()
    .Where(u => u.IsActive && u.LastLogin > DateTime.UtcNow.AddDays(-30))
    .OrderBy(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .Skip(20)
    .Take(10)
    .Include(u => u.Profile)
    .ToListAsync();

// Example 2: Aggregation with conditional ordering
var highValueOrders = await db.Query<Order>()
    .Where(o => o.Total > 1000)
    .OrderByDescending(o => o.Total)
    .Select(o => new { o.Id, o.Total, o.Customer.Name })
    .ToListAsync();
```

## Notes

- **Thread-safety**: Instances are not thread-safe; each thread should use its own `QueryBuilder`.
- **Deferred execution**: All clauses (`Where`, `OrderBy`, etc.) are accumulated until an async materialization method (`ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`) is invoked.
- **Null navigation paths**: Including a navigation property that is `null` in the database will not cause an exception; the property will simply be omitted from the result.
- **Parameter sniffing**: Predicates and key selectors are compiled to expressions and parameterized to avoid SQL injection and optimize query plans.
- **Case sensitivity**: SQL `COLLATE` behavior depends on the underlying database provider and is not controlled by this API.
