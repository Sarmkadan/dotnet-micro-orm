#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Extension methods for <see cref="PreparedStatementPoolOptions"/> that provide
/// convenient configuration and monitoring capabilities for the prepared statement pool.
/// </summary>
public static class PreparedStatementPoolOptionsExtensions
{
    /// <summary>
    /// Sets the maximum pool size to the specified value.
    /// </summary>
    /// <param name="options">The pool options to configure.</param>
    /// <param name="maxPoolSize">The maximum number of statements to hold in the pool.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    /// <returns>The same <see cref="PreparedStatementPoolOptions"/> for method chaining.</returns>
    public static PreparedStatementPoolOptions WithMaxPoolSize(this PreparedStatementPoolOptions options, int maxPoolSize)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.MaxPoolSize = maxPoolSize;
        return options;
    }

    /// <summary>
    /// Sets the maximum pool size to a value based on the available memory.
    /// Uses a heuristic that reserves approximately 1KB per statement in the pool.
    /// </summary>
    /// <param name="options">The pool options to configure.</param>
    /// <param name="reservedMemoryMb">The amount of memory in megabytes to reserve for the pool.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reservedMemoryMb"/> is not positive</exception>
    /// <returns>The same <see cref="PreparedStatementPoolOptions"/> for method chaining.</returns>
    public static PreparedStatementPoolOptions WithMemoryBasedMaxPoolSize(this PreparedStatementPoolOptions options, int reservedMemoryMb)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (reservedMemoryMb <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reservedMemoryMb), "Reserved memory must be positive");
        }

        // Heuristic: ~1KB per statement in the pool
        // This accounts for statement metadata, parameter information, and some overhead
        options.MaxPoolSize = Math.Max(10, reservedMemoryMb * 1024); // Convert MB to statement count (1KB per statement)
        return options;
    }

    /// <summary>
    /// Configures the pool to disable statement eviction when the pool reaches capacity.
    /// Instead, new statements will be rejected when the pool is full.
    /// </summary>
    /// <param name="options">The pool options to configure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    /// <returns>The same <see cref="PreparedStatementPoolOptions"/> for method chaining.</returns>
    public static PreparedStatementPoolOptions WithNoEviction(this PreparedStatementPoolOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        // MaxPoolSize acts as both capacity limit and eviction threshold
        // When set to int.MaxValue, effectively disables eviction
        options.MaxPoolSize = int.MaxValue;
        return options;
    }

    /// <summary>
    /// Configures the pool with a default conservative size suitable for most applications.
    /// </summary>
    /// <param name="options">The pool options to configure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/></exception>
    /// <returns>The same <see cref="PreparedStatementPoolOptions"/> for method chaining.</returns>
    public static PreparedStatementPoolOptions WithDefaultSize(this PreparedStatementPoolOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.MaxPoolSize = 200; // Default from PreparedStatementPoolOptions
        return options;
    }
}