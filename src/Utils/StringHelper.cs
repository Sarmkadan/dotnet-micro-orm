// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Provides utility methods for string manipulation and validation.
/// Handles trimming, case conversion, splitting, formatting, and regular expression operations.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Converts PascalCase or camelCase to kebab-case
    /// Example: "UserProfile" -> "user-profile", "userId" -> "user-id"
    /// </summary>
    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = Regex.Replace(input, "([a-z])([A-Z])", "$1-$2");
        return result.ToLowerInvariant();
    }

    /// <summary>
    /// Converts PascalCase or kebab-case to snake_case
    /// Example: "UserProfile" -> "user_profile", "user-profile" -> "user_profile"
    /// </summary>
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var step1 = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2");
        var step2 = Regex.Replace(step1, "([A-Z]+)([A-Z][a-z])", "$1_$2");
        var step3 = Regex.Replace(step2, "-", "_");

        return step3.ToLowerInvariant();
    }

    /// <summary>
    /// Converts kebab-case or snake_case to PascalCase
    /// Example: "user-profile" -> "UserProfile", "user_profile" -> "UserProfile"
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = Regex.Split(input, "[-_]");
        var sb = new StringBuilder();

        foreach (var word in words)
        {
            if (!string.IsNullOrEmpty(word))
            {
                sb.Append(char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Truncates string to specified length and appends ellipsis if truncated
    /// </summary>
    public static string Truncate(string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Repeats a string the specified number of times
    /// Example: "ab".Repeat(3) -> "ababab"
    /// </summary>
    public static string Repeat(this string input, int count)
    {
        if (count <= 0)
            return string.Empty;
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length * count);
        for (int i = 0; i < count; i++)
            sb.Append(input);

        return sb.ToString();
    }

    /// <summary>
    /// Removes all whitespace from a string
    /// </summary>
    public static string RemoveWhitespace(this string input)
        => string.IsNullOrEmpty(input) ? input : Regex.Replace(input, @"\s+", "");

    /// <summary>
    /// Checks if a string contains any uppercase letters
    /// </summary>
    public static bool ContainsUpperCase(this string input)
        => string.IsNullOrEmpty(input) ? false : input.Any(char.IsUpper);

    /// <summary>
    /// Checks if a string contains any lowercase letters
    /// </summary>
    public static bool ContainsLowerCase(this string input)
        => string.IsNullOrEmpty(input) ? false : input.Any(char.IsLower);

    /// <summary>
    /// Checks if a string contains any digits
    /// </summary>
    public static bool ContainsDigit(this string input)
        => string.IsNullOrEmpty(input) ? false : input.Any(char.IsDigit);

    /// <summary>
    /// Checks if a string contains only alphanumeric characters
    /// </summary>
    public static bool IsAlphanumeric(this string input)
        => string.IsNullOrEmpty(input) ? false : input.All(char.IsLetterOrDigit);

    /// <summary>
    /// Validates email address format (basic validation)
    /// </summary>
    public static bool IsValidEmail(this string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 254)
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates URL format
    /// </summary>
    public static bool IsValidUrl(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return Uri.TryCreate(input, UriKind.Absolute, out _);
    }

    /// <summary>
    /// Compares two strings case-insensitively
    /// </summary>
    public static bool EqualsIgnoreCase(this string input, string other)
        => string.Equals(input, other, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Pluralizes a word based on count (simple implementation)
    /// </summary>
    public static string Pluralize(this string input, int count)
    {
        if (count == 1)
            return input;

        return input switch
        {
            string s when s.EndsWith("y") => s[..^1] + "ies",
            string s when s.EndsWith("s") || s.EndsWith("x") || s.EndsWith("z") => s + "es",
            string s when s.EndsWith("o") => s + "es",
            _ => input + "s"
        };
    }

    /// <summary>
    /// Reverses a string
    /// </summary>
    public static string Reverse(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Extracts first N characters from a string
    /// </summary>
    public static string Left(this string input, int count)
        => string.IsNullOrEmpty(input) ? input : input[..Math.Min(count, input.Length)];

    /// <summary>
    /// Extracts last N characters from a string
    /// </summary>
    public static string Right(this string input, int count)
    {
        if (string.IsNullOrEmpty(input) || count >= input.Length)
            return input;

        return input[^count..];
    }

    /// <summary>
    /// Replaces multiple strings in a single pass (dictionary-based replacement)
    /// More efficient than multiple string.Replace calls
    /// </summary>
    public static string ReplaceMultiple(this string input, Dictionary<string, string> replacements)
    {
        if (string.IsNullOrEmpty(input) || replacements is null || replacements.Count == 0)
            return input;

        var result = input;
        foreach (var kvp in replacements)
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }

        return result;
    }
}
