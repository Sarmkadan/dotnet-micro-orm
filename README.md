
## Architecture

See [docs/architecture.md](docs/architecture.md) for the full picture: project layout, core components (DatabaseContext, Repository, UnitOfWork, batch upsert, query plan cache), data flow, design decisions with their trade-offs, extension points, and known limitations.

## Inventory

The `Inventory` class tracks stock levels for products across different warehouse locations. It provides robust methods for managing inventory movements, including restocking, withdrawing stock, reserving items for orders, and performing periodic stock counts.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class InventoryManager
{
    public void ManageStock(Inventory inventory)
    {
        // Check for low stock
        if (inventory.IsLowStock())
        {
            Console.WriteLine($"Low stock alert for product {inventory.ProductId} at {inventory.WarehouseLocation}.");
            inventory.Restock(50);
        }

        // Perform inventory operations
        inventory.Reserve(10);
        inventory.Withdraw(5);
        inventory.ReleaseReservation(2);
        
        // Validate and perform stock count
        List<string> errors;
        if (inventory.Validate(out errors))
        {
            inventory.PerformStockCount(inventory.CurrentStock);
            inventory.ModifiedDate = DateTime.UtcNow;
        }
    }
}
```


## MigrationRecord

The `MigrationRecord` class represents a persisted record of a migration that has been applied to the database. It stores metadata about the migration, including its version, description, application timestamp, success status, and error message if applicable.

### Example Usage

```csharp
using DotnetMicroOrm.Migrations;

public class MigrationRunner
{
    public async Task ApplyMigrationAsync(string migrationVersion)
    {
        var migrationRecord = new MigrationRecord
        {
            Version = migrationVersion,
            Description = "Applied migration to update user table",
            AppliedAt = DateTime.UtcNow,
            Success = true
        };

        // Save the migration record to the database
        await using var dbContext = new DatabaseContext();
        dbContext.MigrationRecords.Add(migrationRecord);
        await dbContext.SaveChangesAsync();
    }
}
```

## AuditLog

The `AuditLog` class records an audit trail for entity changes and system operations. It tracks who made changes, what was changed, when it happened, and the outcome of the operation. Useful for compliance, debugging, and monitoring entity lifecycle events.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class AuditService
{
    public AuditLog LogUserLogin(int userId, string username, string ipAddress, string userAgent)
    {
        var auditLog = AuditLog.CreateInsert(
            entityType: "User",
            entityId: userId,
            newValues: $"Username: {username}",
            userId: userId,
            username: username
        );
        
        auditLog.SetIpAndUserAgent(ipAddress, userAgent);
        auditLog.MarkAsSuccess("User login successful");
        
        return auditLog;
    }
    
    public AuditLog LogProductUpdate(Product product, string changedProperties, int userId, string username)
    {
        var auditLog = AuditLog.CreateUpdate(
            entityType: "Product",
            entityId: product.Id,
            oldValues: $"Price: {product.CostPrice}, Stock: {product.StockQuantity}",
            newValues: $"Price: {product.Price}, Stock: {product.StockQuantity}",
            changedProps: changedProperties,
            userId: userId,
            username: username
        );
        
        auditLog.SetIpAndUserAgent("192.168.1.100", "Mozilla/5.0");
        auditLog.MarkAsSuccess("Product updated successfully");
        
        return auditLog;
    }
    
    public AuditLog LogFailedOperation(string entityType, int entityId, string errorMessage, int? userId = null, string? username = null)
    {
        var auditLog = new AuditLog(entityType, entityId, "UPDATE")
        {
            UserId = userId,
            Username = username,
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            Description = "Failed to update entity"
        };
        
        auditLog.SetIpAndUserAgent("10.0.0.5", "API-Client/1.0");
        return auditLog;
    }
}
```

## MigrationRunner

The `MigrationRunner` class manages the execution of database migrations in version order. It automatically creates and maintains a `_MigrationHistory` table to track which migrations have been applied, enabling reliable migration management across environments and deployments.

### Example Usage

```csharp
using DotnetMicroOrm.Migrations;
using DotnetMicroOrm.Data;

// Discover and register your migrations (typically done via assembly scanning)
var migrations = new List<IMigration> 
{
    new CreateUsersTableMigration(),
    new AddEmailIndexMigration(),
    new SeedInitialDataMigration()
};

// Create a database context (configured for your database provider)
await using var dbContext = new DatabaseContext();

// Initialize the migration runner
var runner = new MigrationRunner(dbContext, migrations);

// Apply all pending migrations
await runner.MigrateAsync();

// Check which migrations are pending
var pendingMigrations = await runner.GetPendingMigrationsAsync();
Console.WriteLine($"Pending migrations: {pendingMigrations.Count}");

// Check which migrations have been applied
var appliedMigrations = await runner.GetAppliedMigrationsAsync();
Console.WriteLine($"Applied migrations: {appliedMigrations.Count}");

// Migrate to a specific version
await runner.MigrateToAsync("2.1.0");

// Rollback to a previous version
await runner.RollbackToAsync("1.0.0");
```

