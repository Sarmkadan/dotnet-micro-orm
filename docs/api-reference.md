// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete reference for DotnetMicroOrm public APIs.

## IRepository<T>

Generic repository for CRUD operations and querying.

### Query Methods

#### GetByIdAsync(int id)

Retrieve a single entity by ID.

```csharp
var product = await repository.GetByIdAsync(5);
if (product != null)
{
    Console.WriteLine($"Found: {product.Name}");
}
```

**Parameters:**
- `id` (int): Entity ID

**Returns:** Entity if found, null otherwise

**Performance:** O(1) with cache, O(log n) database query

---

#### FirstOrDefaultAsync(Specification<T> spec)

Get first entity matching specification.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name);

var product = await repository.FirstOrDefaultAsync(spec);
```

**Parameters:**
- `spec` (Specification<T>): Query specification

**Returns:** First matching entity or null

**Throws:** QueryException on database error

---

#### GetAsync(Specification<T> spec)

Get all entities matching specification.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.StockQuantity > 0)
    .OrderByDescending(p => p.Price)
    .Take(20);

var products = await repository.GetAsync(spec);
```

**Parameters:**
- `spec` (Specification<T>): Query specification

**Returns:** List of matching entities

**Performance:** Database query with automatic result caching

---

#### CountAsync(Specification<T> spec)

Count entities matching specification.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 500);

int count = await repository.CountAsync(spec);
Console.WriteLine($"Expensive products: {count}");
```

**Parameters:**
- `spec` (Specification<T>): Query specification

**Returns:** Number of matching entities

**Performance:** Optimized COUNT(*) query

---

#### AnyAsync(Specification<T> spec)

Check if any entities match specification.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.StockQuantity == 0);

bool hasOutOfStock = await repository.AnyAsync(spec);
```

**Parameters:**
- `spec` (Specification<T>): Query specification

**Returns:** True if any matches exist

---

### Mutation Methods

#### AddAsync(T entity)

Add single entity to database.

```csharp
var product = new Product
{
    Name = "Laptop",
    Price = 999.99m,
    CreatedAt = DateTime.UtcNow
};

await repository.AddAsync(product);
await unitOfWork.SaveChangesAsync();
```

**Parameters:**
- `entity` (T): Entity to add

**Returns:** Added entity with assigned ID

**Throws:** ValidationException if entity invalid

---

#### AddRangeAsync(IEnumerable<T> entities)

Add multiple entities in batch.

```csharp
var products = new List<Product>
{
    new Product { Name = "Laptop", Price = 999.99m },
    new Product { Name = "Mouse", Price = 29.99m },
    new Product { Name = "Keyboard", Price = 79.99m }
};

await repository.AddRangeAsync(products);
await unitOfWork.SaveChangesAsync();
// 3 products inserted in single batch operation
```

**Parameters:**
- `entities` (IEnumerable<T>): Entities to add

**Returns:** Added entities

**Performance:** ~10x faster than individual AddAsync calls

**Throws:** ValidationException if any entity invalid

---

#### UpdateAsync(T entity)

Update single entity.

```csharp
var product = await repository.GetByIdAsync(1);
product.Price = 899.99m;
product.UpdatedAt = DateTime.UtcNow;

await repository.UpdateAsync(product);
await unitOfWork.SaveChangesAsync();
```

**Parameters:**
- `entity` (T): Entity with changes

**Returns:** Updated entity

**Throws:** ConcurrencyException if version mismatch

---

#### UpdateRangeAsync(IEnumerable<T> entities)

Update multiple entities in batch.

```csharp
var products = await repository.GetAsync(
    new Specification<Product>().Where(p => p.Price > 1000)
);

foreach (var product in products)
{
    product.Price = product.Price * 0.9m; // 10% discount
}

await repository.UpdateRangeAsync(products);
await unitOfWork.SaveChangesAsync();
```

**Parameters:**
- `entities` (IEnumerable<T>): Entities to update

**Returns:** Updated entities

**Performance:** ~10x faster than individual UpdateAsync calls

---

#### DeleteAsync(int id)

Delete entity by ID.

```csharp
await repository.DeleteAsync(5);
await unitOfWork.SaveChangesAsync();
```

**Parameters:**
- `id` (int): Entity ID to delete

**Throws:** EntityNotFoundException if not found

---

#### DeleteAsync(T entity)

Delete specific entity.

```csharp
var product = await repository.GetByIdAsync(5);
if (product != null)
{
    await repository.DeleteAsync(product);
    await unitOfWork.SaveChangesAsync();
}
```

**Parameters:**
- `entity` (T): Entity to delete

---

#### DeleteRangeAsync(IEnumerable<T> entities)

Delete multiple entities in batch.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.StockQuantity == 0);

var outOfStock = await repository.GetAsync(spec);
await repository.DeleteRangeAsync(outOfStock);
await unitOfWork.SaveChangesAsync();
```

**Parameters:**
- `entities` (IEnumerable<T>): Entities to delete

**Performance:** ~10x faster than individual DeleteAsync calls

---

## Specification<T>

Fluent query builder for type-safe queries.

### Where(Expression<Func<T, bool>> predicate)

Add WHERE clause.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .Where(p => p.StockQuantity > 0);
```

**Parameters:**
- `predicate`: LINQ predicate

**Returns:** Specification for chaining

---

### AndWhere(Expression<Func<T, bool>> predicate)

