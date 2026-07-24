#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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

    /// <summary>
    /// Returns detailed information about pending migrations including their Up SQL scripts.
    /// </summary>
    /// <returns>A task that resolves to a read-only list of pending migrations with their SQL scripts.</returns>
    Task<IReadOnlyList<PendingMigration>> GetPendingMigrationsWithDetailsAsync();

    /// <summary>
    /// Generates the SQL script that would be executed to apply the specified migration.
    /// </summary>
    /// <param name="version">The version of the migration to generate SQL for.</param>
    /// <returns>A task that resolves to the SQL script string.</returns>
    /// <exception cref="ArgumentException">Thrown when version is null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no migration with the specified version exists.</exception>
    Task<string> GenerateUpSqlAsync(string version);

    /// <summary>
    /// Generates the SQL script that would be executed to roll back the specified migration.
    /// </summary>
    /// <param name="version">The version of the migration to generate SQL for.</param>
    /// <returns>A task that resolves to the SQL script string.</returns>
    /// <exception cref="ArgumentException">Thrown when version is null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no migration with the specified version exists.</exception>
    Task<string> GenerateDownSqlAsync(string version);

    /// <summary>
    /// Executes migrations in dry-run mode, which validates scripts without applying them to the database.
    /// </summary>
    /// <param name="targetVersion">The target version to migrate to (inclusive). If null or empty, migrates to latest.</param>
    /// <returns>A task that resolves to a list of generated SQL scripts for each migration that would be applied.</returns>
    /// <exception cref="ArgumentException">Thrown when targetVersion is whitespace.</exception>
    Task<IReadOnlyList<string>> MigrateDryRunAsync(string? targetVersion = null);

    /// <summary>
    /// Executes rollback in dry-run mode, which validates scripts without applying them to the database.
    /// </summary>
    /// <param name="targetVersion">The target version to roll back to (exclusive).</param>
    /// <returns>A task that resolves to a list of generated SQL scripts for each migration that would be rolled back.</returns>
    /// <exception cref="ArgumentException">Thrown when targetVersion is null or whitespace.</exception>
    Task<IReadOnlyList<string>> RollbackDryRunAsync(string targetVersion);

    /// <summary>
    /// Represents a pending migration with its SQL script.
    /// </summary>
    sealed class PendingMigration
    {
        /// <summary>Gets the migration version.</summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>Gets the migration description.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>Gets the SQL script that would be executed to apply this migration.</summary>
        public string UpSql { get; init; } = string.Empty;
    }
}