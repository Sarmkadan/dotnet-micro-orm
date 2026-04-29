// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

## General Questions

### Q: What is DotnetMicroOrm?
A: DotnetMicroOrm is a lightweight, high-performance ORM for .NET 10 that emphasizes simplicity and speed. Unlike Entity Framework Core, it focuses on core data access patterns without the overhead of advanced features you may not need.

### Q: How is DotnetMicroOrm different from Entity Framework Core?
A: Key differences:
- **Size**: 50KB vs 2MB
- **Learning Curve**: Simple vs Steep
- **Startup Time**: 10ms vs 500ms
- **Compiled Queries**: Automatic vs Manual
- **Features**: Core only vs Everything
- **Performance**: Optimized for speed vs Flexibility

### Q: Should I use DotnetMicroOrm or Entity Framework Core?
A: Choose DotnetMicroOrm if:
- You want simple, fast data access
- Performance is critical
- You don't need advanced features like migrations or change tracking UI
- You prefer explicit over implicit behavior

Choose Entity Framework Core if:
- You need migrations and model builders
- You want lazy loading and automatic relationships
- You prefer a feature-rich ORM
- Your project is already using it

### Q: Is DotnetMicroOrm production-ready?
A: Yes. It's used in production systems and has comprehensive test coverage. See the GitHub repository for test coverage details.

### Q: What databases does DotnetMicroOrm support?
A: SQL Server, PostgreSQL, MySQL, and SQLite. See configuration documentation for connection string formats.

## Installation & Setup

### Q: How do I install DotnetMicroOrm?
A: Via NuGet:
```bash
dotnet add package DotnetMicroOrm
```

Or manually reference the local package if building from source.

### Q: What are the system requirements?
A: - .NET 10 or later
- Windows, Linux, or macOS
- 4GB RAM (minimum)
- Active internet connection for NuGet

### Q: Can I use DotnetMicroOrm with .NET 8 or 9?
A: While it targets net10.0, some features may work on earlier versions. However, officially .NET 10 is required.

### Q: How do I configure multiple database connections?
A: Create multiple DatabaseContext instances with different configuration:
```csharp
var primaryDb = services.AddDatabaseContext(options =>
{
    options.ConnectionString = "primary-conn";
});

var secondaryDb = services.AddDatabaseContext(options =>
{
    options.ConnectionString = "secondary-conn";
});
```

## Configuration

### Q: What's the default cache TTL?
A: 300 seconds (5 minutes). Configurable via `CacheTTLSeconds` setting.

### Q: Can I disable caching?
A: Yes, set `EnableCaching: false` in configuration or use `.AsNoTracking()` in specification.

### Q: How do I configure command timeout?
A: In configuration:
```json
{
  "Database": {
    "CommandTimeout": 30
  }
}
```

### Q: What's the recommended max pool size?
A: 100 is recommended for most applications. Adjust based on concurrent connections:
- Small app: 20-50
- Medium app: 50-100
- Large app: 100-200

### Q: How do I enable detailed logging?
A: Configure logging:
```json
{
  "Logging": {
    "LogLevel": "Debug",
    "EnableDetailedErrors": true,
    "LogQueryPerformance": true,
    "SlowQueryThresholdMs": 100
  }
}
```

## Usage

### Q: How do I perform a simple CRUD operation?
A: See Getting Started guide. Basic example:
```csharp
var repo = serviceProvider.GetRequiredService<IRepository<Product>>();

// Create
var product = new Product { Name = "Laptop", Price = 999.99m };
await repo.AddAsync(product);

// Read
var fetched = await repo.GetByIdAsync(product.Id);

// Update
fetched.Price = 899.99m;
await repo.UpdateAsync(fetched);

// Delete
await repo.DeleteAsync(product.Id);

await unitOfWork.SaveChangesAsync();
```

### Q: What's the difference between AddAsync and AddRangeAsync?
A: AddRangeAsync is for bulk operations (10-20x faster):
```csharp
// Slow - individual inserts
foreach (var item in items)
    await repo.AddAsync(item);

// Fast - batch insert
await repo.AddRangeAsync(items);
```

### Q: How do I write complex queries?
A: Use Specification pattern:
```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .AndWhere(p => p.StockQuantity > 0)
    .OrderBy(p => p.Name)
    .Skip(20)
    .Take(10);

var results = await repo.GetAsync(spec);
```

### Q: Can I use raw SQL?
A: Yes, via DatabaseContext:
```csharp
var context = serviceProvider.GetRequiredService<IDatabaseContext>();
var result = await context.ExecuteQueryAsync<Product>(
    "SELECT * FROM Products WHERE Price > @price",
    new { price = 100 }
);
```

### Q: How do I implement pagination?
A: Use Skip and Take:
```csharp
var pageSize = 20;
var pageNumber = 2;
var spec = new Specification<Product>()
    .OrderBy(p => p.Name)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize);
```

### Q: How do I check if an entity exists?
A: Use AnyAsync:
```csharp
var spec = new Specification<Product>().Where(p => p.Id == 5);
bool exists = await repo.AnyAsync(spec);
```

### Q: How do I count matching entities?
A: Use CountAsync:
```csharp
var spec = new Specification<Product>().Where(p => p.Price > 100);
int count = await repo.CountAsync(spec);
```

## Transactions

### Q: How do I use transactions?
A: Via IUnitOfWork:
```csharp
try
{
    await unitOfWork.BeginTransactionAsync();
    // Perform operations
    await unitOfWork.CommitAsync();
}
catch
{
    await unitOfWork.RollbackAsync();
    throw;
}
```

