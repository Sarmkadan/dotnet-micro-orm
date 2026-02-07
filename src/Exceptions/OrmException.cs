// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Base exception for all ORM-related errors
/// </summary>
public class OrmException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? ErrorContext { get; set; }

    public OrmException(string message, string? errorCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public OrmException WithContext(string key, object value)
    {
        ErrorContext ??= [];
        ErrorContext[key] = value;
        return this;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (ErrorCode != null)
            baseString += $"\nError Code: {ErrorCode}";
        if (ErrorContext?.Count > 0)
            baseString += $"\nContext: {string.Join(", ", ErrorContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        return baseString;
    }
}

/// <summary>
/// Thrown when database connection fails
/// </summary>
public class DatabaseConnectionException : OrmException
{
    public DatabaseConnectionException(string message, Exception? innerException = null)
        : base(message, "DB_CONNECTION_ERROR", innerException) { }
}

/// <summary>
/// Thrown when entity mapping fails
/// </summary>
public class EntityMappingException : OrmException
{
    public EntityMappingException(string message, string? propertyName = null)
        : base(message, "ENTITY_MAPPING_ERROR")
    {
        if (propertyName != null)
            WithContext("Property", propertyName);
    }
}

/// <summary>
/// Thrown when query execution fails
/// </summary>
public class QueryExecutionException : OrmException
{
    public QueryExecutionException(string message, string? query = null, Exception? innerException = null)
        : base(message, "QUERY_EXECUTION_ERROR", innerException)
    {
        if (query != null)
            WithContext("Query", query);
    }
}

/// <summary>
/// Thrown when entity validation fails
/// </summary>
public class EntityValidationException : OrmException
{
    public List<string> ValidationErrors { get; } = [];

    public EntityValidationException(string message, List<string>? errors = null)
        : base(message, "ENTITY_VALIDATION_ERROR")
    {
        if (errors != null)
            ValidationErrors.AddRange(errors);
    }
}

/// <summary>
/// Thrown when concurrency conflict occurs
/// </summary>
public class ConcurrencyException : OrmException
{
    public ConcurrencyException(string message, object? entityKey = null)
        : base(message, "CONCURRENCY_CONFLICT")
    {
        if (entityKey != null)
            WithContext("EntityKey", entityKey);
    }
}
