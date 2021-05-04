#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Migrations;

using DotnetMicroOrm.Data;

/// <summary>
/// Represents a single, versioned schema change that can be applied or rolled back.
/// Implement this interface for each migration step and register the implementations
/// with the DI container so <see cref="IMigrationRunner"/> can discover them.
/// </summary>
public interface IMigration
{
    /// <summary>
    /// Sortable version string, e.g. <c>"20240101_001"</c> (YYYYMMdd_seq).
    /// Migrations are applied in ascending lexicographic order of this value.
    /// </summary>
    string Version { get; }

    /// <summary>Human-readable description of what this migration does.</summary>
    string Description { get; }

    /// <summary>Applies the migration (forward direction).</summary>
    Task UpAsync(IDatabaseContext context);

    /// <summary>Reverts the migration (backward direction).</summary>
    Task DownAsync(IDatabaseContext context);
}
