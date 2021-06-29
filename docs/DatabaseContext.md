# DatabaseContext

The `DatabaseContext` class provides a lightweight, asynchronous wrapper around a database connection for the `dotnet-micro-orm` library. It manages connection lifecycle, transaction scoping, and exposes methods for executing queries, commands, and streaming results. The class implements `IAsyncDisposable` to ensure proper resource cleanup.

## API

### `public DatabaseContext(DatabaseProvider provider, string connectionString)`

Constructs a new `DatabaseContext` using the specified database provider and connection string.

- **Parameters**  
  - `provider`: A `DatabaseProvider` enum value indicating the target database system (e.g., SQL Server, PostgreSQL, SQLite).  
  - `connectionString`: The connection string used to connect to the database.

- **Throws**  
  - `ArgumentNullException` if `connectionString` is `null` or empty.  
  - `ArgumentException` if `provider` is not a valid `DatabaseProvider` value.

---

### `public async Task<bool> OpenAsync()`

Opens the underlying database connection asynchronously.

- **Returns**  
  `true` if the connection was successfully opened; `false` if the connection was already open or could not be opened.

- **Throws**  
  - `InvalidOperationException` if the context has been disposed.  
  - `DbException` for provider-specific connection failures.

---

### `public async Task<bool> CloseAsync()`

Closes the underlying database connection asynchronously.

- **Returns**  
  `true` if the connection was successfully closed; `false` if the connection was already closed.

- **Throws**  
  - `InvalidOperationException` if the context has been disposed.

---

### `public async Task<bool> TestConnectionAsync()`

Tests whether the database connection is valid by executing a lightweight probe (e.g., `SELECT 1`).

- **Returns**  
  `true` if the connection is open and responsive; `false` otherwise.

- **Throws**  
  - `InvalidOperationException` if the context has been disposed.

---

### `public async Task<object?> ExecuteScalarAsync(string sql, params object[] parameters)`

Executes a SQL query and returns the first column of the first row in the result set. All other columns and rows are ignored.

- **Parameters**  
  - `sql`: The SQL command text.  
  - `parameters`: Optional parameter values to be passed to the command.

- **Returns**  
  The first column of the first row as an `object?`, or `null` if the result set is empty.

- **Throws**  
  - `InvalidOperationException` if the context is not open or has been disposed.  
  - `DbException` for provider-specific execution errors.

---

### `public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, params object[] parameters)`

Executes a SQL query and returns all rows as a list of dictionaries, where each dictionary maps column names to their values.

- **Parameters**  
  - `sql`: The SQL command text.  
  - `parameters`: Optional parameter values to be passed to the command.

- **Returns**  
  A `List<Dictionary<string, object>>` containing the result set. Returns an empty list if no rows are returned.

- **Throws**  
  - `InvalidOperationException` if the context is not open or has been disposed.  
  - `DbException` for provider-specific execution errors.

---

### `public async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)`

Executes a SQL command that does not return rows (e.g., INSERT, UPDATE, DELETE, DDL).

- **Parameters**  
  - `sql`: The SQL command text.  
  - `parameters`: Optional parameter values to be passed to the command.

- **Returns**  
  The number of rows affected by the command.

- **Throws**  
  - `InvalidOperationException` if the context is not open or has been disposed.  
  - `DbException` for provider-specific execution errors.

---

### `public async IAsyncEnumerable<Dictionary<string, object>> ExecuteStreamAsync(string sql, params object[] parameters)`

Executes a SQL query and streams each row as a `Dictionary<string, object>` using an asynchronous enumerable. Useful for processing large result sets without loading all rows into memory.

- **Parameters**  
  - `sql`: The SQL command text.  
  - `parameters`: Optional parameter values to be passed to the command.

- **Yields**  
  A `Dictionary<string, object>` for each row in the result set.

- **Throws**  
  - `InvalidOperationException` if the context is not open or has been disposed.  
  - `DbException` for provider-specific execution errors.

---

### `public async Task<bool> BeginTransactionAsync()`

Begins a new database transaction on the current connection.

- **Returns**  
  `true` if the transaction was successfully started; `false` if a transaction is already in progress.

