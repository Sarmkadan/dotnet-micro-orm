#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides validation helpers for the <see cref="Category"/> entity
/// </summary>
public static class CategoryValidation
{
    /// <summary>
    /// Validates a category and returns a list of human-readable validation errors
    /// </summary>
    /// <param name="value">The category to validate</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this Category value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Name))
            errors.Add("Category name is required");
        else if (value.Name.Length < 2)
            errors.Add("Category name must be at least 2 characters long");
        else if (value.Name.Length > 100)
            errors.Add("Category name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(value.Slug))
            errors.Add("Category slug is required");
        else if (value.Slug.Length < 2)
            errors.Add("Category slug must be at least 2 characters long");
        else if (value.Slug.Length > 100)
            errors.Add("Category slug cannot exceed 100 characters");

        // Validate Description if set
        if (value.Description is not null && value.Description.Length > 500)
            errors.Add("Category description cannot exceed 500 characters");

        // Validate DisplayOrder (must be non-negative)
        if (value.DisplayOrder < 0)
            errors.Add("Display order cannot be negative");

        // Validate CreatedDate (should not be default/MinValue)
        if (value.CreatedDate == default)
            errors.Add("Created date must be set to a valid date");
        else if (value.CreatedDate > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Created date cannot be in the future");

        // Validate ParentCategoryId if set
        if (value.ParentCategoryId.HasValue && value.ParentCategoryId.Value < 0)
            errors.Add("Parent category ID cannot be negative");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a category is valid
    /// </summary>
    /// <param name="value">The category to check</param>
    /// <returns>True if the category is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this Category value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a category is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The category to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the category is invalid, containing the validation errors</exception>
    public static void EnsureValid(this Category value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Category validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}