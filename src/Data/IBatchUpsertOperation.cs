#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Upsert result describing what the batch operation did to a single entity.
/// </summary>
public sealed class UpsertResult<T> where T : BaseEntity
{
    /// <summary>The entity that was upserted.</summary>
    public T Entity { get; init; } = null!;

    /// <summary><c>true</c> when the entity was inserted; <c>false</c> when updated.</summary>
    public bool WasInserted { get; init; }
}

/// <summary>
/// Provides batch upsert (INSERT OR UPDATE) capabilities on top of an existing repository.
/// Uses SQL MERGE semantics so only one round-trip per batch is required.
/// </summary>
public interface IBatchUpsertOperation<T> where T : BaseEntity
{
    /// <summary>
    /// Upserts a single entity.  If a row matching the key selector already exists
    /// it is updated; otherwise a new row is inserted.
    /// </summary>
    /// <param name="entity">Entity to upsert.</param>
    /// <param name="keySelector">Expression identifying the unique key columns used
    /// to detect whether a row already exists.</param>
    Task<UpsertResult<T>> UpsertAsync(T entity, Expression<Func<T, object>> keySelector);

    /// <summary>
    /// Upserts a collection of entities in configurable batches.
    /// </summary>
    /// <param name="entities">Entities to upsert.</param>
    /// <param name="keySelector">Expression identifying the unique key columns.</param>
    /// <param name="batchSize">
    /// Maximum entities per SQL statement.  Defaults to
    /// <see cref="Constants.OrmConstants.DefaultBatchSize"/>.
    /// </param>
    /// <returns>
    /// A list of <see cref="UpsertResult{T}"/> in the same order as <paramref name="entities"/>.
    /// </returns>
    Task<List<UpsertResult<T>>> UpsertRangeAsync(
        List<T> entities,
        Expression<Func<T, object>> keySelector,
        int batchSize = Constants.OrmConstants.DefaultBatchSize);
}
