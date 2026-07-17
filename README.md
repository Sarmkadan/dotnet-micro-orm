// entire file content ...

// ... goes in between

## BatchUpsertOperationUnitTests

The `BatchUpsertOperationUnitTests` class provides comprehensive unit tests for the `BatchUpsertOperation<TEntity>` class. 
These tests validate the behavior of `BatchUpsertOperation` in various scenarios, including error handling, entity validation, and batch operations.

### Example Usage

```csharp
using DotnetMicroOrm.Tests;

// Create an instance of the test class
var tests = new BatchUpsertOperationUnitTests();

// Verify constructor behavior with a null context
tests.Constructor_WithNullContext_ThrowsArgumentNullException();

// Test upserting a valid entity
var user = new User("testuser", "test@example.com", "password");
var result = await tests._batchUpsert.UpsertAsync(user, u => u.Id);

// Test upserting multiple valid entities
var users = new List<User>
{
    new User("user1", "user1@example.com", "password1"),
    new User("user2", "user2@example.com", "password2")
};
var results = await tests._batchUpsert.UpsertRangeAsync(users, u => u.Id);
```

// ... goes in between
