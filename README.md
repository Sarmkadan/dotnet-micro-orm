
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

## IAuditService

The `IAuditService` interface provides a contract for auditing entity changes and system operations. It tracks insertions, updates, deletions, and failures across the application, enabling compliance tracking, debugging, and operational monitoring. The service integrates with the `AuditLog` entity to persist audit records and provides querying capabilities for analyzing historical operations.

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class AuditManager
{
    private readonly IAuditService _auditService;

    public AuditManager(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task TrackUserRegistrationAsync(int userId, string username)
    {
        // Log user registration as an insert operation
        await _auditService.LogInsertAsync(
            entityType: "User",
            entityId: userId,
            newValues: $"Username: {username}",
            userId: userId,
            username: username
        );
    }

    public async Task TrackProductUpdateAsync(Product product, int userId, string username)
    {
        // Log product update with before/after values
        await _auditService.LogUpdateAsync(
            entityType: "Product",
            entityId: product.Id,
            oldValues: $"Price: {product.CostPrice}, Stock: {product.StockQuantity}",
            newValues: $"Price: {product.Price}, Stock: {product.StockQuantity}",
            changedProperties: "Price,StockQuantity",
            userId: userId,
            username: username
        );
    }

    public async Task TrackOrderDeletionAsync(int orderId, int userId, string username)
    {
        // Log order deletion with the deleted values
        await _auditService.LogDeleteAsync(
            entityType: "Order",
            entityId: orderId,
            oldValues: $"Order #{orderId} - Total: {orderId * 100:C}",
            userId: userId,
            username: username
        );
    }

    public async Task TrackFailedOperationAsync(string entityType, int entityId, string action, string errorMessage, int? userId = null, string? username = null)
    {
        // Log failed operations for error tracking and debugging
        await _auditService.LogFailureAsync(
            entityType: entityType,
            entityId: entityId,
            action: action,
            errorMessage: errorMessage,
            userId: userId,
            username: username
        );
    }

    public async Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, int entityId)
    {
        // Retrieve all audit logs for a specific entity
        return await _auditService.GetAuditLogsAsync(entityType, entityId);
    }

    public async Task<List<AuditLog>> GetUserActivityAsync(int userId)
    {
        // Get all activity for a specific user
        return await _auditService.GetUserActivityAsync(userId);
    }

    public async Task<List<AuditLog>> GetFailedOperationsAsync(int daysBack = 7)
    {
        // Get all failed operations in the last N days
        return await _auditService.GetFailedOperationsAsync(daysBack);
    }

    public async Task<int> CleanupOldLogsAsync(int daysToKeep = 90)
    {
        // Purge old audit logs to maintain database size
        return await _auditService.PurgeOldLogsAsync(daysToKeep);
    }

    public async Task<AuditSummary> GetSystemAuditSummaryAsync()
    {
        // Get summary statistics of all audit operations
        return await _auditService.GetSummaryAsync();
    }

    public async Task DisplayAuditReportAsync()
    {
        // Generate a comprehensive audit report
        var summary = await _auditService.GetSummaryAsync();
        
        Console.WriteLine("=== Audit System Report ===");
        Console.WriteLine($"Total Operations: {summary.TotalOperations}");
        Console.WriteLine($"Successful: {summary.SuccessfulOperations} ({summary.SuccessRate:F1}%)");
        Console.WriteLine($"Failed: {summary.FailedOperations}");
        Console.WriteLine($"Breakdown: Inserts={summary.Inserts}, Updates={summary.Updates}, Deletes={summary.Deletes}");
        
        var recentFailures = await _auditService.GetFailedOperationsAsync(7);
        Console.WriteLine($"\nRecent Failures ({recentFailures.Count} in last 7 days):");
        foreach (var failure in recentFailures.Take(5))
        {
            Console.WriteLine($"  - {failure.Timestamp:yyyy-MM-dd HH:mm} - {failure.EntityType}.{failure.EntityId}: {failure.ErrorMessage}");
        }
    }
}

// Usage with dependency injection
var services = new ServiceCollection();
services.AddSingleton<IAuditService, AuditService>();
services.AddSingleton<AuditManager>();

var serviceProvider = services.BuildServiceProvider();
var auditManager = serviceProvider.GetRequiredService<AuditManager>();

// Track operations throughout the application lifecycle
await auditManager.TrackUserRegistrationAsync(42, "johndoe");
await auditManager.TrackProductUpdateAsync(new Product { Id = 101, CostPrice = 500, Price = 799.99m, StockQuantity = 25 }, 42, "johndoe");
await auditManager.TrackOrderDeletionAsync(1001, 42, "johndoe");

// Generate audit report
await auditManager.DisplayAuditReportAsync();
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

## AuthenticationMiddleware

The `AuthenticationMiddleware` provides API authentication and authorization functionality. It validates API keys, authenticates users, and populates `AuthenticationInfo` in the middleware context for downstream handlers. The middleware supports both API key authentication (via `Bearer` or `ApiKey` tokens) and role-based authorization checks.





### Features
- API key authentication with user ID and role mapping
- Role-based access control methods (`HasRole`, `HasAnyRole`)
- API key management (register and revoke)
- Middleware pipeline integration with configurable execution order

### Example Usage

```csharp
using DotnetMicroOrm.Middleware;

// Create authentication middleware
var authMiddleware = new AuthenticationMiddleware();

// Register API keys for users
// In production, load these from secure storage instead of hardcoding
authMiddleware.RegisterApiKey("prod-key-xyz123", 1001, "admin");
authMiddleware.RegisterApiKey("prod-key-abc456", 1002, "user");

// Check user roles (static methods)
var adminUser = new AuthenticationInfo { UserId = 1001, Role = "admin" };
var regularUser = new AuthenticationInfo { UserId = 1002, Role = "user" };

bool isAdmin = AuthenticationMiddleware.HasRole(adminUser, "admin"); // true
bool hasPermission = AuthenticationMiddleware.HasAnyRole(regularUser, "admin", "user"); // true

// Use in middleware pipeline
var pipelineBuilder = new PipelineBuilder();
pipelineBuilder.Use(authMiddleware);

// Revoke API keys when needed
authMiddleware.RevokeApiKey("demo-key-12345");
```

## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` catches exceptions thrown during request processing and converts them into standardized error responses. It prevents unhandled exceptions from propagating up the call stack, ensuring consistent error formatting and proper logging for debugging and monitoring purposes. The middleware handles specific exception types (like `OrmException`, `ArgumentException`, `UnauthorizedAccessException`, and `TimeoutException`) with appropriate error codes, while falling back to a generic "INTERNAL_ERROR" response for unexpected exceptions.

### Example Usage

```csharp
using DotnetMicroOrm.Middleware;
using DotnetMicroOrm.Exceptions;

// Create error handling middleware
var errorMiddleware = new ErrorHandlingMiddleware();

// Build a middleware pipeline with error handling
var pipelineBuilder = new PipelineBuilder();
pipelineBuilder.Use(errorMiddleware);
pipelineBuilder.Use(new AuthenticationMiddleware());
pipelineBuilder.Use(new SomeOtherMiddleware());

// Simulate an ORM error scenario
try
{
    // This would throw an OrmException in real usage
    throw new OrmException("Database connection failed");
}
catch (OrmException ex)
{
    // The middleware will catch this and set context.ResponseData
    var context = new MiddlewareContext
    {
        RequestId = Guid.NewGuid().ToString(),
        Exception = ex
    };
    
    // Invoke the pipeline
    await errorMiddleware.InvokeAsync(context, _ => Task.CompletedTask);
    
    // Access the standardized error response
    var errorResponse = context.ResponseData as ErrorResponse;
    Console.WriteLine($"Error Code: {errorResponse?.Code}");
    Console.WriteLine($"Error Message: {errorResponse?.Message}");
    Console.WriteLine($"Request ID: {errorResponse?.RequestId}");
    Console.WriteLine($"Timestamp: {errorResponse?.Timestamp}");
}

// Real-world usage in ASP.NET Core style pipeline:
// app.UseMiddleware<ErrorHandlingMiddleware>();
// app.UseMiddleware<AuthenticationMiddleware>();
```

