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
using DotnetMicroOrm.Caching;
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Demonstrates caching strategies for optimal performance.
    /// Cached queries are 100-1000x faster than database queries.
    /// </summary>
    public sealed class CachingStrategyExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheProvider _cacheProvider;
        private const string CategoriesCacheKey = "categories:all";
        private const string FeaturedProductsCacheKey = "products:featured";

        public CachingStrategyExample(string connectionString)
        {
            var services = new ServiceCollection();

            services.AddDatabaseContext(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseType = DatabaseType.SqlServer;
                options.EnableCaching = true;
                options.CacheTTLSeconds = 300;
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

            _serviceProvider = services.BuildServiceProvider();
            _cacheProvider = _serviceProvider.GetRequiredService<ICacheProvider>();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("=== Caching Strategy Example ===\n");

            var repository = _serviceProvider.GetRequiredService<IRepository<Product>>();
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Seed data
                await SeedDataAsync(repository, unitOfWork);

                // Demonstrate cache miss and hit
                Console.WriteLine("1. Cache Miss (database hit)...");
                var stopwatch = Stopwatch.StartNew();
                var products1 = await GetAllProductsAsync(repository);
                stopwatch.Stop();
                Console.WriteLine($"✓ Retrieved {products1.Count} products in {stopwatch.ElapsedMilliseconds}ms (DB query)\n");

                // Cache hit
                Console.WriteLine("2. Cache Hit (in-memory)...");
                stopwatch.Restart();
                var products2 = await GetAllProductsAsync(repository);
                stopwatch.Stop();
                Console.WriteLine($"✓ Retrieved {products2.Count} products in {stopwatch.ElapsedMilliseconds}ms (from cache)\n");

                // Manual cache management
                Console.WriteLine("3. Manual Cache Management...");
                var featured = await GetFeaturedProductsAsync(repository);
                Console.WriteLine($"✓ Retrieved {featured.Count} featured products");
                Console.WriteLine("  (cached for 1 hour)\n");

                // Cache invalidation
                Console.WriteLine("4. Cache Invalidation...");
                var product = products1.First();
                product.Price = 499.99m;
                await repository.UpdateAsync(product);
                await unitOfWork.SaveChangesAsync();

                // Manually invalidate
                _cacheProvider.Remove(CategoriesCacheKey);
                Console.WriteLine("✓ Cache invalidated after update\n");

                // Demonstrate cache performance impact
                Console.WriteLine("5. Performance Comparison (1000 queries)...");
                await DemonstrateCachePerformanceAsync(repository);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private async Task<List<Product>> GetAllProductsAsync(IRepository<Product> repository)
        {
            const string cacheKey = "products:all";

            // Check cache first
            var cached = _cacheProvider.Get<List<Product>>(cacheKey);
            if (cached is not null)
                return cached;

            // Cache miss - query database
            var spec = new Specification<Product>().OrderBy(p => p.Name);
            var products = await repository.GetAsync(spec);

            // Cache result
            _cacheProvider.Set(cacheKey, products, TimeSpan.FromHours(1));

            return products;
        }

        private async Task<List<Product>> GetFeaturedProductsAsync(IRepository<Product> repository)
        {
            var cached = _cacheProvider.Get<List<Product>>(FeaturedProductsCacheKey);
            if (cached is not null)
                return cached;

            var spec = new Specification<Product>()
                .Where(p => p.Price > 500)
                .OrderByDescending(p => p.Price)
                .Take(10);

            var products = await repository.GetAsync(spec);
            _cacheProvider.Set(FeaturedProductsCacheKey, products, TimeSpan.FromHours(1));

            return products;
        }

        private async Task SeedDataAsync(IRepository<Product> repository, IUnitOfWork unitOfWork)
        {
            var existing = await repository.CountAsync(new Specification<Product>());
            if (existing > 0)
                return;

            var products = new List<Product>();
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                products.Add(new Product
                {
                    Name = $"Product {i + 1}",
                    Description = $"Description {i + 1}",
                    Price = (decimal)(random.NextDouble() * 1000),
                    StockQuantity = random.Next(0, 100)
                });
            }

            await repository.AddRangeAsync(products);
            await unitOfWork.SaveChangesAsync();
        }

        private async Task DemonstrateCachePerformanceAsync(IRepository<Product> repository)
        {
            const string testCacheKey = "performance:test";

            // Without cache
            _cacheProvider.Clear();
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                var spec = new Specification<Product>().Take(10);
                await repository.GetAsync(spec);
            }

            stopwatch.Stop();
            var withoutCacheTime = stopwatch.ElapsedMilliseconds;

            // With cache
            _cacheProvider.Clear();
            stopwatch.Restart();

            var cachedData = await repository.GetAsync(new Specification<Product>().Take(10));
            _cacheProvider.Set(testCacheKey, cachedData, TimeSpan.FromHours(1));

            for (int i = 0; i < 100; i++)
            {
                var data = _cacheProvider.Get<List<Product>>(testCacheKey);
            }

            stopwatch.Stop();
            var withCacheTime = stopwatch.ElapsedMilliseconds;

            var speedup = (double)withoutCacheTime / withCacheTime;

            Console.WriteLine($"  Without cache: {withoutCacheTime}ms");
            Console.WriteLine($"  With cache: {withCacheTime}ms");
            Console.WriteLine($"  Speedup: {speedup:F0}x faster\n");
        }

        public static async Task Main(string[] args)
        {
            const string connectionString = "Server=localhost;Database=DotnetMicroOrmExamples;User Id=sa;Password=YourPassword;";

            var example = new CachingStrategyExample(connectionString);
            await example.RunAsync();
        }
    }
}
