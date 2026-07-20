#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Migrations;

/// <summary>
/// Discovers, orders, and applies pending <see cref="IMigration"/> implementations.
/// </summary>
public interface IMigrationRunner
{
    /// <summary>
    /// Applies all pending migrations in ascending version order.
    /// Already-applied migrations are skipped.
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Applies migrations up to and including <paramref name="targetVersion"/>.
    /// </summary>
    Task MigrateToAsync(string targetVersion);

    /// <summary>
    /// Rolls back all migrations that were applied after <paramref name="targetVersion"/>,
    /// in descending version order.
    /// </summary>
    Task RollbackToAsync(string targetVersion);

    /// <summary>
    /// Returns all migration records persisted in the history table,
    /// ordered by version ascending.
    /// </summary>
    Task<IReadOnlyList<MigrationRecord>> GetAppliedMigrationsAsync();

    /// <summary>
    /// Returns the versions of registered migrations that have not yet been applied.
    /// </summary>
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync();
}
