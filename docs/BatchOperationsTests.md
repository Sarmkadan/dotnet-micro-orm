# BatchOperationsTests

`BatchOperationsTests` is a test suite that validates the batch-oriented data access methods exposed by the `dotnet-micro-orm` library. It contains two nested test groups: the outer class focuses on `AddRangeAsync` and `DeleteRangeAsync` operations, while the inner `BatchOperationsUpsertTests` class covers upsert (insert-or-update) logic, including single-entity and range-based variants. All tests are asynchronous and exercise both success paths and failure modes such as empty inputs, invalid entities, partial matches, and batch splitting.

## API

### BatchOperationsTests

#### `public BatchOperationsTests`
Constructor. Initializes the test class and any shared infrastructure required by the batch operation tests. No parameters, no return value, does not throw.

#### `public async Task AddRangeAsync_WithEmptyList_ReturnsEmptyList`
Verifies that passing an empty collection to `AddRangeAsync` returns an empty result set without error.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the assertion finishes.  
- **Throws:** test assertion failures if the returned list is non-empty or an unexpected exception occurs.

#### `public async Task AddRangeAsync_WithValidProducts_AddsAllSuccessfully`
Confirms that a list of valid product entities is fully persisted by `AddRangeAsync`.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when all products are verified as added.  
- **Throws:** test assertion failures if any product is missing or the returned count mismatches.

#### `public async Task AddRangeAsync_WithInvalidProduct_ThrowsEntityValidationException`
Ensures that including an entity that fails validation causes `AddRangeAsync` to throw an `EntityValidationException`.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the exception is caught and validated.  
- **Throws:** test assertion failures if the expected exception type is not thrown or the error details are incorrect.

#### `public async Task DeleteRangeAsync_WithEmptyList_ReturnsZero`
Validates that deleting an empty list of identifiers returns a count of zero.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the zero result is confirmed.  
- **Throws:** test assertion failures if a non-zero count is returned.

#### `public async Task DeleteRangeAsync_WithMultipleProducts_DeletesAllSuccessfully`
Tests that providing identifiers for multiple existing products removes all of them and returns the correct deleted count.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when deletion count and absence of the records are verified.  
- **Throws:** test assertion failures if any product remains or the count is wrong.

#### `public async Task DeleteRangeAsync_WithSomeNonExistentProducts_ReturnsPartialCount`
Checks that when some identifiers do not correspond to existing records, `DeleteRangeAsync` returns only the number of actually deleted rows.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the partial count is validated.  
- **Throws:** test assertion failures if the returned count includes non-existent identifiers.

### BatchOperationsUpsertTests

#### `public BatchOperationsUpsertTests`
Constructor for the nested upsert test class. Initializes context and dependencies specific to upsert scenarios. No parameters, no return value, does not throw.

#### `public void Constructor_WithNullContext_ThrowsArgumentNullException`
Synchronous test asserting that instantiating the upsert handler with a null context immediately throws `ArgumentNullException`.  
- **Parameters:** none (test method).  
- **Returns:** void.  
- **Throws:** test assertion failures if the exception is not thrown or is of the wrong type.

#### `public async Task UpsertAsync_WithSingleProductBySku_ReturnsInsertResult`
Verifies that upserting a product identified by SKU when it does not exist performs an insert and returns an appropriate result indicating insertion.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the insert result is confirmed.  
- **Throws:** test assertion failures if an update result is returned or the entity is not persisted.

#### `public async Task UpsertAsync_WithSingleProductById_ReturnsUpdateResult`
Verifies that upserting a product identified by its primary key when it already exists performs an update and returns an appropriate result indicating update.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the update result is confirmed.  
- **Throws:** test assertion failures if an insert result is returned or the existing data is not modified.

#### `public async Task UpsertAsync_WithInvalidProduct_ThrowsEntityValidationException`
Ensures that an invalid product passed to `UpsertAsync` causes an `EntityValidationException` to be thrown before any database operation occurs.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the exception is caught.  
- **Throws:** test assertion failures if the exception is not thrown or validation details are missing.

#### `public async Task UpsertRangeAsync_WithMultipleProducts_ReturnsMultipleResults`
Tests that upserting a collection of products returns a result for each entity, correctly distinguishing inserts from updates.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when the per-entity results are validated.  
- **Throws:** test assertion failures if the result count mismatches or individual outcomes are incorrect.

#### `public async Task UpsertRangeAsync_WithBatchSize_SplitsIntoBatches`
Confirms that when a batch size is specified, `UpsertRangeAsync` internally partitions the input collection and processes it in multiple batches.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when batch splitting behaviour is verified.  
- **Throws:** test assertion failures if all entities are processed in a single batch or the total result set is incomplete.

#### `public async Task UpsertRangeAsync_WithCompositeKey_ReturnsCorrectResults`
Validates upsert behaviour for entities identified by a composite key, ensuring that matching logic correctly handles multiple key columns and returns the expected insert/update outcomes.  
- **Parameters:** none (test method).  
- **Returns:** a `Task` that completes when composite-key matching is verified.  
- **Throws:** test assertion failures if key resolution fails or results are misattributed.

## Usage

### Example 1: Adding and deleting products in bulk

```csharp
[TestClass]
public class ProductBatchTests : BatchOperationsTests
{
    [TestMethod]
    public async Task AddThenDeleteRange_ShouldSucceed()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Sku = "SKU-001", Name = "Widget" },
            new Product { Sku = "SKU-002", Name = "Gadget" }
        };

        // Act — add range
        await AddRangeAsync_WithValidProducts_AddsAllSuccessfully();

        // Act — delete range
        await DeleteRangeAsync_WithMultipleProducts_DeletesAllSuccessfully();

        // Assertions are handled internally by the base test methods
    }
}
```

### Example 2: Upserting a mixed batch with composite keys

```csharp
[TestClass]
public class InventoryUpsertTests : BatchOperationsUpsertTests
{
    [TestMethod]
    public async Task UpsertInventory_WithCompositeKey_ShouldReturnCorrectResults()
    {
        // Arrange — some items exist, some are new
        // The test method validates composite key matching internally

        await UpsertRangeAsync_WithCompositeKey_ReturnsCorrectResults();

        // Additionally verify batch splitting behaviour
        await UpsertRangeAsync_WithBatchSize_SplitsIntoBatches();
    }
}
```

## Notes

- **Empty collections:** Both `AddRangeAsync` and `DeleteRangeAsync` treat empty inputs as no-ops, returning an empty list or zero respectively. No database round-trip is expected.
- **Partial deletion:** When `DeleteRangeAsync` receives identifiers that do not all match existing records, the returned count reflects only the successfully deleted rows. Non-matching identifiers are silently ignored.
- **Validation ordering:** Entity validation occurs before any database command is executed. An invalid entity in a batch will cause `EntityValidationException` to be thrown immediately, preventing partial persistence of the batch.
- **Batch splitting:** `UpsertRangeAsync` with an explicit batch size partitions the input internally. The total set of results is equivalent to processing the entire collection in one call; batching is an implementation detail that does not change the aggregated outcome.
- **Composite keys:** Upsert matching on composite keys relies on all key components being provided. Mismatched or incomplete key values may lead to unexpected insert/update classifications; the tests assume well-formed key data.
- **Thread safety:** These are test methods intended for sequential execution within a test runner. They are not designed for concurrent invocation and do not guarantee thread safety when sharing state across parallel test runs.
