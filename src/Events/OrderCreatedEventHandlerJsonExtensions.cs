#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="OrderCreatedEventHandler"/>.
/// </summary>
public static class OrderCreatedEventHandlerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the <see cref="OrderCreatedEventHandler"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The handler instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this OrderCreatedEventHandler value, bool indented = false) =>
        value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions);

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OrderCreatedEventHandler"/> instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized handler instance, or <see langword="null"/> if JSON is <see langword="null"/>, whitespace, or deserialization fails.</returns>
    /// <exception cref="JsonException">Thrown when JSON is valid but cannot be deserialized to <see cref="OrderCreatedEventHandler"/>.</exception>
    public static OrderCreatedEventHandler? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<OrderCreatedEventHandler>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="OrderCreatedEventHandler"/> instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for the deserialized handler.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, [NotNullWhen(true)] out OrderCreatedEventHandler? value)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<OrderCreatedEventHandler>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}