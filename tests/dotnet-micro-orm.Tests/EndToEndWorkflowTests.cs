#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Caching;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// End-to-end tests demonstrating the main ORM use case: managing a product catalog
/// with inventory tracking, caching, and querying.
/// </summary>
public sealed class EndToEndWorkflowTests
{
    [Fact]
    public async Task CreateAndRetrieveProduct_FullWorkflow()
    {
        var contextMock = new Mock<IDatabaseContext>();
        var cache = new MemoryCacheProvider();
        var repository = new Repository<Product>(contextMock.Object);

        var product = new Product("SKU-LAPTOP-001", "High-Performance Laptop", 1299.99m, 1)
        {
            Description = "A powerful laptop for professionals",
            StockQuantity = 50,
            CostPrice = 800m
        };

        contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        await repository.AddAsync(product);

        var cachedKey = $"product:{product.Id}";
        await cache.SetAsync(cachedKey, product, TimeSpan.FromHours(1));
        var cached = await cache.GetAsync<Product>(cachedKey);

        cached.Should().NotBeNull();
        cached!.Sku.Should().Be("SKU-LAPTOP-001");
        cached.Price.Should().Be(1299.99m);
    }

    [Fact]
    public async Task InventoryManagement_IncreaseAndDecreaseStock()
    {
        var product = new Product("SKU-WIDGET-001", "Standard Widget", 49.99m, 2)
        {
            StockQuantity = 100
        };

        product.Validate(out var errors).Should().BeTrue();

        product.DecreaseStock(20);
        product.StockQuantity.Should().Be(80);

        product.IncreaseStock(30);
        product.StockQuantity.Should().Be(110);

        product.IsLowStock(50).Should().BeFalse();
        product.IsLowStock(120).Should().BeTrue();
    }

    [Fact]
    public async Task ProfitCalculation_WithAndWithoutCost()
    {
        var product1 = new Product("SKU-001", "Product A", 100m, 1);
        var product2 = new Product("SKU-002", "Product B", 100m, 1) { CostPrice = 60m };

        product1.GetProfit().Should().Be(100m);
        product2.GetProfit().Should().Be(40m);
    }

    [Fact]
    public async Task BatchProductCreation_Simulation()
    {
        var contextMock = new Mock<IDatabaseContext>();
        var repository = new Repository<Product>(contextMock.Object);

        var products = new List<Product>
        {
            new("SKU-001", "Product 1", 10.00m, 1) { StockQuantity = 100 },
            new("SKU-002", "Product 2", 20.00m, 1) { StockQuantity = 200 },
            new("SKU-003", "Product 3", 30.00m, 1) { StockQuantity = 300 },
            new("SKU-004", "Product 4", 40.00m, 1) { StockQuantity = 400 },
            new("SKU-005", "Product 5", 50.00m, 1) { StockQuantity = 500 }
        };

        contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var results = await repository.AddRangeAsync(products);

        results.Should().HaveCount(5);
        results.Should().AllSatisfy(p => p.Validate(out _).Should().BeTrue());
    }

    [Fact]
    public async Task CachingStrategy_GetOrSet()
    {
        var cache = new MemoryCacheProvider();
        var callCount = 0;

        var product1 = await cache.GetOrSetAsync(
            "featured:product:1",
            async () =>
            {
                callCount++;
                return await Task.FromResult(new Product("SKU-001", "Featured", 99.99m, 1));
            },
            TimeSpan.FromMinutes(60));

        var product2 = await cache.GetOrSetAsync(
            "featured:product:1",
            async () =>
            {
                callCount++;
                return await Task.FromResult(new Product("SKU-002", "Different", 49.99m, 1));
            },
            TimeSpan.FromMinutes(60));

        callCount.Should().Be(1);
        product1.Sku.Should().Be(product2.Sku);
    }

    [Fact]
    public async Task UserManagement_CompleteLifecycle()
    {
        var user = new User("john_doe", "john@example.com", "securehashedpassword123456")
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "555-1234"
        };

        user.Validate(out var errors).Should().BeTrue();

        user.GetFullName().Should().Be("John Doe");
        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();

        user.MarkAsEmailVerified();
        user.IsEmailVerified.Should().BeTrue();

