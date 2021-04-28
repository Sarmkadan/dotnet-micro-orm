#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Comprehensive e-commerce example showing real-world usage patterns.
    /// Demonstrates inventory management, order processing, and reporting.
    /// </summary>
    public class sealed ECommerceExample
    {
        private readonly IServiceProvider _serviceProvider;

        public ECommerceExample(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDatabaseContext(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseType = DatabaseType.SqlServer;
                options.EnableChangeTracking = true;
                options.EnableCaching = true;
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMemoryCache();

            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("=== E-Commerce Inventory System Example ===\n");

            try
            {
                // Setup
                var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
                var productRepo = unitOfWork.Repository<Product>();
                var orderRepo = unitOfWork.Repository<Order>();

                // Initialize data
                await InitializeInventoryAsync(productRepo, unitOfWork);
                Console.WriteLine();

                // Browse products
                await BrowseProductsAsync(productRepo);
                Console.WriteLine();

                // Search products
                await SearchProductsAsync(productRepo);
                Console.WriteLine();

                // Place order
                await PlaceOrderAsync(productRepo, orderRepo, unitOfWork);
                Console.WriteLine();

                // Generate reports
                await GenerateReportsAsync(productRepo, orderRepo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private async Task InitializeInventoryAsync(IRepository<Product> repository, IUnitOfWork unitOfWork)
        {
            Console.WriteLine("1. Initializing Inventory\n");

            var existing = await repository.CountAsync(new Specification<Product>());
            if (existing > 0)
            {
                Console.WriteLine("✓ Inventory already initialized");
                return;
            }

            var products = new List<Product>
            {
                new Product
                {
                    Name = "MacBook Pro 16\"",
                    Description = "Professional laptop with M3 Max chip",
                    Price = 3499.99m,
                    StockQuantity = 15
                },
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Latest flagship smartphone",
                    Price = 1199.99m,
                    StockQuantity = 50
                },
                new Product
                {
                    Name = "iPad Air",
                    Description = "Versatile tablet computer",
                    Price = 599.99m,
                    StockQuantity = 30
                },
                new Product
                {
                    Name = "AirPods Pro",
                    Description = "Premium wireless earbuds",
                    Price = 249.99m,
                    StockQuantity = 100
                },
                new Product
                {
                    Name = "Apple Watch Series 9",
                    Description = "Smartwatch with health features",
                    Price = 399.99m,
                    StockQuantity = 45
                },
            };

            foreach (var product in products)
            {
                product.CreatedAt = DateTime.UtcNow;
            }

            await repository.AddRangeAsync(products);
            await unitOfWork.SaveChangesAsync();

            Console.WriteLine($"✓ Added {products.Count} products to inventory\n");
        }

        private async Task BrowseProductsAsync(IRepository<Product> repository)
        {
            Console.WriteLine("2. Browsing Products\n");

            var spec = new Specification<Product>()
                .OrderByDescending(p => p.Price)
                .Take(10);

            var products = await repository.GetAsync(spec);

            Console.WriteLine("✓ Top 10 Products by Price:");
            foreach (var product in products)
            {
                Console.WriteLine($"  - {product.Name}");
                Console.WriteLine($"    Price: ${product.Price}");
                Console.WriteLine($"    Stock: {product.StockQuantity} units");
            }
        }

        private async Task SearchProductsAsync(IRepository<Product> repository)
        {
            Console.WriteLine("3. Searching Products\n");

            // Search for products in price range
            var spec = new Specification<Product>()
                .Where(p => p.Price >= 200 && p.Price <= 1000)
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Price);

            var results = await repository.GetAsync(spec);

            Console.WriteLine("✓ Products in $200-$1000 range (in stock):");
            foreach (var product in results)
            {
                Console.WriteLine($"  - {product.Name}: ${product.Price}");
            }

            // Count results
            var count = await repository.CountAsync(spec);
            Console.WriteLine($"\nTotal matching: {count}");
        }

        private async Task PlaceOrderAsync(
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository,
            IUnitOfWork unitOfWork)
        {
            Console.WriteLine("4. Processing Order\n");

            try
            {
                await unitOfWork.BeginTransactionAsync();

                // Get product
                var product = await productRepository.FirstOrDefaultAsync(
                    new Specification<Product>().Where(p => p.Name == "iPhone 15 Pro"));

                if (product is null)
                    throw new Exception("Product not found");

                // Check stock
                if (product.StockQuantity < 1)
                    throw new Exception("Product out of stock");

                // Create order
                var order = new Order
                {
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await orderRepository.AddAsync(order);

                // Update inventory
                product.StockQuantity -= 1;
                await productRepository.UpdateAsync(product);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                Console.WriteLine("✓ Order placed successfully");
                Console.WriteLine($"  Product: {product.Name}");
                Console.WriteLine($"  Remaining stock: {product.StockQuantity}");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                Console.WriteLine($"✗ Order failed: {ex.Message}");
            }
        }

        private async Task GenerateReportsAsync(
            IRepository<Product> productRepository,
            IRepository<Order> orderRepository)
        {
            Console.WriteLine("5. Inventory Reports\n");

            // Stock report
            var totalCount = await productRepository.CountAsync(new Specification<Product>());
            var inStock = await productRepository.CountAsync(
                new Specification<Product>().Where(p => p.StockQuantity > 0));
            var outOfStock = totalCount - inStock;

            Console.WriteLine("✓ Stock Summary:");
            Console.WriteLine($"  Total SKUs: {totalCount}");
            Console.WriteLine($"  In Stock: {inStock}");
            Console.WriteLine($"  Out of Stock: {outOfStock}");

            // Low stock report
            var lowStockSpec = new Specification<Product>()
                .Where(p => p.StockQuantity > 0 && p.StockQuantity < 20)
                .OrderBy(p => p.StockQuantity);

            var lowStock = await productRepository.GetAsync(lowStockSpec);

            if (lowStock.Any())
            {
                Console.WriteLine("\n✓ Low Stock Alerts (< 20 units):");
                foreach (var product in lowStock)
                {
                    Console.WriteLine($"  - {product.Name}: {product.StockQuantity} units");
                }
            }

            // Value report
            var allProducts = await productRepository.GetAsync(new Specification<Product>());
            var totalValue = allProducts.Sum(p => p.Price * p.StockQuantity);

            Console.WriteLine($"\n✓ Inventory Value: ${totalValue:F2}");

            // Order count
            var orderCount = await orderRepository.CountAsync(new Specification<Order>());
            Console.WriteLine($"✓ Total Orders: {orderCount}");
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new ECommerceExample(connectionString);
            await example.RunAsync();
        }
    }
}
