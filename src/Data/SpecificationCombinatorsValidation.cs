#nullable enable

using System.Globalization;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides validation helpers for <see cref="SpecificationCombinators"/> to ensure specification compositions are valid.
/// </summary>
public static class SpecificationCombinatorsValidation
{
    /// <summary>
    /// Validates that specification compositions are valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if left or right is null.</exception>
    public static IReadOnlyList<string> ValidateComposition<T>(
        Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var errors = new List<string>();

        // Validate left specification using existing SpecificationValidation
        errors.AddRange(left.Validate());

        // Validate right specification using existing SpecificationValidation
        errors.AddRange(right.Validate());

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates that specification compositions are valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>True if the composition is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if left or right is null.</exception>
    public static bool IsValidComposition<T>(
        Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return left.IsValid() && right.IsValid();
    }

    /// <summary>
    /// Validates that specification compositions are valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <exception cref="ArgumentNullException">Thrown if left or right is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the composition is invalid.</exception>
    public static void EnsureValidComposition<T>(
        Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var errors = ValidateComposition(left, right);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Specification composition is invalid.\nValidation errors:\n- " +
                string.Join("\n- ", errors));
        }
    }

    /// <summary>
    /// Validates that a specification is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if spec is null.</exception>
    public static IReadOnlyList<string> Validate<T>(Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.Validate();
    }

    /// <summary>
    /// Validates that a specification is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to validate.</param>
    /// <returns>True if the specification is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if spec is null.</exception>
    public static bool IsValid<T>(Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.IsValid();
    }

    /// <summary>
    /// Validates that a specification is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if spec is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the specification is invalid.</exception>
    public static void EnsureValid<T>(Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        spec.EnsureValid();
    }

    /// <summary>
    /// Validates that a specification composition using And is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateAnd<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return ValidateComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Or is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateOr<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return ValidateComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Not is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateNot<T>(
        this Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.Validate();
    }

    /// <summary>
    /// Validates that a specification composition using And is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>True if the composition is valid; otherwise, false.</returns>
    public static bool IsValidAnd<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return IsValidComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Or is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>True if the composition is valid; otherwise, false.</returns>
    public static bool IsValidOr<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return IsValidComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Not is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>True if the composition is valid; otherwise, false.</returns>
    public static bool IsValidNot<T>(
        this Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.IsValid();
    }

    /// <summary>
    /// Validates that a specification composition using And is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <exception cref="ArgumentNullException">Thrown if left or right is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the composition is invalid.</exception>
    public static void EnsureValidAnd<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        EnsureValidComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Or is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <exception cref="ArgumentNullException">Thrown if left or right is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the composition is invalid.</exception>
    public static void EnsureValidOr<T>(
        this Specification<T> left,
        Specification<T> right) where T : class
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        EnsureValidComposition(left, right);
    }

    /// <summary>
    /// Validates that a specification composition using Not is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown if spec is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the composition is invalid.</exception>
    public static void EnsureValidNot<T>(
        this Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        spec.EnsureValid();
    }
}