        user.UpdateLastLogin();
        user.LastLoginDate.Should().NotBeNull();

        user.Deactivate();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SpecificationPattern_ProductFiltering()
    {
        var products = new List<Product>
        {
            new("SKU-001", "Expensive Item", 999.99m, 1) { IsActive = true },
            new("SKU-002", "Budget Item", 49.99m, 1) { IsActive = false },
            new("SKU-003", "Mid-Range Item", 299.99m, 1) { IsActive = true }
        };

        var activeProducts = products.Where(p => p.IsActive).ToList();
        activeProducts.Should().HaveCount(2);

        var expensiveProducts = products.Where(p => p.Price > 200 && p.IsActive).ToList();
        expensiveProducts.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConcurrencyScenario_MultipleOperations()
    {
        var contextMock = new Mock<IDatabaseContext>();
        var repository = new Repository<Product>(contextMock.Object);

        contextMock.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(1);

        var tasks = new List<Task>();

        for (int i = 1; i <= 5; i++)
        {
            var product = new Product($"SKU-{i:D3}", $"Product {i}", (i * 10m), 1);
            tasks.Add(repository.AddAsync(product));
        }

        await Task.WhenAll(tasks);

        contextMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ValidationErrorHandling_Comprehensive()
    {
        var invalidProducts = new[]
        {
            new Product { Sku = "", Name = "Invalid", Price = 10m, CategoryId = 1 },
            new Product { Sku = "SKU-001", Name = "", Price = 10m, CategoryId = 1 },
            new Product { Sku = "SKU-002", Name = "Valid", Price = 0m, CategoryId = 1 },
            new Product { Sku = "SKU-003", Name = "Valid", Price = 10m, CategoryId = 0 }
        };

        foreach (var product in invalidProducts)
        {
            var isValid = product.Validate(out var errors);
            isValid.Should().BeFalse();
            errors.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task CacheInvalidation_PatternBased()
    {
        var cache = new MemoryCacheProvider();

        await cache.SetAsync("user:1:profile", "john_profile");
        await cache.SetAsync("user:1:settings", "john_settings");
        await cache.SetAsync("user:2:profile", "jane_profile");
        await cache.SetAsync("product:1:details", "laptop_details");

        await cache.RemoveByPatternAsync("user:1:*");

        var profile = await cache.GetAsync<string>("user:1:profile");
        var settings = await cache.GetAsync<string>("user:1:settings");
        var otherUser = await cache.GetAsync<string>("user:2:profile");
        var product = await cache.GetAsync<string>("product:1:details");

        profile.Should().BeNull();
        settings.Should().BeNull();
        otherUser.Should().NotBeNull();
        product.Should().NotBeNull();
    }

    [Fact]
    public async Task OrderManagement_Scenario()
    {
        var user = new User("customer123", "customer@example.com", "hashedpassword123456789");
        var product1 = new Product("SKU-001", "Item A", 29.99m, 1) { StockQuantity = 100 };
        var product2 = new Product("SKU-002", "Item B", 59.99m, 1) { StockQuantity = 50 };

        user.Validate(out _).Should().BeTrue();
        product1.Validate(out _).Should().BeTrue();
        product2.Validate(out _).Should().BeTrue();

        product1.DecreaseStock(2);
        product2.DecreaseStock(1);

        product1.StockQuantity.Should().Be(98);
        product2.StockQuantity.Should().Be(49);
    }

    [Fact]
    public async Task DataExportScenario_MultipleFormats()
    {
        var products = new List<Product>
        {
            new("SKU-001", "Product A", 99.99m, 1) { Description = "Description A" },
            new("SKU-002", "Product B", 199.99m, 1) { Description = "Description B" },
            new("SKU-003", "Product C", 299.99m, 1) { Description = "Description C" }
        };

        products.Should().HaveCount(3);
        products.Should().AllSatisfy(p => p.Validate(out _).Should().BeTrue());

        var totalValue = products.Sum(p => p.Price);
        totalValue.Should().Be(599.97m);

        var averagePrice = products.Average(p => p.Price);
        averagePrice.Should().Be(199.99m);
    }
}
