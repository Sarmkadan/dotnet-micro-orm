#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Base exception for all ORM-related errors
/// </summary>
public class sealed OrmException : Exception
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
        if (ErrorCode is not null)
            baseString += $"\nError Code: {ErrorCode}";
        if (ErrorContext?.Count > 0)
            baseString += $"\nContext: {string.Join(", ", ErrorContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        return baseString;
    }
}

/// <summary>
/// Thrown when database connection fails
/// </summary>
public class sealed DatabaseConnectionException : OrmException
{
    public DatabaseConnectionException(string message, Exception? innerException = null)
        : base(message, "DB_CONNECTION_ERROR", innerException) { }
}

/// <summary>
/// Thrown when entity mapping fails
/// </summary>
public class sealed EntityMappingException : OrmException
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
public class sealed QueryExecutionException : OrmException
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
public class sealed EntityValidationException : OrmException
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
public class sealed ConcurrencyException : OrmException
{
    public ConcurrencyException(string message, object? entityKey = null)
        : base(message, "CONCURRENCY_CONFLICT")
    {
        if (entityKey is not null)
            WithContext("EntityKey", entityKey);
    }
}
