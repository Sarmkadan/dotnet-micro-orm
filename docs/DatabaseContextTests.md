# DatabaseContextTests

Unit tests for the `DatabaseContext` class in the dotnet-micro-orm project, verifying core database operations, connection handling, and provider configuration. Focuses on constructor validation, connection lifecycle, query execution, and provider retrieval.

## API

### `Constructor_WithNullConnectionString_ThrowsArgumentNullException`
Verifies that the constructor throws an `ArgumentNullException` when provided with a null connection string. Ensures strict input validation for critical configuration.

### `Constructor_WithEmptyConnectionString_ThrowsArgumentException`
Validates that an empty connection string triggers an `ArgumentException`. Confirms that the constructor enforces non-empty connection strings.

### `Constructor_WithValidConnectionString_CreatesInstance`
Ensures that a valid connection string successfully instantiates the `DatabaseContext`. Confirms basic object creation under correct conditions.

### `OpenAsync_WithValidConnection_OpensSuccessfully`
Tests that `OpenAsync` successfully opens a connection when provided with valid connection parameters. Verifies the connection lifecycle under normal conditions.

### `OpenAsync_WithInvalidConnection_ThrowsDatabaseConnectionException`
Confirms that `OpenAsync` throws a `DatabaseConnectionException` when the connection cannot be established. Validates error handling for connection failures.

### `CloseAsync_WithOpenConnection_ClosesSuccessfully`
Ensures that `CloseAsync` successfully closes an open connection. Verifies proper cleanup of resources after use.

### `TestConnectionAsync_WithValidConnection_ReturnsTrue`
Checks that `TestConnectionAsync` returns `true` when the connection is valid. Validates the ability to test connection health under normal conditions.

### `TestConnectionAsync_WithInvalidConnection_ReturnsFalse`
Confirms that `TestConnectionAsync` returns `false` when the connection is invalid. Ensures accurate feedback on connection state.

### `ExecuteScalarAsync_WithValidQuery_ReturnsResult`
Verifies that `ExecuteScalarAsync` returns a result when executing a valid scalar query. Confirms basic query execution under correct conditions.

### `ExecuteScalarAsync_WithParameters_ReturnsCorrectResult`
Ensures that `ExecuteScalarAsync` returns the correct result when parameters are provided. Validates parameterized query handling.

### `ExecuteScalarAsync_WithInvalidQuery_ThrowsQueryExecutionException`
Confirms that `ExecuteScalarAsync` throws a `QueryExecutionException` when the query is invalid. Validates error handling for malformed queries.

### `ExecuteNonQueryAsync_WithValidQuery_ExecutesSuccessfully`
Tests that `ExecuteNonQueryAsync` successfully executes a valid non-query (e.g., INSERT, UPDATE, DELETE). Verifies write operations under normal conditions.

### `ExecuteNonQueryAsync_WithParameters_ExecutesSuccessfully`
Ensures that `ExecuteNonQueryAsync` executes successfully when parameters are provided. Validates parameterized write operations.

### `ExecuteQueryAsync_WithValidQuery_ReturnsResults`
Confirms that `ExecuteQueryAsync` returns results when executing a valid query. Validates read operations under normal conditions.

### `ExecuteQueryAsync_WithParameters_ReturnsFilteredResults`
Ensures that `ExecuteQueryAsync` returns filtered results when parameters are provided. Validates parameterized read operations.

### `GetDatabaseProvider_ReturnsCorrectProvider`
Verifies that `GetDatabaseProvider` returns the correct provider instance after construction. Confirms provider configuration retrieval.

### `GetDatabaseProvider_AfterConstruction_ReturnsDefault`
Ensures that `GetDatabaseProvider` returns the default provider when called immediately after construction. Validates default state consistency.

## Usage
