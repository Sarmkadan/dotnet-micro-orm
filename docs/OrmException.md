# OrmException

The `OrmException` class serves as the root exception type for the `dotnet-micro-orm` library, providing a standardized mechanism to handle and categorize errors that occur during ORM-related operations. It encapsulates diagnostic information and specific error contexts, facilitating robust error handling and debugging within data access layers by distinguishing between connectivity, mapping, validation, and concurrency issues.

## API

- **`public OrmException()`**
  Initializes a new instance of the `OrmException` class with default values.

- **`public new OrmException WithContext(...)`**
  Returns a new instance of `OrmException` enriched with the provided diagnostic context. This method shadows the base exception behavior to ensure ORM-specific context is preserved in the exception chain.

- **`public DatabaseConnectionException`**
  Represents an exception triggered by failures related to database connectivity or pool exhaustion.

- **`public EntityMappingException`**
  Represents an exception triggered when the ORM fails to correctly map database results to the target C# entity model.

- **`public QueryExecutionException`**
  Represents an exception triggered by failures during the execution of a SQL query or command.

- **`public List<string> ValidationErrors`**
  A collection of validation error messages encountered during an entity validation process. This property is typically populated when an `EntityValidationException` is raised.

- **`public EntityValidationException`**
  Represents an exception triggered when entity validation rules are violated.

- **`public ConcurrencyException`**
  Represents an exception triggered by optimistic or pessimistic concurrency conflicts during data persistence.

## Usage

**Example 1: Catching and handling general ORM exceptions**

```csharp
try
{
    var user = await repository.GetUserByIdAsync(userId);
}
catch (OrmException ex)
{
    // Log the error and take appropriate action
    logger.LogError(ex, "An ORM operation failed.");
    throw;
}
```

**Example 2: Enriching an exception with context during re-throwing**

```csharp
try
{
    await repository.SaveEntityAsync(entity);
}
catch (QueryExecutionException ex)
{
    // Wrap the error with additional diagnostic context
    throw ex.WithContext("Operation: SaveEntityAsync, EntityId: " + entity.Id);
}
```

## Notes

- **Inheritance**: `OrmException` inherits from `System.Exception`. Custom exception handling logic relying on `catch (Exception)` will capture these instances.
- **Thread Safety**: The `ValidationErrors` list property is not thread-safe. If multiple threads are interacting with a single exception instance, appropriate synchronization is required when accessing or modifying this list.
- **Exception Chaining**: It is recommended to populate the `InnerException` property when wrapping lower-level database exceptions to maintain the full stack trace for diagnostics.
- **Usage**: Members like `DatabaseConnectionException` and `ConcurrencyException` are intended for specific error handling blocks to allow for granular recovery strategies, such as retrying on connection failures while aborting on mapping errors.
