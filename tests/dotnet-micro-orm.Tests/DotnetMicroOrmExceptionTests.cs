using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public class DotnetMicroOrmExceptionTests
{
    [Fact]
    public void Constructor_WithMessageOnly_SetsMessageAndNullProperties()
    {
        // Arrange & Act
        var exception = new DotnetMicroOrmException("Test error message");

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().BeNull();
        exception.ErrorContext.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_SetsBothProperties()
    {
        // Arrange & Act
        var exception = new DotnetMicroOrmException("Test error message", "TEST001");

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().Be("TEST001");
        exception.ErrorContext.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageErrorCodeAndInnerException_SetsAllProperties()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DotnetMicroOrmException("Test error message", "TEST001", innerException);

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().Be("TEST001");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.ErrorContext.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullMessage_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new DotnetMicroOrmException(null!);

        // Assert
        exception.Message.Should().NotBeNull();
        exception.ErrorCode.Should().BeNull();
        exception.ErrorContext.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void WithContext_WhenErrorContextIsNull_CreatesNewDictionary()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");

        // Act
        var result = exception.WithContext("key1", "value1");

        // Assert
        result.Should().BeSameAs(exception);
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext.Should().HaveCount(1);
        exception.ErrorContext.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
    }

    [Fact]
    public void WithContext_WhenErrorContextAlreadyExists_AddsNewKeyValue()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message", "TEST001");
        exception.WithContext("key1", "value1");

        // Act
        var result = exception.WithContext("key2", "value2");

        // Assert
        result.Should().BeSameAs(exception);
        exception.ErrorContext.Should().HaveCount(2);
        exception.ErrorContext.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
        exception.ErrorContext.Should().ContainKey("key2").WhoseValue.Should().Be("value2");
    }

    [Fact]
    public void WithContext_WhenOverwritingExistingKey_UpdatesValue()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");
        exception.WithContext("key1", "value1");

        // Act
        var result = exception.WithContext("key1", "newValue");

        // Assert
        result.Should().BeSameAs(exception);
        exception.ErrorContext.Should().HaveCount(1);
        exception.ErrorContext.Should().ContainKey("key1").WhoseValue.Should().Be("newValue");
    }

    [Fact]
    public void WithContext_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");

        // Act
        Action act = () => exception.WithContext(null!, "value");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToString_WithOnlyMessage_ReturnsBaseExceptionString()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error message");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("Test error message");
        result.Should().NotContain("Error Code:");
        result.Should().NotContain("Context:");
    }

    [Fact]
    public void ToString_WithErrorCode_AppendsErrorCode()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error message", "TEST001");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("Test error message");
        result.Should().Contain("Error Code: TEST001");
    }

    [Fact]
    public void ToString_WithErrorContext_AppendsContext()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test error message");
        exception.WithContext("param1", "value1");
        exception.WithContext("param2", 123);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("Test error message");
        result.Should().Contain("Context: param1=value1, param2=123");
    }

    [Fact]
    public void ToString_WithAllProperties_ContainsAllInformation()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var exception = new DotnetMicroOrmException("Test error message", "TEST001", innerException);
        exception.WithContext("param1", "value1");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("Test error message");
        result.Should().Contain("Error Code: TEST001");
        result.Should().Contain("Context: param1=value1");
    }

    [Fact]
    public void ErrorCodeProperty_CanBeSetDirectly()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");

        // Act
        exception.ErrorCode = "CUSTOM002";

        // Assert
        exception.ErrorCode.Should().Be("CUSTOM002");
    }

    [Fact]
    public void ErrorContextProperty_CanBeSetDirectly()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");
        var context = new Dictionary<string, object> { { "key", "value" } };

        // Act
        exception.ErrorContext = context;

        // Assert
        exception.ErrorContext.Should().BeSameAs(context);
        exception.ErrorContext.Should().HaveCount(1);
    }

    [Fact]
    public void ErrorContextProperty_WhenSetToNull_ClearsContext()
    {
        // Arrange
        var exception = new DotnetMicroOrmException("Test message");
        exception.WithContext("key", "value");
        exception.ErrorContext.Should().NotBeNull();

        // Act
        exception.ErrorContext = null;

        // Assert
        exception.ErrorContext.Should().BeNull();
    }
}