## QueryProfile

The `QueryProfile` class represents a single captured profiling record for a database query execution. It contains metadata about the query including the SQL statement, execution parameters, timing information, success status, and caller context. This type is typically consumed through the `QueryProfiler` class which aggregates multiple profiles into a `QueryProfilerSummary`.








### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

public class UserRepository
{
    private readonly QueryProfiler _profiler = new QueryProfiler();

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var profile = new QueryProfile
        {
            Query = "SELECT id, name, email FROM users WHERE id = @id",
            Parameters = new Dictionary<string, object> { ["@id"] = userId },
            ExecutedAt = DateTime.UtcNow,
            CallerMemberName = nameof(GetUserByIdAsync)
        };

        try
        {
            // Simulate query execution
            await Task.Delay(5);
            profile.Duration = TimeSpan.FromMilliseconds(5);
            profile.RowsAffected = 1;
            profile.Succeeded = true;
        }
        catch (Exception ex)
        {
            profile.Succeeded = false;
            profile.ErrorMessage = ex.Message;
        }

        // In real usage, this would be consumed by QueryProfiler
        return new User { Id = userId, Name = "Example User", Email = "user@example.com" };
    }
}
```

## QueryProfiler

The `QueryProfiler` class provides a thread-safe in-process query profiler. It stores profiles in a bounded ring-buffer, ensuring predictable memory usage under sustained load. You can use it to monitor and analyze the performance of your database queries.

### Example Usage

```csharp
public class Program
{
    public static async Task Main()
    {
        var profiler = new QueryProfiler();
        profiler.IsEnabled = true;

        var result = await profiler.ProfileAsync<int>("SELECT * FROM users", async () => await Task.FromResult(42));
        Console.WriteLine($"Result: {result}");

        var profiles = profiler.GetProfiles();
        foreach (var profile in profiles)
        {
            Console.WriteLine($"Query: {profile.Query}, Duration: {profile.Duration}, Succeeded: {profile.Succeeded}");
        }

        var summary = profiler.GetSummary();
        Console.WriteLine($"Total Queries: {summary.TotalQueries}, Total Duration: {summary.TotalDuration}, Average Duration: {summary.AverageDuration}");

        profiler.Clear();
    }
}
```

## IBackgroundJob

The `IBackgroundJob` interface defines a contract for background job execution with support for scheduling, execution tracking, and error handling. It enables asynchronous processing outside the request pipeline with configurable retry logic, timeouts, and execution constraints.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;

public class CleanupJob : IBackgroundJob
{
    public string JobId => "cleanup_job";
    public string Name => "Database Cleanup";
    public string Description => "Removes old records from the database";

    public Task ExecuteAsync()
    {
        Console.WriteLine("Cleaning up old records...");
        // Your cleanup logic here
        return Task.CompletedTask;
    }

    public bool CanExecute()
    {
        // Only run if database is available
        return true;
    }

    public Task OnFailureAsync(Exception ex)
    {
        Console.WriteLine($"Cleanup job failed: {ex.Message}");
        return Task.CompletedTask;
    }
}

// Usage with JobScheduleConfig
var job = new CleanupJob();
var config = new JobScheduleConfig
{
    RunOnStartup = false,
    Interval = TimeSpan.FromHours(1),
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromMinutes(5),
    ExecutionTimeout = TimeSpan.FromMinutes(30),
    Enabled = true
};

// Execute the job
await job.ExecuteAsync();
```

## JobScheduler

The `JobScheduler` class provides a thread-safe scheduler for executing and managing background jobs with support for both interval-based and cron-based scheduling. It handles job registration, execution with retry logic, execution history tracking, and graceful shutdown. The scheduler is designed for distributed scenarios and maintains bounded execution history to prevent memory leaks.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;

// Create a job
public class ReportGenerationJob : IBackgroundJob
{
public string JobId => "report_generation";
public string Name => "Report Generation";
public string Description => "Generates daily reports";

public async Task ExecuteAsync()
{
Console.WriteLine("Generating reports...");
// Report generation logic
await Task.Delay(100);
}

public bool CanExecute() => true;
public Task OnFailureAsync(Exception ex) => Task.CompletedTask;
}