## RateLimitingMiddleware

The `RateLimitingMiddleware` implements request rate limiting to prevent abuse and ensure fair resource usage across your application. It uses a token bucket algorithm to provide smooth rate limiting without sudden cutoffs, tracking requests per user or IP address and blocking excessive traffic based on configurable thresholds. The middleware integrates seamlessly with the middleware pipeline and can be disabled for testing purposes.

### Example Usage

```csharp
using DotnetMicroOrm.Middleware;

// Configure rate limiting with 100 requests per minute
var rateLimitConfig = new RateLimitConfig
{
    MaxRequests = 100,
    WindowDuration = TimeSpan.FromMinutes(1),
    Enabled = true
};

// Create rate limiting middleware
var rateLimitMiddleware = new RateLimitingMiddleware(rateLimitConfig);

// Build middleware pipeline with rate limiting
var pipelineBuilder = new PipelineBuilder();
pipelineBuilder.Use(new ErrorHandlingMiddleware());
pipelineBuilder.Use(rateLimitMiddleware);
pipelineBuilder.Use(new AuthenticationMiddleware());

// Simulate authenticated user context
var context = new MiddlewareContext
{
    RequestId = Guid.NewGuid().ToString(),
    User = new AuthenticationInfo { UserId = 1001, Role = "admin" }
};

// Execute the pipeline
try
{
    await pipelineBuilder.ExecuteAsync(context);
    
    if (context.ResponseData is null)
    {
        Console.WriteLine("Request processed successfully within rate limit");
    }
    else if (context.ResponseData is ErrorResponse errorResponse)
    {
        Console.WriteLine($"Rate limit exceeded: {errorResponse.Message}");
    }
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit exceeded"))
{
    Console.WriteLine($"Rate limit exceeded: {ex.Message}");
}

// Periodically clean up expired rate limit buckets
// Typically called by a background service
rateLimitMiddleware.CleanupExpiredBuckets();
```

## UserService

The `UserService` provides user authentication, registration, profile management, and account lifecycle operations. It handles user registration with validation, password hashing, authentication, profile updates, password changes, email verification, and user status management. The service integrates with the `UserRepository` for data persistence and implements `IAsyncDisposable` for proper resource cleanup.

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create database context and user service
await using var dbContext = new DatabaseContext();
var userService = new UserService(dbContext);

// Register a new user
var newUser = await userService.RegisterUserAsync(
    username: "johndoe",
    email: "john.doe@example.com",
    password: "SecurePassword123!"
);
Console.WriteLine($"User registered: {newUser.Username} (ID: {newUser.Id})");

// Authenticate user
var authenticatedUser = await userService.AuthenticateAsync(
    username: "johndoe",
    password: "SecurePassword123!"
);
if (authenticatedUser is not null)
{
    Console.WriteLine("Authentication successful");
    Console.WriteLine($"Last login: {authenticatedUser.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}");
}

// Get user by ID
var user = await userService.GetUserByIdAsync(newUser.Id);
if (user is not null)
{
    Console.WriteLine($"Found user: {user.Username}, Email: {user.Email}");
}

// Update user profile
var updatedUser = await userService.UpdateProfileAsync(
    userId: newUser.Id,
    firstName: "John",
    lastName: "Doe",
    phoneNumber: "+1-555-123-4567"
);
Console.WriteLine($"Profile updated: {updatedUser.FirstName} {updatedUser.LastName}");

// Change password
var passwordChanged = await userService.ChangePasswordAsync(
    userId: newUser.Id,
    currentPassword: "SecurePassword123!",
    newPassword: "NewSecurePassword456!"
);
Console.WriteLine($"Password changed: {passwordChanged}");

// Verify email
var emailVerified = await userService.VerifyEmailAsync(newUser.Id);
Console.WriteLine($"Email verified: {emailVerified}");

// Get active users count
var activeUsersCount = await userService.GetActiveUsersCountAsync();
Console.WriteLine($"Active users: {activeUsersCount}");

// Get inactive users
var inactiveUsers = await userService.GetInactiveUsersAsync(daysInactive: 30);
Console.WriteLine($"Inactive users: {inactiveUsers.Count}");

// Deactivate user
var deactivated = await userService.DeactivateUserAsync(newUser.Id);
Console.WriteLine($"User deactivated: {deactivated}");

// Dispose the service when done
await userService.DisposeAsync();
```

## ProductService

The `ProductService` provides comprehensive product catalog and inventory management functionality. It handles product creation, retrieval, updates, stock management, category-based filtering, search, and inventory analytics. The service integrates with the `ProductRepository` for data persistence and implements `IAsyncDisposable` for proper resource cleanup.

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create database context and product service
await using var dbContext = new DatabaseContext();
var productService = new ProductService(dbContext);

// Create a new product
var newProduct = await productService.CreateProductAsync(
    sku: "LAP-001",
    name: "Gaming Laptop",
    price: 1299.99m,
    categoryId: 5,
    description: "High-performance gaming laptop with RTX graphics"
);
Console.WriteLine($"Product created: {newProduct.Name} (SKU: {newProduct.Sku}, ID: {newProduct.Id})");

// Get product by ID
var product = await productService.GetProductAsync(newProduct.Id);
if (product is not null)
{
    Console.WriteLine($"Found product: {product.Name} - {product.Price:C}");
}

// Get all active products
var activeProducts = await productService.GetActiveProductsAsync();
Console.WriteLine($"Active products: {activeProducts.Count}");

// Get products by category
var categoryProducts = await productService.GetCategoryProductsAsync(5);
Console.WriteLine($"Products in category 5: {categoryProducts.Count}");

// Update product details
var updatedProduct = await productService.UpdateProductAsync(
    productId: newProduct.Id,
    name: "Gaming Laptop Pro",
    price: 1399.99m,
    description: "Updated: High-performance gaming laptop with RTX 4080 graphics"
);
Console.WriteLine($"Product updated: {updatedProduct.Name} - {updatedProduct.Price:C}");

// Update stock quantity
var stockUpdated = await productService.UpdateStockAsync(newProduct.Id, 25);
Console.WriteLine($"Stock updated to: {stockUpdated.StockQuantity}");

// Increase stock
var stockIncreased = await productService.IncreaseStockAsync(newProduct.Id, 10);
Console.WriteLine($"Stock increased to: {stockIncreased.StockQuantity}");

// Decrease stock
var stockDecreased = await productService.DecreaseStockAsync(newProduct.Id, 3);
Console.WriteLine($"Stock decreased to: {stockDecreased.StockQuantity}");

// Get low stock products
var lowStockProducts = await productService.GetLowStockProductsAsync(threshold: 5);
Console.WriteLine($"Low stock products (threshold 5): {lowStockProducts.Count}");

// Get out of stock products
var outOfStockProducts = await productService.GetOutOfStockProductsAsync();
Console.WriteLine($"Out of stock products: {outOfStockProducts.Count}");

// Search products
var searchResults = await productService.SearchProductsAsync("laptop");
Console.WriteLine($"Search results for 'laptop': {searchResults.Count}");

// Get products by price range
var priceRangeProducts = await productService.GetProductsByPriceAsync(500, 2000);
Console.WriteLine($"Products in price range $500-$2000: {priceRangeProducts.Count}");

// Deactivate product
var deactivatedProduct = await productService.DeactivateProductAsync(newProduct.Id);
Console.WriteLine($"Product deactivated: {deactivatedProduct.IsActive}");

// Get inventory value
var inventoryValue = await productService.GetInventoryValueAsync();
Console.WriteLine($"Total inventory value: {inventoryValue:C}");

// Get product count
var productCount = await productService.GetProductCountAsync();
Console.WriteLine($"Total active products: {productCount}");

// Dispose the service when done
await productService.DisposeAsync();
```

