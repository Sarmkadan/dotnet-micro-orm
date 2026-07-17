#nullable enable

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides validation helpers for <see cref="SpecificationCombinators"/> to ensure specification compositions are valid.
/// This class offers methods to validate specification compositions (And, Or, Not) and individual specifications.
/// </summary>
public static class SpecificationCombinatorsValidation
{
    /// <summary>
    /// Validates that a specification composition is valid by validating both specifications.
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

        errors.AddRange(left.Validate());
        errors.AddRange(right.Validate());

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates that a specification composition is valid by validating both specifications.
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
    /// Ensures that a specification composition is valid, throwing an exception if not.
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
                $"Specification composition is invalid.{
                    Environment.NewLine}Validation errors:{
                    Environment.NewLine}- {
                    string.Join($"{
                    Environment.NewLine}- ", errors)}");
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
    /// Determines whether a specification is valid.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if spec is null.</exception>
    public static bool IsValid<T>(Specification<T> spec) where T : class
    {
        ArgumentNullException.ThrowIfNull(spec);
        return spec.IsValid();
    }

    /// <summary>
    /// Ensures that a specification is valid, throwing an exception if not.
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
}