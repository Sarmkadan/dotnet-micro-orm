// entire file content ...

// ... goes in between

## DatabaseContextTests

The `DatabaseContextTests` class provides comprehensive unit tests for the `DatabaseContext` class, ensuring its constructor, connection management, and query execution functionality work as expected.

### Example Usage

```csharp
using DotnetMicroOrm.Tests;

// Create an instance of the test class
var tests = new DatabaseContextTests();

// Test the constructor with a null connection string
tests.Constructor_WithNullConnectionString_ThrowsArgumentNullException();

// Test the constructor with an empty connection string
tests.Constructor_WithEmptyConnectionString_ThrowsArgumentException();

// Test the constructor with a valid connection string
tests.Constructor_WithValidConnectionString_CreatesInstance();

// Test opening a valid connection
await tests.OpenAsync_WithValidConnection_OpensSuccessfully();

// Test opening an invalid connection
await tests.OpenAsync_WithInvalidConnection_ThrowsDatabaseConnectionException();

// Test closing an open connection
await tests.CloseAsync_WithOpenConnection_ClosesSuccessfully();

// Test testing a valid connection
await tests.TestConnectionAsync_WithValidConnection_ReturnsTrue();

// Test testing an invalid connection
await tests.TestConnectionAsync_WithInvalidConnection_ReturnsFalse();

// Test executing a scalar query with a valid query
await tests.ExecuteScalarAsync_WithValidQuery_ReturnsResult();

// Test executing a scalar query with parameters
await tests.ExecuteScalarAsync_WithParameters_ReturnsCorrectResult();

// Test executing a scalar query with an invalid query
await tests.ExecuteScalarAsync_WithInvalidQuery_ThrowsQueryExecutionException();

// Test executing a non-query with a valid query
await tests.ExecuteNonQueryAsync_WithValidQuery_ExecutesSuccessfully();

// Test executing a non-query with parameters
await tests.ExecuteNonQueryAsync_WithParameters_ExecutesSuccessfully();

// Test executing a query with a valid query
await tests.ExecuteQueryAsync_WithValidQuery_ReturnsResults();

// Test executing a query with parameters
await tests.ExecuteQueryAsync_WithParameters_ReturnsFilteredResults();

// Test getting the database provider
tests.GetDatabaseProvider_ReturnsCorrectProvider();

// Test getting the database provider after construction
tests.GetDatabaseProvider_AfterConstruction_ReturnsDefault();
```

## QueryProfilerTests

The `QueryProfilerTests` class provides unit tests for the `QueryProfiler` class, ensuring its profiling functionality works as expected.

### Example Usage

```csharp
using DotnetMicroOrm.Profiling;

// Create an instance of the test class
var profiler = new QueryProfilerTests();

// Test profiling a successful operation
await profiler.ProfileAsync_SuccessfulOperation_RecordsProfileWithCorrectDuration();

// Test profiling when disabled
await profiler.ProfileAsync_WhenDisabled_DoesNotRecordProfiles();

// Test profiling a failing operation
await profiler.ProfileAsync_FailingOperation_RecordsFailedProfileAndRethrows();

// Test getting a summary of multiple queries
await profiler.GetSummary_MultipleQueries_ReturnsCorrectAggregates();

// Test clearing all profiles
await profiler.Clear_RemovesAllProfiles();

// Test constructor with max profiles exceeded
await profiler.Constructor_MaxProfilesExceeded_EvictsOldEntries();
```

// ... goes in between