## OrderService

The `OrderService` provides order processing and management functionality for an e-commerce system. It handles order creation, item management, status transitions (pending → confirmed → shipped → delivered), cancellations, and revenue analytics. The service integrates with the `OrderRepository` and `OrderItemRepository` for data persistence and implements `IAsyncDisposable` for proper resource cleanup.

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create database context and order service
await using var dbContext = new DatabaseContext();
var orderService = new OrderService(dbContext);

// Create a new order for a user
var newOrder = await orderService.CreateOrderAsync(
    userId: 42,
    shippingAddress: "123 Main St, Springfield, IL 62704"
);
Console.WriteLine($"Order created: #{newOrder.OrderNumber} for user {newOrder.UserId}");

// Add items to the order
var addedItem = await orderService.AddOrderItemAsync(
    orderId: newOrder.Id,
    productId: 101,
    productName: "Gaming Laptop",
    quantity: 2,
    unitPrice: 1299.99m
);
Console.WriteLine($"Added item to order: {addedItem.Items.Count} items total");

// Confirm the order (transition from Pending to Confirmed)
var confirmedOrder = await orderService.ConfirmOrderAsync(newOrder.Id);
Console.WriteLine($"Order confirmed: Status = {confirmedOrder.Status}");

// Ship the order
var shippedOrder = await orderService.ShipOrderAsync(newOrder.Id);
Console.WriteLine($"Order shipped: Shipped on {shippedOrder.ShippingDate?.ToString("yyyy-MM-dd")}");

// Mark order as delivered
var deliveredOrder = await orderService.DeliverOrderAsync(newOrder.Id);
Console.WriteLine($"Order delivered: Delivered on {deliveredOrder.DeliveryDate?.ToString("yyyy-MM-dd")}");

// Get order by ID
var retrievedOrder = await orderService.GetOrderAsync(newOrder.Id);
if (retrievedOrder is not null)
{
    Console.WriteLine($"Found order #{retrievedOrder.OrderNumber} - Total: {retrievedOrder.TotalAmount:C}");
}

// Get all orders for a specific user
var userOrders = await orderService.GetUserOrdersAsync(42);
Console.WriteLine($"User 42 has {userOrders.Count} orders");

// Get orders by status
var pendingOrders = await orderService.GetOrdersByStatusAsync("Pending");
Console.WriteLine($"Pending orders: {pendingOrders.Count}");

// Get pending orders (convenience method)
var pending = await orderService.GetPendingOrdersAsync();
Console.WriteLine($"Pending orders via convenience method: {pending.Count}");

// Get orders within a date range
var dateRangeOrders = await orderService.GetOrdersByDateRangeAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow
);
Console.WriteLine($"Orders in last 30 days: {dateRangeOrders.Count}");

// Calculate total revenue from completed orders
var totalRevenue = await orderService.GetTotalRevenueAsync();
Console.WriteLine($"Total revenue: {totalRevenue:C}");

// Cancel an order
var cancelledOrder = await orderService.CancelOrderAsync(newOrder.Id);
Console.WriteLine($"Order cancelled: Status = {cancelledOrder.Status}");

// Dispose the service when done
await orderService.DisposeAsync();
```

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create database context and user service
await using var dbContext = new DatabaseContext();
var userService = new UserService(dbContext);

// Register a new user
var newUser = await userService.RegisterUserAsync(
    username: "johndoe",
    email: "john.doe@example.com",
    password: "SecurePassword123!"
);
Console.WriteLine($"User registered: {newUser.Username} (ID: {newUser.Id})");

// Authenticate user
var authenticatedUser = await userService.AuthenticateAsync(
    username: "johndoe",
    password: "SecurePassword123!"
);
if (authenticatedUser is not null)
{
    Console.WriteLine("Authentication successful");
    Console.WriteLine($"Last login: {authenticatedUser.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}");
}

// Get user by ID
var user = await userService.GetUserByIdAsync(newUser.Id);
if (user is not null)
{
    Console.WriteLine($"Found user: {user.Username}, Email: {user.Email}");
}

// Update user profile
var updatedUser = await userService.UpdateProfileAsync(
    userId: newUser.Id,
    firstName: "John",
    lastName: "Doe",
    phoneNumber: "+1-555-123-4567"
);
Console.WriteLine($"Profile updated: {updatedUser.FirstName} {updatedUser.LastName}");

// Change password
var passwordChanged = await userService.ChangePasswordAsync(
    userId: newUser.Id,
    currentPassword: "SecurePassword123!",
    newPassword: "NewSecurePassword456!"
);
Console.WriteLine($"Password changed: {passwordChanged}");

// Verify email
var emailVerified = await userService.VerifyEmailAsync(newUser.Id);
Console.WriteLine($"Email verified: {emailVerified}");

// Get active users count
var activeUsersCount = await userService.GetActiveUsersCountAsync();
Console.WriteLine($"Active users: {activeUsersCount}");

// Get inactive users
var inactiveUsers = await userService.GetInactiveUsersAsync(daysInactive: 30);
Console.WriteLine($"Inactive users: {inactiveUsers.Count}");

// Deactivate user
var deactivated = await userService.DeactivateUserAsync(newUser.Id);
Console.WriteLine($"User deactivated: {deactivated}");

// Dispose the service when done
await userService.DisposeAsync();
```

## INotificationService

The `INotificationService` interface provides a unified API for sending notifications through multiple channels including email, SMS, and push notifications. It supports asynchronous notification delivery, queuing for reliability, and template-based message composition. The service is designed for extensibility, allowing different notification providers to be plugged in without changing application code.

### Example Usage

```csharp
using DotnetMicroOrm.Services;

public class NotificationManager
{
    private readonly INotificationService _notificationService;

    public NotificationManager(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        // Send welcome email to new user
        await _notificationService.SendEmailAsync(
            to: email,
            subject: "Welcome to our platform!",
            body: $"Welcome {username}, thank you for registering!"
        );
    }

    public async Task SendOrderConfirmationAsync(string email, int orderNumber)
    {
        // Send order confirmation email
        await _notificationService.SendEmailAsync(
            to: email,
            subject: $"Order #{orderNumber} Confirmation",
            body: $"Your order #{orderNumber} has been received and is being processed."
        );
    }

    public async Task SendSmsAlertAsync(string phoneNumber, string message)
    {
        // Send SMS notification for urgent alerts
        await _notificationService.SendSmsAsync(
            phoneNumber: phoneNumber,
            message: message
        );
    }

    public async Task SendPushNotificationAsync(int userId, string title, string body)
    {
        // Send push notification to mobile app users
        await _notificationService.SendPushNotificationAsync(
            userId: userId,
            title: title,
            message: body
        );
    }

    public async Task ProcessPendingNotificationsAsync()
    {
        // Process all queued notifications (typically called from a background service)
        if (_notificationService is NotificationService notificationService)
        {
            await notificationService.ProcessQueueAsync();
        }
    }
}

// Usage with dependency injection
var notificationService = new NotificationService();

// Register templates for consistent messaging
notificationService.RegisterTemplate("welcome_email", "Welcome {UserName}! Your account is ready.");
notificationService.RegisterTemplate("order_confirmation", "Your order #{OrderNumber} has been confirmed.");

var manager = new NotificationManager(notificationService);

// Send notifications
await manager.SendWelcomeEmailAsync("user@example.com", "johndoe");
await manager.SendOrderConfirmationAsync("user@example.com", 12345);
await manager.SendSmsAlertAsync("+1-555-123-4567", "Your verification code is 123456");
await manager.SendPushNotificationAsync(42, "New Message", "You have a new message from Jane");

// Process queued notifications in background
await manager.ProcessPendingNotificationsAsync();
```

