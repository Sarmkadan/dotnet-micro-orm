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
    /// <param name="results">The upsert results from the operation.</param>
    /// <returns>A list of entities that were inserted.</returns>
    public static List<T> GetInsertedEntities<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        return results
            .Where(r => r.WasInserted)
            .Select(r => r.Entity)
            .ToList();
    }

    /// <summary>
    /// Gets only the entities that were updated during the upsert operation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation.</param>
    /// <returns>A list of entities that were updated.</returns>
    public static List<T> GetUpdatedEntities<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        return results
            .Where(r => !r.WasInserted)
            .Select(r => r.Entity)
            .ToList();
    }

    /// <summary>
    /// Filters the upsert results to only include entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation.</param>
    /// <param name="predicate">The filter predicate to apply to the entities.</param>
    /// <returns>A filtered list of upsert results.</returns>
    public static List<UpsertResult<T>> WhereEntity<T>(
        this List<UpsertResult<T>> results,
        Func<T, bool> predicate) where T : BaseEntity
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return results
            .Where(r => predicate(r.Entity))
            .ToList();
    }

    /// <summary>
    /// Checks if any of the upsert operations resulted in an insert.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="results">The upsert results from the operation.</param>
    /// <returns>True if any entity was inserted; otherwise false.</returns>
    public static bool AnyInserted<T>(this List<UpsertResult<T>> results) where T : BaseEntity
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        return results.Any(r => r.WasInserted);
    }
}
