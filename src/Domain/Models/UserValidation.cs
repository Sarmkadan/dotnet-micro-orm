#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Globalization;
using System.Text.RegularExpressions;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="User"/> entity
/// </summary>
public static class UserValidation
{
    /// <summary>
    /// Validates a user instance and returns a list of human-readable validation errors
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <returns>Empty list if valid, otherwise list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this User value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        ValidateRequiredString(value.Username, nameof(value.Username), 3, 50, errors);
        ValidateEmail(value.Email, nameof(value.Email), errors);
        ValidateRequiredString(value.PasswordHash, nameof(value.PasswordHash), 32, int.MaxValue, errors);

        // Validate optional string properties
        ValidateOptionalString(value.FirstName, nameof(value.FirstName), 50, errors);
        ValidateOptionalString(value.LastName, nameof(value.LastName), 50, errors);
        ValidatePhoneNumber(value.PhoneNumber, nameof(value.PhoneNumber), errors);

        // Validate date properties
        ValidateDateProperty(value.LastLoginDate, nameof(value.LastLoginDate), allowFuture: true, allowPast: true, errors);
        ValidateDateProperty(value.CreatedDate, nameof(value.CreatedDate), allowFuture: false, allowPast: true, errors);
        ValidateDateProperty(value.ModifiedDate, nameof(value.ModifiedDate), allowFuture: true, allowPast: true, errors);

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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this User value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified user is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The user to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
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

    private static void ValidateEmail(string value, string propertyName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{propertyName} cannot be null or whitespace");
            return;
        }

        // Basic email format validation using regex
        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(value, emailPattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)))
        {
            errors.Add($"{propertyName} must be a valid email address");
        }

        if (value.Length < 5)
        {
            errors.Add($"{propertyName} must be at least 5 characters long");
        }

        if (value.Length > 100)
        {
            errors.Add($"{propertyName} cannot exceed 100 characters");
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

    private static void ValidatePhoneNumber(string? value, string propertyName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        // Basic international phone number validation (digits, spaces, dashes, plus, parentheses)
        const string phonePattern = @"^[+\d\s\-\(\)]{8,20}$";
        if (!Regex.IsMatch(value, phonePattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)))
        {
            errors.Add($"{propertyName} must be a valid phone number (8-20 characters)");
        }

        if (value.Length > 20)
        {
            errors.Add($"{propertyName} cannot exceed 20 characters");
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

        // Check for default DateTime (uninitialized) - only for non-nullable DateTime properties
        // Note: CreatedDate is non-nullable, so we should check it
        if (dateValue.Value == default && propertyName != nameof(User.LastLoginDate) && propertyName != nameof(User.ModifiedDate))
        {
            errors.Add($"{propertyName} has not been initialized");
        }
    }
}