// Configure the scheduler
var scheduler = new JobScheduler();

// Register jobs with different scheduling strategies
scheduler.Register(
new ReportGenerationJob(),
new JobScheduleConfig
{
RunOnStartup = true,
Interval = TimeSpan.FromHours(1),
MaxRetries = 3,
RetryDelay = TimeSpan.FromSeconds(30),
ExecutionTimeout = TimeSpan.FromMinutes(10),
Enabled = true
}
);

scheduler.Register(
new CleanupJob(),
new JobScheduleConfig
{
RunOnStartup = false,
CronExpression = "0 2 * * *", // Run at 2 AM daily
MaxRetries = 2,
RetryDelay = TimeSpan.FromMinutes(2),
ExecutionTimeout = TimeSpan.FromMinutes(5),
Enabled = true
}
);

// Start the scheduler
await scheduler.StartAsync();

// Monitor execution history
var recentExecutions = scheduler.GetRecentExecutions(50);
foreach (var execution in recentExecutions)
{
Console.WriteLine($"Job {execution.JobId} executed at {execution.StartTime}: {(execution.Success ? "SUCCESS" : "FAILED")}");
}

// Get history for a specific job
var jobHistory = scheduler.GetExecutionHistory("report_generation");

// Execute a job manually
var result = await scheduler.ExecuteJobAsync(
new ReportGenerationJob(),
new JobScheduleConfig { MaxRetries = 3 }
);

Console.WriteLine($"Execution result: {result.Success}, Duration: {result.Duration}");

// Stop the scheduler when application shuts down
await scheduler.StopAsync();
```

## QueryProfile

The `QueryProfile` class represents a single captured profiling record for a database query execution. It contains metadata about the query including the SQL statement, execution parameters, timing information, success status, and caller context. This type is typically consumed through the `QueryProfiler` class which aggregates multiple profiles into a `QueryProfilerSummary`.








### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

public class UserRepository
{
    private readonly QueryProfiler _profiler = new QueryProfiler();

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var profile = new QueryProfile
        {
            Query = "SELECT id, name, email FROM users WHERE id = @id",
            Parameters = new Dictionary<string, object> { ["@id"] = userId },
            ExecutedAt = DateTime.UtcNow,
            CallerMemberName = nameof(GetUserByIdAsync)
        };

        try
        {
            // Simulate query execution
            await Task.Delay(5);
            profile.Duration = TimeSpan.FromMilliseconds(5);
            profile.RowsAffected = 1;
            profile.Succeeded = true;
        }
        catch (Exception ex)
        {
            profile.Succeeded = false;
            profile.ErrorMessage = ex.Message;
        }

        // In real usage, this would be consumed by QueryProfiler
        return new User { Id = userId, Name = "Example User", Email = "user@example.com" };
    }
}
```

## DataCleanupJob

The `DataCleanupJob` is a background job that maintains database health by removing old audit logs, expired sessions, and soft-deleted records. It operates on a configurable schedule, allowing for fine-tuned control over retention periods, batch sizes for cleanup, and optional database index rebuilding.

### Example Usage

```csharp
using DotnetMicroOrm.BackgroundJobs;
using DotnetMicroOrm.Data;

public class CleanupTask
{
    public async Task RunCleanupAsync(IDatabaseContext dbContext)
    {
        var config = new DataCleanupConfig
        {
            AuditLogRetentionDays = 30,
            DeletedRecordRetentionDays = 15,
            CleanupAuditLogs = true,
            CleanupSoftDeletedRecords = true,
            CleanupTemporaryData = true,
            RebuildIndexes = false,
            BatchSize = 500
        };

        var job = new DataCleanupJob(dbContext, config);

        if (job.CanExecute())
        {
            try
            {
                await job.ExecuteAsync();
            }
            catch (Exception ex)
            {
                await job.OnFailureAsync(ex);
            }
        }
    }
}
```

## TableAttribute

The `TableAttribute` is used to specify the database table name and schema for entity classes. It allows you to map a class to a specific table in the database, including custom schema names for multi-tenant scenarios or when working with different database schemas.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Domain.Models.MappingAttributes;

