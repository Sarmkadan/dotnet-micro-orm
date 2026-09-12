#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Data;
using System.Data.Common;
using DotnetMicroOrm.Constants;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Database context managing connections and command execution
/// </summary>
public sealed class DatabaseContext : IDatabaseContext
{
    private readonly string _connectionString;
    private readonly DatabaseProvider _provider;
    private readonly ConnectionRetryPolicy _retryPolicy;
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// The retry policy applied to connection opening and command execution. Defaults to
    /// <see cref="ConnectionRetryPolicy.None"/> (no retries) when none is supplied at construction.
    /// </summary>
    public ConnectionRetryPolicy RetryPolicy => _retryPolicy;

    /// <summary>
    /// Creates a new <see cref="DatabaseContext"/>.
    /// </summary>
    /// <param name="connectionString">The ADO.NET connection string used to connect to the database.</param>
    /// <param name="provider">The database provider the connection string targets.</param>
    /// <param name="retryPolicy">
    /// Optional retry policy applied to connection opening and command execution. When omitted,
    /// <see cref="ConnectionRetryPolicy.None"/> is used and no retries are attempted.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is empty or whitespace.</exception>
    public DatabaseContext(string connectionString, DatabaseProvider provider = DatabaseProvider.SqlServer, ConnectionRetryPolicy? retryPolicy = null)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty or whitespace.", nameof(connectionString));

