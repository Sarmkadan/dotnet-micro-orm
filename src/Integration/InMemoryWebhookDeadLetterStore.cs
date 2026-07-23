#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// In-memory implementation of IWebhookDeadLetterStore for development and testing
/// </summary>
public sealed class InMemoryWebhookDeadLetterStore : IWebhookDeadLetterStore
{
    private readonly ConcurrentDictionary<string, DeadLetterEntry> _store = new();

    /// <inheritdoc/>
    public Task StoreAsync(
        WebhookPayload payload,
        string error,
        int attemptCount,
        string disposition)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(error);
        ArgumentException.ThrowIfNullOrEmpty(disposition);

        var entry = new DeadLetterEntry
        {
            Id = payload.Id,
            EventType = payload.EventType,
            Timestamp = payload.Timestamp,
            DeadLetterTimestamp = DateTime.UtcNow,
            Payload = System.Text.Json.JsonSerializer.Serialize(payload),
            Error = error,
            AttemptCount = attemptCount,
            Disposition = disposition
        };

        _store[payload.Id] = entry;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DeadLetterEntry>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(_store.Values.ToList().AsReadOnly());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<DeadLetterEntry>> GetByEventTypeAsync(string eventType)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        var entries = _store.Values
            .Where(entry => string.Equals(entry.EventType, eventType, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<DeadLetterEntry>>(entries);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ClearAsync()
    {
        _store.Clear();
        return Task.CompletedTask;
    }
}