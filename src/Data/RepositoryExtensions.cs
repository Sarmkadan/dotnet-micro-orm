#nullable enable

namespace DotnetMicroOrm.Data;

using System.Linq.Expressions;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// Extension methods for Repository&lt;T&gt; providing additional query and bulk operations
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Gets entities by their IDs using the existing GetAsync method
    /// </summary>
    public static async Task<List<T>> GetByIdsAsync<T>(this Repository<T> repository, IEnumerable<int> ids) where T : BaseEntity, new()
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (ids is null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids.ToList();
        if (!idList.Any())
            return new List<T>();

        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty is null)
            throw new OrmException("Entity must have Id property");

        // Create a list of Id values and use the existing GetAsync method
        var tasks = idList.Select(id => repository.GetByIdAsync(id));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r is not null).ToList()!;
    }

    /// <summary>
    /// Gets the first entity matching the predicate or a default value if not found
    /// </summary>
    public static async Task<T?> FirstOrDefaultAsync<T>(this Repository<T> repository, Expression<Func<T, bool>> predicate, T? defaultValue = null) where T : BaseEntity, new()
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var result = await repository.FirstOrDefaultAsync(predicate);
        return result ?? defaultValue;
    }

    /// <summary>
    /// Gets entities with pagination using the existing GetPagedAsync method
    /// </summary>
    public static async Task<(List<T> Items, int TotalCount)> GetPagedAsync<T>(
        this Repository<T> repository,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null) where T : BaseEntity, new()
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be 1 or greater");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be 1 or greater");

        // Use the existing GetPagedWithCountAsync method
        return await repository.GetPagedWithCountAsync(pageNumber, pageSize, predicate);
    }

    /// <summary>
    /// Gets entities with pagination using skip/take parameters
    /// </summary>
    public static async Task<(List<T> Items, int TotalCount)> GetPagedWithSkipAsync<T>(
        this Repository<T> repository,
        int skip,
        int take,
        Expression<Func<T, bool>>? predicate = null) where T : BaseEntity, new()
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

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