[Table("users", Schema = "auth")]
public class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("username", IsNullable: false)]
    public string Username { get; set; }

    [Column("email", MaxLength = 255)]
    public string Email { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

// Usage in repository
public class UserRepository
{
    private readonly DatabaseContext _dbContext;

    public UserRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Query<User>()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }
}
```

## Category

The `Category` class represents a hierarchical product category entity used for organizing products in an e-commerce or inventory system. It supports nested categories through parent-child relationships, provides validation for category data, and includes methods for managing category display order and generating breadcrumb navigation paths.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class CategoryManager
{
    public void ManageCategories(Category category)
    {
        // Create a new category
        var electronics = new Category("Electronics", "electronics")
        {
            Description = "Electronic devices and accessories",
            DisplayOrder = 1,
            IsActive = true
        };

        // Create a subcategory with parent relationship
        var smartphones = new Category("Smartphones", "smartphones")
        {
            Description = "Mobile phones and smartphones",
            DisplayOrder = 1,
            ParentCategoryId = electronics.Id,
            ParentCategory = electronics,
            CreatedDate = DateTime.UtcNow
        };

        // Validate category data
        if (electronics.Validate(out var errors))
        {
            Console.WriteLine("Category is valid");
        }
        else
        {
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));
        }

        // Manage display order
        smartphones.MoveUp(); // Decreases display order by 1
        smartphones.MoveDown(); // Increases display order by 1

        // Get breadcrumb navigation path
        var breadcrumb = smartphones.GetBreadcrumb();
        Console.WriteLine($"Category path: {breadcrumb}");

        // Get product count (requires Products collection to be populated)
        var productCount = smartphones.GetProductCount();
        Console.WriteLine($"Products in category: {productCount}");

        // Deactivate category and all subcategories
        electronics.Deactivate();
    }
}
```

## OrderItem

The `OrderItem` class represents a line item within an order, containing product details, pricing information, quantities, and calculated totals. It provides methods for calculating line totals, applying discounts, and computing tax amounts, making it suitable for e-commerce and order management scenarios.


### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class OrderService
{
    public OrderItem CreateOrderItem(Product product, int quantity, decimal discount = 0)
    {
        var orderItem = new OrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = quantity,
            UnitPrice = product.Price,
            Discount = discount,
            TaxAmount = 0,
            CreatedDate = DateTime.UtcNow
        };

        orderItem.CalculateLineTotal();
        return orderItem;
    }

    public void ApplyDiscountToItem(OrderItem item, decimal discountPercentage)
    {
        var discountAmount = item.GetSubtotal() * discountPercentage / 100;
        item.ApplyDiscount(discountAmount);
        item.CalculateLineTotal();
    }

    public void ProcessOrderItems(Order order)
    {
        foreach (var item in order.Items)
        {
            // Calculate line total
            item.CalculateLineTotal();

            // Apply any discounts
            if (item.Discount > 0)
            {
                item.ApplyDiscount(item.Discount);
            }

            // Calculate tax
            item.TaxAmount = item.GetAfterDiscount() * 0.08m; // 8% tax

            // Final calculation
            item.CalculateLineTotal();
        }
    }
}
```

## Product

The `Product` class represents a product entity in an e-commerce catalog. It tracks essential product information including SKU, pricing, stock levels, and category associations. The class provides inventory management methods, validation logic, and profit calculation capabilities, making it suitable for inventory and order management scenarios.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class ProductCatalog
{
    public void ManageProduct(Product product)
    {
        // Create a new product
        var laptop = new Product("LAP-001", "Gaming Laptop", 1299.99m, 5)
        {
            Description = "High-performance gaming laptop with RTX graphics",
            CostPrice = 950.00m,
            StockQuantity = 25,
            IsActive = true
        };

        // Validate product data
        if (laptop.Validate(out var errors))
        {
            Console.WriteLine("Product is valid");
        }
        else
        {
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));
        }

        // Manage inventory
        laptop.IncreaseStock(10); // Add 10 units to stock
        laptop.DecreaseStock(3); // Remove 3 units from stock

        // Calculate profit
        var profit = laptop.GetProfit();
        Console.WriteLine($"Product profit per unit: {profit:C}");

        // Check stock levels
        if (laptop.IsLowStock(5))
        {
            Console.WriteLine("Low stock alert!");
        }

        // Update modified date
        laptop.ModifiedDate = DateTime.UtcNow;
    }

    public void ProcessProductOrder(Product product, int quantity)
    {
        // Check stock availability
        if (product.StockQuantity >= quantity)
        {
            // Decrease stock for order fulfillment
            product.DecreaseStock(quantity);
            
            Console.WriteLine($"Order processed: {quantity} units of {product.Name}");
        }
        else
        {
            Console.WriteLine("Insufficient stock!");
        }
    }
}
```

## Order

