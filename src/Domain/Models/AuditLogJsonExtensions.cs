#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for AuditLog type
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
    /// Serializes an AuditLog instance to a JSON string
    /// </summary>
    /// <param name="value">The AuditLog instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the AuditLog</returns>
    public static string ToJson(this AuditLog value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented ? new JsonSerializerOptions(_options)
        {
            WriteIndented = true
        } : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes an AuditLog instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized AuditLog instance, or null if JSON is null/empty</returns>
    public static AuditLog? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuditLog>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize an AuditLog instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized AuditLog if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
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