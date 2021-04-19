#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Demonstrates batch operations (bulk insert, update, delete) for high performance.
    /// Batch operations are 10-20x faster than individual operations.
    /// </summary>
    public sealed class BatchOperationsExample
    {
        private readonly IServiceProvider _serviceProvider;

        public BatchOperationsExample(string connectionString)
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
            Console.WriteLine("=== Batch Operations Example ===\n");

            var repository = _serviceProvider.GetRequiredService<IRepository<Product>>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Batch Insert
                Console.WriteLine("1. Batch Insert (1000 products)...");
                var products = GenerateProducts(1000);

                var stopwatch = Stopwatch.StartNew();
                await repository.AddRangeAsync(products);
                await unitOfWork.SaveChangesAsync();
                stopwatch.Stop();

                Console.WriteLine($"✓ Inserted 1000 products in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Throughput: {1000 / (stopwatch.Elapsed.TotalSeconds):F0} ops/sec\n");

                // Batch Update
                Console.WriteLine("2. Batch Update (applying 10% discount)...");
                var spec = new Specification<Product>().Take(500);
                var productsToUpdate = await repository.GetAsync(spec);

                stopwatch.Restart();
                foreach (var product in productsToUpdate)
                {
                    product.Price *= 0.9m; // 10% discount
                }
                await repository.UpdateRangeAsync(productsToUpdate);
                await unitOfWork.SaveChangesAsync();
                stopwatch.Stop();

                Console.WriteLine($"✓ Updated {productsToUpdate.Count} products in {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"  Throughput: {productsToUpdate.Count / (stopwatch.Elapsed.TotalSeconds):F0} ops/sec\n");

                // Batch Delete
                Console.WriteLine("3. Batch Delete (removing out-of-stock items)...");
                var outOfStockSpec = new Specification<Product>().Where(p => p.StockQuantity == 0);
                var outOfStock = await repository.GetAsync(outOfStockSpec);

                if (outOfStock.Any())
                {
                    stopwatch.Restart();
                    await repository.DeleteRangeAsync(outOfStock);
                    await unitOfWork.SaveChangesAsync();
                    stopwatch.Stop();

                    Console.WriteLine($"✓ Deleted {outOfStock.Count} products in {stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine($"  Throughput: {outOfStock.Count / (stopwatch.Elapsed.TotalSeconds):F0} ops/sec\n");
                }
                else
                {
                    Console.WriteLine("✓ No out-of-stock items to delete\n");
                }

                // Show statistics
                var totalCount = await repository.CountAsync(new Specification<Product>());
                var avgPrice = await GetAveragePriceAsync(repository);

                Console.WriteLine("4. Summary Statistics");
                Console.WriteLine($"  Total products: {totalCount}");
                Console.WriteLine($"  Average price: ${avgPrice:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private List<Product> GenerateProducts(int count)
        {
            var products = new List<Product>(count);
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                products.Add(new Product
                {
                    Name = $"Product {i + 1}",
                    Description = $"Description for product {i + 1}",
                    Price = (decimal)(random.NextDouble() * 1000),
                    StockQuantity = random.Next(0, 100)
                });
            }

            return products;
        }

        private async Task<decimal> GetAveragePriceAsync(IRepository<Product> repository)
        {
            var spec = new Specification<Product>();
            var products = await repository.GetAsync(spec);
            return products.Count > 0 ? products.Average(p => p.Price) : 0;
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new BatchOperationsExample(connectionString);
            await example.RunAsync();
        }
    }
}
