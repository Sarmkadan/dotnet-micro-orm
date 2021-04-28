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
    /// Demonstrates basic Create, Read, Update, Delete operations with DotnetMicroOrm.
    /// </summary>
    public class sealed BasicCRUDExample
    {
        private readonly IServiceProvider _serviceProvider;

        public BasicCRUDExample(string connectionString)
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
            Console.WriteLine("=== Basic CRUD Example ===\n");

            var repository = _serviceProvider.GetRequiredService<IRepository<Product>>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // CREATE
                Console.WriteLine("1. Creating products...");
                var product1 = new Product
                {
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99m,
                    StockQuantity = 10
                };

                var product2 = new Product
                {
                    Name = "Mouse",
                    Description = "Wireless mouse",
                    Price = 29.99m,
                    StockQuantity = 50
                };

                await repository.AddAsync(product1);
                await repository.AddAsync(product2);
                await unitOfWork.SaveChangesAsync();

                Console.WriteLine($"✓ Created 2 products\n");

                // READ - Single
                Console.WriteLine("2. Reading single product...");
                var fetched = await repository.GetByIdAsync(product1.Id);
                Console.WriteLine($"✓ Found: {fetched.Name} - ${fetched.Price}\n");

                // READ - All
                Console.WriteLine("3. Reading all products...");
                var spec = new Specification<Product>().OrderBy(p => p.Name);
                var allProducts = await repository.GetAsync(spec);
                Console.WriteLine($"✓ Total products: {allProducts.Count}");
                foreach (var p in allProducts)
                {
                    Console.WriteLine($"  - {p.Name}: ${p.Price} ({p.StockQuantity} in stock)");
                }
                Console.WriteLine();

                // UPDATE
                Console.WriteLine("4. Updating product...");
                fetched.Price = 899.99m;
                fetched.StockQuantity = 15;
                await repository.UpdateAsync(fetched);
                await unitOfWork.SaveChangesAsync();
                Console.WriteLine($"✓ Updated {fetched.Name}: ${fetched.Price}\n");

                // QUERY with specification
                Console.WriteLine("5. Querying with specification...");
                var expensiveSpec = new Specification<Product>()
                    .Where(p => p.Price > 100)
                    .OrderByDescending(p => p.Price);
                var expensive = await repository.GetAsync(expensiveSpec);
                Console.WriteLine($"✓ Products over $100: {expensive.Count}");
                foreach (var p in expensive)
                {
                    Console.WriteLine($"  - {p.Name}: ${p.Price}");
                }
                Console.WriteLine();

                // COUNT
                Console.WriteLine("6. Counting records...");
                var countSpec = new Specification<Product>().Where(p => p.StockQuantity > 20);
                var count = await repository.CountAsync(countSpec);
                Console.WriteLine($"✓ Products with >20 in stock: {count}\n");

                // DELETE
                Console.WriteLine("7. Deleting product...");
                await repository.DeleteAsync(product2.Id);
                await unitOfWork.SaveChangesAsync();
                Console.WriteLine($"✓ Deleted {product2.Name}\n");

                // Verify deletion
                var finalCount = await repository.CountAsync(new Specification<Product>());
                Console.WriteLine($"8. Final product count: {finalCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new BasicCRUDExample(connectionString);
            await example.RunAsync();
        }
    }
}
