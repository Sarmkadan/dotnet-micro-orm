#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Integration;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Tests for WebhookSignatureValidator with timestamp-based anti-replay protection
/// </summary>
public class WebhookSignatureValidatorTests
{
    private const string TestSecret = "test-secret-key-12345";
    private const string TestSecret2 = "different-secret-key";

    [Fact]
    public void ValidateSignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var signatureHeader = validator.GenerateSignatureHeader(payload);

        // Act
        var isValid = validator.ValidateSignature(payload, signatureHeader);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateSignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var wrongValidator = new WebhookSignatureValidator(TestSecret2);
        var signatureHeader = wrongValidator.GenerateSignatureHeader(payload);

        // Act
        var isValid = validator.ValidateSignature(payload, signatureHeader);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignature_WithEmptySignature_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var isValid = validator.ValidateSignature(payload, null);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignature_WithMalformedSignature_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var isValid = validator.ValidateSignature(payload, "invalid-format");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignature_WithOldTimestamp_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret, TimeSpan.FromMinutes(5));
        var oldPayload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow.AddMinutes(-10), // Older than 5 minute tolerance
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var signatureHeader = validator.GenerateSignatureHeader(oldPayload);

        // Act
        var isValid = validator.ValidateSignature(oldPayload, signatureHeader);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignature_WithFutureTimestamp_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret, TimeSpan.FromMinutes(5));
        var futurePayload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow.AddMinutes(10), // Future timestamp
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var signatureHeader = validator.GenerateSignatureHeader(futurePayload);

        // Act
        var isValid = validator.ValidateSignature(futurePayload, signatureHeader);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GenerateSignatureHeader_ProducesValidFormat()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var signatureHeader = validator.GenerateSignatureHeader(payload);

        // Assert
        Assert.NotNull(signatureHeader);
        Assert.StartsWith("t=", signatureHeader);
        Assert.Contains(",v1=", signatureHeader);
    }

    [Fact]
    public void Constructor_WithNullSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WebhookSignatureValidator(null!));
    }

    [Fact]
    public void Constructor_WithEmptySecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WebhookSignatureValidator(""));
    }

    [Fact]
    public void Constructor_WithWhitespaceSecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WebhookSignatureValidator("   "));
    }

    [Fact]
    public void ValidateSignature_WithTamperedPayload_ReturnsFalse()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var originalPayload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var signatureHeader = validator.GenerateSignatureHeader(originalPayload);

        // Tamper with the payload
        var tamperedPayload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.updated", // Changed event type
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        // Act
        var isValid = validator.ValidateSignature(tamperedPayload, signatureHeader);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateSignature_WithCustomTimestampTolerance_WorksCorrectly()
    {
        // Arrange - Use 1 second tolerance for testing
        var validator = new WebhookSignatureValidator(TestSecret, TimeSpan.FromSeconds(1));
        var payload = new WebhookPayload
        {
            Id = "test-id-123",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 42 } }
        };

        var signatureHeader = validator.GenerateSignatureHeader(payload);

        // Act - Should be valid immediately
        var isValid1 = validator.ValidateSignature(payload, signatureHeader);
        Assert.True(isValid1);

        // Act - Should be invalid after waiting
        await Task.Delay(TimeSpan.FromSeconds(2));
        var isValid2 = validator.ValidateSignature(payload, signatureHeader);
        Assert.False(isValid2);
    }

    [Fact]
    public void ValidateSignature_WithDifferentPayloads_ProducesDifferentSignatures()
    {
        // Arrange
        var validator = new WebhookSignatureValidator(TestSecret);
        var payload1 = new WebhookPayload
        {
            Id = "test-id-1",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 1 } }
        };

        var payload2 = new WebhookPayload
        {
            Id = "test-id-2",
            EventType = "user.created",
            Timestamp = DateTime.UtcNow,
            Data = new Dictionary<string, object> { { "userId", 2 } }
        };

        var signatureHeader1 = validator.GenerateSignatureHeader(payload1);
        var signatureHeader2 = validator.GenerateSignatureHeader(payload2);

        // Act
        var isValid1 = validator.ValidateSignature(payload1, signatureHeader1);
        var isValid2 = validator.ValidateSignature(payload2, signatureHeader2);

        // Assert
        Assert.True(isValid1);
        Assert.True(isValid2);
        Assert.NotEqual(signatureHeader1, signatureHeader2);
    }
}