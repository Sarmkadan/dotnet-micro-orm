#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class UserModelTests
{
    [Fact]
    public void Validate_WithValidUser_ReturnsTrue()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = "", Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username"));
    }

    [Fact]
    public void Validate_WithNullUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = null!, Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username"));
    }

    [Fact]
    public void Validate_WithShortUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = "ab", Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username") && e.Contains("3 characters"));
    }

    [Fact]
    public void Validate_WithInvalidEmail_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "invalidemail", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    [Fact]
    public void Validate_WithEmptyEmail_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    [Fact]
    public void Validate_WithNullEmail_ReturnsFalseWithError()
    {
        var user = new User { Username = "johndoe", Email = null!, PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    [Fact]
    public void Validate_WithShortPasswordHash_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "john@example.com", "short");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Password hash"));
    }

    [Fact]
    public void Validate_WithEmptyPasswordHash_ReturnsFalseWithError()
    {
        var user = new User { Username = "johndoe", Email = "john@example.com", PasswordHash = "" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Password hash"));
    }

    [Fact]
    public void Validate_WithLongFirstName_ReturnsFalseWithError()
    {
        var longName = new string('a', 51);
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            FirstName = longName
        };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("First name"));
    }

    [Fact]
    public void Validate_WithLongLastName_ReturnsFalseWithError()
    {
        var longName = new string('a', 51);
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            LastName = longName
        };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Last name"));
    }

    [Fact]
    public void Validate_WithValidFirstAndLastNames_IncludesInValidation()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            FirstName = "John",
            LastName = "Doe"
        };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var user = new User { Username = "a", Email = "invalid", PasswordHash = "short" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void GetFullName_WithFirstAndLastNames_ReturnsCombined()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            FirstName = "John",
            LastName = "Doe"
        };

        var fullName = user.GetFullName();

        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void GetFullName_WithOnlyFirstName_ReturnsFirstName()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            FirstName = "John"
        };

        var fullName = user.GetFullName();

        fullName.Should().Be("John");
    }

    [Fact]
    public void GetFullName_WithOnlyLastName_ReturnsLastName()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            LastName = "Doe"
        };

        var fullName = user.GetFullName();

        fullName.Should().Be("Doe");
    }

    [Fact]
    public void GetFullName_WithNoNames_ReturnsEmpty()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        var fullName = user.GetFullName();

        fullName.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsEmailVerified_ChangesEmailVerificationFlag()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");
        user.IsEmailVerified.Should().BeFalse();

        user.MarkAsEmailVerified();

        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public void UpdateLastLogin_SetsLastLoginDate()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");
        user.LastLoginDate.Should().BeNull();

        var beforeUpdate = DateTime.UtcNow;
        user.UpdateLastLogin();
        var afterUpdate = DateTime.UtcNow;

        user.LastLoginDate.Should().NotBeNull();
        user.LastLoginDate.Should().BeOnOrAfter(beforeUpdate);
        user.LastLoginDate.Should().BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public async Task UpdateLastLogin_MultipleUpdates_UpdatesToLatestTime()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        user.UpdateLastLogin();
        var firstLogin = user.LastLoginDate;

        await Task.Delay(10);
        user.UpdateLastLogin();
        var secondLogin = user.LastLoginDate;

        secondLogin.Should().BeAfter(firstLogin!.Value);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");
        user.IsActive.Should().BeTrue();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithParameters_InitializesFieldsCorrectly()
    {
        var user = new User("testuser", "test@example.com", "hashedpasswordtoken");

        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hashedpasswordtoken");
        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
        user.CreatedDate.Should().BeOnOrBefore(DateTime.UtcNow);
        user.Version.Should().Be(1);
    }

    [Fact]
    public void Constructor_Default_InitializesWithDefaults()
    {
        var user = new User();

        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
        user.Orders.Should().BeEmpty();
    }

    [Fact]
    public void IsActive_CanBeModifiedDirectly()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            IsActive = false
        };

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Version_DefaultsToOne()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        user.Version.Should().Be(1);
    }
}
