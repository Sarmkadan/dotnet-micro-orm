// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with DotnetMicroOrm

This guide will help you set up and start using DotnetMicroOrm in your .NET 10 application.

## Prerequisites

- .NET 10 SDK or later
- SQL Server, PostgreSQL, MySQL, or SQLite
- Visual Studio 2024, VS Code, or any .NET-compatible IDE

## Step 1: Create a New Project

```bash
dotnet new console -n MyOrmApp
cd MyOrmApp
```

## Step 2: Install DotnetMicroOrm

```bash
dotnet add package DotnetMicroOrm
```

## Step 3: Define Your First Entity

Create a file `Models/Product.cs`:

```csharp
using DotnetMicroOrm.Domain.Models;

namespace MyOrmApp.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

## Step 4: Configure Database Connection

Create `Services/DatabaseSetup.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;

namespace MyOrmApp.Services
{
    public static class DatabaseSetup
    {
        public static IServiceProvider ConfigureDatabase(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDatabaseContext(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseType = DatabaseType.SqlServer;
                options.EnableChangeTracking = true;
                options.EnableCaching = true;
                options.CacheTTLSeconds = 300;
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMemoryCache();

            return services.BuildServiceProvider();
        }
    }
}
```

## Step 5: Create a Service

Create `Services/ProductService.cs`:

```csharp
using MyOrmApp.Models;
using DotnetMicroOrm.Data;

namespace MyOrmApp.Services
{
    public class ProductService
    {
        private readonly IRepository<Product> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IRepository<Product> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> CreateAsync(string name, string description, decimal price)
        {
            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var spec = new Specification<Product>().OrderBy(p => p.Name);
            return await _repository.GetAsync(spec);
        }

        public async Task UpdateAsync(Product product)
        {
            await _repository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

## Step 6: Use in Program

Update `Program.cs`:

```csharp
using MyOrmApp.Models;
using MyOrmApp.Services;

const string connectionString = "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;";

var provider = DatabaseSetup.ConfigureDatabase(connectionString);
var productService = provider.GetRequiredService<ProductService>();

// Create
var product = await productService.CreateAsync("Laptop", "High-performance laptop", 999.99m);
Console.WriteLine($"Created: {product.Name} (ID: {product.Id})");

// Read
var fetched = await productService.GetAsync(product.Id);
Console.WriteLine($"Retrieved: {fetched.Name}");

// List
var products = await productService.GetAllAsync();
foreach (var p in products)
{
    Console.WriteLine($"- {p.Name}: ${p.Price}");
}

// Update
fetched.Price = 899.99m;
await productService.UpdateAsync(fetched);
Console.WriteLine("Updated successfully");

// Delete
await productService.DeleteAsync(product.Id);
Console.WriteLine("Deleted successfully");
```

## Step 7: Run the Application

```bash
dotnet run
```

## Database Schema Creation

For the initial database setup, create a migration script or use SQL directly:

### SQL Server

```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL
);
```

### PostgreSQL

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(18,2) NOT NULL,
    stock_quantity INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NULL
);
```

## Configuration Options

### Minimal Configuration

```csharp
services.AddDatabaseContext(options =>
{
    options.ConnectionString = connectionString;
    options.DatabaseType = DatabaseType.SqlServer;
});
```

### Full Configuration

```csharp
services.AddDatabaseContext(options =>
{
    options.ConnectionString = connectionString;
    options.DatabaseType = DatabaseType.SqlServer;
    options.EnableChangeTracking = true;
    options.EnableCaching = true;
    options.CacheTTLSeconds = 300;
    options.CommandTimeout = 30;
    options.MaxPoolSize = 100;
    options.EnableDetailedLogging = false;
    options.LogSlowQueries = true;
    options.SlowQueryThresholdMs = 500;
});

services.Configure<CachingOptions>(options =>
{
    options.Provider = "Memory";
    options.DefaultTTLSeconds = 300;
    options.MaxEntriesPerKey = 1000;
});
```

## Common Database Configurations

### SQL Server (Local)
```csharp
"Server=(local);Database=MyDb;Integrated Security=true;"
```

### SQL Server (Remote)
```csharp
"Server=myserver.database.windows.net;Database=MyDb;User Id=admin;Password=P@ssw0rd;"
```

### PostgreSQL
```csharp
"Host=localhost;Database=mydb;Username=postgres;Password=password"
```

### MySQL
```csharp
"Server=localhost;Database=mydb;User Id=root;Password=password;"
```

### SQLite
```csharp
"Data Source=mydb.db"
```

## Next Steps

- Read the [Architecture Guide](./architecture.md)
- Explore [Usage Examples](../examples)
- Check the [API Reference](./api-reference.md)
- See [FAQ](./faq.md) for common questions
