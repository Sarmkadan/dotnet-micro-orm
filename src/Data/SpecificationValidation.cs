#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides validation helpers for <see cref="Specification{T}"/> objects.
/// Validates criteria expressions, includes, pagination parameters, and ordering configurations.
/// </summary>
/// <remarks>
/// This static class offers extension methods to validate Specification instances, ensuring they are properly configured
/// before being used in queries. Validation includes checking for null expressions, proper pagination settings,
/// and detecting potentially problematic default DateTime values in criteria expressions.
/// </remarks>
public static class SpecificationValidation
{
    /// <summary>
    /// Validates a Specification instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The specification to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this Specification<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Criteria
        if (value.Criteria is null)
        {
            problems.Add("Criteria expression is null");
        }

        // Validate Includes
        ArgumentNullException.ThrowIfNull(value.Includes);

        if (value.Includes.Any(include => include is null))
        {
            problems.Add("Includes collection contains null expressions");
        }

        // Validate IncludeStrings
        ArgumentNullException.ThrowIfNull(value.IncludeStrings);

        foreach (var includeString in value.IncludeStrings)
        {
            if (string.IsNullOrWhiteSpace(includeString))
            {
                problems.Add("IncludeStrings collection contains null, empty, or whitespace strings");
                break;
            }
        }

        // Validate OrderBy
        if (value.OrderBy is null && value.OrderByDescending is null && !value.IsPagingEnabled)
        {
            // This is not necessarily an error, but worth noting for completeness
            problems.Add("No ordering or pagination configured - queries may return unordered results");
        }

        // Validate OrderBy expressions
        if (value.OrderBy is null && value.OrderByDescending is not null)
        {
            problems.Add("OrderByDescending is set but OrderBy is null - consider setting both for clarity");
        }

        if (value.OrderByDescending is null && value.OrderBy is not null)
        {
            problems.Add("OrderBy is set but OrderByDescending is null - consider setting both for clarity");
        }

        // Validate that both OrderBy and OrderByDescending aren't set simultaneously (potential conflict)
        if (value.OrderBy is not null && value.OrderByDescending is not null)
        {
            problems.Add("Both OrderBy and OrderByDescending are set - this may cause conflicting ordering behavior");
        }

        // Validate pagination parameters
        if (value.IsPagingEnabled)
        {
            if (value.PageNumber is null)
            {
                problems.Add("IsPagingEnabled is true but PageNumber is null");
            }
            else if (value.PageNumber < 1)
            {
                problems.Add($"PageNumber must be positive, but was {value.PageNumber}");
            }

            if (value.PageSize is null)
            {
                problems.Add("IsPagingEnabled is true but PageSize is null");
            }
            else if (value.PageSize < 1)
            {
                problems.Add($"PageSize must be positive, but was {value.PageSize}");
            }
            else if (value.PageSize > 1000)
            {
                problems.Add($"PageSize {value.PageSize} exceeds reasonable maximum of 1000");
            }
        }
        else
        {
            if (value.PageNumber is not null)
            {
                problems.Add("IsPagingEnabled is false but PageNumber is set");
            }

            if (value.PageSize is not null)
            {
                problems.Add("IsPagingEnabled is false but PageSize is set");
            }
        }

        // Validate default dates in criteria (if present)
        if (value.Criteria is not null)
        {
            var defaultDateCheck = new DefaultDateExpressionVisitor();
            if (defaultDateCheck.HasDefaultDate(value.Criteria))
            {
                problems.Add("Criteria expression contains default DateTime values (DateTime.MinValue or DateTime.MaxValue)");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a Specification instance is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The specification to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid<T>(this Specification<T> value) where T : class
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a Specification instance is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The specification to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems</exception>
    public static void EnsureValid<T>(this Specification<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Specification validation failed:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", problems)
                }",
                nameof(value));
        }
    }

    /// <summary>
    /// Visitor to check if an expression contains default DateTime values.
    /// </summary>
    private sealed class DefaultDateExpressionVisitor : ExpressionVisitor
    {
        private bool _hasDefaultDate;

        public bool HasDefaultDate(Expression expression)
        {
            _hasDefaultDate = false;
            Visit(expression);
            return _hasDefaultDate;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is DateTime dateValue)
            {
                if (dateValue == default || dateValue == DateTime.MinValue || dateValue == DateTime.MaxValue)
                {
                    _hasDefaultDate = true;
                }
            }
            else if (node.Value is DateTimeOffset dateOffsetValue)
            {
                if (dateOffsetValue == default || dateOffsetValue == DateTimeOffset.MinValue || dateOffsetValue == DateTimeOffset.MaxValue)
                {
                    _hasDefaultDate = true;
                }
            }

            return base.VisitConstant(node);
        }
    }
}