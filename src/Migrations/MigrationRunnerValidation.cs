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

        // Validate context
    if (value.Context is null)
    {
        problems.Add("MigrationRunner context is null.");
    }
    else
    {
        // Validate context can connect
        try
        {
            var connectionTest = value.Context.TestConnectionAsync().GetAwaiter().GetResult();
            if (!connectionTest)
            {
                problems.Add("Database connection test failed.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Database connection test failed: {ex.Message}");
        }
    }

    // Validate migrations list
    if (value.Migrations is null)
    {
        problems.Add("MigrationRunner migrations collection is null.");
    }
    else if (value.Migrations.Count == 0)
    {
        problems.Add("MigrationRunner has no migrations to apply.");
    }
    else
    {
        // Validate each migration
        foreach (var migration in value.Migrations)
        {
            if (migration is null)
            {
                problems.Add("MigrationRunner contains a null migration.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(migration.Version))
            {
                problems.Add($"Migration has empty or null version: {migration.Description ?? "unknown"}.");
            }

            if (string.IsNullOrWhiteSpace(migration.Description))
            {
                problems.Add($"Migration '{migration.Version ?? "unknown"}' has empty or null description.");
            }
        }
    }

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
                $"MigrationRunner is not valid. Problems:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }",
                nameof(value));
        }
    }
}