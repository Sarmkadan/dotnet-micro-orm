# dotnet-micro-orm

A high-performance micro-ORM for .NET with support for compiled expressions, batch operations, change tracking, and multi-database support.

## Features

- **Compiled Expressions**: Leverages LINQ expression trees for high-performance queries
- **Batch Operations**: Efficient bulk insert, update, and delete operations
- **Change Tracking**: Entity-level change tracking and state management
- **Multi-Database Support**: SQL Server, PostgreSQL, MySQL, Oracle, SQLite ready
- **Type-Safe Queries**: Compile-time checked LINQ support
- **Transaction Support**: Full transaction management with isolation levels
- **Attribute-Based Mapping**: Declarative entity-to-table mapping
- **Fluent Configuration**: Easy setup with extension methods
- **Audit Logging**: Built-in audit trail for entity changes
- **Generic Repositories**: Type-safe repository pattern implementation
- **Unit of Work Pattern**: Transaction-aware unit of work implementation

## Quick Start

### Installation

```bash
dotnet add package DotnetMicroOrm
```

### Configuration

```csharp
var services = new ServiceCollection();

services.AddDotnetMicroOrm(
    "Server=localhost;Database=MyDb;User Id=sa;Password=Pass@123;",
    DatabaseProvider.SqlServer,
    options =>
    {
        options.EnableChangeTracking = true;
        options.EnableAuditLogging = true;
        options.DefaultBatchSize = 1000;
    });

var provider = services.BuildServiceProvider();
```

### Define Entities

```csharp
[Table("Users")]
public class User : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Username", IsNullable = false, MaxLength = 50)]
    [Unique]
    public string Username { get; set; }

    [Column("Email", IsNullable = false, MaxLength = 100)]
    public string Email { get; set; }

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }
}
```

### Basic CRUD Operations

```csharp
var userService = provider.GetRequiredService<UserService>();

// Create
var user = await userService.RegisterUserAsync("john_doe", "john@example.com", "password");

// Read
var retrieved = await userService.GetUserByIdAsync(user.Id);

// Update
await userService.UpdateProfileAsync(user.Id, "John", "Doe", "+1-555-0123");

// Delete
await userService.DeactivateUserAsync(user.Id);
```

### Batch Operations

```csharp
var users = new List<User>
{
    new User { Username = "user1", Email = "user1@example.com" },
    new User { Username = "user2", Email = "user2@example.com" },
    new User { Username = "user3", Email = "user3@example.com" }
};

var inserted = await userRepository.AddRangeAsync(users);
var deleted = await userRepository.DeleteRangeAsync(users);
```

### Unit of Work Pattern

```csharp
using var unitOfWork = new UnitOfWork(dbContext);

await unitOfWork.BeginTransactionAsync();

try
{
    var userRepo = unitOfWork.Repository<User>();
    var productRepo = unitOfWork.Repository<Product>();

    var user = await userRepo.AddAsync(new User { ... });
    var product = await productRepo.AddAsync(new Product { ... });

    await unitOfWork.CommitAsync();
}
catch
{
    await unitOfWork.RollbackAsync();
    throw;
}
```

## Architecture

### Core Components

- **IDatabaseContext**: Low-level database connection management
- **IRepository<T>**: Generic CRUD operations
- **IUnitOfWork**: Transaction management
- **BaseEntity**: Base class for all domain entities
- **ServiceLayer**: Business logic and domain operations

### Services

- **UserService**: User management and authentication
- **ProductService**: Product catalog and inventory
- **OrderService**: Order processing and fulfillment
- **AuditService**: Audit trail and compliance logging

## Entity Mapping

Entities are mapped to database tables using attributes:

```csharp
[Table("Products", Schema = "dbo")]
public class Product : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Sku", IsNullable = false, MaxLength = 50)]
    [Unique]
    [Indexed]
    public string Sku { get; set; }

    [Column("Price", Precision = 18, Scale = 2)]
    public decimal Price { get; set; }

    [NotMapped]
    public virtual Category Category { get; set; }
}
```

## Query Examples

### Simple Queries

```csharp
var user = await userRepository.GetByIdAsync(1);
var users = await userRepository.GetAllAsync();
var activeUsers = await userRepository.GetAsync(u => u.IsActive);
```

### Paged Queries

```csharp
var (items, total) = await userRepository.GetPagedWithCountAsync(
    pageNumber: 1,
    pageSize: 20);

Console.WriteLine($"Page 1 of {Math.Ceiling(total / 20.0)}");
```

### Custom Repositories

```csharp
public class UserRepository : Repository<User>
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Username == username);
    }
}
```

## Performance Features

- **Expression Caching**: Compiled LINQ expressions are cached
- **Batch Operations**: Efficient bulk processing with configurable batch sizes
- **Connection Pooling**: Built-in SQL Server connection pooling
- **Lazy Evaluation**: Deferred query execution where possible

## Transaction Support

```csharp
await dbContext.BeginTransactionAsync(TransactionIsolationLevel.ReadCommitted);

try
{
    // Perform operations
    await dbContext.CommitAsync();
}
catch
{
    await dbContext.RollbackAsync();
}
```

## Audit Logging

```csharp
var auditService = new AuditService(dbContext);

// Log entity changes
await auditService.LogInsertAsync("User", userId, jsonData, currentUserId);
await auditService.LogUpdateAsync("Product", productId, oldData, newData);
await auditService.LogDeleteAsync("Order", orderId);

// Get audit trail
var logs = await auditService.GetAuditLogsAsync("User", userId);
var summary = await auditService.GetSummaryAsync();
```

## Testing

Create a test database and run integration tests:

```csharp
[TestClass]
public class UserServiceTests
{
    private IDatabaseContext _context;
    private UserService _service;

    [TestInitialize]
    public void Setup()
    {
        var connectionString = "Server=.;Database=TestDb;Integrated Security=true;";
        _context = new DatabaseContext(connectionString);
        _service = new UserService(_context);
    }

    [TestMethod]
    public async Task RegisterUser_WithValidData_ShouldSucceed()
    {
        var user = await _service.RegisterUserAsync("testuser", "test@example.com", "password");
        Assert.IsNotNull(user);
    }
}
```

## License

MIT License - Copyright 2026 Vladyslav Zaiets

See LICENSE file for details.

## Contributing

Contributions are welcome! Please follow these guidelines:
- Fork the repository
- Create a feature branch
- Make your changes
- Write or update tests
- Submit a pull request

## Support

For issues, questions, or feature requests, visit the repository.

## Roadmap

- [ ] Entity Framework Core integration layer
- [ ] Query interceptors
- [ ] Advanced caching strategies
- [ ] GraphQL support
- [ ] Full text search capabilities
- [ ] Sharding and partitioning support
