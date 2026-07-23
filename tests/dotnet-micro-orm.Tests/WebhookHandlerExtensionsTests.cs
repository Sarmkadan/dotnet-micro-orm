#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetMicroOrm.Integration;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class WebhookHandlerExtensionsTests : IDisposable
{
    private const string Secret = "super-secret-key";
    private readonly WebhookHandler _handler;

    public WebhookHandlerExtensionsTests()
    {
        _handler = new WebhookHandler(Secret);
    }

    public void Dispose()
    {
        // No resources to dispose for WebhookHandler
    }

    // ========== ProcessWithRetryAsync tests ==========

    [Fact]
    public async Task ProcessWithRetryAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated);
        var signature = _handler.GenerateSignature(payload);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            WebhookHandlerExtensions.ProcessWithRetryAsync(handler!, payload, signature));
    }

    [Fact]
    public async Task ProcessWithRetryAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;
        var signature = _handler.GenerateSignature(payload);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.ProcessWithRetryAsync(payload, signature));
    }

    [Fact]
    public async Task ProcessWithRetryAsync_WithNullOrEmptySignature_ThrowsArgumentException()
    {
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.ProcessWithRetryAsync(payload, null!));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.ProcessWithRetryAsync(payload, string.Empty));
    }

    [Fact]
    public async Task ProcessWithRetryAsync_OnSuccess_ReturnsSuccessfulResult()
    {
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated, new Dictionary<string, object> { ["id"] = 123 });
        var signature = _handler.GenerateSignature(payload);

        var result = await _handler.ProcessWithRetryAsync(payload, signature, maxRetries: 3, retryDelayMs: 10);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProcessWithRetryAsync_OnTransientFailure_RetriesAndSucceeds()
    {
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated);
        var signature = _handler.GenerateSignature(payload);
        int callCount = 0;

        // Subscribe a handler that fails twice then succeeds
        _handler.Subscribe(WebhookEvents.UserCreated, _ =>
        {
            callCount++;
            if (callCount < 3)
                return Task.FromException(new InvalidOperationException("Transient error"));
            return Task.CompletedTask;
        });

        var result = await _handler.ProcessWithRetryAsync(payload, signature, maxRetries: 5, retryDelayMs: 10);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        callCount.Should().Be(3); // Failed twice, succeeded on third attempt
    }

    [Fact]
    public async Task ProcessWithRetryAsync_OnPermanentFailure_ReturnsFailureAfterMaxRetries()
    {
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated);
        var signature = _handler.GenerateSignature(payload);
        int callCount = 0;

        // Subscribe a handler that always fails
        _handler.Subscribe(WebhookEvents.UserCreated, _ =>
        {
            callCount++;
            return Task.FromException(new InvalidOperationException("Permanent error"));
        });

        var result = await _handler.ProcessWithRetryAsync(payload, signature, maxRetries: 3, retryDelayMs: 10);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Be("All retry attempts failed");
        callCount.Should().Be(4); // Initial attempt + 3 retries
    }

    // ========== ProcessAsync tests ==========

    [Fact]
    public async Task ProcessAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload(WebhookEvents.UserCreated);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            WebhookHandlerExtensions.ProcessAsync(handler!, payload));
    }

    [Fact]
    public async Task ProcessAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.ProcessAsync(payload));
    }

    [Fact]
    public async Task ProcessAsync_OnSuccess_ReturnsSuccessfulResult()
    {
        var payload = _handler.CreatePayload(WebhookEvents.OrderCreated, new Dictionary<string, object> { ["orderId"] = 456 });
        var result = await _handler.ProcessAsync(payload);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProcessAsync_OnHandlerException_ReturnsErrorResult()
    {
        var payload = _handler.CreatePayload(WebhookEvents.ProductUpdated);
        _handler.Subscribe(WebhookEvents.ProductUpdated, _ => throw new InvalidOperationException("Processing failed"));

        var result = await _handler.ProcessAsync(payload);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Processing failed");
    }

    // ========== ProcessBatchAsync tests ==========

    [Fact]
    public async Task ProcessBatchAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payloads = new List<WebhookPayload> { _handler.CreatePayload(WebhookEvents.UserCreated) };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            WebhookHandlerExtensions.ProcessBatchAsync(handler!, payloads));
    }

    [Fact]
    public async Task ProcessBatchAsync_WithNullPayloads_ThrowsArgumentNullException()
    {
        IEnumerable<WebhookPayload>? payloads = null;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.ProcessBatchAsync(payloads));
    }

    [Fact]
    public async Task ProcessBatchAsync_WithEmptyPayloads_ReturnsEmptyDictionary()
    {
        var payloads = new List<WebhookPayload>();

        var result = await _handler.ProcessBatchAsync(payloads);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessBatchAsync_WithNullPayloadsFilteredOut_ProcessesOnlyValidOnes()
    {
        var payloads = new List<WebhookPayload> {
            _handler.CreatePayload(WebhookEvents.UserCreated),
            _handler.CreatePayload(WebhookEvents.UserCreated),
            _handler.CreatePayload(WebhookEvents.OrderCreated)
        };

        var result = await _handler.ProcessBatchAsync(payloads);

        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only user.created and order.created groups
        result.Should().ContainKey(WebhookEvents.UserCreated);
        result.Should().ContainKey(WebhookEvents.OrderCreated);

        // Each group should have success=true since no handlers registered (default success)
        result[WebhookEvents.UserCreated]!.Success.Should().BeTrue();
        result[WebhookEvents.OrderCreated]!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessBatchAsync_WithMixedSuccessFailure_ReturnsAppropriateResults()
    {
        var userPayload1 = _handler.CreatePayload(WebhookEvents.UserCreated, new Dictionary<string, object> { ["id"] = 1 });
        var userPayload2 = _handler.CreatePayload(WebhookEvents.UserCreated, new Dictionary<string, object> { ["id"] = 2 });
        var orderPayload = _handler.CreatePayload(WebhookEvents.OrderCreated, new Dictionary<string, object> { ["orderId"] = 100 });

        var payloads = new List<WebhookPayload> { userPayload1, userPayload2, orderPayload };

        // Subscribe handlers: user.created succeeds, order.created fails
        _handler.Subscribe(WebhookEvents.UserCreated, _ => Task.CompletedTask);
        _handler.Subscribe(WebhookEvents.OrderCreated, _ => throw new InvalidOperationException("Order processing failed"));

        var result = await _handler.ProcessBatchAsync(payloads);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        // User created group should succeed (both payloads processed successfully)
        result[WebhookEvents.UserCreated]!.Success.Should().BeTrue();
        result[WebhookEvents.UserCreated]!.Message.Should().Contain("Processed 2 payload(s)");

        // Order created group should fail (one payload failed)
        result[WebhookEvents.OrderCreated]!.Success.Should().BeFalse();
        result[WebhookEvents.OrderCreated]!.Message.Should().Contain("Processed 1 payload(s)");
    }

    // ========== CreatePayload tests ==========

    [Fact]
    public void CreatePayload_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;

        Assert.Throws<ArgumentNullException>(() =>
            WebhookHandlerExtensions.CreatePayload(handler!, "test.event", null));
    }

    [Fact]
    public void CreatePayload_WithNullOrEmptyEventType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _handler.CreatePayload(null!, null));

        Assert.Throws<ArgumentException>(() =>
            _handler.CreatePayload(string.Empty, null));
    }

    [Fact]
    public void CreatePayload_WithValidParameters_ReturnsPopulatedPayload()
    {
        var eventType = "test.event";
        var data = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 };

        var payload = _handler.CreatePayload(eventType, data);

        payload.Should().NotBeNull();
        payload.EventType.Should().Be(eventType);
        payload.Data.Should().NotBeNull();
        payload.Data.Should().HaveCount(2);
        payload.Data.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
        payload.Data.Should().ContainKey("key2").WhoseValue.Should().Be(42);
        payload.Version.Should().Be("1.0");
        payload.Id.Should().NotBeNullOrEmpty();
        payload.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreatePayload_WithNullData_CreatesEmptyDictionary()
    {
        var payload = _handler.CreatePayload("test.event", null!);

        payload.Should().NotBeNull();
        payload.Data.Should().NotBeNull();
        payload.Data.Should().BeEmpty();
    }

    // ========== GetData<T> tests ==========

    [Fact]
    public void GetData_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload("test.event");

        Assert.Throws<ArgumentNullException>(() =>
            WebhookHandlerExtensions.GetData<TestData>(handler!, payload));
    }

    [Fact]
    public void GetData_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;

        Assert.Throws<ArgumentNullException>(() =>
            _handler.GetData<TestData>(payload));
    }

    [Fact]
    public void GetData_WithEmptyPayloadData_ReturnsDefault()
    {
        var payload = _handler.CreatePayload("test.event");

        var result = _handler.GetData<TestData>(payload);

        result.Should().BeNull(); // Reference type default
    }

    [Fact]
    public void GetData_WithValidData_ReturnsDeserializedObject()
    {
        var testData = new TestData { Id = 123, Name = "Test Item", Value = 45.67 };
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["Id"] = testData.Id,
            ["Name"] = testData.Name,
            ["Value"] = testData.Value
        });

        var result = _handler.GetData<TestData>(payload);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public void GetData_WithInvalidJson_ReturnsDefault()
    {
        // Create payload with data that can't be deserialized to TestData
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["Id"] = "not-a-number",
            ["Name"] = 123, // Should be string
            ["Value"] = "not-a-float"
        });

        var result = _handler.GetData<TestData>(payload);

        result.Should().BeNull(); // Should return default due to deserialization failure
    }

    [Fact]
    public void GetData_ValueType_ReturnsDefaultOnFailure()
    {
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["Value"] = "not-an-int"
        });

        var result = _handler.GetData<int>(payload);

        result.Should().Be(0); // Default value for int
    }

    // ========== IsEventType tests ==========

    [Fact]
    public void IsEventType_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload("test.event");

        Assert.Throws<ArgumentNullException>(() =>
            WebhookHandlerExtensions.IsEventType(handler!, payload, "test.event"));
    }

    [Fact]
    public void IsEventType_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;

        Assert.Throws<ArgumentNullException>(() =>
            _handler.IsEventType(payload, "test.event"));
    }

    [Fact]
    public void IsEventType_WithNullOrEmptyExpectedEventType_ThrowsArgumentException()
    {
        var payload = _handler.CreatePayload("test.event");

        Assert.Throws<ArgumentNullException>(() =>
            _handler.IsEventType(payload, null!));

        Assert.Throws<ArgumentException>(() =>
            _handler.IsEventType(payload, string.Empty));
    }

    [Fact]
    public void IsEventType_WithMatchingEventType_ReturnsTrue()
    {
        var payload = _handler.CreatePayload("user.created");

        var result = _handler.IsEventType(payload, "user.created");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsEventType_WithMatchingEventType_IgnoreCase_ReturnsTrue()
    {
        var payload = _handler.CreatePayload("USER.CREATED");

        var result = _handler.IsEventType(payload, "user.created");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsEventType_WithNonMatchingEventType_ReturnsFalse()
    {
        var payload = _handler.CreatePayload("user.created");

        var result = _handler.IsEventType(payload, "order.created");

        result.Should().BeFalse();
    }

    // ========== GetAgeInMilliseconds tests ==========

    [Fact]
    public void GetAgeInMilliseconds_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload("test.event");

        Assert.Throws<ArgumentNullException>(() =>
            WebhookHandlerExtensions.GetAgeInMilliseconds(handler!, payload));
    }

    [Fact]
    public void GetAgeInMilliseconds_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;

        Assert.Throws<ArgumentNullException>(() =>
            _handler.GetAgeInMilliseconds(payload));
    }

    [Fact]
    public void GetAgeInMilliseconds_WithRecentPayload_ReturnsSmallPositiveValue()
    {
        var payload = _handler.CreatePayload("test.event");
        // Timestamp is set to UtcNow in constructor, so age should be very small

        var ageMs = _handler.GetAgeInMilliseconds(payload);

        ageMs.Should().BeGreaterThanOrEqualTo(0);
        ageMs.Should().BeLessThan(1000); // Should be less than 1 second
    }

    [Fact]
    public void GetAgeInMilliseconds_WithOldPayload_ReturnsLargePositiveValue()
    {
        var payload = _handler.CreatePayload("test.event");
        payload.Timestamp = DateTime.UtcNow.AddMinutes(-5); // 5 minutes ago

        var ageMs = _handler.GetAgeInMilliseconds(payload);

        ageMs.Should().BeGreaterThan(4 * 60 * 1000L); // At least 4 minutes
        ageMs.Should().BeLessThan(6 * 60 * 1000L);    // At most 6 minutes
    }

    // ========== HasRequiredData tests ==========

    [Fact]
    public void HasRequiredData_WithNullHandler_ThrowsArgumentNullException()
    {
        WebhookHandler? handler = null;
        var payload = _handler.CreatePayload("test.event");
        var requiredKeys = new List<string> { "key1" };

        Assert.Throws<ArgumentNullException>(() =>
            WebhookHandlerExtensions.HasRequiredData(handler!, payload, requiredKeys));
    }

    [Fact]
    public void HasRequiredData_WithNullPayload_ThrowsArgumentNullException()
    {
        var payload = (WebhookPayload)null!;
        var requiredKeys = new List<string> { "key1" };

        Assert.Throws<ArgumentNullException>(() =>
            _handler.HasRequiredData(payload, requiredKeys));
    }

    [Fact]
    public void HasRequiredData_WithNullRequiredKeys_ThrowsArgumentNullException()
    {
        var payload = _handler.CreatePayload("test.event");
        IEnumerable<string>? requiredKeys = null;

        Assert.Throws<ArgumentNullException>(() =>
            _handler.HasRequiredData(payload, requiredKeys));
    }

    [Fact]
    public void HasRequiredData_WithEmptyPayload_ReturnsFalse()
    {
        var payload = _handler.CreatePayload("test.event");
        var requiredKeys = new List<string> { "key1" };

        var result = _handler.HasRequiredData(payload, requiredKeys);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasRequiredData_WithAllRequiredKeysPresent_ReturnsTrue()
    {
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["id"] = 123,
            ["name"] = "Test",
            ["active"] = true
        });
        var requiredKeys = new List<string> { "id", "name" };

        var result = _handler.HasRequiredData(payload, requiredKeys);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasRequiredData_WithSomeMissingRequiredKeys_ReturnsFalse()
    {
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["id"] = 123,
            ["name"] = "Test"
            // Missing "email"
        });
        var requiredKeys = new List<string> { "id", "name", "email" };

        var result = _handler.HasRequiredData(payload, requiredKeys);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasRequiredData_WithNullValuesInData_ReturnsFalseForThoseKeys()
    {
        var payload = _handler.CreatePayload("test.event", new Dictionary<string, object>
        {
            ["id"] = 123,
            ["name"] = null! // Explicitly null value
        });
        var requiredKeys = new List<string> { "id", "name" };

        var result = _handler.HasRequiredData(payload, requiredKeys);

        result.Should().BeFalse(); // Should be false because name is null
    }

    // Helper class for GetData tests
    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
    }
}