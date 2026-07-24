#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Base exception for all ORM-related errors
/// </summary>
public class OrmException : DotnetMicroOrmException
{
    public OrmException(string message, string? errorCode = null, Exception? innerException = null)
        : base(message, errorCode, innerException)
    {
    }

    public new OrmException WithContext(string key, object value)
    {
        base.WithContext(key, value);
        return this;
    }
}

/// <summary>
/// Thrown when database connection fails
/// </summary>
public sealed class DatabaseConnectionException : OrmException
{
    /// <summary>
    /// Indicates whether the underlying failure is transient (e.g. a timeout, deadlock, or
    /// temporary service throttling) and therefore safe to retry. Consulted by
    /// <see cref="DotnetMicroOrm.Data.ConnectionRetryPolicy"/> when deciding whether to retry the
    /// failed operation.
    /// </summary>
    public bool IsTransient { get; }

    /// <summary>
    /// Creates a new <see cref="DatabaseConnectionException"/>.
    /// </summary>
    /// <param name="message">Human-readable description of the failure.</param>
    /// <param name="innerException">The underlying exception raised by the ADO.NET provider, if any.</param>
    /// <param name="isTransient">
    /// Whether the failure is transient and worth retrying. Defaults to <c>false</c> when not specified.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    public DatabaseConnectionException(string message, Exception? innerException = null, bool isTransient = false)
        : base(message, "DB_CONNECTION_ERROR", innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        IsTransient = isTransient;
    }
}

/// <summary>
/// Thrown when entity mapping fails
/// </summary>
public sealed class EntityMappingException : OrmException
{
    public EntityMappingException(string message, string? propertyName = null)
        : base(message, "ENTITY_MAPPING_ERROR")
    {
        if (propertyName is not null)
            WithContext("Property", propertyName);
    }
}

/// <summary>
/// Thrown when query execution fails
/// </summary>
public sealed class QueryExecutionException : OrmException
{
    public QueryExecutionException(string message, string? query = null, Exception? innerException = null)
        : base(message, "QUERY_EXECUTION_ERROR", innerException)
    {
        if (query is not null)
            WithContext("Query", query);
    }
}

/// <summary>
/// Thrown when entity validation fails
/// </summary>
public sealed class EntityValidationException : OrmException
{
    public List<string> ValidationErrors { get; } = [];

    public EntityValidationException(string message, List<string>? errors = null)
        : base(message, "ENTITY_VALIDATION_ERROR")
    {
        if (errors is not null)
            ValidationErrors.AddRange(errors);
    }
}

/// <summary>
/// Thrown when concurrency conflict occurs
/// </summary>
public sealed class ConcurrencyException : OrmException
{
    public ConcurrencyException(string message, object? entityKey = null)
        : base(message, "CONCURRENCY_CONFLICT")
    {
        if (entityKey is not null)
            WithContext("EntityKey", entityKey);
    }
}
