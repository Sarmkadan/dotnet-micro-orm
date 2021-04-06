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
using DotnetMicroOrm.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Demonstrates entity validation before persistence.
    /// Shows how to validate business rules and constraints.
    /// </summary>
    public class sealed ValidationExample
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationExample(string connectionString)
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
            Console.WriteLine("=== Validation Example ===\n");

            var repository = _serviceProvider.GetRequiredService<IRepository<Product>>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Valid product
                await DemonstrateValidProductAsync(repository, unitOfWork);
                Console.WriteLine();

                // Invalid products
                await DemonstrateInvalidProductsAsync(repository, unitOfWork);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private async Task DemonstrateValidProductAsync(
            IRepository<Product> repository,
            IUnitOfWork unitOfWork)
        {
            Console.WriteLine("1. Valid Product Creation\n");

            var product = new Product
            {
                Name = "Gaming Laptop",
                Description = "High-performance gaming laptop with RTX 4090",
                Price = 2499.99m,
                StockQuantity = 5
            };

            var validationResult = ValidateProduct(product);
            if (validationResult.IsValid)
            {
                await repository.AddAsync(product);
                await unitOfWork.SaveChangesAsync();
                Console.WriteLine("✓ Product created successfully");
                Console.WriteLine($"  - Name: {product.Name}");
                Console.WriteLine($"  - Price: ${product.Price}");
                Console.WriteLine($"  - Stock: {product.StockQuantity}");
            }
            else
            {
                Console.WriteLine("✗ Validation failed:");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }

        private async Task DemonstrateInvalidProductsAsync(
            IRepository<Product> repository,
            IUnitOfWork unitOfWork)
        {
            Console.WriteLine("2. Invalid Product Examples\n");

            var invalidProducts = new[]
            {
                new { Product = new Product { Name = "", Price = 99.99m, StockQuantity = 10 }, Issue = "Empty name" },
                new { Product = new Product { Name = "Product", Price = -50m, StockQuantity = 10 }, Issue = "Negative price" },
                new { Product = new Product { Name = "Product", Price = 99.99m, StockQuantity = -5 }, Issue = "Negative stock" },
                new { Product = new Product { Name = "X", Price = 99.99m, StockQuantity = 10 }, Issue = "Name too short" },
            };

            foreach (var item in invalidProducts)
            {
                var validationResult = ValidateProduct(item.Product);
                Console.WriteLine($"  Case: {item.Issue}");

                if (validationResult.IsValid)
                {
                    Console.WriteLine("    ✗ Unexpectedly valid");
                }
                else
                {
                    Console.WriteLine("    ✓ Correctly rejected");
                    foreach (var error in validationResult.Errors)
                    {
                        Console.WriteLine($"      Reason: {error}");
                    }
                }
                Console.WriteLine();
            }
        }

        private ValidationResult ValidateProduct(Product product)
        {
            var errors = new List<string>();

            // Name validation
            if (string.IsNullOrWhiteSpace(product.Name))
                errors.Add("Product name is required");

            if (!string.IsNullOrEmpty(product.Name) && product.Name.Length < 2)
                errors.Add("Product name must be at least 2 characters long");

            if (!string.IsNullOrEmpty(product.Name) && product.Name.Length > 255)
                errors.Add("Product name cannot exceed 255 characters");

            // Price validation
            if (product.Price <= 0)
                errors.Add("Product price must be greater than zero");

            if (product.Price > 1000000)
                errors.Add("Product price seems unreasonably high (max $1,000,000)");

            // Stock quantity validation
            if (product.StockQuantity < 0)
                errors.Add("Stock quantity cannot be negative");

            if (product.StockQuantity > 1000000)
                errors.Add("Stock quantity seems unreasonably high (max 1,000,000)");

            // Description validation
            if (string.IsNullOrEmpty(product.Description))
                errors.Add("Product description is recommended");

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; }
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new ValidationExample(connectionString);
            await example.RunAsync();
        }
    }
}
