using System;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using Xunit;

namespace DotnetMicroOrm.Tests
{
    public class SpecificationTests
    {
        [Fact]
        public void ActiveProductsSpecification_ShouldMatchActiveProduct()
        {
            // Arrange
            var spec = new ActiveProductsSpecification();
            var product = new Product
            {
                IsActive = true,
                Name = "Test Product",
                Price = 10.0m,
                StockQuantity = 5
            };

            // Act
            var predicate = spec.Criteria;

            // Assert
            Assert.NotNull(predicate);
            Assert.True(predicate!.Compile()(product));
        }

        [Fact]
        public void ActiveProductsSpecification_ShouldRejectInactiveProduct()
        {
            // Arrange
            var spec = new ActiveProductsSpecification();
            var product = new Product
            {
                IsActive = false,
                Name = "Inactive Product",
                Price = 15.0m,
                StockQuantity = 3
            };

            // Act
            var predicate = spec.Criteria;

            // Assert
            Assert.NotNull(predicate);
            Assert.False(predicate!.Compile()(product));
        }

        [Fact]
        public void ActiveUsersSpecification_ShouldMatchActiveUser()
        {
            // Arrange
            var spec = new ActiveUsersSpecification();
            var user = new User
            {
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            var predicate = spec.Criteria;

            // Assert
            Assert.NotNull(predicate);
            Assert.True(predicate!.Compile()(user));
        }

        [Fact]
        public void ActiveUsersSpecification_ShouldRejectInactiveUser()
        {
            // Arrange
            var spec = new ActiveUsersSpecification();
            var user = new User
            {
                IsActive = false,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            var predicate = spec.Criteria;

            // Assert
            Assert.NotNull(predicate);
            Assert.False(predicate!.Compile()(user));
        }
    }
}