### Q: What isolation level should I use?
A: Default is ReadCommitted (good balance). Options:
- ReadUncommitted: Dirty reads possible
- ReadCommitted: Default, good balance
- RepeatableRead: Prevents phantom reads
- Serializable: Most restrictive, slowest

### Q: Can I nest transactions?
A: No. Each UnitOfWork instance supports one transaction.

### Q: What happens if I don't call SaveChangesAsync?
A: Changes are not persisted to database.

## Performance

### Q: How can I improve query performance?
A: 1. Enable caching for read-heavy operations
2. Create database indexes on filtered columns
3. Use specifications to filter at database level
4. Use batch operations for bulk data
5. Monitor slow query logs

### Q: What's the performance overhead of change tracking?
A: ~50 bytes per entity. Negligible for most applications.

### Q: Should I use AsNoTracking?
A: Yes, for read-only queries. Saves change tracking overhead:
```csharp
var spec = new Specification<Product>().AsNoTracking();
```

### Q: How does expression compilation improve performance?
A: LINQ expressions are compiled to IL on first execution and cached. Subsequent identical queries reuse compiled code (5-10x faster).

### Q: Can I manually clear the expression cache?
A: Currently not exposed. Cache is automatically managed.

## Caching

### Q: How does caching work?
A: Three levels:
1. Query result caching (entire result set)
2. Entity caching (individual entities)
3. Expression caching (compiled queries)

### Q: How do I invalidate cache?
A: Manually or automatically on mutations:
```csharp
// Manual invalidation
cacheProvider.Remove("products:all");

// Automatic - cache cleared after Update/Delete
await repo.UpdateAsync(product);
```

### Q: Can I use distributed caching?
A: Currently only in-memory caching. Redis support planned for future release.

### Q: How do I set custom cache TTL?
A: Per-cache basis:
```csharp
cacheProvider.Set(key, value, TimeSpan.FromHours(1));
```

## Change Tracking

### Q: What is change tracking?
A: Automatic detection of entity modifications. Tracked entities automatically update when calling SaveChangesAsync.

### Q: How do I get tracked entities?
A: Via UnitOfWork:
```csharp
var tracked = unitOfWork.GetTrackedEntities();
var changed = unitOfWork.GetChangedEntities();
```

### Q: What are entity states?
A: - Unchanged: No changes
- Added: New entity
- Modified: Changed
- Deleted: Marked for deletion
- Detached: Not tracked

### Q: Can I disable change tracking?
A: Yes, set `EnableChangeTracking: false` in configuration.

## Concurrency

### Q: How does optimistic concurrency work?
A: Version-based:
```csharp
public class BaseEntity
{
    public int Version { get; set; }
}
```

Update fails if version doesn't match, preventing lost updates.

### Q: What happens on concurrency conflict?
A: ConcurrencyException thrown. Handle by reloading entity:
```csharp
try
{
    await repo.UpdateAsync(product);
}
catch (ConcurrencyException)
{
    // Reload and retry
    product = await repo.GetByIdAsync(product.Id);
}
```

## Testing

### Q: How do I test with DotnetMicroOrm?
A: Use SQLite for in-memory testing:
```csharp
services.AddDatabaseContext(options =>
{
    options.ConnectionString = "Data Source=:memory:";
    options.DatabaseType = DatabaseType.SQLite;
});
```

### Q: How do I mock the repository?
A: Create mock implementation:
```csharp
public class MockRepository<T> : IRepository<T> where T : BaseEntity
{
    // Implement methods for testing
}
```

### Q: Should I use real database for integration tests?
A: Yes, recommended. Use SQLite in-memory for speed.

## Troubleshooting

### Q: I get "Object reference not set" error
A: Entity not loaded before modification. Use GetByIdAsync first:
```csharp
var product = await repo.GetByIdAsync(1); // Load first
product.Price = 99.99m;
await repo.UpdateAsync(product);
```

### Q: Connection timeout errors
A: Check:
1. Database server is running
2. Connection string is correct
3. Network connectivity
4. Increase CommandTimeout setting

### Q: Changes not persisting
A: Ensure SaveChangesAsync is called:
```csharp
await repo.UpdateAsync(product);
await unitOfWork.SaveChangesAsync(); // Don't forget this
```

### Q: Cached data is stale
A: Invalidate cache after modifications:
```csharp
await repo.UpdateAsync(product);
cacheProvider.Remove("cache-key");
```

### Q: Slow performance
A: 1. Enable slow query logging
2. Check database indexes
3. Review cache configuration
4. Use batch operations
5. Profile application

## Contributing

### Q: How can I contribute?
A: See Contributing section in README. General process:
1. Fork repository
2. Create feature branch
3. Make changes
4. Write tests
5. Submit pull request

### Q: How do I report a bug?
A: Open GitHub issue with:
1. Detailed description
2. Steps to reproduce
3. Expected behavior
4. Actual behavior
5. Environment (OS, .NET version, database)

### Q: Can I propose new features?
A: Yes, open an issue to discuss first. Ensure alignment with project goals.

## Support

### Q: Where can I get help?
A: - GitHub Issues
- Documentation
- Examples directory
- Community discussions

### Q: Is there a forum or chat?
A: GitHub Discussions available for community support.

### Q: How often is DotnetMicroOrm updated?
A: Regular updates based on bug reports and feature requests. See CHANGELOG for version history.
