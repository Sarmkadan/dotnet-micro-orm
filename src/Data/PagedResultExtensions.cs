#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides convenience extensions for <see cref="PagedResult{T}"/>.
/// </summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// Determines whether the current page contains no items.
    /// </summary>
    /// <typeparam name="T">The type of item in the paged result.</typeparam>
    /// <param name="pagedResult">The paged result to inspect.</param>
    /// <returns><see langword="true"/> when the current page contains no items; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pagedResult"/> is <see langword="null"/>.</exception>
    public static bool IsEmpty<T>(this PagedResult<T> pagedResult)
    {
        ArgumentNullException.ThrowIfNull(pagedResult);

        return pagedResult.Items.Count == 0;
    }
}
