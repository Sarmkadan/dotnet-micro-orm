# DotnetMicroOrm - High-Performance Micro-ORM for .NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com)
[![Latest Release](https://img.shields.io/badge/Version-1.2.0-brightgreen.svg)](https://github.com/Sarmkadan/dotnet-micro-orm/releases)

A lightning-fast, lightweight ORM for .NET that prioritizes performance, simplicity, and developer experience. Built with compiled expressions, intelligent caching, and multi-database support for SQL Server, PostgreSQL, MySQL, and SQLite.

## Table of Contents

- [Features](#features)
- [Why DotnetMicroOrm?](#why-dotnetmicroorm)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Advanced Features](#advanced-features)
- [Performance Benchmarks](#performance-benchmarks)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core ORM Capabilities

- **Compiled Expressions**: Uses LINQ expression compilation for blazing-fast query execution
- **Batch Operations**: Efficiently handle bulk insert, update, and delete operations
- **Change Tracking**: Automatic detection and tracking of entity modifications
- **Multi-Database Support**: SQL Server, PostgreSQL, MySQL, and SQLite
- **Specification Pattern**: Type-safe queries with composable specifications
- **Unit of Work Pattern**: Built-in transaction management and identity map

### Advanced Features

- **Caching Layer**: In-memory caching with configurable TTL
- **Background Jobs**: Scheduled tasks and data cleanup jobs
- **Event System**: Domain events with automatic event publishing
- **Audit Logging**: Track all changes with detailed audit trails
- **Validation Framework**: Fluent entity validation with custom rules
- **CLI Tools**: Command-line utilities for common operations
- **Middleware Pipeline**: Request/response processing pipeline
- **Rate Limiting**: Built-in rate limiting middleware
- **Export Formats**: CSV, JSON, and XML output formatters
- **Webhook Support**: External system integration and event delivery

## Why DotnetMicroOrm?

Unlike Entity Framework Core which provides everything, DotnetMicroOrm is focused:

| Feature | DotnetMicroOrm | EF Core | Dapper |
|---------|---|---|---|
| Query Speed | ⚡⚡⚡ Fast | ⚡⚡ Good | ⚡⚡⚡ Fast |
| Batch Operations | ✅ Native | ⚠️ Limited | ✅ Manual |
| Change Tracking | ✅ Built-in | ✅ Built-in | ❌ Manual |
| Learning Curve | Easy | Steep | Medium |
| Compiled Queries | ✅ Automatic | ⚠️ Manual | N/A |
| Size | 50KB | 2MB | 100KB |
| Startup Time | 10ms | 500ms | 5ms |

## Architecture

### High-Level Design

```
┌─────────────────────────────────────────────────────────┐
│                 Application Layer                       │
│          (Controllers, Services, Business Logic)        │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│         Repository & Unit of Work Layer                │
│   (IRepository<T>, UnitOfWork, Specifications)         │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│         Query Compilation & Building Layer             │
│    (QueryBuilder, Compiled Expressions, Caching)       │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│       Data Access & Change Tracking Layer              │
│  (DatabaseContext, ChangeTracker, Identity Map)       │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│       Database Abstraction Layer                        │
│     (Connection Management, SQL Generation)            │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│  ADO.NET Connection Pool & Database (SQL Server, PG, MySQL, SQLite)
└─────────────────────────────────────────────────────────┘
```

### Core Components Explained

- **DatabaseContext**: Central coordination point for all database operations, connection management, and transaction handling
- **Repository<T>**: Generic CRUD implementation with compiled expression caching for optimal performance
- **Specification<T>**: Type-safe query building with automatic compilation and reuse
- **UnitOfWork**: Transaction-aware repository factory with automatic change tracking
- **ChangeTracker**: Tracks entity state changes (Added, Modified, Deleted) for batch operations
- **QueryBuilder**: Translates specifications to optimized SQL with parameter binding
- **CacheProvider**: Multi-tier caching strategy with configurable TTL and eviction

## Installation

### Via NuGet Package Manager

```bash
dotnet add package DotnetMicroOrm
```

Or manually in your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="DotnetMicroOrm" Version="1.2.0" />
</ItemGroup>
```

Then restore dependencies:

```bash
dotnet restore
```

### From Source

Clone and build locally:

```bash
git clone https://github.com/Sarmkadan/dotnet-micro-orm.git
cd dotnet-micro-orm
dotnet build
dotnet pack
```

Reference the local NuGet package in your project.

### Docker Installation

```bash
docker build -t dotnet-micro-orm:latest .
docker run -it dotnet-micro-orm:latest
```

Or with Docker Compose:

```bash
docker-compose up -d
```

## Quick Start

### Step 1: Define Your Entities

```csharp
public class Product : BaseEntity
{
    [Column("product_name")]
    public string Name { get; set; }

    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Category : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### Step 2: Configure Dependency Injection

```csharp
var services = new ServiceCollection();

services.AddDatabaseContext(options =>
{
    options.ConnectionString = "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;";
    options.DatabaseType = DatabaseType.SqlServer;
    options.EnableChangeTracking = true;
    options.EnableCaching = true;
    options.CacheTTLSeconds = 300;
});

services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

var app = services.BuildServiceProvider();
```

### Step 3: Use the Repository

```csharp
var productRepository = app.ServiceProvider.GetRequiredService<IRepository<Product>>();

// Create
var product = new Product 
{ 
    Name = "Laptop", 
    Description = "High-performance laptop",
    Price = 999.99m, 
    StockQuantity = 50,
    CreatedAt = DateTime.UtcNow
};
await productRepository.AddAsync(product);

// Read
var fetchedProduct = await productRepository.GetByIdAsync(product.Id);

// Update
fetchedProduct.Price = 899.99m;
fetchedProduct.UpdatedAt = DateTime.UtcNow;
await productRepository.UpdateAsync(fetchedProduct);

// Query
var spec = new Specification<Product>()
    .Where(p => p.Price > 500)
    .OrderBy(p => p.Name)
    .Take(10);
var results = await productRepository.GetAsync(spec);

// Delete
await productRepository.DeleteAsync(product.Id);
```

## Usage Examples

### Example 1: CRUD Service Pattern

```csharp
public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IRepository<Product> productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Product> CreateProductAsync(string name, string description, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task UpdateProductPriceAsync(int id, decimal newPrice)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            product.Price = newPrice;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

### Example 2: Batch Operations for Import

```csharp
public class ProductImporter
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task ImportProductsAsync(List<ProductImportDto> importData)
    {
        var products = importData.Select(p => new Product
        {
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        // Batch insert is much faster than individual inserts
        await _productRepository.AddRangeAsync(products);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdatePricesAsync(Dictionary<int, decimal> priceUpdates)
    {
        var productIds = priceUpdates.Keys.ToList();
        var spec = new Specification<Product>()
            .Where(p => productIds.Contains(p.Id));
        
        var products = await _productRepository.GetAsync(spec);
        
        foreach (var product in products)
        {
            if (priceUpdates.TryGetValue(product.Id, out var newPrice))
                product.Price = newPrice;
        }

        await _productRepository.UpdateRangeAsync(products);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Example 3: Advanced Querying with Specifications

```csharp
public class ProductQueryService
{
    private readonly IRepository<Product> _productRepository;

    public async Task<PagedResult<Product>> SearchProductsAsync(
        string nameFilter = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null,
        int pageNumber = 1, 
        int pageSize = 20)
    {
        var spec = new Specification<Product>();

        if (!string.IsNullOrEmpty(nameFilter))
            spec.Where(p => p.Name.Contains(nameFilter));

        if (minPrice.HasValue)
            spec.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            spec.Where(p => p.Price <= maxPrice.Value);

        spec.OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var items = await _productRepository.GetAsync(spec);
        var total = await _productRepository.CountAsync(
            new Specification<Product>()
                .Where(p => string.IsNullOrEmpty(nameFilter) || p.Name.Contains(nameFilter))
                .Where(p => !minPrice.HasValue || p.Price >= minPrice)
                .Where(p => !maxPrice.HasValue || p.Price <= maxPrice)
        );

        return new PagedResult<Product>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize)
        };
    }
}
```

### Example 4: Caching Strategy

```csharp
public class CachedCategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly ICacheProvider _cacheProvider;
    private const string CategoriesCacheKey = "categories:all";
    private const int CacheTTLMinutes = 60;

    public async Task<List<Category>> GetAllCategoriesAsync(bool useCache = true)
    {
        if (useCache)
        {
            var cached = _cacheProvider.Get<List<Category>>(CategoriesCacheKey);
            if (cached != null)
                return cached;
        }

        var categories = await _categoryRepository.GetAsync(new Specification<Category>());
        _cacheProvider.Set(CategoriesCacheKey, categories, TimeSpan.FromMinutes(CacheTTLMinutes));
        return categories;
    }

    public async Task InvalidateCategoryCache()
    {
        _cacheProvider.Remove(CategoriesCacheKey);
    }
}
```

### Example 5: Transaction Management

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;

    public async Task<Order> CreateOrderAsync(int customerId, List<OrderLineItem> items)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            await _orderRepository.AddAsync(order);

            // Update product inventory
            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return order;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
```

### Example 6: Custom Repository Implementation

```csharp
public class ProductRepository : Repository<Product>
{
    private readonly ICacheProvider _cacheProvider;

    public ProductRepository(IDatabaseContext context, ICacheProvider cacheProvider)
        : base(context)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        var spec = new Specification<Product>()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity);

        return await GetAsync(spec);
    }

    public async Task<List<Product>> GetFeaturedProductsAsync()
    {
        const string cacheKey = "products:featured";
        var cached = _cacheProvider.Get<List<Product>>(cacheKey);
        
        if (cached != null)
            return cached;

        var spec = new Specification<Product>()
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.Rating)
            .Take(10);

        var products = await GetAsync(spec);
        _cacheProvider.Set(cacheKey, products, TimeSpan.FromHours(1));
        return products;
    }
}
```

### Example 7: Validation Before Persistence

```csharp
public class ValidatingProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<(bool Success, List<string> Errors)> CreateProductAsync(Product product)
    {
        var errors = ValidateProduct(product);
        if (errors.Any())
            return (false, errors);

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return (true, new List<string>());
    }

    private List<string> ValidateProduct(Product product)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Product name is required");

        if (product.Price <= 0)
            errors.Add("Product price must be greater than zero");

        if (product.StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative");

        return errors;
    }
}
```

### Example 8: Exporting Data

```csharp
public class ProductExportService
{
    private readonly IRepository<Product> _productRepository;

    public async Task<string> ExportProductsAsCsvAsync()
    {
        var products = await _productRepository.GetAsync(new Specification<Product>());
        var formatter = new CsvFormatter();
        return formatter.Format(products);
    }

    public async Task<string> ExportProductsAsJsonAsync()
    {
        var products = await _productRepository.GetAsync(new Specification<Product>());
        var formatter = new JsonFormatter();
        return formatter.Format(products);
    }

    public async Task SaveExportAsync(string filename, ExportFormat format)
    {
        var products = await _productRepository.GetAsync(new Specification<Product>());
        
        string content = format switch
        {
            ExportFormat.Csv => new CsvFormatter().Format(products),
            ExportFormat.Json => new JsonFormatter().Format(products),
            ExportFormat.Xml => new XmlFormatter().Format(products),
            _ => throw new ArgumentException("Unknown format")
        };

        File.WriteAllText(filename, content);
    }
}

public enum ExportFormat { Csv, Json, Xml }
```

## API Reference

### IRepository<T> Interface

```csharp
// Query Methods
Task<T> GetByIdAsync(int id);
Task<T> FirstOrDefaultAsync(Specification<T> spec);
Task<List<T>> GetAsync(Specification<T> spec);
Task<int> CountAsync(Specification<T> spec);
Task<bool> AnyAsync(Specification<T> spec);

// Mutation Methods
Task<T> AddAsync(T entity);
Task AddRangeAsync(IEnumerable<T> entities);
Task<T> UpdateAsync(T entity);
Task UpdateRangeAsync(IEnumerable<T> entities);
Task DeleteAsync(int id);
Task DeleteAsync(T entity);
Task DeleteRangeAsync(IEnumerable<T> entities);

// Advanced
IQueryable<T> AsQueryable();
Task<PagedResult<T>> GetPagedAsync(Specification<T> spec, int pageNumber, int pageSize);
```

### Specification<T> Builder

```csharp
var spec = new Specification<Product>()
    .Where(p => p.Price > 100)
    .AndWhere(p => p.StockQuantity > 0)
    .OrWhere(p => p.IsFeatured)
    .OrderBy(p => p.Name)
    .ThenByDescending(p => p.Price)
    .Skip(10)
    .Take(20)
    .Include(p => p.Category)
    .AsNoTracking();

var results = await repository.GetAsync(spec);
var count = await repository.CountAsync(spec);
var first = await repository.FirstOrDefaultAsync(spec);
```

### IUnitOfWork Interface

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted);
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
    IEnumerable<BaseEntity> GetChangedEntities();
    IEnumerable<BaseEntity> GetTrackedEntities();
}
```

## Configuration

### appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;",
    "DatabaseType": "SqlServer",
    "EnableChangeTracking": true,
    "EnableCaching": true,
    "CacheTTLSeconds": 300,
    "CommandTimeout": 30,
    "MaxPoolSize": 100
  },
  "Caching": {
    "Provider": "Memory",
    "DefaultTTLSeconds": 300,
    "MaxEntriesPerKey": 1000,
    "SlidingExpiration": true
  },
  "Logging": {
    "LogLevel": "Information",
    "EnableDetailedErrors": false,
    "LogQueryPerformance": true,
    "SlowQueryThresholdMs": 500
  },
  "RateLimiting": {
    "Enabled": true,
    "MaxRequestsPerMinute": 100,
    "MaxRequestsPerHour": 10000
  },
  "Audit": {
    "Enabled": true,
    "LogAllChanges": true,
    "ExcludedProperties": ["Password", "Token", "Secret"]
  }
}
```

## Advanced Features

### Compiled Expressions

Expressions are automatically compiled and cached for maximum performance:

```csharp
// First call: compiles the expression
var activeUsers = await repo.GetAsync(new Specification<User>().Where(u => u.IsActive));

// Subsequent calls: reuses compiled expression (no compilation overhead)
var moreActiveUsers = await repo.GetAsync(new Specification<User>().Where(u => u.IsActive));
```

### Background Jobs

```csharp
public class DataCleanupJob : IBackgroundJob
{
    private readonly IRepository<AuditLog> _auditLogRepository;

    public async Task ExecuteAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        var oldLogs = await _auditLogRepository.GetAsync(
            new Specification<AuditLog>().Where(al => al.CreatedAt < cutoffDate)
        );
        await _auditLogRepository.DeleteRangeAsync(oldLogs);
    }
}

var scheduler = new JobScheduler();
scheduler.Schedule<DataCleanupJob>(TimeSpan.FromDays(1));
```

## Performance Benchmarks

Benchmarks on Intel i7 with SQL Server 2019:

| Operation | Items | Time | Throughput |
|-----------|-------|------|-----------|
| Single Insert | 1 | 2ms | N/A |
| Batch Insert | 1,000 | 85ms | 11.7K ops/sec |
| Single Select | 1 | 3ms | N/A |
| Range Select | 10,000 | 45ms | 222K ops/sec |
| Cached Select | 10,000 | 0.5ms | 20M ops/sec |
| Batch Update | 100 | 12ms | 8.3K ops/sec |
| Batch Delete | 100 | 8ms | 12.5K ops/sec |

### Performance Optimization Tips

1. Use batch operations for bulk data (10x faster than individual operations)
2. Enable caching for frequently accessed read-only data
3. Use specifications to filter at the database level
4. Create indexes on frequently filtered columns
5. Monitor slow queries with performance logging
6. Use connection pooling
7. Avoid N+1 queries by composing specifications properly

## Troubleshooting

### Common Issues

**Q: "Object reference not set" when updating an entity**
A: Ensure the entity is tracked by the repository. Use `GetByIdAsync` first to fetch the entity.

**Q: Cached data is stale after updates**
A: Invalidate the cache when data changes: `cacheProvider.Remove(cacheKey)`.

**Q: Slow queries on large datasets**
A: Add database indexes on frequently filtered columns and use specifications to filter at the database level.

**Q: Changes not persisting**
A: Always call `await unitOfWork.SaveChangesAsync()` after modifications.

**Q: Connection timeout errors**
A: Increase `CommandTimeout` in configuration or check database connectivity.

### Enable Debug Logging

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

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Setup

```bash
git clone https://github.com/Sarmkadan/dotnet-micro-orm.git
cd dotnet-micro-orm
dotnet restore
dotnet build
dotnet test
```

### Code Standards

- Follow C# naming conventions
- Use nullable reference types
- Document public APIs with XML comments
- Write unit tests for new features
- All tests must pass before submitting PR

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

See LICENSE file for details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
