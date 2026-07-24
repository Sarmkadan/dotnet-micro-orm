#nullable enable

using DotnetMicroOrm.Domain.Models;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the AuditLogJsonExtensions class that provides JSON serialization helpers for AuditLog type
/// </summary>
public sealed class AuditLogJsonExtensionsTests
{
    /// <summary>
    /// Tests that ToJson serializes an AuditLog instance to a JSON string with camelCase property names
    /// </summary>
    [Fact]
    public void ToJson_WithAuditLog_SerializesToCamelCaseJson()
    {
        // Arrange
        var auditLog = new AuditLog("Product", 123, "UPDATE")
        {
            UserId = 42,
            Username = "testuser",
            OldValues = "{\"Name\":\"Old Product\"}",
            NewValues = "{\"Name\":\"New Product\",\"Price\":199}",
            ChangedProperties = "Name,Price",
            IPAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            IsSuccessful = true,
            Timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var json = auditLog.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("entityType");
        json.Should().Contain("entityId");
        json.Should().Contain("action");
        json.Should().Contain("userId");
        json.Should().Contain("username");
        json.Should().Contain("oldValues");
        json.Should().Contain("newValues");
        json.Should().Contain("changedProperties");
        json.Should().Contain("ipAddress");
        json.Should().Contain("userAgent");
        json.Should().Contain("isSuccessful");
        json.Should().Contain("timestamp");
        json.Should().Contain("2024-01-15T10:30:00");
    }

    /// <summary>
    /// Tests that ToJson with indented parameter produces formatted JSON
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedTrue_ProducesFormattedJson()
    {
        // Arrange
        var auditLog = new AuditLog("Category", 456, "INSERT")
        {
            UserId = 1,
            Username = "admin",
            IsSuccessful = false,
            ErrorMessage = "Validation failed"
        };

        // Act
        var json = auditLog.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("{");
        json.Should().Contain("\n");
        json.Should().Contain("  "); // indentation
    }

    /// <summary>
    /// Tests that ToJson with indented parameter false produces compact JSON
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedFalse_ProducesCompactJson()
    {
        // Arrange
        var auditLog = new AuditLog("User", 789, "DELETE")
        {
            UserId = 2,
            Username = "moderator",
            IsSuccessful = true
        };

        // Act
        var json = auditLog.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().NotContain("\n"); // No newlines
        json.Should().NotContain("  "); // No indentation
    }

    /// <summary>
    /// Tests that ToJson throws ArgumentNullException when null is passed
    /// </summary>
    [Fact]
    public void ToJson_WithNullAuditLog_ThrowsArgumentNullException()
    {
        // Arrange
        AuditLog? auditLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => auditLog!.ToJson());
    }

