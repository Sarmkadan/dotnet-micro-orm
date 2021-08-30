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
    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    public DatabaseContext(string connectionString, DatabaseProvider provider = DatabaseProvider.SqlServer)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty or whitespace.", nameof(connectionString));

        _connectionString = connectionString;
        _provider = provider;
    }

    // Opens database connection
    public async Task<bool> OpenAsync()
    {
        try
        {
            if (_connection?.State != ConnectionState.Open)
            {
                _connection = CreateConnection();
                await _connection.OpenAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new DatabaseConnectionException($"Failed to open database connection: {ex.Message}", ex);
        }
    }

    // Closes database connection
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

    // Tests database connection
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

    // Executes scalar query
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
            return await command.ExecuteScalarAsync();
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Scalar query execution failed: {ex.Message}", query, ex);
        }
    }

    // Executes query returning rows
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));

        await OpenAsync();
        var results = new List<Dictionary<string, object>>();

        await using var command = _connection!.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = Constants.OrmConstants.DefaultCommandTimeout;

        ApplyParameters(command, parameters);

        try
        {
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
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Query execution failed: {ex.Message}", query, ex);
        }
    }

    // Executes non-query command
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
            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new QueryExecutionException($"Non-query execution failed: {ex.Message}", query, ex);
        }
    }

    // Executes query returning rows as an IAsyncEnumerable
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
            reader = await command.ExecuteReaderAsync();
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

    // Begins transaction
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

    // Commits transaction
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

    // Rollbacks transaction
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

    public DatabaseProvider GetDatabaseProvider() => _provider;

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
