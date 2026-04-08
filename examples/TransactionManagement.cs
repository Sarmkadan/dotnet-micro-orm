#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Demonstrates transaction management and ACID compliance.
    /// Shows how to handle rollback and maintain data integrity.
    /// </summary>
    public class sealed TransactionManagementExample
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionManagementExample(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDatabaseContext(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseType = DatabaseType.SqlServer;
                options.EnableChangeTracking = true;
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMemoryCache();

            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("=== Transaction Management Example ===\n");

            try
            {
                // Successful transaction
                await DemonstrateSuccessfulTransactionAsync();
                Console.WriteLine();

                // Failed transaction with rollback
                await DemonstrateFailedTransactionAsync();
                Console.WriteLine();

                // Transaction isolation
                await DemonstrateTransactionIsolationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private async Task DemonstrateSuccessfulTransactionAsync()
        {
            Console.WriteLine("1. Successful Transaction");
            Console.WriteLine("   (create product and category)...\n");

            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            var productRepo = unitOfWork.Repository<Product>();
            var categoryRepo = unitOfWork.Repository<Category>();

            try
            {
                await unitOfWork.BeginTransactionAsync();

                // Create category
                var category = new Category
                {
                    Name = "Electronics",
                    Description = "Electronic devices"
                };
                await categoryRepo.AddAsync(category);

                // Create product
                var product = new Product
                {
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99m,
                    StockQuantity = 10
                };
                await productRepo.AddAsync(product);

                // Commit
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                Console.WriteLine("✓ Transaction committed successfully");
                Console.WriteLine("  - Category created");
                Console.WriteLine("  - Product created");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                Console.WriteLine($"✗ Transaction rolled back: {ex.Message}");
            }
        }

        private async Task DemonstrateFailedTransactionAsync()
        {
            Console.WriteLine("2. Failed Transaction with Rollback\n");

            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            var productRepo = unitOfWork.Repository<Product>();

            try
            {
                await unitOfWork.BeginTransactionAsync();

                // Create product
                var product1 = new Product
                {
                    Name = "Mouse",
                    Description = "Wireless mouse",
                    Price = 29.99m,
                    StockQuantity = 50
                };
                await productRepo.AddAsync(product1);

                // Simulate error condition
                throw new InvalidOperationException("Insufficient inventory for associated items");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                Console.WriteLine("✓ Transaction rolled back due to error");
                Console.WriteLine($"  Reason: {ex.Message}");
                Console.WriteLine("  All changes discarded");
            }
        }

        private async Task DemonstrateTransactionIsolationAsync()
        {
            Console.WriteLine("3. Transaction Isolation Levels\n");

            Console.WriteLine("  Default: ReadCommitted");
            Console.WriteLine("  - Prevents dirty reads");
            Console.WriteLine("  - Allows non-repeatable reads");
            Console.WriteLine("  - Allows phantom reads\n");

            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            var productRepo = unitOfWork.Repository<Product>();

            try
            {
                // Demonstrate transaction with multiple operations
                await unitOfWork.BeginTransactionAsync();

                var product = new Product
                {
                    Name = "Keyboard",
                    Description = "Mechanical keyboard",
                    Price = 79.99m,
                    StockQuantity = 25
                };

                await productRepo.AddAsync(product);

                // Check current state in transaction
                var allProducts = await productRepo.GetAsync(new Specification<Product>());
                Console.WriteLine($"  Products in transaction: {allProducts.Count}");

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                Console.WriteLine("✓ Transaction completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new TransactionManagementExample(connectionString);
            await example.RunAsync();
        }
    }
}
