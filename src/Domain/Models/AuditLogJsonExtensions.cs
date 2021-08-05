#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="AuditLog"/> type
/// </summary>
public static class AuditLogJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes an <see cref="AuditLog"/> instance to a JSON string
    /// </summary>
    /// <param name="value">The <see cref="AuditLog"/> instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <returns>A JSON string representation of the <see cref="AuditLog"/></returns>
    public static string ToJson(this AuditLog value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an <see cref="AuditLog"/> instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized <see cref="AuditLog"/> instance, or <see langword="null"/> if JSON is null/empty</returns>
    public static AuditLog? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<AuditLog>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize an <see cref="AuditLog"/> instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized <see cref="AuditLog"/> if successful</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/></returns>
    public static bool TryFromJson(string json, out AuditLog? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<AuditLog>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}