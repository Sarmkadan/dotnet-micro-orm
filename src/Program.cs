// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Services;

namespace DotnetMicroOrm;

/// <summary>
/// Main entry point demonstrating ORM usage
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Example connection string for SQL Server
        var connectionString = "Server=localhost;Database=MicroOrmDemo;User Id=sa;Password=YourPassword123;";

        // Initialize database context
        var dbContext = new DatabaseContext(connectionString, DatabaseProvider.SqlServer);

        try
        {
            // Test connection
            Console.WriteLine("Testing database connection...");
            var isConnected = await dbContext.TestConnectionAsync();
            if (!isConnected)
            {
                Console.WriteLine("Failed to connect to database");
                return;
            }
            Console.WriteLine("✓ Database connection successful");

            // Demonstrate user service
            await DemonstrateUserService(dbContext);

            // Demonstrate product service
            await DemonstrateProductService(dbContext);

            // Demonstrate order service
            await DemonstrateOrderService(dbContext);

            // Demonstrate audit service
            await DemonstrateAuditService(dbContext);

            Console.WriteLine("\n✓ All demonstrations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }
        finally
        {
            await dbContext.DisposeAsync();
        }
    }

    static async Task DemonstrateUserService(IDatabaseContext dbContext)
    {
        Console.WriteLine("\n--- User Service Demo ---");
        using var userService = new UserService(dbContext);

        try
        {
            // Register user
            var user = await userService.RegisterUserAsync(
                "john_doe",
                "john@example.com",
                "SecurePassword123");
            Console.WriteLine($"✓ Registered user: {user.Username} ({user.Email})");

            // Get user
            var retrievedUser = await userService.GetUserByIdAsync(user.Id);
            if (retrievedUser != null)
                Console.WriteLine($"✓ Retrieved user: {retrievedUser.GetFullName()}");

            // Authenticate
            var authenticated = await userService.AuthenticateAsync("john_doe", "SecurePassword123");
            if (authenticated != null)
                Console.WriteLine($"✓ User authenticated successfully");

            // Update profile
            await userService.UpdateProfileAsync(user.Id, "John", "Doe", "+1-555-0123");
            Console.WriteLine($"✓ User profile updated");

            // Get active users count
            var activeCount = await userService.GetActiveUsersCountAsync();
            Console.WriteLine($"✓ Active users: {activeCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ User service error: {ex.Message}");
        }
    }

    static async Task DemonstrateProductService(IDatabaseContext dbContext)
    {
        Console.WriteLine("\n--- Product Service Demo ---");
        using var productService = new ProductService(dbContext);

        try
        {
            // Create product
            var product = await productService.CreateProductAsync(
                "PROD-001",
                "Laptop Computer",
                999.99m,
                1,
                "High-performance laptop for professionals");
            Console.WriteLine($"✓ Created product: {product.Name} (${product.Price})");

            // Get product
            var retrieved = await productService.GetProductAsync(product.Id);
            if (retrieved != null)
                Console.WriteLine($"✓ Retrieved product: {retrieved.Sku}");

            // Update stock
            await productService.IncreaseStockAsync(product.Id, 50);
            Console.WriteLine($"✓ Updated stock: +50 units");

            // Get active products
            var active = await productService.GetActiveProductsAsync();
            Console.WriteLine($"✓ Total active products: {active.Count}");

            // Search products
            var searched = await productService.SearchProductsAsync("Laptop");
            Console.WriteLine($"✓ Search results: {searched.Count} product(s) found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Product service error: {ex.Message}");
        }
    }

    static async Task DemonstrateOrderService(IDatabaseContext dbContext)
    {
        Console.WriteLine("\n--- Order Service Demo ---");
        using var orderService = new OrderService(dbContext);

        try
        {
            // Create order
            var order = await orderService.CreateOrderAsync(
                1,
                "123 Main Street, Springfield, IL 62701");
            Console.WriteLine($"✓ Created order: {order.OrderNumber}");

            // Add item to order
            var updatedOrder = await orderService.AddOrderItemAsync(
                order.Id,
                1,
                "Laptop Computer",
                1,
                999.99m);
            Console.WriteLine($"✓ Added item to order");

            // Confirm order
            var confirmed = await orderService.ConfirmOrderAsync(order.Id);
            Console.WriteLine($"✓ Order confirmed: {confirmed.Status}");

            // Ship order
            var shipped = await orderService.ShipOrderAsync(order.Id);
            Console.WriteLine($"✓ Order shipped: {shipped.Status}");

            // Get user orders
            var orders = await orderService.GetUserOrdersAsync(1);
            Console.WriteLine($"✓ User orders: {orders.Count}");

            // Get total revenue
            var revenue = await orderService.GetTotalRevenueAsync();
            Console.WriteLine($"✓ Total revenue: ${revenue:N2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Order service error: {ex.Message}");
        }
    }

    static async Task DemonstrateAuditService(IDatabaseContext dbContext)
    {
        Console.WriteLine("\n--- Audit Service Demo ---");
        using var auditService = new AuditService(dbContext);

        try
        {
            // Log insert
            await auditService.LogInsertAsync("User", 1, "{\"Name\":\"John Doe\"}", 1, "admin");
            Console.WriteLine($"✓ Logged insert operation");

            // Log update
            await auditService.LogUpdateAsync(
                "Product", 1,
                "{\"Stock\":50}",
                "{\"Stock\":55}",
                "Stock",
                1, "admin");
            Console.WriteLine($"✓ Logged update operation");

            // Get summary
            var summary = await auditService.GetSummaryAsync();
            Console.WriteLine($"✓ Audit summary:");
            Console.WriteLine($"  - Total operations: {summary.TotalOperations}");
            Console.WriteLine($"  - Successful: {summary.SuccessfulOperations}");
            Console.WriteLine($"  - Failed: {summary.FailedOperations}");
            Console.WriteLine($"  - Success rate: {summary.SuccessRate:F2}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Audit service error: {ex.Message}");
        }
    }
}
