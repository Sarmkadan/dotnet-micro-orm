#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data;

using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for QueryBuilder{T}
/// </summary>
public static class QueryBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Converts the QueryBuilder{T} to a JSON string
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The QueryBuilder instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the QueryBuilder</returns>
    public static string ToJson<T>(this QueryBuilder<T> value, bool indented = false) where T : BaseEntity
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a QueryBuilder{T} instance
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A QueryBuilder{T} instance, or null if the JSON represents a null value</returns>
    public static QueryBuilder<T>? FromJson<T>(string json) where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON string cannot be null or whitespace", nameof(json));

        try
        {
            return JsonSerializer.Deserialize<QueryBuilder<T>>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize QueryBuilder from JSON", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a QueryBuilder{T} instance
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The resulting QueryBuilder{T} instance, or null if deserialization failed</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    public static bool TryFromJson<T>(string json, out QueryBuilder<T>? value) where T : BaseEntity
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            value = JsonSerializer.Deserialize<QueryBuilder<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}