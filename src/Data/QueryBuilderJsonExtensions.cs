#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetMicroOrm.Data;

using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for QueryBuilder{T}
/// </summary>
public static class QueryBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson<T>(this QueryBuilder<T> value, bool indented = false) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a QueryBuilder{T} instance
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A QueryBuilder{T} instance, or null if the JSON represents a null value</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace</exception>
    /// <exception cref="JsonException">Thrown when deserialization fails</exception>
    public static QueryBuilder<T>? FromJson<T>(string json) where T : BaseEntity
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

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
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace</exception>
    public static bool TryFromJson<T>(string json, out QueryBuilder<T>? value) where T : BaseEntity
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = null;

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