## AnalyticsService

The `AnalyticsService` tracks application metrics, events, and generates analytics reports. It provides real-time metric recording, event tracking, and historical aggregation capabilities with thread-safe operations. The service is useful for monitoring application performance, tracking business KPIs, and generating operational reports.

### Example Usage

```csharp
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Domain.Models;

public class AnalyticsDemo
{
    private readonly AnalyticsService _analytics = new();

    public void TrackApplicationMetrics()
    {
        // Record counter metrics
        _analytics.IncrementCounter("api_requests");
        _analytics.IncrementCounter("api_requests", 5); // Increment by 5
        
        // Record gauge metrics
        _analytics.RecordMetric("memory_usage_mb", 1250.5);
        _analytics.RecordMetric("memory_usage_mb", 1320.8, new Dictionary<string, string> { ["process"] = "web" });
        
        // Record custom events
        _analytics.RecordEvent("user_login", "User successfully authenticated");
        _analytics.RecordEvent("user_login", "User login failed - invalid password", 
            new Dictionary<string, object> { ["user_id"] = 42, ["ip_address"] = "192.168.1.100" });
        
        _analytics.RecordEvent("order_placed", "New order created", 
            new Dictionary<string, object> { ["order_id"] = 1001, ["amount"] = 99.99 });
    }

    public void GenerateAnalyticsReport()
    {
        // Get summary for a specific metric
        var requestSummary = _analytics.GetMetricSummary("api_requests");
        if (requestSummary != null)
        {
            Console.WriteLine($"API Requests - Total: {requestSummary.Count}, " +
                           $"Average: {requestSummary.Average:F2}, " +
                           $"Last: {requestSummary.LastValue}");
        }
        
        // Get all recorded metrics
        var allMetrics = _analytics.GetAllMetrics();
        Console.WriteLine($"Total metrics tracked: {allMetrics.Count}");
        
        // Get recent events
        var recentEvents = _analytics.GetRecentEvents(10);
        Console.WriteLine($"Last 10 events:");
        foreach (var e in recentEvents)
        {
            Console.WriteLine($"  [{e.Timestamp:T}] {e.Type}: {e.Description}");
        }
        
        // Generate a complete analytics report
        var report = _analytics.GenerateReport();
        Console.WriteLine($"\n{report.GetSummary()}");
        Console.WriteLine($"Metrics: {report.Metrics.Count}, Event Types: {report.EventTypes.Count}");
        
        // Get events by type
        var loginEvents = _analytics.GetEvents("user_login", 5);
        Console.WriteLine($"\nLast 5 login events:");
        foreach (var e in loginEvents)
        {
            Console.WriteLine($"  [{e.Timestamp:T}] {e.Description}");
        }
        
        // Clear data when needed
        // _analytics.ClearMetrics();
        // _analytics.ClearEvents();
    }
}
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

## QueryPlan

The `QueryPlan` class represents a cached query execution plan that stores metadata about SQL statements to avoid redundant parsing and optimization overhead. It tracks the normalized SQL fingerprint, original SQL text, estimated execution cost, query parameters, cache statistics, and timing information. Query plans are typically managed by the `IQueryPlanCache` interface for efficient plan reuse across identical queries.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using System.Data;

public class QueryPlanCacheExample
{
    private readonly IQueryPlanCache _queryPlanCache;

    public QueryPlanCacheExample(IQueryPlanCache queryPlanCache)
    {
        _queryPlanCache = queryPlanCache;
    }

    public async Task CacheAndRetrieveQueryPlanAsync()
    {
        // Create a query plan for a common query
        var queryPlan = new QueryPlan
        {
            Fingerprint = QueryPlanCache.ComputeFingerprint("SELECT * FROM users WHERE id = @id"),
            Sql = "SELECT * FROM users WHERE id = @id",
            EstimatedCost = 12.5,
            Parameters = new List<QueryParameterDescriptor>
            {
                new QueryParameterDescriptor
                {
                    Name = "@id",
                    DbType = DbType.Int32,
                    Size = null
                }
            },
            HitCount = 0
        };

        // Store the query plan in cache
        await _queryPlanCache.StorePlanAsync(queryPlan);

        // Retrieve the cached plan
        var fingerprint = QueryPlanCache.ComputeFingerprint("SELECT * FROM users WHERE id = @id");
        var cachedPlan = await _queryPlanCache.GetPlanAsync(fingerprint);

        if (cachedPlan != null)
        {
            Console.WriteLine($"Cached plan found: {cachedPlan.Fingerprint}");
            Console.WriteLine($"SQL: {cachedPlan.Sql}");
            Console.WriteLine($"Estimated cost: {cachedPlan.EstimatedCost}");
            Console.WriteLine($"Parameters: {cachedPlan.Parameters.Count}");
            Console.WriteLine($"Cached at: {cachedPlan.CachedAt}");
            Console.WriteLine($"Hit count: {cachedPlan.HitCount}");
        }

        // Get or analyze a query (cache hit or miss)
        var plan = await _queryPlanCache.GetOrAnalyzeAsync(
            "SELECT * FROM products WHERE category_id = @categoryId",
            async (sql, ct) =>
            {
                // Analyzer function that generates the query plan
                return new QueryPlan
                {
                    Fingerprint = QueryPlanCache.ComputeFingerprint(sql),
                    Sql = sql,
                    EstimatedCost = 8.2,
                    Parameters = new List<QueryParameterDescriptor>
                    {
                        new QueryParameterDescriptor
                        {
                            Name = "@categoryId",
                            DbType = DbType.Int32,
                            Size = null
                        }
                    }
                };
            }
        );

        // Check cache statistics
        var stats = await _queryPlanCache.GetStatisticsAsync();
        Console.WriteLine($"Cache entries: {stats.Entries}, Hits: {stats.Hits}, Misses: {stats.Misses}");
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

## QueryBuilder

The `QueryBuilder` class provides a fluent interface for constructing type-safe SQL queries with LINQ-like syntax. It enables building complex queries with filtering, sorting, pagination, and eager loading while maintaining strong typing throughout the query construction process. The builder supports async operations for executing queries against the database.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task SearchProductsAsync(string searchTerm, int page = 0, int pageSize = 10)
    {
        // Build a query with multiple conditions
        var products = await _productRepository.Query
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get total count for pagination
        var totalCount = await _productRepository.Query
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .CountAsync();

        Console.WriteLine($"Found {products.Count} products (Total: {totalCount})");
        foreach (var product in products)
        {
            Console.WriteLine($" - {product.Name} ({product.Price:C}) - {product.Category?.Name}");
        }
    }

    public async Task GetExpensiveProductsAsync()
    {
        // Query with ordering and pagination
        var expensiveProducts = await _productRepository.Query
            .Where(p => p.Price > 1000)
            .OrderByDescending(p => p.Price)
            .Take(5)
            .ToListAsync();

        Console.WriteLine("Top 5 most expensive products:");
        foreach (var product in expensiveProducts)
        {
            Console.WriteLine($" - {product.Name}: {product.Price:C}");
        }
    }

    public async Task<Product?> FindProductByIdAsync(int productId)
    {
        // Simple query with eager loading
        return await _productRepository.Query
            .Where(p => p.Id == productId)
            .Include(p => p.Category)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetProductCountAsync()
    {
        // Simple count query
        return await _productRepository.Query
            .CountAsync();
    }
}
```