    /// <summary>
    /// Tests that FromJson deserializes a JSON string back to an AuditLog instance
    /// </summary>
    [Fact]
    public void FromJson_WithValidJson_ReturnsAuditLogInstance()
    {
        // Arrange
        var json = "{\"entityType\":\"Product\",\"entityId\":123,\"action\":\"UPDATE\",\"userId\":42,\"username\":\"testuser\",\"oldValues\":\"{\\\"Name\\\":\\\"Old Product\\\"}\",\"newValues\":\"{\\\"Name\\\":\\\"New Product\\\",\\\"Price\\\":199}\",\"changedProperties\":\"Name,Price\",\"ipAddress\":\"192.168.1.1\",\"userAgent\":\"Mozilla/5.0\",\"isSuccessful\":true,\"timestamp\":\"2024-01-15T10:30:00Z\"}";

        // Act
        var auditLog = AuditLogJsonExtensions.FromJson(json);

        // Assert
        auditLog.Should().NotBeNull();
        auditLog!.EntityType.Should().Be("Product");
        auditLog.EntityId.Should().Be(123);
        auditLog.Action.Should().Be("UPDATE");
        auditLog.UserId.Should().Be(42);
        auditLog.Username.Should().Be("testuser");
        auditLog.OldValues.Should().Be("{\"Name\":\"Old Product\"}");
        auditLog.NewValues.Should().Be("{\"Name\":\"New Product\",\"Price\":199}");
        auditLog.ChangedProperties.Should().Be("Name,Price");
        auditLog.IPAddress.Should().Be("192.168.1.1");
        auditLog.UserAgent.Should().Be("Mozilla/5.0");
        auditLog.IsSuccessful.Should().BeTrue();
        auditLog.Timestamp.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentException when empty or whitespace string is passed
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromJson_WithEmptyOrWhitespaceJson_ThrowsArgumentException(string json)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => AuditLogJsonExtensions.FromJson(json));
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentNullException when null string is passed
    /// </summary>
    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AuditLogJsonExtensions.FromJson(null));
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentException when whitespace-only string is passed
    /// </summary>
    [Fact]
    public void FromJson_WithWhitespaceOnlyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AuditLogJsonExtensions.FromJson(json));
    }

    /// <summary>
    /// Tests that TryFromJson returns true and deserializes valid JSON
    /// </summary>
    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = "{\"entityType\":\"Category\",\"entityId\":456,\"action\":\"INSERT\",\"userId\":1,\"username\":\"admin\",\"isSuccessful\":false,\"errorMessage\":\"Validation failed\"}";

        // Act
        var result = AuditLogJsonExtensions.TryFromJson(json, out var auditLog);

        // Assert
        result.Should().BeTrue();
        auditLog.Should().NotBeNull();
        auditLog!.EntityType.Should().Be("Category");
        auditLog.EntityId.Should().Be(456);
        auditLog.Action.Should().Be("INSERT");
        auditLog.UserId.Should().Be(1);
        auditLog.Username.Should().Be("admin");
        auditLog.IsSuccessful.Should().BeFalse();
        auditLog.ErrorMessage.Should().Be("Validation failed");
    }

    /// <summary>
    /// Tests that TryFromJson returns false when null or empty JSON is passed
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryFromJson_WithNullOrEmptyJson_ReturnsFalse(string? json)
    {
        // Act
        var result = AuditLogJsonExtensions.TryFromJson(json, out var auditLog);

        // Assert
        result.Should().BeFalse();
        auditLog.Should().BeNull();
    }

    /// <summary>
    /// Tests that TryFromJson returns false when invalid JSON is passed
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "{ invalid json";

        // Act
        var result = AuditLogJsonExtensions.TryFromJson(json, out var auditLog);

        // Assert
        result.Should().BeFalse();
        auditLog.Should().BeNull();
    }

    /// <summary>
    /// Tests round-trip serialization and deserialization preserves all data
    /// </summary>
    [Fact]
    public void RoundTrip_ToJsonThenFromJson_PreservesAllData()
    {
        // Arrange
        var original = new AuditLog("User", 999, "DELETE")
        {
            UserId = 7,
            Username = "superadmin",
            OldValues = "{\"Username\":\"john_doe\"}",
            IPAddress = "10.0.0.1",
            UserAgent = "curl/7.68.0",
            IsSuccessful = false,
            ErrorMessage = "User deletion failed: foreign key constraint",
            Timestamp = new DateTime(2024, 6, 20, 14, 45, 30, DateTimeKind.Utc)
        };

        // Act
        var json = original.ToJson();
        var deserialized = AuditLogJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(original);
    }

    /// <summary>
    /// Tests that round-trip with minimal AuditLog preserves data
    /// </summary>
    [Fact]
    public void RoundTrip_WithMinimalAuditLog_PreservesRequiredFields()
    {
        // Arrange
        var original = new AuditLog("Product", 1, "INSERT");

        // Act
        var json = original.ToJson();
        var deserialized = AuditLogJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.EntityType.Should().Be("Product");
        deserialized.EntityId.Should().Be(1);
        deserialized.Action.Should().Be("INSERT");
        deserialized.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        deserialized.IsSuccessful.Should().BeTrue();
    }

    /// <summary>
    /// Tests that TryFromJson out parameter is set to null when deserialization fails
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_SetsOutParameterToNull()
    {
        // Arrange
        var json = "{ invalid json";

        // Act
        var result = AuditLogJsonExtensions.TryFromJson(json, out var auditLog);

        // Assert
        result.Should().BeFalse();
        auditLog.Should().BeNull();
    }

    /// <summary>
    /// Tests that TryFromJson works with Try-parse pattern
    /// </summary>
    [Fact]
    public void TryFromJson_WithTryParsePattern_WorksCorrectly()
    {
        // Arrange
        var json = "{\"entityType\":\"Order\",\"entityId\":12345,\"action\":\"UPDATE\"}";

        // Act
        if (AuditLogJsonExtensions.TryFromJson(json, out var auditLog))
        {
            // Success path
            auditLog.Should().NotBeNull();
            auditLog!.EntityType.Should().Be("Order");
            auditLog.EntityId.Should().Be(12345);
            auditLog.Action.Should().Be("UPDATE");
        }
        else
        {
            // This should not happen for valid JSON
            Assert.Fail("TryFromJson should have succeeded for valid JSON");
        }
    }
}