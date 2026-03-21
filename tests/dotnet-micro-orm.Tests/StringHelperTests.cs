#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Utils;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class StringHelperTests
{
    [Fact]
    public void ToKebabCase_PascalCaseInput_ReturnsKebabCase()
    {
        var result = StringHelper.ToKebabCase("UserProfile");

        result.Should().Be("user-profile");
    }

    [Fact]
    public void ToSnakeCase_PascalCaseInput_ReturnsSnakeCase()
    {
        var result = StringHelper.ToSnakeCase("UserProfile");

        result.Should().Be("user_profile");
    }

    [Fact]
    public void ToPascalCase_KebabCaseInput_ReturnsPascalCase()
    {
        var result = StringHelper.ToPascalCase("user-profile");

        result.Should().Be("UserProfile");
    }

    [Fact]
    public void Truncate_StringExceedsMaxLength_AppendsSuffixAndRespectsBound()
    {
        var result = StringHelper.Truncate("Hello World", 8);

        result.Should().Be("Hello...");
        result.Length.Should().Be(8);
    }
}
