#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Utils;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Tests for the StringHelper utility methods.
/// </summary>
public sealed class StringHelperTests
{
    /// <summary>
    /// Verifies that <see cref="StringHelper.ToKebabCase(string)"/> converts a PascalCase string to kebab-case.
    /// </summary>
    [Fact]
    public void ToKebabCase_PascalCaseInput_ReturnsKebabCase()
    {
        var result = StringHelper.ToKebabCase("UserProfile");

        result.Should().Be("user-profile");
    }

    /// <summary>
    /// Verifies that <see cref="StringHelper.ToSnakeCase(string)"/> converts a PascalCase string to snake_case.
    /// </summary>
    [Fact]
    public void ToSnakeCase_PascalCaseInput_ReturnsSnakeCase()
    {
        var result = StringHelper.ToSnakeCase("UserProfile");

        result.Should().Be("user_profile");
    }

    /// <summary>
    /// Verifies that <see cref="StringHelper.ToPascalCase(string)"/> converts a kebab-case string to PascalCase.
    /// </summary>
    [Fact]
    public void ToPascalCase_KebabCaseInput_ReturnsPascalCase()
    {
        var result = StringHelper.ToPascalCase("user-profile");

        result.Should().Be("UserProfile");
    }

    /// <summary>
    /// Verifies that <see cref="StringHelper.Truncate(string,int)"/> truncates a string that exceeds the maximum length,
    /// appends the ellipsis suffix, and respects the specified bound.
    /// </summary>
    [Fact]
    public void Truncate_StringExceedsMaxLength_AppendsSuffixAndRespectsBound()
    {
        var result = StringHelper.Truncate("Hello World", 8);

        result.Should().Be("Hello...");
        result.Length.Should().Be(8);
    }
}
