#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides useful extension methods for <see cref="BaseEntity"/> and its derived types
/// </summary>
public static class BaseEntityExtensions
{
    /// <summary>
    /// Gets the entity's <c>CreatedDate</c> value when the property is present, otherwise null.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>The created date, or null when the entity has no <c>CreatedDate</c> property</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static DateTime? GetCreatedDate(this BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return GetDateTimeProperty(entity, "CreatedDate");
    }

    /// <summary>
    /// Gets the entity's <c>ModifiedDate</c> value when the property is present, otherwise null.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>The modified date, or null when the entity has no <c>ModifiedDate</c> property</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static DateTime? GetModifiedDate(this BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return GetDateTimeProperty(entity, "ModifiedDate");
    }

    /// <summary>
    /// Determines whether the entity is new, i.e. it has no persisted identity yet.
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True when the entity's <c>Id</c> is null or default, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static bool IsNew(this BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = entity.GetType().GetProperty("Id")?.GetValue(entity);
        if (id is null)
            return true;

        return id.Equals(GetDefaultValue(id.GetType()));
    }

    /// <summary>
    /// Gets the age of the entity in days since it was created.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>Whole days since creation, or -1 when the entity has no <c>CreatedDate</c> property</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static int GetAgeInDays(this BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var createdDate = GetCreatedDate(entity);
        return createdDate.HasValue
            ? (int)(DateTime.UtcNow - createdDate.Value).TotalDays
            : -1;
    }

    /// <summary>
    /// Determines whether the entity was created within the given number of days.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <param name="days">Number of days to consider recent (default: 7)</param>
    /// <returns>True when the entity was created within <paramref name="days"/> days, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static bool IsRecent(this BaseEntity entity, int days = 7)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var createdDate = GetCreatedDate(entity);
        return createdDate.HasValue && (DateTime.UtcNow - createdDate.Value).TotalDays <= days;
    }

    /// <summary>
    /// Determines whether the entity has been modified since it was created.
    /// </summary>
    /// <param name="entity">The entity to inspect</param>
    /// <returns>True when the entity has a <c>ModifiedDate</c> later than its <c>CreatedDate</c>, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null</exception>
    public static bool IsModified(this BaseEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var createdDate = GetCreatedDate(entity);
        var modifiedDate = GetModifiedDate(entity);

        return modifiedDate.HasValue &&
               (!createdDate.HasValue || modifiedDate.Value > createdDate.Value);
    }

    private static DateTime? GetDateTimeProperty(BaseEntity entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property is null)
            return null;

        var value = property.GetValue(entity);
        return value is DateTime dateTime ? dateTime : null;
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}