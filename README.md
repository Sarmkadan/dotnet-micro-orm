// entire file content ...
// ... goes in between

## FormatterFactory

The `FormatterFactory` class is a centralized factory for creating output formatters based on requested formats. It provides a way to instantiate formatters with consistent configuration and supports registration of custom formatter implementations.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Formatters;

class Program
{
    static void Main()
    {
        var factory = new FormatterFactory();
        var jsonFormatter = factory.GetFormatter(OutputFormat.Json);
        var csvFormatter = factory.GetFormatter(OutputFormat.Csv);

        Console.WriteLine(jsonFormatter.Format(new { Name = "John", Age = 30 }));
        Console.WriteLine(csvFormatter.Format(new { Name = "Jane", Age = 25 }));
    }
}

// ... goes in between

## CategoryModelTests

`CategoryModelTests` is a collection of unit tests that verify the behavior of the `Category` domain model. The suite checks construction, validation rules, ordering operations, breadcrumb generation, product counting, and deactivation logic, ensuring the model behaves correctly in a variety of scenarios.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Domain.Models;

// The test class lives in the global namespace (no explicit namespace)
// and its public methods are the individual test cases.
var tests = new CategoryModelTests();

// Example: creating a valid category (mirrors the Constructor test)
var category = new Category("Books", "books")
{
    DisplayOrder = 1,
    ParentCategoryId = null,
    IsActive = true
};

// Example: validating a correct category (mirrors Validate_WithValidCategory_ReturnsTrue)
bool isValid = category.Validate(out var validationErrors);
// isValid == true, validationErrors is empty

// Example: validating a category with invalid display order (mirrors Validate_WithInvalidDisplayOrder_ReturnsFalseWithError)
var invalidCategory = new Category("Invalid", "invalid") { DisplayOrder = -1 };
isValid = invalidCategory.Validate(out validationErrors);
// isValid == false, validationErrors contains error about DisplayOrder

// Example: moving a category up (mirrors MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder)
category.DisplayOrder = 5;
tests.MoveUp_WithDisplayOrderGreaterThanZero_DecrementsOrder(); // internally calls category.MoveUp()
Console.WriteLine($"DisplayOrder after MoveUp: {category.DisplayOrder}");

// Example: generating a breadcrumb (mirrors GetBreadcrumb_WithParentCategory_ReturnsFullPath)
var parent = new Category("Electronics", "electronics") { Id = 1 };
var child = new Category("Laptops", "laptops") { Id = 2, ParentCategory = parent };
string breadcrumb = child.GetBreadcrumb();
Console.WriteLine($"Breadcrumb: {breadcrumb}");
```

## ExceptionTests

The `ExceptionTests` class contains a suite of unit tests that verify the behavior of the custom exception types shipped with the library. It ensures that each exception correctly populates its message, error code, inner exception, and any additional context data supplied via the fluent `WithContext` API.

### Example Usage

```csharp
using DotnetMicroOrm.Exceptions;

// Instantiate the test class (it lives in the global namespace)
var tests = new ExceptionTests();

