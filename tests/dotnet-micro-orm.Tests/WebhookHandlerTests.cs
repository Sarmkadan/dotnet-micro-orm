using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetMicroOrm.Integration;
using Xunit;

namespace DotnetMicroOrm.Tests;

public class WebhookHandlerTests
{
    private const string Secret = "super-secret-key";

    [Fact]
    public void Constructor_Throws_WhenSecretIsNullOrWhiteSpace()
    {
        Assert.Throws<ArgumentException>(() => new WebhookHandler(null!));
        Assert.Throws<ArgumentException>(() => new WebhookHandler(string.Empty));
        Assert.Throws<ArgumentException>(() => new WebhookHandler("   "));
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalidSignatureResult_WhenSignatureIsWrong()
    {
        var handler = new WebhookHandler(Secret);
        var payload = new WebhookPayload { EventType = WebhookEvents.UserCreated };
        var result = await handler.ProcessAsync(payload, "invalid-signature");

        Assert.False(result.Success);
        Assert.Equal("Invalid signature", result.Error);
    }

    [Fact]
    public async Task ProcessAsync_ExecutesRegisteredHandler_AndReturnsSuccess()
    {
        var handler = new WebhookHandler(Secret);
        var payload = new WebhookPayload { EventType = WebhookEvents.OrderCreated };
        var called = false;

        handler.Subscribe(WebhookEvents.OrderCreated, p =>
        {
            called = true;
            return Task.CompletedTask;
        });

        var signature = handler.GenerateSignature(payload);
        var result = await handler.ProcessAsync(payload, signature);

        Assert.True(result.Success);
        Assert.Contains("Processed", result.Message);
        Assert.True(called);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccessMessage_WhenNoHandlersRegistered()
    {
        var handler = new WebhookHandler(Secret);
        var payload = new WebhookPayload { EventType = WebhookEvents.ProductDeleted };
        var signature = handler.GenerateSignature(payload);

        var result = await handler.ProcessAsync(payload, signature);

        Assert.True(result.Success);
        Assert.Equal("No handlers registered for this event type", result.Message);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenHandlerThrows()
    {
        var handler = new WebhookHandler(Secret);
        var payload = new WebhookPayload { EventType = WebhookEvents.UserUpdated };

        handler.Subscribe(WebhookEvents.UserUpdated, _ => throw new InvalidOperationException("boom"));

        var signature = handler.GenerateSignature(payload);
        var result = await handler.ProcessAsync(payload, signature);

        Assert.False(result.Success);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void GenerateSignature_ProducesSignatureThatVerifySignatureAccepts()
    {
        var handler = new WebhookHandler(Secret);
        var payload = new WebhookPayload
        {
            EventType = WebhookEvents.ProductCreated,
            Data = new Dictionary<string, object> { ["id"] = 123, ["name"] = "Test" }
        };

        var signature = handler.GenerateSignature(payload);
        // Use reflection to call the private VerifySignature method via the public ProcessAsync path
        var resultTask = handler.ProcessAsync(payload, signature);
        var result = resultTask.GetAwaiter().GetResult();

        Assert.True(result.Success);
    }

    [Fact]
    public void WebhookPayload_DefaultValues_AreSetCorrectly()
    {
        var payload = new WebhookPayload();

        Assert.False(string.IsNullOrWhiteSpace(payload.Id));
        Assert.Equal(string.Empty, payload.EventType);
        Assert.NotNull(payload.Data);
        Assert.Empty(payload.Data);
        Assert.Equal("1.0", payload.Version);
        Assert.True((DateTime.UtcNow - payload.Timestamp).TotalSeconds < 5);
    }
}