The `Order` class represents a customer order entity that tracks order details including order number, customer information, items, pricing, status, and delivery information. It provides methods for managing order items, calculating totals, and updating order status throughout the order lifecycle from creation to delivery.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

public class OrderService
{
    public Order CreateNewOrder(int userId, string shippingAddress)
    {
        // Create a new order for a user
        var order = new Order(userId, shippingAddress)
        {
            Status = "Pending",
            TaxAmount = 0,
            Notes = "Standard delivery"
        };
        
        return order;
    }

    public void ProcessOrder(Order order, List<OrderItem> items)
    {
        // Add items to the order
        foreach (var item in items)
        {
            order.AddItem(item);
        }

        // Validate order before processing
        if (order.Validate(out var errors))
        {
            Console.WriteLine("Order is valid");
            
            // Mark order as confirmed
            order.Status = "Confirmed";
            order.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            Console.WriteLine("Validation errors: " + string.Join(", ", errors));
        }
    }

    public void UpdateOrderStatus(Order order, string newStatus)
    {
        switch (newStatus)
        {
            case "Shipped":
                order.Ship(DateTime.UtcNow);
                Console.WriteLine($"Order {order.OrderNumber} shipped on {order.ShippingDate}");
                break;
                
            case "Delivered":
                order.MarkAsDelivered();
                Console.WriteLine($"Order {order.OrderNumber} delivered on {order.DeliveryDate}");
                break;
                
            case "Cancelled":
                order.Cancel();
                Console.WriteLine($"Order {order.OrderNumber} cancelled");
                break;
        }
    }

    public void DisplayOrderSummary(Order order)
    {
        Console.WriteLine($"Order #{order.OrderNumber}");
        Console.WriteLine($"Date: {order.OrderDate:yyyy-MM-dd}");
        Console.WriteLine($"Status: {order.Status}");
        Console.WriteLine($"Total: {order.TotalAmount:C}");
        Console.WriteLine($"Tax: {order.TaxAmount:C}");
        Console.WriteLine($"Taxable Amount: {order.GetTaxableAmount():C}");
        Console.WriteLine($"Items: {order.Items.Count}");
        Console.WriteLine($"Shipping: {order.ShippingAddress}");
        if (order.BillingAddress != null)
        {
            Console.WriteLine($"Billing: {order.BillingAddress}");
        }
        
        if (order.ShippingDate.HasValue)
        {
            Console.WriteLine($"Shipped: {order.ShippingDate.Value:yyyy-MM-dd}");
        }
        
        if (order.DeliveryDate.HasValue)
        {
            Console.WriteLine($"Delivered: {order.DeliveryDate.Value:yyyy-MM-dd}");
        }
        
        if (!string.IsNullOrEmpty(order.Notes))
        {
            Console.WriteLine($"Notes: {order.Notes}");
        }
    }
}
```

## PipelineBuilder

The `PipelineBuilder` class constructs a middleware pipeline for processing requests through a sequence of middleware components. It enables composing multiple middleware in a specific order, with support for both sequential execution and custom ordering via the `Order` property on middleware. The pipeline executes middleware in FIFO order, allowing for flexible request/response processing patterns.

### Example Usage

```csharp
using DotnetMicroOrm.Pipeline;
using DotnetMicroOrm.Middleware;

// Define custom middleware
public class LoggingMiddleware : IMiddleware
{
    public int Order => 1; // Lower order executes first
    
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        Console.WriteLine($"Before middleware execution at {DateTime.UtcNow}");
        await next(context);
        Console.WriteLine("After middleware execution");
    }
}

public class AuthMiddleware : IMiddleware
{
    public int Order => 2;
    
    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        Console.WriteLine("Authenticating request...");
        await next(context);
    }
}

// Build and execute a pipeline
var builder = new PipelineBuilder();

// Add middleware to the pipeline
builder.Use(new LoggingMiddleware());
builder.Use(new AuthMiddleware());

// Alternatively, add multiple middleware at once
var additionalMiddlewares = new IMiddleware[]
{
    new ErrorHandlingMiddleware(),
    new MetricsMiddleware()
};
builder.UseAll(additionalMiddlewares);

// Build the pipeline delegate
var pipeline = builder.Build();

// Create a context and execute the pipeline
var context = new MiddlewareContext
{
    Items = new Dictionary<string, object>(),
    Request = new HttpRequestMessage(),
    Response = null
};

await builder.ExecuteAsync(context);

// Inspect the ordered middleware
var orderedMiddlewares = builder.GetOrdered();
Console.WriteLine($"Pipeline contains {builder.Count} middleware components");

// Clear the pipeline when needed
builder.Clear();
```
```