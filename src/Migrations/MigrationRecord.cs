#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Migrations;

/// <summary>
/// Persisted record of a migration that has been applied to the database.
/// Stored in the <c>_MigrationHistory</c> table managed by <see cref="MigrationRunner"/>.
/// </summary>
public sealed class MigrationRecord
{
    /// <summary>Auto-incremented row identifier.</summary>
    public int Id { get; set; }

    /// <summary>Version string matching <see cref="IMigration.Version"/>.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Description copied from <see cref="IMigration.Description"/>.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the migration was applied.</summary>
    public DateTime AppliedAt { get; set; }

    /// <summary><c>true</c> if the migration completed without error.</summary>
    public bool Success { get; set; }

    /// <summary>Error message when the migration failed; <c>null</c> on success.</summary>
    public string? ErrorMessage { get; set; }
}
