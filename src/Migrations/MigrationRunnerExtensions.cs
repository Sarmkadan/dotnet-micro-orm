using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetMicroOrm.Migrations
{
    /// <summary>
    /// Extension methods that add convenience operations to <see cref="MigrationRunner"/>.
    /// </summary>
    public static class MigrationRunnerExtensions
    {
        /// <summary>
        /// Retrieves the most recently applied migration, or <c>null</c> if no migrations have been applied.
        /// </summary>
        /// <param name="runner">The <see cref="MigrationRunner"/> instance.</param>
        /// <returns>A task that resolves to the latest <see cref="MigrationRecord"/>, or <c>null</c> when none exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runner"/> is <c>null</c>.</exception>
        public static async Task<MigrationRecord?> GetLatestAppliedMigrationAsync(this MigrationRunner runner)
        {
            ArgumentNullException.ThrowIfNull(runner);
            IReadOnlyList<MigrationRecord> applied = await runner.GetAppliedMigrationsAsync().ConfigureAwait(false);
            return applied.LastOrDefault();
        }

        /// <summary>
        /// Counts the number of pending migrations that have not yet been applied.
        /// </summary>
        /// <param name="runner">The <see cref="MigrationRunner"/> instance.</param>
        /// <returns>A task that resolves to the count of pending migrations.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runner"/> is <c>null</c>.</exception>
        public static async Task<int> GetPendingMigrationsCountAsync(this MigrationRunner runner)
        {
            ArgumentNullException.ThrowIfNull(runner);
            IReadOnlyList<IMigration> pending = await runner.GetPendingMigrationsAsync().ConfigureAwait(false);
            return pending.Count;
        }

        /// <summary>
        /// Executes a full migration (applies all pending migrations) and then returns the complete list of applied migrations.
        /// </summary>
        /// <param name="runner">The <see cref="MigrationRunner"/> instance.</param>
        /// <returns>
        /// A task that resolves to the read‑only list of <see cref="MigrationRecord"/> objects representing all migrations
        /// that have been applied after the operation completes.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="runner"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<MigrationRecord>> MigrateAndGetAppliedAsync(this MigrationRunner runner)
        {
            ArgumentNullException.ThrowIfNull(runner);
            await runner.MigrateAsync().ConfigureAwait(false);
            return await runner.GetAppliedMigrationsAsync().ConfigureAwait(false);
        }
    }
}
