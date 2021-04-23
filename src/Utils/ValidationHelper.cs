// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Provides validation helper methods for common data types and business rules.
/// Used to validate user input before database operations and API calls.
/// Provides detailed error messages for validation failures.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates required string field
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates string length
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateLength(
        string? value, int minLength, int maxLength, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
            return (false, $"{fieldName} is required");

        if (value.Length < minLength)
            return (false, $"{fieldName} must be at least {minLength} characters");

        if (value.Length > maxLength)
            return (false, $"{fieldName} must not exceed {maxLength} characters");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");

        if (!email.IsValidEmail())
            return (false, "Email format is invalid");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates password strength
    /// Requires: at least 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
    /// </summary>
    public static (bool isValid, string errorMessage) ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required");

        var validations = new[]
        {
            (password.Length >= 8, "at least 8 characters"),
            (password.ContainsUpperCase(), "an uppercase letter"),
            (password.ContainsLowerCase(), "a lowercase letter"),
            (password.ContainsDigit(), "a digit"),
            (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};:'"",<>.?/]"), "a special character")
        };

        var missing = validations
            .Where(v => !v.Item1)
            .Select(v => v.Item2)
            .ToList();

        if (missing.Count > 0)
        {
            var requirements = string.Join(", ", missing);
            return (false, $"Password must contain {requirements}");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates numeric range
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateRange(
        int value, int minValue, int maxValue, string fieldName)
    {
        if (value < minValue || value > maxValue)
            return (false, $"{fieldName} must be between {minValue} and {maxValue}");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates decimal range
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateRange(
        decimal value, decimal minValue, decimal maxValue, string fieldName)
    {
        if (value < minValue || value > maxValue)
            return (false, $"{fieldName} must be between {minValue} and {maxValue}");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a value is positive
    /// </summary>
    public static (bool isValid, string errorMessage) ValidatePositive(decimal value, string fieldName)
    {
        if (value <= 0)
            return (false, $"{fieldName} must be greater than zero");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that a value is non-negative
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateNonNegative(decimal value, string fieldName)
    {
        if (value < 0)
            return (false, $"{fieldName} cannot be negative");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates phone number format
    /// </summary>
    public static (bool isValid, string errorMessage) ValidatePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return (false, "Phone number is required");

        // Basic international phone validation (simple pattern)
        if (!Regex.IsMatch(phoneNumber, @"^\+?[1-9]\d{1,14}$"))
            return (false, "Phone number format is invalid");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates URL format
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (false, "URL is required");

        if (!url.IsValidUrl())
            return (false, "URL format is invalid");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates that collection has items
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateNotEmpty<T>(
        IEnumerable<T>? collection, string fieldName)
    {
        if (collection is null || !collection.Any())
            return (false, $"{fieldName} must contain at least one item");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates collection size
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateCollectionSize<T>(
        IEnumerable<T>? collection, int minSize, int maxSize, string fieldName)
    {
        var list = collection?.ToList() ?? [];

        if (list.Count < minSize)
            return (false, $"{fieldName} must contain at least {minSize} item(s)");

        if (list.Count > maxSize)
            return (false, $"{fieldName} must not contain more than {maxSize} item(s)");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates date is in the future
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateFutureDate(DateTime value, string fieldName)
    {
        if (value <= DateTime.UtcNow)
            return (false, $"{fieldName} must be in the future");

        return (true, string.Empty);
    }

    /// <summary>
    /// Validates date is in the past
    /// </summary>
    public static (bool isValid, string errorMessage) ValidatePastDate(DateTime value, string fieldName)
    {
        if (value >= DateTime.UtcNow)
            return (false, $"{fieldName} must be in the past");

        return (true, string.Empty);
    }

    /// <summary>
    /// Runs all provided validations and returns first error found
    /// </summary>
    public static (bool isValid, string errorMessage) ValidateAll(
        params (bool isValid, string errorMessage)[] validations)
    {
        foreach (var validation in validations)
        {
            if (!validation.isValid)
                return validation;
        }

        return (true, string.Empty);
    }
}
