#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Handles incoming webhooks with signature verification and event processing.
/// Supports multiple webhook types (user events, order events, etc).
/// Verifies authenticity using HMAC-SHA256 signatures.
/// </summary>
public class sealed WebhookHandler
{
    private readonly Dictionary<string, List<Func<WebhookPayload, Task>>> _handlers = [];
    private readonly string _secret;

    public WebhookHandler(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret cannot be empty", nameof(secret));

        _secret = secret;
    }

    /// <summary>
    /// Registers a handler for a specific webhook event type
    /// </summary>
    public void Subscribe(string eventType, Func<WebhookPayload, Task> handler)
    {
        if (string.IsNullOrEmpty(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _handlers[eventType] = handlers;
        }

        handlers.Add(handler);
    }

    /// <summary>
    /// Processes an incoming webhook request with signature verification
    /// </summary>
    public async Task<WebhookResult> ProcessAsync(WebhookPayload payload, string signature)
    {
        if (payload is null)
            throw new ArgumentNullException(nameof(payload));

        // Verify signature
        if (!VerifySignature(payload, signature))
        {
            return new WebhookResult
            {
                Success = false,
                Error = "Invalid signature",
                ProcessedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Find handlers for this event type
            if (!_handlers.TryGetValue(payload.EventType, out var handlers) || handlers.Count == 0)
            {
                return new WebhookResult
                {
                    Success = true,
                    Message = "No handlers registered for this event type",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // Execute all handlers
            var tasks = handlers.Select(h => h(payload));
            await Task.WhenAll(tasks);

            return new WebhookResult
            {
                Success = true,
                Message = $"Processed {handlers.Count} handler(s)",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new WebhookResult
            {
                Success = false,
                Error = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Verifies webhook signature using HMAC-SHA256
    /// </summary>
    private bool VerifySignature(WebhookPayload payload, string signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        try
        {
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            var secretBytes = Encoding.UTF8.GetBytes(_secret);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
                var expectedSignature = Convert.ToHexString(hash).ToLowerInvariant();

                return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a signature for a webhook payload (for testing)
    /// </summary>
    public string GenerateSignature(WebhookPayload payload)
    {
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var secretBytes = Encoding.UTF8.GetBytes(_secret);

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}

/// <summary>
/// Represents a webhook payload
/// </summary>
public class sealed WebhookPayload
{
    /// <summary>Unique identifier for this webhook</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Type of event that triggered this webhook</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>When the event occurred</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>The actual event data</summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>Version of the webhook format</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Result of processing a webhook
/// </summary>
public class sealed WebhookResult
{
    /// <summary>Whether processing was successful</summary>
    public bool Success { get; set; }

    /// <summary>Success message</summary>
    public string? Message { get; set; }

    /// <summary>Error message if processing failed</summary>
    public string? Error { get; set; }

    /// <summary>When the webhook was processed</summary>
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Webhook event types
/// </summary>
public static class WebhookEvents
{
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string UserDeleted = "user.deleted";
    public const string OrderCreated = "order.created";
    public const string OrderShipped = "order.shipped";
    public const string OrderCancelled = "order.cancelled";
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string ProductDeleted = "product.deleted";
}
