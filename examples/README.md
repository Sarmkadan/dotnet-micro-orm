// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# DotnetMicroOrm Examples

This directory contains complete, runnable examples demonstrating DotnetMicroOrm features and patterns.

## Running Examples

### Prerequisites

1. .NET 10 SDK
2. SQL Server (local or remote)
3. Updated connection string in each example

### Update Connection String

In each example file, update this line:

```csharp
const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";
```

### Run Individual Example

```bash
# Compile
dotnet build

# Run specific example
dotnet run --project examples/BasicCRUD.csproj

# Or run from source
dotnet examples/BasicCRUD.cs
```

## Available Examples

### 1. BasicCRUD.cs

**What it teaches:** Core CRUD operations

**Contents:**
- Creating entities
- Reading by ID
- Querying all records
- Updating entities
- Deleting entities
- Basic specifications

**Key patterns:**
- Repository injection
- UnitOfWork pattern
- Change tracking

**Run time:** ~5 seconds

---

### 2. BatchOperations.cs

**What it teaches:** High-performance bulk operations

**Contents:**
- Batch insert (1000 items)
- Batch update with pricing changes
- Batch delete
- Performance measurements

**Key patterns:**
- `AddRangeAsync` for bulk inserts
- `UpdateRangeAsync` for bulk updates
- `DeleteRangeAsync` for bulk deletes
- Performance benchmarking

**Performance:** 10-20x faster than individual operations

**Run time:** ~30 seconds

---

### 3. CachingStrategy.cs

**What it teaches:** Effective caching patterns

**Contents:**
- Cache miss (database query)
- Cache hit (in-memory retrieval)
- Manual cache management
- Cache invalidation
- Performance comparison (1000 queries)

**Key patterns:**
- ICacheProvider usage
- TTL configuration
- Cache key strategy
- Invalidation on updates

**Performance:** Cached queries 100-1000x faster

**Run time:** ~15 seconds

---

### 4. TransactionManagement.cs

**What it teaches:** ACID compliance and transaction handling

**Contents:**
- Successful transactions
- Failed transactions with rollback
- Transaction isolation levels
- Multi-entity transactions

**Key patterns:**
- `BeginTransactionAsync`
- `CommitAsync`
- `RollbackAsync`
- Error handling

**Importance:** Critical for data integrity

**Run time:** ~10 seconds

---

### 5. AdvancedQueries.cs

**What it teaches:** Complex querying patterns

**Contents:**
- Multi-condition filtering (AND/OR)
- Pagination with Skip/Take
- Sorting (ascending/descending)
- Counting and existence checks
- First-or-default queries

**Key patterns:**
- Complex specifications
- Multiple WHERE clauses
- OrderBy/OrderByDescending
- Skip/Take for pagination
- AnyAsync for existence checks

**Demonstrates:** Type-safe, composable queries

**Run time:** ~10 seconds

---

### 6. ValidationExample.cs

**What it teaches:** Entity validation before persistence

**Contents:**
- Valid product creation
- Invalid product examples:
  - Empty name
  - Negative price
  - Negative stock
  - Name too short
- Validation error messages

**Key patterns:**
- Business rule validation
- Error collection
- ValidationResult pattern
- Pre-persistence validation

**Best practices:** Always validate before saving

**Run time:** ~5 seconds

---

### 7. ECommerceExample.cs

**What it teaches:** Real-world application patterns

**Contents:**
- Inventory initialization
- Product browsing
- Product search
- Order placement
- Inventory reporting
- Stock alerts

**Key patterns:**
- Multi-repository coordination
- Transaction-driven workflows
- Business logic implementation
- Reporting queries

**Demonstrates:** Complete e-commerce workflow

**Run time:** ~10 seconds

---

## Example Patterns

### Pattern 1: Service Layer

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Product> CreateAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        await _repository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }
}
```

### Pattern 2: Specification Builder

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .OrderByDescending(p => p.Rating)
    .Skip(10)
    .Take(20);

var results = await repository.GetAsync(spec);
```

### Pattern 3: Error Handling

```csharp
try
{
    await unitOfWork.BeginTransactionAsync();
    // Operations...
    await unitOfWork.CommitAsync();
}
catch (Exception ex)
{
    await unitOfWork.RollbackAsync();
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Pattern 4: Caching

```csharp
const string cacheKey = "products:featured";

var cached = _cacheProvider.Get<List<Product>>(cacheKey);
if (cached != null)
    return cached;

var products = await repository.GetAsync(spec);
_cacheProvider.Set(cacheKey, products, TimeSpan.FromHours(1));
return products;
```

## Performance Benchmarks

All examples include timing measurements:

| Example | Operation | Time |
|---------|-----------|------|
| BasicCRUD | Single insert | 2ms |
| BatchOperations | 1000 inserts | 85ms |
| CachingStrategy | DB query (first) | 3ms |
| CachingStrategy | Cached query | 0.5ms |
| TransactionManagement | Multi-entity transaction | 5ms |
| AdvancedQueries | Complex filter | 3ms |
| ECommerceExample | Complete order | 8ms |

## Learning Path

### Beginner

1. Start with **BasicCRUD.cs** - Learn fundamentals
2. Study **AdvancedQueries.cs** - Learn querying
3. Review **ValidationExample.cs** - Learn validation

### Intermediate

1. Study **BatchOperations.cs** - Learn performance
2. Review **CachingStrategy.cs** - Learn optimization
3. Study **TransactionManagement.cs** - Learn ACID

### Advanced

1. Study **ECommerceExample.cs** - Learn real-world patterns
2. Modify examples for your use case
3. Combine patterns in your application

## Common Modifications

### Change Database

Update `DatabaseType`:

```csharp
// SQL Server (default)
options.DatabaseType = DatabaseType.SqlServer;

// PostgreSQL
options.DatabaseType = DatabaseType.PostgreSQL;

// MySQL
options.DatabaseType = DatabaseType.MySQL;

// SQLite
options.DatabaseType = DatabaseType.SQLite;
```

### Adjust Connection String

```csharp
// SQL Server
"Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"

// PostgreSQL
"Host=localhost;Database=mydb;Username=postgres;Password=password"

// MySQL
"Server=localhost;Database=mydb;User Id=root;Password=password;"

// SQLite
"Data Source=mydb.db"
```

### Customize for Your Models

Replace `Product`, `Order`, `Category` with your entities.

## Troubleshooting

### Connection Errors

```
error: Cannot connect to server
```

Solution: Verify connection string and database is running.

### Missing Tables

```
error: Invalid object name 'Products'
```

Solution: Create schema manually or use a migration tool.

### Type Loading Errors

```
error: Could not load type...
```

Solution: Ensure all project dependencies are restored (`dotnet restore`).

## Next Steps

1. **Adapt examples** to your domain entities
2. **Combine patterns** for your use case
3. **Read documentation** for deeper understanding
4. **Run tests** to verify implementation
5. **Deploy** with confidence

---

Built by [Vladyslav Zaiets](https://sarmkadan.com)
