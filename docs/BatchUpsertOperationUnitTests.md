# BatchUpsertOperationUnitTests

Unit test suite for the `BatchUpsertOperation` class within the `dotnet-micro-orm` project. This class validates the behavior of batch upsert operations, ensuring proper argument validation, entity validation, result handling, and metadata retrieval under various edge-case and success scenarios.

## API

### `public BatchUpsertOperationUnitTests`
Constructor. Initializes the test class and any shared test infrastructure (e.g., mock database context, entity validators) required by the individual test methods. No parameters or return value.

### `public void Constructor_WithNullContext_ThrowsArgumentNullException`
Verifies that constructing a `BatchUpsertOperation` with a null database context throws an `ArgumentNullException`.  
**Throws:** `ArgumentNullException` (expected in the tested constructor).

### `public async Task UpsertAsync_WithNullEntity_ThrowsArgumentNullException`
Ensures that calling `UpsertAsync` with a null entity argument throws an `ArgumentNullException`.  
**Throws:** `ArgumentNullException` (expected from the method under test).

### `public async Task UpsertAsync_WithNullKeySelector_ThrowsArgumentNullException`
Ensures that calling `UpsertAsync` with a null key selector argument throws an `ArgumentNullException`.  
**Throws:** `ArgumentNullException` (expected from the method under test).

### `public async Task UpsertAsync_WithInvalidEntity_ThrowsEntityValidationException`
Validates that passing an entity that fails validation causes `UpsertAsync` to throw an `EntityValidationException`.  
**Throws:** `EntityValidationException` (expected from the method under test).

### `public async Task UpsertAsync_WithValidEntity_ReturnsUpsertResult`
Confirms that `UpsertAsync` with a valid entity and key selector returns a non-null `UpsertResult` indicating the operation completed successfully.  
**Returns:** `UpsertResult` (asserted as valid and non-null).

### `public async Task UpsertRangeAsync_WithNullEntities_ThrowsArgumentNullException`
Verifies that `UpsertRangeAsync` throws an `ArgumentNullException` when the entities collection is null.  
**Throws:** `ArgumentNullException` (expected from the method under test).

### `public async Task UpsertRangeAsync_WithNullKeySelector_ThrowsArgumentNullException`
Verifies that `UpsertRangeAsync` throws an `ArgumentNullException` when the key selector is null.  
**Throws:** `ArgumentNullException` (expected from the method under test).

### `public async Task UpsertRangeAsync_WithInvalidBatchSize_ThrowsArgumentOutOfRangeException`
Tests that specifying a batch size less than 1 causes `UpsertRangeAsync` to throw an `ArgumentOutOfRangeException`.  
**Throws:** `ArgumentOutOfRangeException` (expected from the method under test).

### `public async Task UpsertRangeAsync_WithTooLargeBatchSize_ThrowsArgumentOutOfRangeException`
Tests that specifying a batch size exceeding the maximum allowed limit causes `UpsertRangeAsync` to throw an `ArgumentOutOfRangeException`.  
**Throws:** `ArgumentOutOfRangeException` (expected from the method under test).

### `public async Task UpsertRangeAsync_WithEmptyList_ReturnsEmptyList`
Ensures that calling `UpsertRangeAsync` with an empty entity collection returns an empty result collection without throwing.  
**Returns:** An empty collection of upsert results.

### `public async Task UpsertRangeAsync_WithInvalidEntity_ThrowsEntityValidationException`
Validates that when any entity in the batch fails validation, `UpsertRangeAsync` throws an `EntityValidationException`.  
**Throws:** `EntityValidationException` (expected from the method under test).

### `public async Task UpsertRangeAsync_WithValidEntities_ReturnsMultipleResults`
Confirms that a batch of valid entities produces a collection of `UpsertResult` objects corresponding to each input entity.  
**Returns:** A collection of `UpsertResult` with count matching the input.

### `public async Task UpsertRangeAsync_WithBatchSize_ReturnsCorrectNumberOfResults`
Verifies that when a specific batch size is provided, the total number of returned results equals the number of input entities, regardless of internal batching.  
**Returns:** A collection of `UpsertResult` with count matching the input.

### `public void GetTableName_ReturnsCorrectTableName`
Tests that the `GetTableName` method returns the expected table name for the entity type used in the batch operation.  
**Returns:** The correct table name string.

### `public void GetTableSchema_ReturnsCorrectSchema`
Tests that the `GetTableSchema` method returns the expected schema identifier (e.g., `"dbo"`) for the entity type.  
**Returns:** The correct schema string.

## Usage

```csharp
// Example 1: Testing single-entity upsert validation and success
[TestMethod]
public async Task UpsertAsync_ValidFlow_ReturnsResult()
{
    var tests = new BatchUpsertOperationUnitTests();
    
    // Should throw on null entity
    await tests.UpsertAsync_WithNullEntity_ThrowsArgumentNullException();
    
    // Should throw on null key selector
    await tests.UpsertAsync_WithNullKeySelector_ThrowsArgumentNullException();
    
    // Should throw on invalid entity
    await tests.UpsertAsync_WithInvalidEntity_ThrowsEntityValidationException();
    
    // Should succeed with valid entity
    await tests.UpsertAsync_WithValidEntity_ReturnsUpsertResult();
}
```

```csharp
// Example 2: Testing batch upsert with edge cases
[TestMethod]
public async Task UpsertRangeAsync_BatchScenarios_HandlesCorrectly()
{
    var tests = new BatchUpsertOperationUnitTests();
    
    // Null arguments
    await tests.UpsertRangeAsync_WithNullEntities_ThrowsArgumentNullException();
    await tests.UpsertRangeAsync_WithNullKeySelector_ThrowsArgumentNullException();
    
    // Invalid batch sizes
    await tests.UpsertRangeAsync_WithInvalidBatchSize_ThrowsArgumentOutOfRangeException();
    await tests.UpsertRangeAsync_WithTooLargeBatchSize_ThrowsArgumentOutOfRangeException();
    
    // Empty list yields empty result
    await tests.UpsertRangeAsync_WithEmptyList_ReturnsEmptyList();
    
    // Invalid entity in batch
    await tests.UpsertRangeAsync_WithInvalidEntity_ThrowsEntityValidationException();
    
    // Successful batch processing
    await tests.UpsertRangeAsync_WithValidEntities_ReturnsMultipleResults();
    await tests.UpsertRangeAsync_WithBatchSize_ReturnsCorrectNumberOfResults();
}
```

## Notes

- **Argument validation order:** Tests imply that null checks for context, entities, and key selectors occur before any database interaction. Validation exceptions are thrown synchronously or as part of the async flow before the operation proceeds.
- **Entity validation:** The `EntityValidationException` tests indicate that entities are validated prior to upsert execution. Both single and batch operations enforce this, and a single invalid entity in a batch causes the entire batch operation to fail.
- **Batch size constraints:** The batch size must be at least 1 and must not exceed an internally defined maximum. The exact maximum is determined by the underlying implementation (likely database or provider limits).
- **Empty collection handling:** An empty input list is treated as a valid no-op, returning an empty result set rather than throwing.
- **Thread safety:** These are unit tests and do not assert thread safety. The tested `BatchUpsertOperation` methods are expected to be used with scoped or transient contexts; concurrent usage is not addressed by this test suite.
- **Metadata methods:** `GetTableName` and `GetTableSchema` are likely static or instance methods that rely on entity type metadata (e.g., attributes or conventions). They do not require a database connection and return deterministic values.
