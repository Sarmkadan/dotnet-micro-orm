#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Tests;

public static class BatchUpsertOperationTestsExtensions
{
    /// <summary>
    /// Gets only the entities that were inserted during the upsert operation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation. Cannot be <see langword="null"/>.</param>
    /// <returns>A list of entities that were inserted. Never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static List<T> GetInsertedEntities<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .Where(r => r.WasInserted)
            .Select(r => r.Entity)
            .ToList();
    }

    /// <summary>
    /// Gets only the entities that were updated during the upsert operation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation. Cannot be <see langword="null"/>.</param>
    /// <returns>A list of entities that were updated. Never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static List<T> GetUpdatedEntities<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .Where(r => !r.WasInserted)
            .Select(r => r.Entity)
            .ToList();
    }

    /// <summary>
    /// Filters the upsert results to only include entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">The filter predicate to apply to the entities. Cannot be <see langword="null"/>.</param>
    /// <returns>A filtered list of upsert results. Never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="results"/> or <paramref name="predicate"/> is <see langword="null"/>.
    /// </exception>
    public static List<UpsertResult<T>> WhereEntity<T>(
        this List<UpsertResult<T>> results,
        Func<T, bool> predicate) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(predicate);

        return results
            .Where(r => predicate(r.Entity))
            .ToList();
    }

    /// <summary>
    /// Checks if any of the upsert operations resulted in an insert.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if any entity was inserted; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
    public static bool AnyInserted<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Any(r => r.WasInserted);
    }
}
