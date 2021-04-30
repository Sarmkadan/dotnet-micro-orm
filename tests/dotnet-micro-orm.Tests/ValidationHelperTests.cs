#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Utils;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class ValidationHelperTests
{
    [Fact]
    public void ValidatePassword_StrongPassword_ReturnsSuccess()
    {
        var (isValid, error) = ValidationHelper.ValidatePassword("SecureP@ss1");

        isValid.Should().BeTrue();
        error.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePassword_NullInput_ReturnsRequiredError()
    {
        var (isValid, error) = ValidationHelper.ValidatePassword(null);

        isValid.Should().BeFalse();
        error.Should().Be("Password is required");
    }

    [Fact]
    public void ValidateEmail_ValidEmail_ReturnsSuccess()
    {
        var (isValid, error) = ValidationHelper.ValidateEmail("user@example.com");

        isValid.Should().BeTrue();
        error.Should().BeEmpty();
    }

    [Fact]
    public void ValidateEmail_NoAtSymbol_ReturnsInvalidFormatError()
    {
        var (isValid, error) = ValidationHelper.ValidateEmail("notanemail");

        isValid.Should().BeFalse();
        error.Should().ContainEquivalentOf("invalid");
    }
}
