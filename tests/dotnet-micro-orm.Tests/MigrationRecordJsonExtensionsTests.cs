#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Migrations;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the MigrationRecordJsonExtensions class.
/// </summary>
public sealed class MigrationRecordJsonExtensionsTests
{
    /// <summary>
    /// Tests that ToJson serializes a valid MigrationRecord to JSON.
    /// </summary>
    [Fact]
    public void ToJson_WithValidMigrationRecord_ReturnsJsonString()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = 1,
            Version = "20240101_001",
            Description = "Initial migration",
            AppliedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Success = true,
            ErrorMessage = null
        };

        // Act
        var json = record.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("20240101_001");
        json.Should().Contain("Initial migration");
        json.Should().Contain("true");
    }

    /// <summary>
    /// Tests that ToJson with indented parameter produces formatted JSON.
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = 2,
            Version = "20240102_001",
            Description = "Second migration",
            AppliedAt = DateTime.UtcNow,
            Success = true
        };

        // Act
        var json = record.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\n"); // Should contain newlines for formatting
        json.Should().Contain("  "); // Should contain indentation
    }

    /// <summary>
    /// Tests that ToJson with indented parameter false produces compact JSON.
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = 3,
            Version = "20240103_001",
            Description = "Third migration",
            AppliedAt = DateTime.UtcNow,
            Success = false,
            ErrorMessage = "Test error"
        };

        // Act
        var json = record.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().NotContain("\n"); // Should not contain newlines
    }

    /// <summary>
    /// Tests that ToJson throws ArgumentNullException when passed null.
    /// </summary>
    [Fact]
    public void ToJson_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        MigrationRecord? record = null;

        // Act
        Action act = () => record!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that FromJson deserializes valid JSON to a MigrationRecord.
    /// </summary>
    [Fact]
    public void FromJson_WithValidJson_ReturnsMigrationRecord()
    {
        // Arrange
        var json = "{\"id\":4,\"version\":\"20240104_001\",\"description\":\"Fourth migration\",\"appliedAt\":\"2024-01-04T12:00:00Z\",\"success\":true}";

        // Act
        var record = MigrationRecordJsonExtensions.FromJson(json);

        // Assert
        record.Should().NotBeNull();
        record!.Id.Should().Be(4);
        record.Version.Should().Be("20240104_001");
        record.Description.Should().Be("Fourth migration");
        record.Success.Should().BeTrue();
        record.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromJson returns null when passed null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_WithNullOrWhitespace_ReturnsNull(string? json)
    {
        // Act
        var record = MigrationRecordJsonExtensions.FromJson(json);

        // Assert
        record.Should().BeNull();
    }

    /// <summary>
    /// Tests that FromJson deserializes JSON with all properties including error message.
    /// </summary>
    [Fact]
    public void FromJson_WithErrorMessage_ReturnsMigrationRecordWithError()
    {
        // Arrange
        var json = "{\"id\":5,\"version\":\"20240105_001\",\"description\":\"Failed migration\",\"appliedAt\":\"2024-01-05T12:00:00Z\",\"success\":false,\"errorMessage\":\"Migration failed due to database constraint\"}";

        // Act
        var record = MigrationRecordJsonExtensions.FromJson(json);

        // Assert
        record.Should().NotBeNull();
        record!.Id.Should().Be(5);
        record.Version.Should().Be("20240105_001");
        record.Success.Should().BeFalse();
        record.ErrorMessage.Should().Be("Migration failed due to database constraint");
    }

    /// <summary>
    /// Tests that TryFromJson returns true and deserializes valid JSON.
    /// </summary>
    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = "{\"id\":6,\"version\":\"20240106_001\",\"description\":\"Sixth migration\",\"appliedAt\":\"2024-01-06T12:00:00Z\",\"success\":true}";

        // Act
        var result = MigrationRecordJsonExtensions.TryFromJson(json, out var record);

        // Assert
        result.Should().BeTrue();
        record.Should().NotBeNull();
        record!.Id.Should().Be(6);
        record.Version.Should().Be("20240106_001");
    }

    /// <summary>
    /// Tests that TryFromJson returns false when passed null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryFromJson_WithNullOrWhitespace_ReturnsFalseAndNull(string? json)
    {
        // Act
        var result = MigrationRecordJsonExtensions.TryFromJson(json, out var record);

        // Assert
        result.Should().BeFalse();
        record.Should().BeNull();
    }

    /// <summary>
    /// Tests that TryFromJson handles invalid JSON gracefully.
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = MigrationRecordJsonExtensions.TryFromJson(invalidJson, out var record);

        // Assert
        result.Should().BeFalse();
        record.Should().BeNull();
    }

    /// <summary>
    /// Tests that TryFromJson handles malformed JSON (missing required properties).
    /// </summary>
    [Fact]
    public void TryFromJson_WithMalformedJson_ReturnsFalseAndNull()
    {
        // Arrange
        var malformedJson = "{ invalid syntax here }";

        // Act
        var result = MigrationRecordJsonExtensions.TryFromJson(malformedJson, out var record);

        // Assert
        result.Should().BeFalse();
        record.Should().BeNull();
    }

    /// <summary>
    /// Tests round-trip serialization and deserialization preserves all properties.
    /// </summary>
    [Fact]
    public void ToJson_FromJson_RoundTripPreservesAllProperties()
    {
        // Arrange
        var original = new MigrationRecord
        {
            Id = 100,
            Version = "20241231_999",
            Description = "Round-trip test migration",
            AppliedAt = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            Success = false,
            ErrorMessage = "Test error message for round-trip"
        };

        // Act
        var json = original.ToJson();
        var deserialized = MigrationRecordJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.Version.Should().Be(original.Version);
        deserialized.Description.Should().Be(original.Description);
        deserialized.AppliedAt.Should().Be(original.AppliedAt);
        deserialized.Success.Should().Be(original.Success);
        deserialized.ErrorMessage.Should().Be(original.ErrorMessage);
    }

    /// <summary>
    /// Tests round-trip with TryFromJson preserves all properties.
    /// </summary>
    [Fact]
    public void ToJson_TryFromJson_RoundTripPreservesAllProperties()
    {
        // Arrange
        var original = new MigrationRecord
        {
            Id = 200,
            Version = "20250101_001",
            Description = "TryFromJson round-trip test",
            AppliedAt = DateTime.UtcNow,
            Success = true,
            ErrorMessage = null
        };

        // Act
        var json = original.ToJson();
        var result = MigrationRecordJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeTrue();
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.Version.Should().Be(original.Version);
        deserialized.Description.Should().Be(original.Description);
        deserialized.AppliedAt.Should().BeCloseTo(original.AppliedAt, TimeSpan.FromSeconds(1));
        deserialized.Success.Should().Be(original.Success);
        deserialized.ErrorMessage.Should().Be(original.ErrorMessage);
    }

    /// <summary>
    /// Tests that JSON uses camelCase property naming as expected.
    /// </summary>
    [Fact]
    public void ToJson_ProducesCamelCaseProperties()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = 300,
            Version = "20250102_001",
            Description = "CamelCase test",
            AppliedAt = DateTime.UtcNow,
            Success = true
        };

        // Act
        var json = record.ToJson();

        // Assert
        json.Should().Contain("version");
        json.Should().Contain("description");
        json.Should().Contain("appliedAt");
        json.Should().Contain("success");
        json.Should().NotContain("errorMessage"); // Should not contain null properties
        json.Should().NotContain("Version"); // PascalCase should not be present
        json.Should().NotContain("Description");
    }

    /// <summary>
    /// Tests that JSON omits null properties when serialized.
    /// </summary>
    [Fact]
    public void ToJson_OmitsNullProperties()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = 400,
            Version = "20250103_001",
            Description = "Null properties test",
            AppliedAt = DateTime.UtcNow,
            Success = true,
            ErrorMessage = null
        };

        // Act
        var json = record.ToJson();

        // Assert
        json.Should().NotContain("errorMessage"); // Should be omitted when null
    }
}