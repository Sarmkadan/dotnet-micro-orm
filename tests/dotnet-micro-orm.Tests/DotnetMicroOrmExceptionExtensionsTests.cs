#nullable enable

using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the DotnetMicroOrmExceptionExtensions class.
/// </summary>
public sealed class DotnetMicroOrmExceptionExtensionsTests
{
    /// <summary>
    /// Tests that WithContexts adds multiple context entries to an exception.
    /// </summary>
    [Fact]
    public void WithContexts_AddsMultipleContextEntries()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");
        var contexts = new Dictionary<string, object>
        {
            { "Key1", "Value1" },
            { "Key2", 42 },
            { "Key3", true }
        };

        // Act
        var result = exception.WithContexts(contexts);

        // Assert
        result.Should().BeSameAs(exception, "WithContexts should return the same exception instance for chaining");
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext.Should().HaveCount(3);
        exception.ErrorContext!["Key1"].Should().Be("Value1");
        exception.ErrorContext!["Key2"].Should().Be(42);
        exception.ErrorContext!["Key3"].Should().Be(true);
    }

    /// <summary>
    /// Tests that WithContexts throws ArgumentNullException when exception is null.
    /// </summary>
    [Fact]
    public void WithContexts_ThrowsOnNullException()
    {
        // Arrange
        DotnetMicroOrmException? exception = null;
        var contexts = new Dictionary<string, object> { { "Key", "Value" } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.WithContexts(contexts));
    }

    /// <summary>
    /// Tests that WithContexts throws ArgumentNullException when contexts dictionary is null.
    /// </summary>
    [Fact]
    public void WithContexts_ThrowsOnNullContexts()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");
        Dictionary<string, object>? contexts = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.WithContexts(contexts!));
    }

    /// <summary>
    /// Tests that WithContexts throws ArgumentNullException when contexts dictionary is empty.
    /// </summary>
    [Fact]
    public void WithContexts_HandlesEmptyContexts()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");
        var contexts = new Dictionary<string, object>();

        // Act
        var result = exception.WithContexts(contexts);

        // Assert
        result.Should().BeSameAs(exception);
        exception.ErrorContext.Should().BeNull("Empty contexts should not create ErrorContext");
    }

    /// <summary>
    /// Tests that GetContextValue retrieves a string value from the context.
    /// </summary>
    [Fact]
    public void GetContextValue_RetrievesStringValue()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error")
            .WithContext("Username", "john_doe");

        // Act
        var result = exception.GetContextValue<string>("Username");

        // Assert
        result.Should().Be("john_doe");
    }

    /// <summary>
    /// Tests that GetContextValue retrieves an integer value from the context.
    /// </summary>
    [Fact]
    public void GetContextValue_RetrievesIntegerValue()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error")
            .WithContext("UserId", 123);

        // Act
        var result = exception.GetContextValue<int>("UserId");

        // Assert
        result.Should().Be(123);
    }

    /// <summary>
    /// Tests that GetContextValue retrieves a boolean value from the context.
    /// </summary>
    [Fact]
    public void GetContextValue_RetrievesBooleanValue()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error")
            .WithContext("IsActive", true);

        // Act
        var result = exception.GetContextValue<bool>("IsActive");

        // Assert
        result.Should().Be(true);
    }

    /// <summary>
    /// Tests that GetContextValue returns default when key doesn't exist.
    /// </summary>
    [Fact]
    public void GetContextValue_ReturnsDefaultWhenKeyMissing()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");

        // Act
        var result = exception.GetContextValue<string>("NonExistentKey");

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetContextValue returns default when cast fails.
    /// </summary>
    [Fact]
    public void GetContextValue_ReturnsDefaultWhenCastFails()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error")
            .WithContext("Age", "NotANumber"); // String value, but we ask for int

        // Act
        var result = exception.GetContextValue<int>("Age");

        // Assert
        result.Should().Be(0); // Default for int
    }

    /// <summary>
    /// Tests that GetContextValue throws ArgumentNullException when exception is null.
    /// </summary>
    [Fact]
    public void GetContextValue_ThrowsOnNullException()
    {
        // Arrange
        DotnetMicroOrmException? exception = null;
        const string key = "Key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.GetContextValue<string>(key));
    }

    /// <summary>
    /// Tests that GetContextValue throws ArgumentNullException when key is null.
    /// </summary>
    [Fact]
    public void GetContextValue_ThrowsOnNullKey()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");
        string? key = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.GetContextValue<string>(key!));
    }

    /// <summary>
    /// Tests that GetContextValue throws ArgumentException when key is empty.
    /// </summary>
    [Fact]
    public void GetContextValue_ThrowsOnEmptyKey()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => exception.GetContextValue<string>(""));
    }

    /// <summary>
    /// Tests that GetContextValue accepts whitespace keys (ArgumentException.ThrowIfNullOrEmpty only checks for null or empty, not whitespace).
    /// </summary>
    [Fact]
    public void GetContextValue_AcceptsWhitespaceKey()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error")
            .WithContext("   ", "whitespace value");

        // Act
        var result = exception.GetContextValue<string>("   ");

        // Assert
        result.Should().Be("whitespace value");
    }

    /// <summary>
    /// Tests that ToDetailedString includes base exception information.
    /// </summary>
    [Fact]
    public void ToDetailedString_IncludesBaseExceptionInformation()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Base error message");

        // Act
        var result = exception.ToDetailedString();

        // Assert
        result.Should().Contain("Base error message");
        result.Should().Contain("DotnetMicroOrmException");
    }

    /// <summary>
    /// Tests that ToDetailedString includes context when present.
    /// </summary>
    [Fact]
    public void ToDetailedString_IncludesContextWhenPresent()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Base error message")
            .WithContext("UserId", 42)
            .WithContext("Action", "Update");

        // Act
        var result = exception.ToDetailedString();

        // Assert
        result.Should().Contain("Base error message");
        result.Should().Contain("Context:");
        result.Should().Contain("UserId=42");
        result.Should().Contain("Action=Update");
    }

    /// <summary>
    /// Tests that ToDetailedString returns base ToString when no context.
    /// </summary>
    [Fact]
    public void ToDetailedString_ReturnsBaseToStringWhenNoContext()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Base error message");

        // Act
        var result = exception.ToDetailedString();

        // Assert
        result.Should().Be(exception.ToString());
    }

    /// <summary>
    /// Tests that ToDetailedString includes error code when present.
    /// </summary>
    [Fact]
    public void ToDetailedString_IncludesErrorCodeWhenPresent()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Base error message", "TEST_ERROR_CODE");

        // Act
        var result = exception.ToDetailedString();

        // Assert
        result.Should().Contain("TEST_ERROR_CODE");
    }

    /// <summary>
    /// Tests that ToDetailedString throws ArgumentNullException when exception is null.
    /// </summary>
    [Fact]
    public void ToDetailedString_ThrowsOnNullException()
    {
        // Arrange
        DotnetMicroOrmException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ToDetailedString());
    }

    /// <summary>
    /// Tests that all three methods work together correctly.
    /// </summary>
    [Fact]
    public void AllMethods_WorkTogether()
    {
        // Arrange
        var contexts = new Dictionary<string, object>
        {
            { "EntityType", "User" },
            { "EntityId", 123 },
            { "Operation", "Delete" }
        };

        var exception = new DotnetMicroOrmException("Entity operation failed", "ENTITY_OP_ERROR")
            .WithContexts(contexts);

        // Act - use GetContextValue to retrieve values
        var entityType = exception.GetContextValue<string>("EntityType");
        var entityId = exception.GetContextValue<int>("EntityId");
        var operation = exception.GetContextValue<string>("Operation");
        var detailedString = exception.ToDetailedString();

        // Assert
        entityType.Should().Be("User");
        entityId.Should().Be(123);
        operation.Should().Be("Delete");
        detailedString.Should().Contain("Entity operation failed");
        detailedString.Should().Contain("Context:");
        detailedString.Should().Contain("ENTITY_OP_ERROR");
    }
}