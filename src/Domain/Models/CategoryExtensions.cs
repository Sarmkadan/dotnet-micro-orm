#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides helper extension methods for the <see cref="Category"/> entity.
/// Validation helpers live in <see cref="CategoryValidation"/>.
/// </summary>
public static class CategoryExtensions
{
    /// <summary>
    /// Determines whether the category is a root category (has no parent).
    /// </summary>
    /// <param name="category">The category to check</param>
    /// <returns>True if the category has no parent; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is null</exception>
    public static bool IsRoot(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return !category.ParentCategoryId.HasValue;
    }

    /// <summary>
    /// Determines whether the category has any sub-categories.
    /// </summary>
    /// <param name="category">The category to check</param>
    /// <returns>True if the category has at least one sub-category; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is null</exception>
    public static bool HasSubCategories(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return category.SubCategories is { Count: > 0 };
    }

    /// <summary>
    /// Returns the active sub-categories of a category, ordered by display order.
    /// </summary>
    /// <param name="category">The category to inspect</param>
    /// <returns>An ordered sequence of active sub-categories</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is null</exception>
    public static IEnumerable<Category> GetActiveSubCategories(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return category.SubCategories?
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            ?? Enumerable.Empty<Category>();
    }

    /// <summary>
    /// Computes the depth of the category within the hierarchy (root = 0).
    /// </summary>
    /// <param name="category">The category to measure</param>
    /// <returns>The number of ancestors between the category and the root</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is null</exception>
    public static int GetHierarchyDepth(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        var depth = 0;
        var current = category.ParentCategory;
        while (current is not null)
        {
            depth++;
            current = current.ParentCategory;
        }

        return depth;
    }

    /// <summary>
    /// Returns the full hierarchy path of the category as a display string.
    /// </summary>
    /// <param name="category">The category to format</param>
    /// <returns>A string like "Root > Child > Leaf", or the category name when it is a root</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="category"/> is null</exception>
    public static string GetFullPath(this Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        return category.GetBreadcrumb();
    }
}