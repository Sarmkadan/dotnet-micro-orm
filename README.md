// entire file content ...

// ... goes in between

## BatchUpsertOperationTests

The `BatchUpsertOperationTests` class provides unit tests for the `BatchUpsertOperation<TEntity>` class, verifying its behavior in various scenarios. It tests operations such as upserting entities in batches, handling empty lists, new entity insertions, existing entity updates, batch size validation, entity validation, and multiple batch executions.

### Example Usage

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Tests;

// Instantiate the test class (it lives in the global namespace)
var tests = new BatchUpsertOperationTests();

// Test upserting an empty list
await tests.UpsertRangeAsync_EmptyList_ReturnsEmptyResults();

// Test upserting a new entity
await tests.UpsertAsync_NewEntity_ReturnsInsertedResult();

// Test upserting an existing entity
await tests.UpsertAsync_ExistingEntity_ReturnsUpdatedResult();

// Test invalid batch size
await tests.UpsertRangeAsync_InvalidBatchSize_ThrowsArgumentOutOfRange();

// Test invalid entity validation
await tests.UpsertRangeAsync_InvalidEntity_ThrowsEntityValidationException();

// Test multiple batches
await tests.UpsertRangeAsync_MultipleBatches_ExecutesMultipleQueries();
```

// ... goes in between
