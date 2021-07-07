using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="BatchOperationsBenchmarks"/> benchmark class.
/// Provides validation, validation checks, and exception throwing for benchmark configuration.
/// </summary>
public static class BatchOperationsBenchmarksValidation
{
    /// <summary>
    /// Validates the specified <see cref="BatchOperationsBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>An enumerable of validation messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BatchOperationsBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate GlobalSetup method existence and signature
        if (value.GlobalSetup == null)
        {
            errors.Add("GlobalSetup property must not be null.");
        }

        // Validate GlobalCleanup method existence and signature
        if (value.GlobalCleanup == null)
        {
            errors.Add("GlobalCleanup property must not be null.");
        }

        // Validate AddRangeAsync_1000_Entities method existence and signature
        if (value.AddRangeAsync_1000_Entities == null)
        {
            errors.Add("AddRangeAsync_1000_Entities method must not be null.");
        }

        // Validate AddRangeAsync_5000_Entities method existence and signature
        if (value.AddRangeAsync_5000_Entities == null)
        {
            errors.Add("AddRangeAsync_5000_Entities method must not be null.");
        }

        // Validate UpdateRangeAsync_1000_Entities method existence and signature
        if (value.UpdateRangeAsync_1000_Entities == null)
        {
            errors.Add("UpdateRangeAsync_1000_Entities method must not be null.");
        }

        // Validate UpdateRangeAsync_5000_Entities method existence and signature
        if (value.UpdateRangeAsync_5000_Entities == null)
        {
            errors.Add("UpdateRangeAsync_5000_Entities method must not be null.");
        }

        // Validate DeleteRangeAsync_1000_Entities method existence and signature
        if (value.DeleteRangeAsync_1000_Entities == null)
        {
            errors.Add("DeleteRangeAsync_1000_Entities method must not be null.");
        }

        // Validate DeleteRangeAsync_5000_Entities method existence and signature
        if (value.DeleteRangeAsync_5000_Entities == null)
        {
            errors.Add("DeleteRangeAsync_5000_Entities method must not be null.");
        }

        // Validate BulkInsert_100_Entities method existence and signature
        if (value.BulkInsert_100_Entities == null)
        {
            errors.Add("BulkInsert_100_Entities method must not be null.");
        }

        // Validate BulkInsert_10000_Entities method existence and signature
        if (value.BulkInsert_10000_Entities == null)
        {
            errors.Add("BulkInsert_10000_Entities method must not be null.");
        }

        // Validate BatchInsert_1000_Entities_With_Relations method existence and signature
        if (value.BatchInsert_1000_Entities_With_Relations == null)
        {
            errors.Add("BatchInsert_1000_Entities_With_Relations method must not be null.");
        }

        // Validate BatchUpdate_Complex_Predicate method existence and signature
        if (value.BatchUpdate_Complex_Predicate == null)
        {
            errors.Add("BatchUpdate_Complex_Predicate method must not be null.");
        }

        // Validate BatchDelete_With_Where_Clause method existence and signature
        if (value.BatchDelete_With_Where_Clause == null)
        {
            errors.Add("BatchDelete_With_Where_Clause method must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BatchOperationsBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this BatchOperationsBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="BatchOperationsBenchmarks"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this BatchOperationsBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BatchOperationsBenchmarks instance is not valid. Validation errors: {string.Join("; ", errors)}");
        }
    }
}