- **Throws**  
  - `InvalidOperationException` if the context is not open or has been disposed.

---

### `public async Task<bool> CommitAsync()`

Commits the current database transaction.

- **Returns**  
  `true` if the transaction was successfully committed; `false` if no transaction is active.

- **Throws**  
  - `InvalidOperationException` if the context has been disposed.  
  - `DbException` if the commit fails (e.g., due to a constraint violation).

---

### `public async Task<bool> RollbackAsync()`

Rolls back the current database transaction.

- **Returns**  
  `true` if the transaction was successfully rolled back; `false` if no transaction is active.

- **Throws**  
  - `InvalidOperationException` if the context has been disposed.  
  - `DbException` if the rollback fails.

---

### `public DatabaseProvider GetDatabaseProvider()`

Returns the `DatabaseProvider` value that was specified when the context was constructed.

- **Returns**  
  The `DatabaseProvider` enum value.

---

### `public string GetConnectionString()`

Returns the connection string that was specified when the context was constructed.

- **Returns**  
  The connection string as a `string`.

---

### `public async ValueTask DisposeAsync()`

Asynchronously releases all resources used by the `DatabaseContext`, including closing the connection and rolling back any active transaction.

- **Throws**  
  - `DbException` if the underlying connection fails to close cleanly.

## Usage

### Example 1: Basic query and scalar execution

```csharp
await using var context = new DatabaseContext(DatabaseProvider.Sqlite, "Data Source=app.db");
await context.OpenAsync();

// Execute a scalar query
var count = await context.ExecuteScalarAsync("SELECT COUNT(*) FROM Users");
Console.WriteLine($"User count: {count}");

// Execute a query and read results
var users = await context.ExecuteQueryAsync("SELECT Id, Name FROM Users WHERE Active = @active", true);
foreach (var user in users)
{
    Console.WriteLine($"Id: {user["Id"]}, Name: {user["Name"]}");
}

await context.CloseAsync();
```

### Example 2: Transaction with non-query and streaming

```csharp
await using var context = new DatabaseContext(DatabaseProvider.PostgreSql, "Host=localhost;Database=orders");
await context.OpenAsync();

if (await context.BeginTransactionAsync())
{
    try
    {
        // Insert a new order
        var rowsAffected = await context.ExecuteNonQueryAsync(
            "INSERT INTO Orders (CustomerId, Total) VALUES (@customerId, @total)",
            42, 99.95m);
        Console.WriteLine($"Inserted {rowsAffected} row(s).");

        // Stream recent orders
        await foreach (var order in context.ExecuteStreamAsync(
            "SELECT Id, Total FROM Orders WHERE CreatedAt > @since",
            DateTime.UtcNow.AddDays(-1)))
        {
            Console.WriteLine($"Order {order["Id"]}: {order["Total"]}");
        }

        await context.CommitAsync();
    }
    catch
    {
        await context.RollbackAsync();
        throw;
    }
}
```

## Notes

- **Thread safety**: `DatabaseContext` is not thread-safe. All operations on a single instance should be performed from one thread or synchronized externally. Concurrent calls to `OpenAsync`, `CloseAsync`, or any execution method may lead to undefined behavior.
- **Disposal**: Always dispose the context (via `await using` or explicit `DisposeAsync` call) to ensure the underlying connection is closed and any active transaction is rolled back. Failing to dispose may cause connection leaks.
- **Transaction state**: `BeginTransactionAsync` returns `false` if a transaction is already active. Nested transactions are not supported. Calling `CommitAsync` or `RollbackAsync` when no transaction is active returns `false` and does not throw.
- **Connection state**: `OpenAsync` and `CloseAsync` are idempotent in the sense that they return `false` if the connection is already in the requested state. However, `OpenAsync` will re-establish a closed connection; it does not check for prior disposal.
- **Streaming**: `ExecuteStreamAsync` holds the connection open until the enumeration is complete or the context is disposed. Ensure the enumeration is fully consumed or the context is disposed to avoid blocking the connection pool.
- **Parameter handling**: Parameters are passed positionally. The underlying provider translates them into the appropriate parameter syntax (e.g., `@p0`, `?`). Do not mix named and positional parameters in the same call.
