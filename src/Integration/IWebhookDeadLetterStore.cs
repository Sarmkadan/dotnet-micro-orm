#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Interface for storing webhook payloads that have exhausted all retry attempts
/// </summary>
public interface IWebhookDeadLetterStore
{
    /// <summary>
    /// Stores a webhook payload in the dead-letter queue
    /// </summary>
    /// <param name="payload">Webhook payload to store</param>
    /// <param name="error">Error that caused the failure</param>
    /// <param name="attemptCount">Number of retry attempts made</param>
    /// <param name="disposition">Final disposition of the webhook</param>
    /// <returns>Task representing the operation</returns>
    Task StoreAsync(
        WebhookPayload payload,
        string error,
        int attemptCount,
        string disposition);

    /// <summary>
    /// Gets all dead-lettered webhook payloads
    /// </summary>
    /// <returns>Collection of dead-lettered webhooks</returns>
    Task<IReadOnlyList<DeadLetterEntry>> GetAllAsync();

    /// <summary>
    /// Gets dead-lettered webhook payloads by event type
    /// </summary>
    /// <param name="eventType">Event type to filter by</param>
    /// <returns>Collection of dead-lettered webhooks for the specified event type</returns>
    Task<IReadOnlyList<DeadLetterEntry>> GetByEventTypeAsync(string eventType);

    /// <summary>
    /// Removes a dead-letter entry from the store
    /// </summary>
    /// <param name="id">ID of the dead-letter entry to remove</param>
    /// <returns>Task representing the operation</returns>
    Task RemoveAsync(string id);

    /// <summary>
    /// Clears all dead-letter entries
    /// </summary>
    /// <returns>Task representing the operation</returns>
    Task ClearAsync();
}

/// <summary>
/// Represents a dead-letter entry containing information about a failed webhook delivery
/// </summary>
public sealed class DeadLetterEntry
{
    /// <summary>Unique identifier for this dead-letter entry</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Event type that triggered the webhook</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Timestamp when the webhook was originally processed</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Timestamp when the webhook was moved to dead-letter queue</summary>
    public DateTime DeadLetterTimestamp { get; set; }

    /// <summary>Webhook payload data</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Error message that caused the failure</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>Number of retry attempts made</summary>
    public int AttemptCount { get; set; }

    /// <summary>Final disposition of the webhook</summary>
    public string Disposition { get; set; } = string.Empty;
}