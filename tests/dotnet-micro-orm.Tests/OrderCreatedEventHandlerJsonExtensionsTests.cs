using System;
using DotnetMicroOrm.Events;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public class OrderCreatedEventHandlerJsonExtensionsTests
{
    private readonly OrderCreatedEventHandler _sampleHandler = new();

    [Fact]
    public void ToJson_WithValidHandler_ReturnsNonEmptyJsonString()
    {
        // Act
        var json = _sampleHandler.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("priority");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Act
        var json = _sampleHandler.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("{\n"); // Should have newlines
        json.Should().Contain("  "); // Should have indentation
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Act
        var json = _sampleHandler.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().NotContain("\n"); // Should not have newlines
    }

    [Fact]
    public void ToJson_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        OrderCreatedEventHandler nullHandler = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullHandler.ToJson());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_WithNullOrWhitespace_ReturnsNull(string? invalidJson)
    {
        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedHandler()
    {
        // Arrange
        var json = _sampleHandler.ToJson();

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(_sampleHandler);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "{ invalid json {{{";

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithEmptyObject_ReturnsHandlerWithDefaultValues()
    {
        // Arrange
        var emptyJson = "{}";

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.FromJson(emptyJson);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new OrderCreatedEventHandler());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryFromJson_WithNullOrWhitespace_ReturnsFalseAndNull(string? invalidJson)
    {
        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedHandler()
    {
        // Arrange
        var json = _sampleHandler.ToJson();

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeEquivalentTo(_sampleHandler);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json {{{";

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithEmptyObject_ReturnsTrueAndHandlerWithDefaultValues()
    {
        // Arrange
        var emptyJson = "{}";

        // Act
        var result = OrderCreatedEventHandlerJsonExtensions.TryFromJson(emptyJson, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value.Should().BeEquivalentTo(new OrderCreatedEventHandler());
    }

    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesData()
    {
        // Arrange
        var originalHandler = _sampleHandler;

        // Act
        var json = originalHandler.ToJson();
        var deserializedHandler = OrderCreatedEventHandlerJsonExtensions.FromJson(json);

        // Assert
        deserializedHandler.Should().NotBeNull();
        deserializedHandler.Should().BeEquivalentTo(originalHandler);
    }

    [Fact]
    public void Roundtrip_WithIndentedSerialization_PreservesData()
    {
        // Arrange
        var originalHandler = _sampleHandler;

        // Act
        var json = originalHandler.ToJson(indented: true);
        var deserializedHandler = OrderCreatedEventHandlerJsonExtensions.FromJson(json);

        // Assert
        deserializedHandler.Should().NotBeNull();
        deserializedHandler.Should().BeEquivalentTo(originalHandler);
    }

    [Fact]
    public void TryFromJson_WithNullInput_DoesNotThrow()
    {
        // Arrange
        string nullJson = null!;

        // Act
        var act = () => OrderCreatedEventHandlerJsonExtensions.TryFromJson(nullJson, out var _);

        // Assert - TryFromJson should not throw for null, it should return false
        act.Should().NotThrow();
        var result = OrderCreatedEventHandlerJsonExtensions.TryFromJson(nullJson, out var value);
        result.Should().BeFalse();
        value.Should().BeNull();
    }
}