// Run the individual test methods directly – each method validates a specific
// exception constructor or context‑adding behavior.
tests.OrmException_WithMessage_CreatesInstance();
tests.OrmException_WithMessageAndErrorCode_CreatesInstance();
tests.OrmException_WithMessageInnerExceptionAndErrorCode_CreatesInstance();
tests.OrmException_WithContext_AddsContext();
tests.DatabaseConnectionException_WithMessage_CreatesInstance();
tests.EntityMappingException_WithMessage_CreatesInstance();
tests.EntityMappingException_WithMessageAndPropertyName_CreatesInstance();
tests.QueryExecutionException_WithMessage_CreatesInstance();
tests.QueryExecutionException_WithMessageAndQuery_CreatesInstance();
tests.EntityValidationException_WithMessage_CreatesInstance();
tests.EntityValidationException_WithMessageAndErrors_CreatesInstance();
tests.ConcurrencyException_WithMessage_CreatesInstance();
tests.ConcurrencyException_WithMessageAndEntityKey_CreatesInstance();
```

## UserModelTests

`UserModelTests` is a comprehensive suite of unit tests that verify the behavior of the `User` domain model. The tests cover validation scenarios (username, email, password hash, name lengths), property behaviors, and method functionality including email verification, last login tracking, and name formatting, ensuring the `User` model works correctly in various scenarios.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Domain.Models;

// Instantiate the test class (it lives in the global namespace)
var tests = new UserModelTests();

// Example: creating a valid user (mirrors Constructor_WithParameters_InitializesFieldsCorrectly)
var user = new User("johndoe", "john@example.com", "hashedpassword1234567890123456789012")
{
    FirstName = "John",
    LastName = "Doe"
};

// Example: validating a user with valid properties (mirrors Validate_WithValidUser_ReturnsTrue)
bool isValid = user.Validate(out var validationErrors);
// isValid == true, validationErrors is empty

// Example: validating a user with invalid username (mirrors Validate_WithEmptyUsername_ReturnsFalseWithError)
var invalidUser = new User { Username = "", Email = "test@example.com", PasswordHash = "hashedpassword123456789012" };
isValid = invalidUser.Validate(out validationErrors);
// isValid == false, validationErrors contains error about Username

// Example: getting full name (mirrors GetFullName_WithFirstAndLastNames_ReturnsCombined)
string fullName = user.GetFullName();
// fullName == "John Doe"

// Example: marking email as verified (mirrors MarkAsEmailVerified_ChangesEmailVerificationFlag)
user.MarkAsEmailVerified();
// user.IsEmailVerified == true

// Example: updating last login (mirrors UpdateLastLogin_SetsLastLoginDate)
user.UpdateLastLogin();
// user.LastLoginDate is set to current UTC time

// Example: running individual test methods directly
// Each test method exercises specific User model functionality
tests.Validate_WithValidUser_ReturnsTrue();
tests.Validate_WithEmptyUsername_ReturnsFalseWithError();
tests.GetFullName_WithFirstAndLastNames_ReturnsCombined();
tests.MarkAsEmailVerified_ChangesEmailVerificationFlag();
tests.UpdateLastLogin_SetsLastLoginDate();
```

## RepositoryTests

The `RepositoryTests` class provides unit tests for the `Repository<T>` class, verifying that repository operations work correctly with mocked database contexts. It tests constructor validation, CRUD operations, filtering, counting, existence checks, and proper exception handling, ensuring the repository behaves correctly across various scenarios.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using Moq;

// Instantiate the test class (it lives in the global namespace)
var tests = new RepositoryTests();

// Test constructor validation
tests.Constructor_WithNullContext_ThrowsArgumentNullException();

// Create a mock database context and repository
var mockContext = new Mock<IDatabaseContext>();
var repository = new Repository<User>(mockContext.Object);

// Test basic CRUD operations

// Test adding a valid entity
tests.AddAsync_WithValidEntity_AddsSuccessfully();

// Test adding an invalid entity (should throw validation exception)
tests.AddAsync_WithInvalidEntity_ThrowsEntityValidationException();

// Test getting by ID
tests.GetByIdAsync_WithExistingId_ReturnsEntity();
tests.GetByIdAsync_WithNonExistingId_ReturnsNull();

// Test getting all entities
tests.GetAllAsync_WithMultipleEntities_ReturnsAllEntities();
tests.GetAllAsync_WithNoEntities_ReturnsEmptyList();

// Test filtering with predicate
tests.GetAsync_WithMatchingPredicate_ReturnsFilteredEntities();
tests.GetAsync_WithNoMatchingPredicate_ReturnsEmptyList();

// Test counting entities
tests.CountAsync_WithPredicate_ReturnsCorrectCount();
tests.CountAsync_WithNullPredicate_ReturnsTotalCount();

