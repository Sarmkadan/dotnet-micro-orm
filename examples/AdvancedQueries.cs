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
    /// Demonstrates advanced query patterns and filtering strategies.
    /// </summary>
    public sealed class AdvancedQueriesExample
    {
        private readonly IServiceProvider _serviceProvider;

        public AdvancedQueriesExample(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDatabaseContext(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseType = DatabaseType.SqlServer;
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMemoryCache();

            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("=== Advanced Queries Example ===\n");

            var repository = _serviceProvider.GetRequiredService<IRepository<Product>>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Seed data
                await SeedDataAsync(repository, unitOfWork);

                // Complex filtering
                await DemonstrateComplexFilteringAsync(repository);
                Console.WriteLine();

                // Pagination
                await DemonstratePaginationAsync(repository);
                Console.WriteLine();

                // Sorting and grouping
                await DemonstrateSortingAsync(repository);
                Console.WriteLine();

                // Counting and existence checks
                await DemonstrateCountingAsync(repository);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private async Task DemonstrateComplexFilteringAsync(IRepository<Product> repository)
        {
            Console.WriteLine("1. Complex Filtering\n");

            // Multi-condition WHERE clause
            var spec = new Specification<Product>()
                .Where(p => p.Price > 100)
                .Where(p => p.StockQuantity > 0)
                .Where(p => p.Name.Contains("Pro"));

            var results = await repository.GetAsync(spec);
            Console.WriteLine($"✓ Products matching all conditions: {results.Count}");
            foreach (var p in results.Take(5))
            {
                Console.WriteLine($"  - {p.Name}: ${p.Price}");
            }

            // OR conditions
            var orSpec = new Specification<Product>()
                .Where(p => p.Price > 1000)
                .OrWhere(p => p.StockQuantity > 500);

            var orResults = await repository.GetAsync(orSpec);
            Console.WriteLine($"\n✓ Products (expensive OR high stock): {orResults.Count}");
        }

        private async Task DemonstratePaginationAsync(IRepository<Product> repository)
        {
            Console.WriteLine("2. Pagination\n");

            const int pageSize = 10;
            var totalCount = await repository.CountAsync(new Specification<Product>());
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            Console.WriteLine($"  Total products: {totalCount}");
            Console.WriteLine($"  Page size: {pageSize}");
            Console.WriteLine($"  Total pages: {totalPages}\n");

            // Get first 3 pages
            for (int pageNumber = 1; pageNumber <= Math.Min(3, totalPages); pageNumber++)
            {
                var spec = new Specification<Product>()
                    .OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                var pageResults = await repository.GetAsync(spec);

                Console.WriteLine($"  Page {pageNumber}:");
                foreach (var p in pageResults)
                {
                    Console.WriteLine($"    - {p.Name}");
                }
            }
        }

        private async Task DemonstrateSortingAsync(IRepository<Product> repository)
        {
            Console.WriteLine("3. Sorting and Ordering\n");

            // Single sort
            var ascSpec = new Specification<Product>()
                .OrderBy(p => p.Price)
                .Take(5);

            var cheapest = await repository.GetAsync(ascSpec);
            Console.WriteLine("✓ Cheapest 5 products:");
            foreach (var p in cheapest)
            {
                Console.WriteLine($"  - {p.Name}: ${p.Price}");
            }

            // Multiple sort criteria
            Console.WriteLine("\n✓ Most expensive by stock availability:");
            var descSpec = new Specification<Product>()
                .OrderByDescending(p => p.Price)
                .Take(5);

            var mostExpensive = await repository.GetAsync(descSpec);
            foreach (var p in mostExpensive)
            {
                Console.WriteLine($"  - {p.Name}: ${p.Price} ({p.StockQuantity} stock)");
            }
        }

        private async Task DemonstrateCountingAsync(IRepository<Product> repository)
        {
            Console.WriteLine("4. Counting and Existence Checks\n");

            // Total count
            var totalCount = await repository.CountAsync(new Specification<Product>());
            Console.WriteLine($"✓ Total products: {totalCount}");

            // Conditional count
            var inStockCount = await repository.CountAsync(
                new Specification<Product>().Where(p => p.StockQuantity > 0));
            Console.WriteLine($"✓ In-stock products: {inStockCount}");

            var expensiveCount = await repository.CountAsync(
                new Specification<Product>().Where(p => p.Price > 500));
            Console.WriteLine($"✓ Products over $500: {expensiveCount}");

            // Existence check
            var hasExpensive = await repository.AnyAsync(
                new Specification<Product>().Where(p => p.Price > 1000));
            Console.WriteLine($"✓ Has products over $1000: {(hasExpensive ? "Yes" : "No")}");

            // First or default
            var firstExpensive = await repository.FirstOrDefaultAsync(
                new Specification<Product>()
                    .Where(p => p.Price > 500)
                    .OrderByDescending(p => p.Price));

            if (firstExpensive is not null)
            {
                Console.WriteLine($"✓ First expensive product: {firstExpensive.Name}");
            }
        }

        private async Task SeedDataAsync(IRepository<Product> repository, IUnitOfWork unitOfWork)
        {
            var existing = await repository.CountAsync(new Specification<Product>());
            if (existing > 0)
                return;

            var products = new List<Product>
            {
                new Product { Name = "Laptop Pro", Price = 1499.99m, StockQuantity = 5 },
                new Product { Name = "Laptop Standard", Price = 899.99m, StockQuantity = 15 },
                new Product { Name = "Desktop Pro", Price = 1999.99m, StockQuantity = 3 },
                new Product { Name = "Wireless Mouse", Price = 29.99m, StockQuantity = 100 },
                new Product { Name = "Mechanical Keyboard", Price = 149.99m, StockQuantity = 25 },
                new Product { Name = "USB Hub", Price = 39.99m, StockQuantity = 0 },
                new Product { Name = "Monitor 27\"", Price = 349.99m, StockQuantity = 8 },
                new Product { Name = "Monitor 34\"", Price = 799.99m, StockQuantity = 2 },
                new Product { Name = "Headphones", Price = 199.99m, StockQuantity = 12 },
                new Product { Name = "Webcam", Price = 79.99m, StockQuantity = 30 },
                new Product { Name = "Desk Lamp", Price = 49.99m, StockQuantity = 20 },
                new Product { Name = "Phone Stand", Price = 19.99m, StockQuantity = 50 },
            };

            foreach (var product in products)
            {
                product.Description = $"High-quality {product.Name}";
            }

            await repository.AddRangeAsync(products);
            await unitOfWork.SaveChangesAsync();
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new AdvancedQueriesExample(connectionString);
            await example.RunAsync();
        }
    }
}
