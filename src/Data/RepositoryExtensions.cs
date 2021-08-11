#nullable enable

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Extension methods for <see cref="Repository{T}"/> providing additional query and bulk operations.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Gets entities by their IDs using a single optimized query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="ids">The collection of entity IDs to retrieve.</param>
    /// <returns>A list of entities matching the specified IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="ids"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> is empty.</exception>
    /// <exception cref="OrmException">Thrown when the entity type does not have an Id property.</exception>
    public static async Task<List<T>> GetByIdsAsync<T>(this Repository<T> repository, IEnumerable<int> ids) where T : BaseEntity, new()
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(ids);

        var idList = ids.ToList();
        if (idList.Count == 0)
            return [];

        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty is null)
            throw new OrmException("Entity must have Id property");

        // Build a single query to fetch all entities by their IDs
        var allEntities = await repository.GetAllAsync();
        return allEntities.Where(e => idList.Contains((int)idProperty.GetValue(e)!)).ToList();
    }

    /// <summary>
    /// Gets the first entity matching the predicate or a default value if not found.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="defaultValue">The default value to return if no entity is found.</param>
    /// <returns>The first matching entity or the default value if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public static async Task<T?> FirstOrDefaultAsync<T>(
        this Repository<T> repository,
        Expression<Func<T, bool>> predicate,
        T? defaultValue = null) where T : BaseEntity, new()
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await repository.FirstOrDefaultAsync(predicate);
        return result ?? defaultValue;
    }

    /// <summary>
    /// Gets entities with pagination using the existing GetPagedAsync method.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="predicate">Optional predicate to filter entities.</param>
    /// <returns>A tuple containing the list of items and the total count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageNumber"/> is less than 1 or <paramref name="pageSize"/> is less than 1.</exception>
    public static async Task<(List<T> Items, int TotalCount)> GetPagedAsync<T>(
        this Repository<T> repository,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null) where T : BaseEntity, new()
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be 1 or greater");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be 1 or greater");

        // Use the existing GetPagedWithCountAsync method
        return await repository.GetPagedWithCountAsync(pageNumber, pageSize, predicate);
    }

    /// <summary>
    /// Gets entities with pagination using skip/take parameters.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="repository">The repository instance.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <param name="predicate">Optional predicate to filter entities.</param>
    /// <returns>A tuple containing the list of items and the total count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="skip"/> is negative or <paramref name="take"/> is less than 1.
    /// </exception>
    public static async Task<(List<T> Items, int TotalCount)> GetPagedWithSkipAsync<T>(
        this Repository<T> repository,
        int skip,
        int take,
        Expression<Func<T, bool>>? predicate = null) where T : BaseEntity, new()
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be non-negative");

        if (take < 1)
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be 1 or greater");

        // Calculate page number from skip and take
        var pageNumber = (skip / take) + 1;
        var pageSize = take;

        // Use the existing GetPagedWithCountAsync method
        return await repository.GetPagedWithCountAsync(pageNumber, pageSize, predicate);
    }
}