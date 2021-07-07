#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Provides validation helpers for ORM-related exceptions
/// </summary>
public static class OrmExceptionValidation
{
    /// <summary>
    /// Validates an <see cref="OrmException"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <returns>A read-only list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this OrmException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate base exception properties
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            errors.Add("Message cannot be null, empty, or whitespace.");
        }

        if (value.ErrorCode is null or { Length: 0 })
        {
            errors.Add("ErrorCode must be a non-empty string.");
        }

        // Validate derived exception types
        switch (value)
        {
            case EntityValidationException entityValidation:
                ValidateEntityValidationException(entityValidation, errors);
                break;
            case EntityMappingException entityMapping:
                ValidateEntityMappingException(entityMapping, errors);
                break;
            case QueryExecutionException queryExecution:
                ValidateQueryExecutionException(queryExecution, errors);
                break;
            case ConcurrencyException concurrency:
                ValidateConcurrencyException(concurrency, errors);
                break;
            case DatabaseConnectionException dbConnection:
                ValidateDatabaseConnectionException(dbConnection, errors);
                break;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified exception is valid.
    /// </summary>
    /// <param name="value">The exception to check</param>
    /// <returns>True if the exception is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this OrmException? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the exception and throws an <see cref="ArgumentException"/> if invalid.
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is invalid, containing a list of validation problems</exception>
    public static void EnsureValid(this OrmException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"OrmException is invalid. Validation errors:\n- {string.Join("\n- ", errors)}",
                nameof(value));
        }
    }

    private static void ValidateEntityValidationException(EntityValidationException exception, List<string> errors)
    {
        if (exception.ValidationErrors is null)
        {
            errors.Add("EntityValidationException.ValidationErrors cannot be null.");
        }
        else if (exception.ValidationErrors.Count == 0)
        {
            errors.Add("EntityValidationException must have at least one validation error in ValidationErrors list.");
        }
    }

    private static void ValidateEntityMappingException(EntityMappingException exception, List<string> errors)
    {
        // EntityMappingException inherits from OrmException and adds a propertyName parameter
        // The propertyName is optional, so we only validate if it was provided
        if (exception.ErrorContext?.TryGetValue("Property", out var propertyValue) == true
            && propertyValue is string propertyName
            && string.IsNullOrWhiteSpace(propertyName))
        {
            errors.Add("EntityMappingException.Property context value cannot be null, empty, or whitespace.");
        }
    }

    private static void ValidateQueryExecutionException(QueryExecutionException exception, List<string> errors)
    {
        if (exception.ErrorContext?.TryGetValue("Query", out var queryValue) == true
            && queryValue is string query
            && string.IsNullOrWhiteSpace(query))
        {
            errors.Add("QueryExecutionException.Query context value cannot be null, empty, or whitespace.");
        }
    }

    private static void ValidateConcurrencyException(ConcurrencyException exception, List<string> errors)
    {
        // ConcurrencyException adds an entityKey parameter which is optional
        // No validation needed for object values
    }

    private static void ValidateDatabaseConnectionException(DatabaseConnectionException exception, List<string> errors)
    {
        // DatabaseConnectionException inherits from OrmException
        // No additional validation needed beyond base exception
    }
}