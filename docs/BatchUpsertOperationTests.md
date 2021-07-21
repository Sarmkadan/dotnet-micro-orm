# BatchUpsertOperationTests

Unit tests for the batch upsert operations in dotnet-micro-orm, verifying behavior of `UpsertAsync` and `UpsertRangeAsync` methods under various conditions including empty inputs, existing entities, batch size constraints, and multi-batch execution.

## API

### `UpsertRangeAsync_EmptyList_ReturnsEmptyResults`
Verifies that calling `UpsertRangeAsync` with an empty collection returns a task that completes with an empty result set. The test ensures the method handles empty inputs gracefully without throwing exceptions or attempting database operations.

### `UpsertAsync_NewEntity_ReturnsInsertedResult`
Tests that inserting a new entity via `UpsertAsync` results in a successful insert operation. The test confirms the returned result contains the inserted entity with a valid identifier and reflects the correct state after insertion.

### `UpsertAsync_ExistingEntity_ReturnsUpdatedResult`
Validates that updating an existing entity via `UpsertAsync` produces a successful update operation. The test checks that the returned result reflects the updated state and that the entity's identifier remains unchanged.

### `UpsertRangeAsync_InvalidBatchSize_ThrowsArgumentOutOfRange`
Ensures that passing a non-positive or excessively large batch size to `UpsertRangeAsync` throws an `ArgumentOutOfRangeException`. The test confirms the method validates batch size constraints before processing.

### `UpsertRangeAsync_InvalidEntity_ThrowsEntityValidationException`
Confirms that `UpsertRangeAsync` throws an `EntityValidationException` when any entity in the input collection fails validation. The test verifies the exception is thrown before any database operations are attempted.

### `UpsertRangeAsync_MultipleBatches_ExecutesMultipleQueries`
Validates that `UpsertRangeAsync` correctly splits a large input collection into multiple batches and executes separate queries for each batch. The test ensures all entities are processed and no data is lost across batch boundaries.

## Usage
