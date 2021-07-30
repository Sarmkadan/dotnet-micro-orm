# BatchUpsertOperationUnitTestsExtensions

This static class provides extension methods for configuring `BatchUpsertOperationUnitTests` instances in a fluent manner. Each method sets up a specific test scenario—such as a successful mock context, validation failure, custom results, or simulated database failure—allowing developers to prepare unit tests for the batch upsert operation with minimal boilerplate.

## API

### WithSuccessfulMockContext
**Purpose:** Prepares the test to use a mock EF Core context that simulates a successful database operation.  
**Parameters:** `this BatchUpsertOperationUnitTests tests` – the test instance to configure.  
**Return value:** The same `BatchUpsertOperationUnitTests` instance, enabling fluent chaining.  
**Exceptions:** Throws `ArgumentNullException` if `tests` is `null`.

### WithValidationFailure
**Purpose:** Configures the test so that entity validation fails, causing the upsert operation to raise a validation exception.  
**Parameters:** `this BatchUpsertOperationUnitTests tests` – the test instance to configure.  
**Return value:** The same `BatchUpsertOperationUnitTests` instance.  
**Exceptions:** Throws `ArgumentNullException` if `tests` is `null`.

### WithCustomResults
**Purpose:** Supplies custom result objects that the upsert operation will return during the test.  
**Parameters:** `this BatchUpsertOperationUnitTests tests, object customResults` – the test instance and the custom results to be used.  
**Return value:** The same `BatchUpsertOperationUnitTests` instance.  
**Exceptions:** Throws `ArgumentNullException` if `tests` is `null`; may throw `InvalidOperationException` if `customResults` is `null`.

### WithDatabaseFailure
**Purpose:** Sets up the test to simulate a database failure (e.g., a `DbUpdateException`) during the upsert operation.  
**Parameters:** `this BatchUpsertOperationUnitTests tests` – the test instance to configure.  
**Return value:** The same `BatchUpsertOperationUnitTests` instance.  
**Exceptions:** Throws `ArgumentNullException` if `tests` is `null`.

## Usage

```csharp
var test = new BatchUpsertOperationUnitTests();
test.WithSuccessfulMockContext()
    .RunTest(); // Executes the test with a successful mock context
```

```csharp
var test = new BatchUpsertOperationUnitTests();
test.WithCustomResults(new[]
{
    new BatchUpsertResult { Id = 10, Status = UpsertStatus.Updated },
    new BatchUpsertResult { Id = 20, Status = UpsertStatus.Inserted }
})
.RunTest(); // Executes the test using the supplied custom results
```

## Notes

- The extension methods are designed to be called on a single `BatchUpsertOperationUnitTests` instance and mutate only that instance’s internal configuration.
- They are thread‑safe with respect to distinct instances; concurrent calls on the same instance without external synchronization may lead to race conditions because the instance’s state is mutated.
- Passing `null` for the test instance results in an `ArgumentNullException` from each method.
- After configuring a test with any of these methods, the instance should not be reused for a different scenario unless it is explicitly reset, as the configuration accumulates.
- The methods return the same instance to support a fluent API; they do not allocate new objects.
