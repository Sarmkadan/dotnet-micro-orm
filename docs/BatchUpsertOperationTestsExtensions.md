# BatchUpsertOperationTestsExtensions
The `BatchUpsertOperationTestsExtensions` class provides a set of extension methods for testing batch upsert operations. It allows developers to verify the results of upsert operations, including inserted and updated entities, and to check for specific conditions such as the presence of inserted entities.

## API
* `GetInsertedEntities<T>`: Returns a list of entities that were inserted during the upsert operation. The method takes no parameters and returns a `List<T>`. It does not throw any exceptions.
* `GetUpdatedEntities<T>`: Returns a list of entities that were updated during the upsert operation. The method takes no parameters and returns a `List<T>`. It does not throw any exceptions.
* `WhereEntity<T>`: Returns a list of upsert results for the specified entity type. The method takes no parameters and returns a `List<UpsertResult<T>>`. It does not throw any exceptions.
* `AnyInserted<T>`: Returns a boolean indicating whether any entities of the specified type were inserted during the upsert operation. The method takes no parameters and returns a `bool`. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `BatchUpsertOperationTestsExtensions` class:
```csharp
// Example 1: Verify inserted entities
var insertedEntities = BatchUpsertOperationTestsExtensions.GetInsertedEntities<MyEntity>();
Assert.AreEqual(2, insertedEntities.Count);

// Example 2: Check for updated entities and verify upsert results
var updatedEntities = BatchUpsertOperationTestsExtensions.GetUpdatedEntities<MyEntity>();
var upsertResults = BatchUpsertOperationTestsExtensions.WhereEntity<MyEntity>();
Assert.AreEqual(1, updatedEntities.Count);
Assert.IsTrue(BatchUpsertOperationTestsExtensions.AnyInserted<MyEntity>());
```

## Notes
When using the `BatchUpsertOperationTestsExtensions` class, note that the `GetInsertedEntities<T>` and `GetUpdatedEntities<T>` methods return lists of entities that were inserted or updated during the upsert operation, respectively. The `WhereEntity<T>` method returns a list of upsert results for the specified entity type. The `AnyInserted<T>` method can be used to check for the presence of inserted entities. Additionally, the methods in this class are thread-safe, as they do not modify any shared state. However, the underlying data structures used to store the upsert results may not be thread-safe, so caution should be exercised when accessing these methods from multiple threads.
