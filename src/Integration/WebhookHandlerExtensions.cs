#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookHandler"/> that provide common webhook processing utilities.
/// </summary>
public static class WebhookHandlerExtensions
{
    /// <summary>
    /// Safely processes a webhook payload with automatic retry logic for transient failures.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload to process.</param>
    /// <param name="signature">The HMAC signature for verification.</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="retryDelayMs">Delay between retries in milliseconds (default: 100).</param>
    /// <returns>A webhook result indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="payload"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="signature"/> is empty.</exception>
    public static async Task<WebhookResult> ProcessWithRetryAsync(
        this WebhookHandler handler,
        WebhookPayload payload,
        string signature,
        int maxRetries = 3,
        int retryDelayMs = 100)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(signature);

        WebhookResult? lastResult = null;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            lastResult = await handler.ProcessAsync(payload, signature).ConfigureAwait(false);

            if (lastResult.Success)
            {
                return lastResult;
            }

            retryCount++;

            if (retryCount < maxRetries && lastResult.Error is not null)
            {
                // Wait before retrying
                await Task.Delay(retryDelayMs).ConfigureAwait(false);
            }
        }

        // Final attempt and return the result
        lastResult = await handler.ProcessAsync(payload, signature).ConfigureAwait(false);
        return lastResult.Success
            ? lastResult
            : new WebhookResult
            {
                Success = false,
                Error = "All retry attempts failed",
                ProcessedAt = DateTime.UtcNow
            };
    }

    /// <summary>
    /// Processes a webhook payload and automatically generates the expected signature header.
    /// Useful for testing scenarios where you need to simulate verified webhooks.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload to process.</param>
    /// <returns>A webhook result indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="payload"/> is null.</exception>
    public static async Task<WebhookResult> ProcessAsync(
        this WebhookHandler handler,
        WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);

        var signatureHeader = handler.GenerateSignatureHeader(payload);
        return await handler.ProcessAsync(payload, signatureHeader).ConfigureAwait(false);
    }

    /// <summary>
    /// Filters handlers by event type and executes them in parallel.
    /// Returns a dictionary mapping event types to their processing results.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payloads">Collection of webhook payloads to process.</param>
    /// <returns>Dictionary mapping event types to their processing results.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="payloads"/> is null.</exception>
    public static async Task<IReadOnlyDictionary<string, WebhookResult>> ProcessBatchAsync(
        this WebhookHandler handler,
        IEnumerable<WebhookPayload> payloads)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payloads);

        var results = new Dictionary<string, WebhookResult>();
        var groupedPayloads = payloads
            .Where(p => p is not null)
            .GroupBy(p => p.EventType)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());

        foreach (var group in groupedPayloads)
        {
            var payloadList = group.Value.ToList();
            var tasks = payloadList
                .Select(p => handler.ProcessAsync(p, handler.GenerateSignatureHeader(p)))
                .ToArray();

            var batchResults = await Task.WhenAll(tasks).ConfigureAwait(false);

            results[group.Key] = new WebhookResult
            {
                Success = batchResults.All(r => r.Success),
                Message = $"Processed {batchResults.Length} payload(s) for event type '{group.Key}'",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Creates a new webhook payload with the specified event type and data.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type (e.g., "user.created").</param>
    /// <param name="data">The event data dictionary.</param>
    /// <returns>A new webhook payload instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="eventType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> is empty.</exception>
    public static WebhookPayload CreatePayload(
        this WebhookHandler handler,
        string eventType,
        Dictionary<string, object>? data = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        return new WebhookPayload
        {
            Id = Guid.NewGuid().ToString("N"),
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Data = data ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
            Version = "1.0"
        };
    }

    /// <summary>
    /// Safely extracts strongly-typed data from a webhook payload.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the data to.</typeparam>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload.</param>
    /// <returns>The deserialized data or default if conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="payload"/> is null.</exception>
    public static T? GetData<T>(
        this WebhookHandler handler,
        WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);

        if (payload.Data is null || payload.Data.Count == 0)
        {
            return default;
        }

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload.Data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not ThreadAbortException)
        {
            return default;
        }
    }

    /// <summary>
    /// Checks if a webhook payload matches a specific event type.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload to check.</param>
    /// <param name="expectedEventType">The expected event type to match.</param>
    /// <returns>True if the payload matches the expected event type; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/>, <paramref name="payload"/>, or <paramref name="expectedEventType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="expectedEventType"/> is empty.</exception>
    public static bool IsEventType(
        this WebhookHandler handler,
        WebhookPayload payload,
        string expectedEventType)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(expectedEventType);

        return string.Equals(
            payload.EventType,
            expectedEventType,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the age of a webhook payload in milliseconds.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload.</param>
    /// <returns>The age in milliseconds, or 0 if timestamp is not set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> or <paramref name="payload"/> is null.</exception>
    public static long GetAgeInMilliseconds(
        this WebhookHandler handler,
        WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);

        var age = DateTime.UtcNow - payload.Timestamp;
        return (long)age.TotalMilliseconds;
    }

    /// <summary>
    /// Validates that a webhook payload contains required data fields.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="payload">The webhook payload to validate.</param>
    /// <param name="requiredKeys">Collection of required key names.</param>
    /// <returns>True if all required keys are present; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/>, <paramref name="payload"/>, or <paramref name="requiredKeys"/> is null.</exception>
    public static bool HasRequiredData(
        this WebhookHandler handler,
        WebhookPayload payload,
        IEnumerable<string> requiredKeys)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(requiredKeys);

        if (payload.Data is null || payload.Data.Count == 0)
        {
            return false;
        }

        return requiredKeys.All(key =>
            payload.Data.ContainsKey(key) &&
            payload.Data[key] is not null);
    }
}