Add AND WHERE clause.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .AndWhere(p => p.StockQuantity > 0);
```

---

### OrWhere(Expression<Func<T, bool>> predicate)

Add OR WHERE clause.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 1000)
    .OrWhere(p => p.Rating > 4.5m);
```

---

### OrderBy(Expression<Func<T, TKey>> keySelector)

Add ORDER BY ascending.

```csharp
var spec = new Specification<Product>()
    .OrderBy(p => p.Name)
    .OrderBy(p => p.Price);
```

---

### OrderByDescending(Expression<Func<T, TKey>> keySelector)

Add ORDER BY descending.

```csharp
var spec = new Specification<Product>()
    .OrderByDescending(p => p.CreatedAt);
```

---

### Skip(int count)

Skip rows (pagination).

```csharp
var pageSize = 20;
var pageNumber = 2;

var spec = new Specification<Product>()
    .OrderBy(p => p.Name)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize);
```

---

### Take(int count)

Take rows (pagination).

```csharp
var spec = new Specification<Product>()
    .Take(10); // First 10 products
```

---

### Include(Expression<Func<T, object>> navigationProperty)

Eager load related entities.

```csharp
var spec = new Specification<Order>()
    .Include(o => o.Customer)
    .Include(o => o.OrderItems);
```

---

### AsNoTracking()

Load entities without change tracking.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .AsNoTracking(); // Faster for read-only scenarios
```

---

## IUnitOfWork

Transaction and repository management.

### Repository<T>() Method

Get repository for entity type.

```csharp
var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
var productRepository = unitOfWork.Repository<Product>();
var orderRepository = unitOfWork.Repository<Order>();
```

**Returns:** IRepository<T>

**Note:** Repositories are cached per UnitOfWork instance

---

### BeginTransactionAsync()

Start transaction.

```csharp
await unitOfWork.BeginTransactionAsync();
try
{
    // Perform operations
    await unitOfWork.CommitAsync();
}
catch
{
    await unitOfWork.RollbackAsync();
}
```

**Throws:** TransactionException if already in transaction

---

### CommitAsync()

Commit transaction and save changes.

```csharp
await unitOfWork.CommitAsync();
```

**Throws:** DatabaseException on commit failure

---

### RollbackAsync()

Rollback transaction, discarding changes.

```csharp
await unitOfWork.RollbackAsync();
```

---

### SaveChangesAsync()

Save all tracked changes without transaction.

```csharp
// Individual operation commits
await unitOfWork.SaveChangesAsync();
```

**Returns:** Number of affected rows

---

### GetChangedEntities()

Get all modified entities.

```csharp
var changed = unitOfWork.GetChangedEntities();
foreach (var entity in changed)
{
    Console.WriteLine($"{entity.GetType().Name}: {entity.Id}");
}
```

**Returns:** Enumerable of changed entities

---

### GetTrackedEntities()

Get all tracked entities.

```csharp
var tracked = unitOfWork.GetTrackedEntities();
```

**Returns:** Enumerable of tracked entities

---

## BaseEntity

Base class for all entities.

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Properties:**
- `Id`: Primary key
- `Version`: Optimistic concurrency control
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last modification timestamp

---

## ICacheProvider

Cache management interface.

### Get<T>(string key)

Retrieve cached value.

```csharp
var cached = cacheProvider.Get<List<Product>>("products:all");
if (cached != null)
{
    return cached;
}
```

**Parameters:**
- `key` (string): Cache key

**Returns:** Cached value or null

---

### Set<T>(string key, T value, TimeSpan? expiration)

Store value in cache.

```csharp
var products = await repository.GetAsync(spec);
cacheProvider.Set("products:all", products, TimeSpan.FromHours(1));
```

**Parameters:**
- `key` (string): Cache key
- `value` (T): Value to cache
- `expiration` (TimeSpan): Optional TTL

---

### Remove(string key)

Remove cached value.

```csharp
cacheProvider.Remove("products:all");
```

---

### Clear()

Clear all cache entries.

```csharp
cacheProvider.Clear();
```

---

## Configuration

### DatabaseContextOptions

Configuration options for database context.

```csharp
public class DatabaseContextOptions
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public bool EnableChangeTracking { get; set; }
    public bool EnableCaching { get; set; }
    public int CacheTTLSeconds { get; set; }
    public int CommandTimeout { get; set; }
    public int MaxPoolSize { get; set; }
}
```

---

## Exceptions

### OrmException

Base exception for ORM errors.

```csharp
try
{
    await repository.GetAsync(spec);
}
catch (OrmException ex)
{
    Console.WriteLine($"ORM Error: {ex.Message}");
}
```

---

### ConcurrencyException

Version mismatch during update.

```csharp
try
{
    await repository.UpdateAsync(product);
}
catch (ConcurrencyException ex)
{
    // Reload and retry
    product = await repository.GetByIdAsync(product.Id);
}
```

---

### DatabaseException

Database connectivity or query error.

```csharp
try
{
    await repository.GetByIdAsync(1);
}
catch (DatabaseException ex)
{
    Console.WriteLine($"Database Error: {ex.Message}");
}
```

---

## Extension Methods

### AddDatabaseContext

Register database context in DI container.

```csharp
services.AddDatabaseContext(options =>
{
    options.ConnectionString = connectionString;
    options.DatabaseType = DatabaseType.SqlServer;
});
```

---

### AddRepositories

Register generic repository implementation.

```csharp
services.AddRepositories();
```

---

### AddServices

Register all ORM services.

```csharp
services.AddServices();
```
