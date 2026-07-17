#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Data;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides validation helpers for <see cref="QueryPlan"/> instances to ensure
/// all required fields are present and values are within valid ranges.
/// </summary>
public static class QueryPlanValidation
{
    /// <summary>
    /// Validates a <see cref="QueryPlan"/> instance and returns a list of human-readable
    /// problems found. Returns an empty list if the plan is valid.
    /// </summary>
    /// <param name="value">The query plan to validate.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryPlan? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Fingerprint))
        {
            problems.Add("QueryPlan.Fingerprint must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Sql))
        {
            problems.Add("QueryPlan.Sql must not be null or whitespace.");
        }

        // Validate numeric properties
        if (double.IsNaN(value.EstimatedCost) || double.IsInfinity(value.EstimatedCost))
        {
            problems.Add("QueryPlan.EstimatedCost must be a valid finite number.");
        }
        else if (value.EstimatedCost < 0)
        {
            problems.Add("QueryPlan.EstimatedCost must not be negative.");
        }

        // Validate Parameters collection
        if (value.Parameters is null)
        {
            problems.Add("QueryPlan.Parameters must not be null.");
        }
        else if (value.Parameters.Any(p => p is null))
        {
            problems.Add("QueryPlan.Parameters must not contain null elements.");
        }

        // Validate DateTime properties
        // CachedAt should be a reasonable date (not default/MinValue which indicates not set)
        if (value.CachedAt == default)
        {
            problems.Add("QueryPlan.CachedAt must be set to a valid DateTime.");
        }
        else if (value.CachedAt > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("QueryPlan.CachedAt appears to be in the future.");
        }
        else if (value.CachedAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("QueryPlan.CachedAt appears to be excessively old.");
        }

        // Validate HitCount (should be non-negative)
        if (value.HitCount < 0)
        {
            problems.Add("QueryPlan.HitCount must not be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="QueryPlan"/> is valid.
    /// </summary>
    /// <param name="value">The query plan to check.</param>
    /// <returns><see langword="true"/> if the plan is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this QueryPlan? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="QueryPlan"/> is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message listing all validation problems
    /// if any are found.
    /// </summary>
    /// <param name="value">The query plan to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the plan contains validation errors.</exception>
    public static void EnsureValid(this QueryPlan? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"QueryPlan validation failed:{Environment.NewLine}- {
            string.Join(Environment.NewLine + "- ", problems)
        }");
    }
}