## Repository

The `Repository<T>` class provides a generic data access layer for performing CRUD operations on entities. It implements the repository pattern to abstract database operations, supporting both synchronous and asynchronous operations with LINQ query capabilities. The repository works with `BaseEntity` types and provides methods for common data access patterns including filtering, sorting, pagination, and bulk operations.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly DatabaseContext _dbContext;

    public ProductService(IRepository<Product> productRepository, DatabaseContext dbContext)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
    }

    public async Task ManageProductsAsync()
    {
        // Create a new product
        var newProduct = new Product("LAP-001", "Gaming Laptop", 1299.99m, 5)
        {
            Description = "High-performance gaming laptop with RTX graphics",
            CostPrice = 950.00m,
            StockQuantity = 25,
            IsActive = true
        };

        // Add a product using repository
        var addedProduct = await _productRepository.AddAsync(newProduct);
        Console.WriteLine($"Added product: {addedProduct.Name} (ID: {addedProduct.Id})");

        // Get a product by ID
        var retrievedProduct = await _productRepository.GetByIdAsync(addedProduct.Id);
        if (retrievedProduct is not null)
        {
            Console.WriteLine($"Retrieved product: {retrievedProduct.Name} - {retrievedProduct.Price:C}");
        }

        // Update a product
        retrievedProduct!.Price = 1399.99m;
        var updatedProduct = await _productRepository.UpdateAsync(retrievedProduct);
        Console.WriteLine($"Updated product price to: {updatedProduct.Price:C}");

        // Check if a product exists
        var exists = await _productRepository.ExistsAsync(p => p.Sku == "LAP-001");
        Console.WriteLine($"Product exists: {exists}");

        // Get all products (with optional filtering)
        var allProducts = await _productRepository.GetAllAsync();
        Console.WriteLine($"Total products: {allProducts.Count}");

        // Get first matching product
        var firstExpensiveProduct = await _productRepository.FirstOrDefaultAsync(
            p => p.Price > 1000,
            p => p.OrderByDescending(x => x.Price)
        );
        Console.WriteLine($"First expensive product: {firstExpensiveProduct?.Name}");

        // Count products matching criteria
        var expensiveCount = await _productRepository.CountAsync(p => p.Price > 1000);
        Console.WriteLine($"Products over $1000: {expensiveCount}");

        // Get paged results
        var pagedResults = await _productRepository.GetPagedAsync(
            pageIndex: 0,
            pageSize: 10,
            orderBy: p => p.OrderBy(x => x.Name)
        );
        Console.WriteLine($"Page 1: {pagedResults.Count} products");

        // Get paged results with total count
        var (items, totalCount) = await _productRepository.GetPagedWithCountAsync(
            pageIndex: 0,
            pageSize: 10,
            orderBy: p => p.OrderBy(x => x.Name)
        );
        Console.WriteLine($"Total items: {totalCount}, Returned: {items.Count}");

        // Use IQueryable for complex queries
        var query = _productRepository.Query
            .Where(p => p.IsActive)
            .OrderBy(p => p.CategoryId)
            .ThenBy(p => p.Name);

        var activeProducts = await query.ToListAsync();
        Console.WriteLine($"Active products: {activeProducts.Count}");

        // Bulk operations
        var productsToAdd = new List<Product>
        {
            new Product("MOB-001", "Gaming Mouse", 79.99m, 10),
            new Product("KBD-001", "Mechanical Keyboard", 129.99m, 8)
        };

        var addedProducts = await _productRepository.AddRangeAsync(productsToAdd);
        Console.WriteLine($"Added {addedProducts.Count} products in bulk");

        // Delete a product
        var deleteSuccess = await _productRepository.DeleteAsync(addedProduct.Id);
        Console.WriteLine($"Delete successful: {deleteSuccess}");
    }
}
```

## DatabaseContext

The `DatabaseContext` class manages database connections and provides low-level SQL execution capabilities. It handles connection lifecycle, transaction management, and raw SQL command execution with support for both scalar queries and streaming result sets. The context supports multiple database providers (SQL Server and SQLite) and provides methods for executing queries, managing transactions, and testing database connectivity.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Constants;

public class DatabaseOperations
{
    private readonly DatabaseContext _dbContext;

    public DatabaseOperations(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteDatabaseOperationsAsync()
    {
        // Get database provider and connection string
        var provider = _dbContext.GetDatabaseProvider();
        var connectionString = _dbContext.GetConnectionString();
        Console.WriteLine($"Using {provider} database provider");

        // Test database connection
        var connectionSuccessful = await _dbContext.TestConnectionAsync();
        Console.WriteLine($"Database connection test: {(connectionSuccessful ? "Success" : "Failed")}");

        // Execute a scalar query (e.g., count)
        var countResult = await _dbContext.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM products WHERE is_active = @isActive",
            new Dictionary<string, object> { ["@isActive"] = true }
        );
        Console.WriteLine($"Active products count: {countResult}");

        // Execute a query returning multiple rows
        var products = await _dbContext.ExecuteQueryAsync(
            "SELECT id, name, price, stock_quantity FROM products WHERE price > @minPrice ORDER BY price DESC",
            new Dictionary<string, object> { ["@minPrice"] = 500 }
        );

        foreach (var product in products)
        {
            Console.WriteLine($"Product: {product["name"]} - {product["price"]:C} (Stock: {product["stock_quantity"]})");
        }

        // Execute a non-query command (INSERT/UPDATE/DELETE)
        var rowsAffected = await _dbContext.ExecuteNonQueryAsync(
            "UPDATE products SET price = @newPrice WHERE id = @productId",
            new Dictionary<string, object> { ["@newPrice"] = 1399.99m, ["@productId"] = 1 }
        );
        Console.WriteLine($"Rows updated: {rowsAffected}");

        // Begin a transaction
        var transactionStarted = await _dbContext.BeginTransactionAsync(TransactionIsolationLevel.ReadCommitted);
        Console.WriteLine($"Transaction started: {transactionStarted}");

        try
        {
            // Execute multiple commands in transaction
            await _dbContext.ExecuteNonQueryAsync(
                "UPDATE products SET stock_quantity = stock_quantity - @quantity WHERE id = @productId",
                new Dictionary<string, object> { ["@quantity"] = 2, ["@productId"] = 1 }
            );

            await _dbContext.ExecuteNonQueryAsync(
                "INSERT INTO order_items (product_id, quantity, unit_price) VALUES (@productId, @quantity, @unitPrice)",
                new Dictionary<string, object> { ["@productId"] = 1, ["@quantity"] = 2, ["@unitPrice"] = 1399.99m }
            );

            // Commit transaction
            var committed = await _dbContext.CommitAsync();
            Console.WriteLine($"Transaction committed: {committed}");
        }
        catch (Exception ex)
        {
            // Rollback on error
            await _dbContext.RollbackAsync();
            Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
        }

        // Stream large result sets without loading all data into memory
        await foreach (var row in _dbContext.ExecuteStreamAsync(
            "SELECT id, name, price FROM products WHERE is_active = @isActive ORDER BY price DESC",
            new Dictionary<string, object> { ["@isActive"] = true }
        ))
        {
            Console.WriteLine($"Streamed product: {row["name"]} - {row["price"]:C}");
        }

        // Close connection when done
        await _dbContext.CloseAsync();
    }
}
```

