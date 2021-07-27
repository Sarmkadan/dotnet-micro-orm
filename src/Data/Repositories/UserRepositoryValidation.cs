#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="UserRepository"/> instances
/// </summary>
public static class UserRepositoryValidation
{
    /// <summary>
    /// Validates the repository instance and its state
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this UserRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Repository-level validations
        // Note: Repository validation primarily ensures the repository instance itself is valid
        // rather than validating repository state, as repositories are stateless services

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository instance is valid
    /// </summary>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if the repository is valid; otherwise, false</returns>
    public static bool IsValid(this UserRepository value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures the repository instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid with a list of problems</exception>
    public static void EnsureValid(this UserRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"UserRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}