        _connectionString = connectionString;
        _provider = provider;
        _retryPolicy = retryPolicy ?? ConnectionRetryPolicy.None;
    }

    /// <summary>
    /// Opens the database connection, retrying transient failures per the configured retry policy.
    /// </summary>
    /// <returns><c>true</c> when the connection is open after the operation completes.</returns>
    /// <exception cref="DatabaseConnectionException">Thrown when the connection cannot be opened.</exception>
    public async Task<bool> OpenAsync()
    {
        try
        {
            if (_connection?.State != ConnectionState.Open)
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _connection?.Dispose();
                    _connection = CreateConnection();
                    await _connection.OpenAsync();
                    return true;
                }, _provider);
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new DatabaseConnectionException(
                $"Failed to open database connection: {ex.Message}",
                ex,
                ConnectionRetryPolicy.DefaultTransientClassifier(ex, _provider));
        }
    }

    /// <summary>
    /// Closes the database connection if it is currently open.
    /// </summary>
    /// <returns><c>true</c> when the connection is closed after the operation completes.</returns>
    /// <exception cref="DatabaseConnectionException">Thrown when the connection cannot be closed.</exception>
    public async Task<bool> CloseAsync()
    {
        try
        {
            if (_connection?.State == ConnectionState.Open)
                await _connection.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new DatabaseConnectionException($"Failed to close database connection: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tests the database connection by executing a simple <c>SELECT 1</c> query.
    /// </summary>
    /// <returns><c>true</c> when the connection is usable and the query returns a non-null result; otherwise <c>false</c>.</returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await OpenAsync();
            await using var command = _connection!.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;
            var result = await command.ExecuteScalarAsync();
            return result is not null && result != DBNull.Value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a scalar value.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters to apply to the command.</param>
    /// <returns>The scalar result, or <c>null</c> when the query returns no value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null, empty, or whitespace.</exception>
    /// <exception cref="QueryExecutionException">Thrown when the query execution fails.</exception>
    public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        await OpenAsync();
        await using var command = _connection!.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;

        ApplyParameters(command, parameters);

        try
        {
            return await _retryPolicy.ExecuteAsync(() => command.ExecuteScalarAsync(), _provider);
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Scalar query execution failed: {ex.Message}", query, ex);
        }
    }

    /// <summary>
    /// Executes a query and returns the resulting rows as a list of column dictionaries.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters to apply to the command.</param>
    /// <returns>A list of rows, each represented as a dictionary of column name to value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null, empty, or whitespace.</exception>
    /// <exception cref="QueryExecutionException">Thrown when the query execution fails.</exception>
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        await OpenAsync();

        await using var command = _connection!.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;

        ApplyParameters(command, parameters);

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var results = new List<Dictionary<string, object>>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                return results;
            }, _provider);
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Query execution failed: {ex.Message}", query, ex);
        }
    }

    /// <summary>
    /// Executes a non-query command such as INSERT, UPDATE, or DELETE.
    /// </summary>
    /// <param name="query">The SQL command to execute.</param>
    /// <param name="parameters">Optional parameters to apply to the command.</param>
    /// <returns>The number of rows affected by the command.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null, empty, or whitespace.</exception>
    /// <exception cref="QueryExecutionException">Thrown when the command execution fails.</exception>
    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        await OpenAsync();
        await using var command = _connection!.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;
        command.Transaction = _transaction;

        ApplyParameters(command, parameters);

        try
        {
            return await _retryPolicy.ExecuteAsync(() => command.ExecuteNonQueryAsync(), _provider);
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Non-query execution failed: {ex.Message}", query, ex);
        }
    }

    /// <summary>
    /// Executes a query and returns the resulting rows as an asynchronous enumerable.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters to apply to the command.</param>
    /// <returns>An asynchronous enumerable of rows, each represented as a dictionary of column name to value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null, empty, or whitespace.</exception>
    /// <exception cref="QueryExecutionException">Thrown when the query execution fails.</exception>
    public async IAsyncEnumerable<Dictionary<string, object>> ExecuteStreamAsync(string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        await OpenAsync();

        await using var command = _connection!.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;
        command.Transaction = _transaction;

        ApplyParameters(command, parameters);

        DbDataReader reader;
        try
        {
            reader = await _retryPolicy.ExecuteAsync(() => command.ExecuteReaderAsync(), _provider);
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Streaming query execution failed: {ex.Message}", query, ex);
        }

        await using (reader)
        {
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                yield return row;
            }
        }
    }

    /// <summary>
    /// Begins a new database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <returns><c>true</c> when the transaction is successfully begun.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active.</exception>
    public async Task<bool> BeginTransactionAsync(TransactionIsolationLevel isolationLevel)
    {
        await OpenAsync();
        if (_transaction is not null)
            throw new InvalidOperationException("Transaction already active");

        var dbIsolationLevel = isolationLevel switch
        {
            TransactionIsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
            TransactionIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            TransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
            TransactionIsolationLevel.Snapshot => IsolationLevel.Snapshot,
            _ => IsolationLevel.ReadCommitted
        };

        _transaction = _connection!.BeginTransaction(dbIsolationLevel);
        return true;
    }

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    /// <returns><c>true</c> when the transaction is successfully committed.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active transaction exists to commit.</exception>
    /// <exception cref="OrmException">Thrown when the transaction commit fails.</exception>
    public async Task<bool> CommitAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit");

        try
        {
            await _transaction.CommitAsync();
            _transaction.Dispose();
            _transaction = null;
            return true;
        }
        catch (Exception ex)
        {
            throw new OrmException($"Transaction commit failed: {ex.Message}", innerException: ex);
        }
    }

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    /// <returns><c>true</c> when the transaction is successfully rolled back.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active transaction exists to roll back.</exception>
    /// <exception cref="OrmException">Thrown when the transaction rollback fails.</exception>
    public async Task<bool> RollbackAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to rollback");

        try
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
            return true;
        }
        catch (Exception ex)
        {
            throw new OrmException($"Transaction rollback failed: {ex.Message}", innerException: ex);
        }
    }

    /// <summary>
    /// Gets the database provider associated with this context.
    /// </summary>
    /// <returns>The database provider.</returns>
    public DatabaseProvider GetDatabaseProvider() => _provider;

    /// <summary>
    /// Gets the connection string used by this context.
    /// </summary>
    /// <returns>The connection string.</returns>
    public string GetConnectionString() => _connectionString;

    private DbConnection CreateConnection() => _provider switch
    {
        DatabaseProvider.SqlServer => new SqlConnection(_connectionString),
        DatabaseProvider.Sqlite => new SqliteConnection(_connectionString),
        _ => throw new NotSupportedException($"Database provider {_provider} is not yet supported")
    };

    private void ApplyParameters(DbCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters is null || parameters.Count == 0)
            return;

        foreach (var param in parameters)
        {
            var dbParam = command.CreateParameter();
            dbParam.ParameterName = param.Key;
            dbParam.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(dbParam);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await RollbackAsync().ConfigureAwait(false);
        await CloseAsync().ConfigureAwait(false);
        _connection?.Dispose();
        _transaction?.Dispose();
        _disposed = true;
    }
}