## Specification

The `Specification` class provides a flexible way to build reusable, composable query specifications for filtering, sorting, and paginating data. It supports complex queries with criteria expressions, eager loading of related entities, and configurable pagination. The pattern is commonly used with repositories to implement the Specification pattern for clean separation of query logic from business logic.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create a specification to find active users
var activeUsersSpec = new Specification<User>()
    .Where(u => u.IsActive)
    .Include(u => u.Orders)
    .OrderBy(u => u.Username)
    .Take(10);

// Create a specification for products in a price range with eager loading
var expensiveProductsSpec = new Specification<Product>()
    .Where(p => p.Price >= 500 && p.Price <= 2000)
    .Include(p => p.Category)
    .OrderByDescending(p => p.Price)
    .Skip(20)
    .Take(10);

// Create a specification to find users by email domain
var gmailUsersSpec = new Specification<User>()
    .Where(u => u.Email.EndsWith("@gmail.com"))
    .IncludeString("Orders")
    .OrderBy(u => u.LastLoginDate)
    .Page(2, 50);

// Usage with repository
public class UserRepository
{
    private readonly DatabaseContext _dbContext;

    public UserRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        var spec = new Specification<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username);

        return await _dbContext.Query<User>()
            .Where(spec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var spec = new Specification<Product>()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .Include(p => p.Category)
            .OrderByDescending(p => p.Price);

        return await _dbContext.Query<Product>()
            .Where(spec.Criteria)
            .Include(spec.Includes)
            .ToListAsync();
    }
}

// Predefined specifications for common queries
var activeUsers = new ActiveUsersSpecification();
var userById = new UserByIdSpecification(42);
var productsByPrice = new ProductsByPriceRangeSpecification(100, 1000);
var lowStockProducts = new LowStockProductsSpecification(5);
```

## PreparedStatementPoolOptions

The `PreparedStatementPoolOptions` class provides configuration settings for the `PreparedStatementPool` system, which caches prepared SQL statements to eliminate redundant `DbCommand` construction overhead on high-frequency query paths. The pool uses an LRU (Least Recently Used) eviction policy based on the configured `MaxPoolSize`, automatically removing least-used entries when the pool reaches capacity.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using Microsoft.Extensions.DependencyInjection;

// Configure prepared statement pooling with dependency injection
var services = new ServiceCollection();

// Configure pool options
services.Configure<PreparedStatementPoolOptions>(options =>
{
    options.MaxPoolSize = 500; // Maximum number of statements to cache
});

// Register the pool service
services.AddSingleton<IPreparedStatementPool, PreparedStatementPool>();

var serviceProvider = services.BuildServiceProvider();

// Resolve and use the pool
var pool = serviceProvider.GetRequiredService<IPreparedStatementPool>();

// Borrow a prepared statement from the pool
var entry = await pool.BorrowAsync("SELECT * FROM users WHERE id = @id");

if (entry != null)
{
    // Use the prepared statement
    Console.WriteLine($"Reusing prepared statement: {entry.StatementKey}");
    
    // Return the statement to the pool when done
    await pool.ReturnAsync(entry);
}
else
{
    // Statement not in pool, create new one
    Console.WriteLine("Prepared statement not found in pool, creating new one");
}

// Get pool statistics
var stats = await pool.GetPoolStatsAsync();
Console.WriteLine($"Pool size: {stats.PoolSize}, Hit ratio: {stats.HitRatio:P1}");

// Release a statement from the pool
await pool.ReleaseAsync("SELECT * FROM users WHERE id = @id");
```

## QueryPlanCacheOptions

The `QueryPlanCacheOptions` class provides configuration settings for the `QueryPlanCache` system, which caches parsed SQL query execution plans to avoid redundant parsing and optimization overhead. The cache uses an LRU (Least Recently Used) eviction policy based on the configured capacity, and each plan has a configurable TTL (Time to Live) for automatic expiration.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure query plan caching with dependency injection
var services = new ServiceCollection();

// Configure cache options
services.Configure<QueryPlanCacheOptions>(options =>
{
    options.Capacity = 1000;           // Maximum number of plans to cache (LRU eviction)
    options.DefaultTtl = TimeSpan.FromHours(1); // Default expiration for cached plans
});

// Register the cache service
services.AddSingleton<IQueryPlanCache, QueryPlanCache>();
services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve and use the cache
var cache = serviceProvider.GetRequiredService<IQueryPlanCache>();

// Store a query plan with default TTL (1 hour)
await cache.StorePlanAsync(new QueryPlan
{
    Fingerprint = QueryPlanCache.ComputeFingerprint("SELECT * FROM users WHERE id = @id"),
    Sql = "SELECT * FROM users WHERE id = @id",
    Parameters = new Dictionary<string, object> { ["@id"] = 42 },
    HitCount = 0
});

// Retrieve a cached plan
var fingerprint = QueryPlanCache.ComputeFingerprint("SELECT * FROM users WHERE id = @id");
var cachedPlan = await cache.GetPlanAsync(fingerprint);

// Get or analyze a query (cache hit or miss)
var plan = await cache.GetOrAnalyzeAsync(
    "SELECT * FROM products WHERE category_id = @categoryId",
    async (sql, ct) => 
    {
        // Analyzer function that generates the query plan
        return new QueryPlan
        {
            Fingerprint = QueryPlanCache.ComputeFingerprint(sql),
            Sql = sql,
            HitCount = 0
        };
    }
);

// Check cache statistics
var stats = await cache.GetStatisticsAsync();
Console.WriteLine($"Cache entries: {stats.Entries}, Hits: {stats.Hits}, Misses: {stats.Misses}");

// Clear the cache when needed
await cache.ClearAsync();
```

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

// Create a specification to find active users
var activeUsersSpec = new Specification<User>()
    .Where(u => u.IsActive)
    .Include(u => u.Orders)
    .OrderBy(u => u.Username)
    .Take(10);

// Create a specification for products in a price range with eager loading
var expensiveProductsSpec = new Specification<Product>()
    .Where(p => p.Price >= 500 && p.Price <= 2000)
    .Include(p => p.Category)
    .OrderByDescending(p => p.Price)
    .Skip(20)
    .Take(10);

// Create a specification to find users by email domain
var gmailUsersSpec = new Specification<User>()
    .Where(u => u.Email.EndsWith("@gmail.com"))
    .IncludeString("Orders")
    .OrderBy(u => u.LastLoginDate)
    .Page(2, 50);

// Usage with repository
public class UserRepository
{
    private readonly DatabaseContext _dbContext;

    public UserRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        var spec = new Specification<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username);

        return await _dbContext.Query<User>()
            .Where(spec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var spec = new Specification<Product>()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .Include(p => p.Category)
            .OrderByDescending(p => p.Price);

        return await _dbContext.Query<Product>()
            .Where(spec.Criteria)
            .Include(spec.Includes)
            .ToListAsync();
    }
}

// Predefined specifications for common queries
var activeUsers = new ActiveUsersSpecification();
var userById = new UserByIdSpecification(42);
var productsByPrice = new ProductsByPriceRangeSpecification(100, 1000);
var lowStockProducts = new LowStockProductsSpecification(5);
```

## SpecificationCombinators

