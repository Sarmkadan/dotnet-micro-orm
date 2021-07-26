#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides validation helpers for the User entity
/// </summary>
public static class UserValidation
{
    /// <summary>
    /// Validates a user instance and returns a list of human-readable validation errors
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <returns>Empty list if valid, otherwise list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this User value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        ValidateRequiredString(value.Username, nameof(value.Username), 3, 50, errors);
        ValidateRequiredString(value.Email, nameof(value.Email), 5, 100, errors);
        ValidateRequiredString(value.PasswordHash, nameof(value.PasswordHash), 32, int.MaxValue, errors);

        // Validate email format
        if (!string.IsNullOrEmpty(value.Email) && !value.Email.Contains('@'))
        {
            errors.Add("Email must contain '@' symbol");
        }

        // Validate optional string properties
        ValidateOptionalString(value.FirstName, nameof(value.FirstName), 50, errors);
        ValidateOptionalString(value.LastName, nameof(value.LastName), 50, errors);
        ValidateOptionalString(value.PhoneNumber, nameof(value.PhoneNumber), 20, errors);

        // Boolean properties are always valid as they are non-nullable bool type

        // Validate date properties
        ValidateDateProperty(value.LastLoginDate, nameof(value.LastLoginDate),
            allowFuture: true, allowPast: true, errors);
        ValidateDateProperty(value.CreatedDate, nameof(value.CreatedDate),
            allowFuture: false, allowPast: true, errors);
        ValidateDateProperty(value.ModifiedDate, nameof(value.ModifiedDate),
            allowFuture: true, allowPast: true, errors);

        // Validate version
        if (value.Version <= 0)
        {
            errors.Add("Version must be a positive integer");
        }

        // Validate navigation properties
        if (value.Orders is null)
        {
            errors.Add("Orders collection cannot be null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified user is valid
    /// </summary>
    /// <param name="value">The user to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this User value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified user is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if user is invalid, containing validation errors</exception>
    public static void EnsureValid(this User value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"User validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    private static void ValidateRequiredString(string value, string propertyName, int minLength, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{propertyName} cannot be null or whitespace");
            return;
        }

        if (value.Length < minLength)
        {
            errors.Add($"{propertyName} must be at least {minLength} characters long");
        }

        if (value.Length > maxLength)
        {
            errors.Add($"{propertyName} cannot exceed {maxLength} characters");
        }
    }

    private static void ValidateOptionalString(string? value, string propertyName, int maxLength, List<string> errors)
    {
        if (value is null)
        {
            return;
        }

        if (value.Length > maxLength)
        {
            errors.Add($"{propertyName} cannot exceed {maxLength} characters");
        }
    }

    private static void ValidateDateProperty(DateTime? dateValue, string propertyName, bool allowFuture, bool allowPast, List<string> errors)
    {
        if (dateValue is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var utcDate = dateValue.Value.ToUniversalTime();

        if (!allowFuture && utcDate > now)
        {
            errors.Add($"{propertyName} cannot be in the future");
        }

        if (!allowPast && utcDate < now.AddMinutes(-1))
        {
            errors.Add($"{propertyName} cannot be in the past");
        }

        // Check for default DateTime (uninitialized)
        if (dateValue.Value == default)
        {
            errors.Add($"{propertyName} has not been initialized");
        }
    }
}