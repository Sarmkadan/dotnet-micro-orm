using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotnetMicroOrm.Migrations
{
    /// <summary>
    /// Provides extension methods for <see cref="MigrationRecord"/> to facilitate common migration operations.
    /// </summary>
    public static class MigrationRecordExtensions
    {
        /// <summary>
        /// Determines whether the migration was successful based on the <see cref="Success"/> property.
        /// </summary>
        /// <param name="record">The migration record to check.</param>
        /// <returns><see langword="true"/> if the migration succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/></exception>
        public static bool WasSuccessful(this MigrationRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return record.Success;
        }

        /// <summary>
        /// Gets the error message if the migration failed, or an empty string if it succeeded.
        /// </summary>
        /// <param name="record">The migration record to check.</param>
        /// <returns>The error message if available; otherwise, an empty string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/></exception>
        public static string GetErrorMessage(this MigrationRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            return record.ErrorMessage ?? string.Empty;
        }

        /// <summary>
        /// Determines whether this migration was applied before a specified cutoff date.
        /// </summary>
        /// <param name="record">The migration record to check.</param>
        /// <param name="cutoffDate">The cutoff date to compare against.</param>
        /// <returns><see langword="true"/> if the migration was applied before the cutoff date; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="cutoffDate"/> is <see langword="null"/></exception>
        public static bool WasAppliedBefore(this MigrationRecord record, DateTime cutoffDate)
        {
            ArgumentNullException.ThrowIfNull(record);
            ArgumentNullException.ThrowIfNull(cutoffDate);
            return record.AppliedAt < cutoffDate;
        }

        /// <summary>
        /// Formats the migration record as a human-readable string.
        /// </summary>
        /// <param name="record">The migration record to format.</param>
        /// <returns>A formatted string representation of the migration.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="record"/> is <see langword="null"/></exception>
        public static string ToDisplayString(this MigrationRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            return $"Migration {record.Id}: {record.Version} - {record.Description} " +
                   $"(Applied: {record.AppliedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}) " +
                   $"Status: {(record.Success ? "SUCCESS" : "FAILED")}";
        }

        /// <summary>
        /// Gets all failed migrations from a sequence of migration records.
        /// </summary>
        /// <param name="records">The sequence of migration records to filter.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing all failed migrations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="records"/> is <see langword="null"/></exception>
        public static IReadOnlyList<MigrationRecord> GetFailedMigrations(this IEnumerable<MigrationRecord> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            return records.Where(r => !r.Success).ToList().AsReadOnly();
        }
    }
}