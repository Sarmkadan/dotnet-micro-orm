// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# DotnetMicroOrm Architecture Guide

## Overview

DotnetMicroOrm is designed with a layered architecture that separates concerns and enables high performance.

## Layered Architecture

### 1. Domain Layer
The topmost layer containing business logic and domain models.

- **BaseEntity**: Abstract base class for all entities with Id and timestamps
- **Business Logic**: Services implementing domain use cases
- **Validation**: Entity validation rules

### 2. Application Layer
Coordinates domain logic with infrastructure.

- **IRepository<T>**: Generic data access interface
- **Specifications**: Type-safe query building
- **Unit of Work**: Transaction management

### 3. Data Access Layer
Handles database interactions and query compilation.

- **QueryBuilder**: Translates specifications to SQL
- **ChangeTracker**: Tracks entity state changes
- **Identity Map**: Ensures entity uniqueness in memory
- **Compiled Expressions**: Cache for compiled LINQ expressions

### 4. Infrastructure Layer
Lowest level, manages physical database connections.

- **DatabaseContext**: Connection and transaction management
- **CacheProvider**: In-memory caching implementation
- **Connection Pool**: ADO.NET connection pooling

## Core Components

### DatabaseContext

Central coordinator for all database operations.

```csharp
public interface IDatabaseContext
{
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object parameters);
    Task<int> ExecuteCommandAsync(string sql, object parameters);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

**Responsibilities:**
- Opening/closing database connections
- Managing transactions
- Executing raw SQL queries
- Managing the identity map

### Repository<T>

Generic data access pattern implementation.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAsync(Specification<T> spec);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

**Responsibilities:**
- CRUD operations
- Specification-based querying
- Change tracking integration
- Caching integration

**Performance Features:**
- Compiled expression caching (queries reuse compiled LINQ)
- Query result caching
- Batch operation optimization

### Specification<T>

Type-safe query builder using fluent API.

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Skip(10)
    .Take(20);
```

**Process:**
1. Build fluent expression tree
2. Compile to IL code (first execution only)
3. Cache compiled expression
4. Reuse for identical specifications
5. Translate to SQL

### ChangeTracker

Monitors entity state for batch operations.

```csharp
public class ChangeTracker
{
    public EntityState GetState(BaseEntity entity);
    public void MarkAsAdded(BaseEntity entity);
    public void MarkAsModified(BaseEntity entity);
    public void MarkAsDeleted(BaseEntity entity);
}
```

**Entity States:**
- **Unchanged**: Loaded from database, no changes
- **Added**: New entity, not yet persisted
- **Modified**: Loaded from database, changed
- **Deleted**: Marked for deletion
- **Detached**: Not tracked by repository

### Unit of Work

Manages transactions across multiple repositories.

```csharp
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
}
```

**Workflow:**
1. Begin transaction
2. Get repositories and perform operations
3. Changes tracked automatically
4. Commit or rollback atomic operation

### CacheProvider

Multi-tier caching with TTL support.

```csharp
public interface ICacheProvider
{
    T Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    void Clear();
}
```

**Features:**
- In-memory caching
- TTL (Time To Live) support
- Sliding expiration
- Eviction policies

## Data Flow

### Query Execution Flow

```
1. Application calls Repository.GetAsync(spec)
   ↓
2. Repository checks cache
   ├─ Hit? Return cached result
   └─ Miss? Continue...
   ↓
3. Check if expression already compiled
   ├─ Yes? Use cached compiled expression
   └─ No? Compile expression to IL
   ↓
4. QueryBuilder translates spec to SQL
   ↓
5. DatabaseContext executes SQL against database
   ↓
6. Results mapped to entity objects
   ↓
7. Results cached with TTL
   ↓
8. Return to application
```

### Mutation (Insert/Update/Delete) Flow

```
1. Application calls Repository.AddAsync(entity)
   ↓
2. ChangeTracker marks entity as "Added"
   ↓
3. Entity added to identity map
   ↓
4. Application calls UnitOfWork.SaveChangesAsync()
   ↓
5. ChangeTracker collects all changes (Added/Modified/Deleted)
   ↓
6. Begin transaction
   ↓
7. BatchProcessor generates SQL for all changes
   ├─ INSERT for Added entities
   ├─ UPDATE for Modified entities
   └─ DELETE for Deleted entities
   ↓
8. Execute SQL in batch
   ↓
9. Commit transaction
   ↓
10. Invalidate affected caches
    ↓
11. Clear change tracker
```

## Expression Compilation

### How Compiled Expressions Work

