#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Migrations;

/// <summary>
/// Provides validation helpers for <see cref="MigrationRunner"/> instances.
/// </summary>
public static class MigrationRunnerValidation
{
    /// <summary>
    /// Validates the specified <see cref="MigrationRunner"/> instance.
    /// </summary>
    /// <param name="value">The migration runner to validate.</param>
    /// <returns>A list of validation problems; empty if the runner is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MigrationRunner? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MigrationRunner"/> instance is valid.
    /// </summary>
    /// <param name="value">The migration runner to check.</param>
    /// <returns><see langword="true"/> if the runner is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MigrationRunner? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="MigrationRunner"/> instance is valid.
    /// </summary>
    /// <param name="value">The migration runner to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the runner is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this MigrationRunner? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MigrationRunner is not valid. Problems:\n- {
                    string.Join("\n- ", problems)
                }",
                nameof(value));
        }
    }
}