The `SpecificationCombinators` class provides extension methods for combining `Specification<T>` instances using logical operators (AND, OR, NOT). These combinators enable building complex query specifications by composing simpler ones, supporting reusable and maintainable query logic. The class handles criteria combination, parameter rebinding, and includes propagation automatically.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetExpensiveElectronicsAsync()
    {
        // Create base specifications
        var expensiveSpec = new Specification<Product>()
            .Where(p => p.Price > 1000);

        var electronicsSpec = new Specification<Product>()
            .Where(p => p.Category.Name == "Electronics");

        // Combine specifications using AND
        var expensiveElectronicsSpec = expensiveSpec.And(electronicsSpec);

        // Use the combined specification with repository
        return await _productRepository.Query
            .Where(expensiveElectronicsSpec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetAffordableOrClearanceAsync()
    {
        // Create base specifications
        var affordableSpec = new Specification<Product>()
            .Where(p => p.Price <= 200);

        var clearanceSpec = new Specification<Product>()
            .Where(p => p.IsOnSale);

        // Combine specifications using OR
        var affordableOrClearanceSpec = affordableSpec.Or(clearanceSpec);

        return await _productRepository.Query
            .Where(affordableOrClearanceSpec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetNonElectronicsAsync()
    {
        // Create base specification
        var electronicsSpec = new Specification<Product>()
            .Where(p => p.Category.Name == "Electronics");

        // Negate the specification using NOT
        var nonElectronicsSpec = electronicsSpec.Not();

        return await _productRepository.Query
            .Where(nonElectronicsSpec.Criteria)
            .ToListAsync();
    }
}
```

## UnitOfWork

The `UnitOfWork` class implements the Unit of Work pattern for managing transactions and coordinating changes across multiple repositories. It provides transaction management (begin, commit, rollback), change tracking, and centralized repository access, ensuring that all operations within a transaction succeed or fail together. The class implements `IAsyncDisposable` for proper resource cleanup and supports concurrent repository access through a thread-safe repository cache.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Constants;

public class OrderProcessingService : IAsyncDisposable
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;

    public OrderProcessingService(DatabaseContext dbContext)
    {
        _unitOfWork = new UnitOfWork(dbContext);
        _productRepository = _unitOfWork.Repository<Product>();
        _orderRepository = _unitOfWork.Repository<Order>();
    }

    public async Task ProcessOrderAsync(int productId, int quantity, int userId, string shippingAddress)
    {
        // Begin transaction
        var transactionStarted = await _unitOfWork.BeginTransactionAsync(TransactionIsolationLevel.ReadCommitted);
        Console.WriteLine($"Transaction started: {transactionStarted}");

        try
        {
            // Get product and check stock
            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null || product.StockQuantity < quantity)
            {
                throw new InvalidOperationException("Product not available or insufficient stock");
            }

            // Create order
            var order = new Order(userId, shippingAddress)
            {
                Status = "Pending"
            };

            // Add order items
            order.AddItem(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price,
                CreatedDate = DateTime.UtcNow
            });

            // Add to repository (tracked by UnitOfWork)
            await _orderRepository.AddAsync(order);

            // Decrease product stock
            product.DecreaseStock(quantity);
            await _productRepository.UpdateAsync(product);

            // Save changes
            var changesSaved = await _unitOfWork.SaveChangesAsync();
            Console.WriteLine($"Changes saved: {changesSaved}");

            // Commit transaction
            var committed = await _unitOfWork.CommitAsync();
            Console.WriteLine($"Transaction committed: {committed}");
        }
        catch (Exception ex)
        {
            // Rollback on error
            Console.WriteLine($"Error processing order: {ex.Message}");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
    }
}

// Usage example
await using var orderService = new OrderProcessingService(dbContext);
await orderService.ProcessOrderAsync(
    productId: 101,
    quantity: 2,
    userId: 42,
    shippingAddress: "123 Main St, Springfield"
);
```

public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetExpensiveElectronicsAsync()
    {
        // Create base specifications
        var expensiveSpec = new Specification<Product>()
            .Where(p => p.Price > 1000);

        var electronicsSpec = new Specification<Product>()
            .Where(p => p.Category.Name == "Electronics");

        // Combine specifications using AND
        var expensiveElectronicsSpec = expensiveSpec.And(electronicsSpec);

        // Use the combined specification with repository
        return await _productRepository.Query
            .Where(expensiveElectronicsSpec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetAffordableOrClearanceAsync()
    {
        // Create base specifications
        var affordableSpec = new Specification<Product>()
            .Where(p => p.Price <= 200);

        var clearanceSpec = new Specification<Product>()
            .Where(p => p.IsOnSale);

        // Combine specifications using OR
        var affordableOrClearanceSpec = affordableSpec.Or(clearanceSpec);

        return await _productRepository.Query
            .Where(affordableOrClearanceSpec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetNonElectronicsAsync()
    {
        // Create base specification
        var electronicsSpec = new Specification<Product>()
            .Where(p => p.Category.Name == "Electronics");

        // Negate the specification using NOT
        var nonElectronicsSpec = electronicsSpec.Not();

        return await _productRepository.Query
            .Where(nonElectronicsSpec.Criteria)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        // Create base specification
        var priceRangeSpec = new Specification<Product>()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice);

        // Combine with category specification using AND
        var resultSpec = priceRangeSpec
            .And(new Specification<Product>().Where(p => p.IsActive))
            .And(new Specification<Product>().Where(p => p.StockQuantity > 0));

        return await _productRepository.Query
            .Where(resultSpec.Criteria)
            .Include(p => p.Category)
            .ToListAsync();
    }
}
```

## PagedResult

The `PagedResult<T>` class represents a paginated result set with metadata for navigation. It provides efficient data retrieval with pagination support, including total count, page information, and navigation methods. The class is commonly used for implementing list endpoints with pagination capabilities.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class ProductService
{
    private readonly IRepository<Product> _productRepository;

    public ProductService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<Product>> GetActiveProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        // Create paged result from queryable
        var pagedResult = PagedResult<Product>.FromQueryable(
            _productRepository.Query
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name),
            pageNumber, pageSize);

        Console.WriteLine($"Page {pagedResult.PageNumber} of {pagedResult.TotalPages}");
        Console.WriteLine($"Total products: {pagedResult.TotalCount}");
        Console.WriteLine($"Items on this page: {pagedResult.Items.Count}");
        Console.WriteLine($"Has next page: {pagedResult.HasNextPage}");
        Console.WriteLine($"Has previous page: {pagedResult.HasPreviousPage}");

        return pagedResult;
    }

    public async Task<PagedResult<Product>> SearchProductsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        // Get paged results with custom ordering
        var pagedResult = PagedResult<Product>.FromQueryable(
            _productRepository.Query
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .OrderByDescending(p => p.Price),
            pageNumber, pageSize);

        return pagedResult;
    }

    public async Task<PagedResult<ProductDto>> GetProductDtosAsync(int pageNumber = 1, int pageSize = 10)
    {
        // Get paged results and map to DTO
        var pagedResult = PagedResult<Product>.FromQueryable(
            _productRepository.Query
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name),
            pageNumber, pageSize);

        // Map to DTO type while preserving pagination metadata
        return pagedResult.Map(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        });
    }

    public async Task NavigatePagesAsync(int currentPage, int pageSize = 10)
    {
        // Get first page
        var firstPage = PagedResult<Product>.FromQueryable(
            _productRepository.Query.Where(p => p.IsActive),
            1, pageSize);

        Console.WriteLine($"First page: {firstPage.PageNumber}");

        // Navigate to next page if available
        if (firstPage.HasNextPage)
        {
            var nextPageInfo = firstPage.GetNextPageInfo();
            var nextPage = PagedResult<Product>.FromQueryable(
                _productRepository.Query
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name),
                nextPageInfo.PageNumber, nextPageInfo.PageSize);

            Console.WriteLine($"Next page: {nextPage.PageNumber}");
        }

        // Navigate to previous page if available
        if (firstPage.HasPreviousPage)
        {
            var prevPageInfo = firstPage.GetPreviousPageInfo();
            var prevPage = PagedResult<Product>.FromQueryable(
                _productRepository.Query
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name),
                prevPageInfo.PageNumber, prevPageInfo.PageSize);

            Console.WriteLine($"Previous page: {prevPage.PageNumber}");
        }
    }
}

// DTO for mapping
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

## BatchUpsertOperation

The `BatchUpsertOperation<T>` class provides an efficient batch upsert (INSERT or UPDATE) implementation that minimizes database round-trips. It generates optimized SQL statements (MERGE for SQL Server, or individual upserts for other providers) to perform bulk operations on entities while tracking whether each entity was inserted or updated.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

public class ProductService
{
    private readonly IDatabaseContext _dbContext;

    public ProductService(IDatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpdateProductPricesAsync(List<Product> products)
    {
        // Create batch upsert operation
        var upsertOperation = new BatchUpsertOperation<Product>(_dbContext);

        // Upsert a single entity
        var result = await upsertOperation.UpsertAsync(
            new Product { Id = 1, Name = "Updated Product", Price = 99.99m, StockQuantity = 100 },
            p => p.Id
        );

        Console.WriteLine($"Entity was inserted: {result.WasInserted}");

        // Upsert multiple entities in batches
        var batchResults = await upsertOperation.UpsertRangeAsync(
            products,
            p => p.Id,  // Primary key selector
            batchSize: 50  // Optional: override default batch size
        );

        // Process results
        foreach (var batchResult in batchResults)
        {
            Console.WriteLine($"Entity {batchResult.Entity.Id}: {(batchResult.WasInserted ? "Inserted" : "Updated")}");
        }
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

## User

The `User` class represents a system user entity with authentication and profile information. It stores essential user data including credentials, contact information, account status flags, and navigation properties for related orders. The class provides validation logic, account lifecycle methods, and supports ORM mapping via attributes.

### Example Usage

```csharp
using DotnetMicroOrm.Domain.Models;

// Create a new user
var user = new User("johndoe", "john@example.com", "$2a$12$hashedpassword12345678901234")
{
    FirstName = "John",
    LastName = "Doe",
    PhoneNumber = "+1-555-123-4567",
    IsActive = true,
    IsEmailVerified = false,
    CreatedDate = DateTime.UtcNow,
    Version = 1
};

// Validate user data before saving
if (user.Validate(out var errors))
{
    Console.WriteLine("User is valid");
}
else
{
    Console.WriteLine("Validation errors: " + string.Join(", ", errors));
}

// Get user's full name
var fullName = user.GetFullName();
Console.WriteLine($"User: {fullName}");

// Mark email as verified after verification
user.MarkAsEmailVerified();

// Update last login timestamp
user.UpdateLastLogin();

// Deactivate user account
user.Deactivate();

// Add a related order
var order = new Order(user.Id, "123 Main St, City");
user.Orders = [order];

// Display user information
Console.WriteLine($"User ID: {user.Id}");
Console.WriteLine($"Username: {user.Username}");
Console.WriteLine($"Email: {user.Email}");
Console.WriteLine($"Full Name: {user.GetFullName()}");
Console.WriteLine($"Status: {(user.IsActive ? "Active" : "Inactive")}");
Console.WriteLine($"Email Verified: {(user.IsEmailVerified ? "Yes" : "No")}");
Console.WriteLine($"Last Login: {user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}");
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
}

## IMiddleware

The `IMiddleware` interface defines the contract for middleware components in the request processing pipeline. Middleware implementations handle cross-cutting concerns such as logging, authentication, authorization, error handling, and request/response transformation. Middleware components are executed in order of their `Order` property, allowing for flexible composition of processing steps.

Middleware implementations receive a `MiddlewareContext` containing request/response data, authenticated user information, timing metadata, and custom state. The `InvokeAsync` method receives a continuation delegate (`next`) that invokes the next middleware in the pipeline, enabling both pre-processing and post-processing behavior.

### Example Usage

```csharp
using DotnetMicroOrm.Middleware;
using System.Net.Http;

// Create a custom middleware that validates requests
public class RequestValidationMiddleware : IMiddleware
{
    public int Order => 50; // Execute after auth but before business logic

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        // Validate request data before processing
        if (context.RequestData == null)
        {
            context.ResponseData = new {
                Success = false,
                Error = "Request data is required"
            };
            context.IsHandled = true; // Short-circuit the pipeline
            return;
        }

        // Log request start
        Console.WriteLine($"[{context.RequestId}] Starting {context.Operation}");
        
        // Call next middleware in the pipeline
        await next(context);
        
        // Log completion and add response metadata
        if (context.ResponseData != null)
        {
            Console.WriteLine($"[{context.RequestId}] Completed in {context.ElapsedTime.TotalMilliseconds}ms");
        }
    }
}

// Create a middleware pipeline with authentication and validation
var authMiddleware = new AuthenticationMiddleware();
authMiddleware.RegisterApiKey("secure-key-123", 42, "admin");

var validationMiddleware = new RequestValidationMiddleware();

var pipelineBuilder = new PipelineBuilder();
pipelineBuilder.Use(authMiddleware);
pipelineBuilder.Use(validationMiddleware);

// Create a request context
var requestContext = new MiddlewareContext
{
    RequestId = Guid.NewGuid().ToString(),
    Operation = "GetUserProfile",
    RequestData = new { UserId = 42 },
    Metadata = new Dictionary<string, object> {
        { "request_source", "api-gateway" }
    }
};

// Execute the pipeline
await pipelineBuilder.ExecuteAsync(requestContext);

// Access results
if (requestContext.ResponseData is not null)
{
    Console.WriteLine($"Response: {requestContext.ResponseData}");
}

// Access authenticated user information
if (requestContext.User != null)
{
    Console.WriteLine($"Authenticated as: {requestContext.User.Username} ({requestContext.User.Role})");
}
```

The `AuthenticationMiddleware` provides API authentication and authorization functionality. It validates API keys, authenticates users, and populates `AuthenticationInfo` in the middleware context for downstream handlers. The middleware supports both API key authentication (via `Bearer` or `ApiKey` tokens) and role-based authorization checks.




### Features
- API key authentication with user ID and role mapping
- Role-based access control methods (`HasRole`, `HasAnyRole`)
- API key management (register and revoke)
- Middleware pipeline integration with configurable execution order

### Example Usage

```csharp
using DotnetMicroOrm.Middleware;

// Create authentication middleware
var authMiddleware = new AuthenticationMiddleware();

// Register API keys for users
// In production, load these from secure storage instead of hardcoding
authMiddleware.RegisterApiKey("prod-key-xyz123", 1001, "admin");
authMiddleware.RegisterApiKey("prod-key-abc456", 1002, "user");

// Check user roles (static methods)
var adminUser = new AuthenticationInfo { UserId = 1001, Role = "admin" };
var regularUser = new AuthenticationInfo { UserId = 1002, Role = "user" };

bool isAdmin = AuthenticationMiddleware.HasRole(adminUser, "admin"); // true
bool hasPermission = AuthenticationMiddleware.HasAnyRole(regularUser, "admin", "user"); // true

// Use in middleware pipeline
var pipelineBuilder = new PipelineBuilder();
pipelineBuilder.Use(authMiddleware);

// Revoke API keys when needed
authMiddleware.RevokeApiKey("demo-key-12345");
```