// Test existence checks
tests.ExistsAsync_WithExistingEntity_ReturnsTrue();
tests.ExistsAsync_WithNonExistingEntity_ReturnsFalse();

// Test updating entities
tests.UpdateAsync_WithValidEntity_UpdatesSuccessfully();
tests.UpdateAsync_WithInvalidEntity_ThrowsEntityValidationException();

// Test deleting entities
tests.DeleteAsync_WithValidId_DeletesSuccessfully();
tests.DeleteAsync_WithNonExistingId_ReturnsFalse();
```

## RepositoryIntegrationTests

The `RepositoryIntegrationTests` class provides integration tests for the `Repository<TEntity>` class, verifying that repository operations work correctly with mocked database contexts. It tests constructor validation, CRUD operations, batch operations, existence checks, and pagination functionality, ensuring proper interaction patterns and exception handling across various scenarios.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using Moq;

// Instantiate the test class (it lives in the global namespace)
var tests = new RepositoryIntegrationTests();

// Test constructor validation
tests.Constructor_WithNullContext_ThrowsArgumentNullException();

// Test basic CRUD operations
var mockContext = new Mock<IDatabaseContext>();
var repository = new Repository<Product>(mockContext.Object);

// Test adding a valid entity
tests.AddAsync_WithValidProduct_InsertsSuccessfully();

// Test adding an invalid entity (should throw validation exception)
tests.AddAsync_WithInvalidProduct_ThrowsEntityValidationException();

// Test updating an existing entity
tests.UpdateAsync_WithValidProduct_UpdatesSuccessfully();

// Test updating a non-existent entity (should throw exception)
tests.UpdateAsync_WithNonExistentProduct_ThrowsOrmException();

// Test getting by ID
tests.GetByIdAsync_WithValidId_ReturnsProductWithMatchingId();

// Test getting all entities
tests.GetAllAsync_WithMultipleProducts_ReturnsAllProducts();

// Test counting entities
tests.CountAsync_WithData_ReturnsCorrectCount();
tests.CountAsync_WithNoData_ReturnsZero();

// Test existence checks
tests.ExistsAsync_WithExistingEntity_ReturnsTrue();
tests.ExistsAsync_WithNonExistentEntity_ReturnsFalse();

// Test batch operations
tests.AddRangeAsync_WithValidProducts_InsertsAll();
tests.AddRangeAsync_WithEmptyList_ReturnsEmptyList();
tests.DeleteRangeAsync_WithValidEntities_DeletesAll();

// Test delete operations
tests.DeleteAsync_WithValidId_DeletesSuccessfully();
tests.DeleteAsync_WithNonExistentId_ReturnsFalse();
tests.DeleteAsync_WithEntity_DeletesSuccessfully();
```

## CoreRepositoryTests

The `CoreRepositoryTests` class contains a set of unit tests for the `Repository<T>` class, focusing on its core functionality such as CRUD operations, filtering, and existence checks. It ensures that the repository behaves correctly under various scenarios, including valid and invalid data, existing and non-existing entities, and different query parameters.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using Moq;

// Create a mock database context
var mockContext = new Mock<IDatabaseContext>();

// Create a repository instance for testing
var repository = new Repository<Product>(mockContext.Object);

// Test getting a product by ID
var product = await repository.GetByIdAsync(1);

// Test adding a new product
var newProduct = new Product("NEW001", "New Product", 19.99m, 1);
await repository.AddAsync(newProduct);

// Test updating an existing product
var updatedProduct = new Product("UPD001", "Updated Product", 24.99m, 2) { Id = 1 };
await repository.UpdateAsync(updatedProduct);

// Test deleting a product
await repository.DeleteAsync(1);

// Test filtering products with a predicate
var filteredProducts = await repository.GetAsync(p => p.IsActive);
```

// ... goes in between
```