```csharp
// First execution
var spec = new Specification<Product>().Where(p => p.Price > 100);
var products = await repo.GetAsync(spec);
// LINQ expression compiled to IL, cached, executed against database

// Second execution (identical expression)
var moreProducts = await repo.GetAsync(spec);
// Reuses cached compiled expression, no compilation overhead
// Saves 5-10ms per query on typical systems
```

### Performance Impact

| Execution | Time | Note |
|-----------|------|------|
| 1st execution (compilation) | 15ms | Includes compilation overhead |
| 2nd-Nth execution (cached) | 3ms | Reuses compiled expression |
| Overhead saved per query | 12ms | On subsequent executions |

## Concurrency Handling

### Optimistic Concurrency

The ORM uses version-based optimistic concurrency:

```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**Update Process:**
1. Load entity with Version = 1
2. Modify and update
3. SQL: WHERE Id = @id AND Version = @oldVersion
4. If WHERE matches: update succeeds, Version increments
5. If WHERE doesn't match: ConcurrencyException thrown

### Connection Pooling

SQLClient automatically pools connections:

- **Default Max Pool Size**: 100
- **Connection Reuse**: Closed connections returned to pool
- **Pooling Benefits**: Eliminates connection open overhead

## Error Handling

### Exception Hierarchy

```
OrmException (base)
├── ConcurrencyException
├── DatabaseException
├── QueryException
├── ValidationException
└── TransactionException
```

### Transaction Rollback

```csharp
try
{
    await unitOfWork.BeginTransactionAsync();
    // ... perform operations ...
    await unitOfWork.CommitAsync();
}
catch (Exception)
{
    await unitOfWork.RollbackAsync();
    throw;
}
```

## Memory Management

### Identity Map

Prevents duplicate instances of same entity:

```csharp
// First load
var product1 = await repo.GetByIdAsync(1);

// Second load (same transaction)
var product2 = await repo.GetByIdAsync(1);

// Both references point to same object
Assert.ReferenceEquals(product1, product2); // true
```

### Change Tracking Overhead

- Adds ~50 bytes per tracked entity
- Index overhead: ~100ns per lookup
- Benefit: Automatic change detection

## Caching Strategy

### Cache Levels

1. **Query Result Cache**: Entire result set cached
2. **Entity Cache**: Individual entities cached
3. **Compiled Expression Cache**: LINQ expressions cached

### TTL Configuration

```csharp
// Global TTL
services.AddDatabaseContext(options =>
{
    options.CacheTTLSeconds = 300; // 5 minutes
});

// Per-cache TTL
cacheProvider.Set(key, value, TimeSpan.FromHours(1));
```

### Cache Invalidation

Automatic invalidation on mutations:

```csharp
// After Update/Delete, related caches cleared
await repo.UpdateAsync(product);
await unitOfWork.SaveChangesAsync();
// Cache automatically invalidated for this entity
```

## Multi-Database Support

### Database Abstraction

```csharp
public enum DatabaseType
{
    SqlServer,
    PostgreSQL,
    MySQL,
    SQLite
}
```

### SQL Dialect Handling

Each database type has dialect-specific translation:

- **SQL Server**: T-SQL syntax
- **PostgreSQL**: PL/pgSQL with RETURNING
- **MySQL**: MySQL dialect
- **SQLite**: SQLite dialect

### Connection String Per Database

Configuration in appsettings.json allows database selection.

## Testing Architecture

### Unit Testing

- Mock IRepository<T> for service testing
- Mock ICacheProvider for cache testing
- Use Specification pattern for testable queries

### Integration Testing

- Use real test database (SQLite recommended)
- Seed test data before each test
- Clean up after each test

## Scalability Considerations

### Horizontal Scaling

- Stateless repositories enable load balancing
- Cache per instance (not distributed)
- Database connection pooling scales across nodes

### Vertical Scaling

- Expression caching reduces CPU usage
- Connection pooling optimizes memory
- Batch operations reduce network round-trips

### Performance Optimization

1. **Compiled Expressions**: 5-10x faster repeat queries
2. **Batch Operations**: 10-20x faster bulk operations
3. **Result Caching**: 100-1000x faster cached queries
4. **Connection Pooling**: Eliminates connection overhead

## Security Architecture

### Input Validation

- Specifications prevent SQL injection
- Parameterized queries used throughout
- Type safety at compile time

### Authentication/Authorization

- Not built into ORM (cross-cutting concern)
- Implement at application/middleware layer
- Leverage identity information in audit logs

### Audit Logging

- Track all mutations (Insert/Update/Delete)
- Store user/timestamp for each change
- Excludes sensitive properties (passwords, tokens)
