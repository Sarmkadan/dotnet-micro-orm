#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Contains unit tests for the <see cref="User"/> model class.
/// Tests various validation scenarios, property behaviors, and method functionality
/// to ensure the User model works as expected in different use cases.
/// </summary>
public sealed class UserModelTests
{
    /// <summary>
    /// Tests that validation passes when all user properties are valid.
    /// </summary>
    [Fact]
    public void Validate_WithValidUser_ReturnsTrue()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that validation fails when username is empty and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = "", Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username"));
    }

    /// <summary>
    /// Tests that validation fails when username is null and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithNullUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = null!, Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username"));
    }

    /// <summary>
    /// Tests that validation fails when username is too short (less than 3 characters) and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithShortUsername_ReturnsFalseWithError()
    {
        var user = new User { Username = "ab", Email = "test@example.com", PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Username") && e.Contains("3 characters"));
    }

    /// <summary>
    /// Tests that validation fails when email is invalid and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithInvalidEmail_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "invalidemail", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    /// <summary>
    /// Tests that validation fails when email is empty and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyEmail_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "", "hashedpassword1234567890");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    /// <summary>
    /// Tests that validation fails when email is null and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithNullEmail_ReturnsFalseWithError()
    {
        var user = new User { Username = "johndoe", Email = null!, PasswordHash = "hashedpassword1234567890" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Email"));
    }

    /// <summary>
    /// Tests that validation fails when password hash is too short and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithShortPasswordHash_ReturnsFalseWithError()
    {
        var user = new User("johndoe", "john@example.com", "short");

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Password hash"));
    }

    /// <summary>
    /// Tests that validation fails when password hash is empty and returns appropriate error message.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyPasswordHash_ReturnsFalseWithError()
    {
        var user = new User { Username = "johndoe", Email = "john@example.com", PasswordHash = "" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Password hash"));
    }

    /// <summary>
    /// Tests that validation fails when first name exceeds maximum length (50 characters) and returns appropriate error message.
    /// </summary>
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

    /// <summary>
    /// Tests that validation fails when last name exceeds maximum length (50 characters) and returns appropriate error message.
    /// </summary>
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

    /// <summary>
    /// Tests that validation passes when both first and last names are valid and included in full name.
    /// </summary>
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

    /// <summary>
    /// Tests that validation returns all validation errors when multiple properties are invalid.
    /// </summary>
    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var user = new User { Username = "a", Email = "invalid", PasswordHash = "short" };

        var isValid = user.Validate(out var errors);

        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// Tests that GetFullName() returns combined first and last names separated by space when both are present.
    /// </summary>
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

    /// <summary>
    /// Tests that GetFullName() returns only the first name when last name is not set.
    /// </summary>
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

    /// <summary>
    /// Tests that GetFullName() returns only the last name when first name is not set.
    /// </summary>
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

    /// <summary>
    /// Tests that GetFullName() returns empty string when neither first nor last name is set.
    /// </summary>
    [Fact]
    public void GetFullName_WithNoNames_ReturnsEmpty()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        var fullName = user.GetFullName();

        fullName.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that MarkAsEmailVerified() sets the IsEmailVerified property to true.
    /// </summary>
    [Fact]
    public void MarkAsEmailVerified_ChangesEmailVerificationFlag()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");
        user.IsEmailVerified.Should().BeFalse();

        user.MarkAsEmailVerified();

        user.IsEmailVerified.Should().BeTrue();
    }

    /// <summary>
    /// Tests that UpdateLastLogin() sets the LastLoginDate property to current UTC time.
    /// </summary>
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

    /// <summary>
    /// Tests that UpdateLastLogin() updates the LastLoginDate to the latest time when called multiple times.
    /// </summary>
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

    /// <summary>
    /// Tests that Deactivate() sets the IsActive property to false.
    /// </summary>
    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");
        user.IsActive.Should().BeTrue();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the constructor with parameters initializes all user properties correctly including defaults.
    /// </summary>
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

    /// <summary>
    /// Tests that the default constructor initializes user with default values for all properties.
    /// </summary>
    [Fact]
    public void Constructor_Default_InitializesWithDefaults()
    {
        var user = new User();

        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
        user.Orders.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the IsActive property can be modified directly after user creation.
    /// </summary>
    [Fact]
    public void IsActive_CanBeModifiedDirectly()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890")
        {
            IsActive = false
        };

        user.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Version property defaults to 1 when a user is created.
    /// </summary>
    [Fact]
    public void Version_DefaultsToOne()
    {
        var user = new User("johndoe", "john@example.com", "hashedpassword1234567890");

        user.Version.Should().Be(1);
    }
}
