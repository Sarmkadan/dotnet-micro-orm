// entire file content ...

// ... goes in between

## BatchOperationsTests

The `BatchOperationsTests` class provides comprehensive unit tests for the batch operations functionality of the DotnetMicroOrm library. It covers various scenarios, including adding and deleting multiple products, handling invalid products, and testing the constructor.

### Example Usage

```csharp
using DotnetMicroOrm.Tests;

// Create an instance of the test class
var tests = new BatchOperationsTests();

// Test adding an empty list of products
await tests.AddRangeAsync_WithEmptyList_ReturnsEmptyList();

// Test adding a list of valid products
await tests.AddRangeAsync_WithValidProducts_AddsAllSuccessfully();

// Test adding an invalid product
await tests.AddRangeAsync_WithInvalidProduct_ThrowsEntityValidationException();

// Test deleting an empty list of products
await tests.DeleteRangeAsync_WithEmptyList_ReturnsZero();

// Test deleting a list of multiple products
await tests.DeleteRangeAsync_WithMultipleProducts_DeletesAllSuccessfully();

// Test deleting some non-existent products
await tests.DeleteRangeAsync_WithSomeNonExistentProducts_ReturnsPartialCount();
```

// ... goes in between
