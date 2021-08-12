#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for ProductRepository
/// </summary>
public static class ProductRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the ProductRepository instance to a JSON string
    /// </summary>
    /// <param name="value">The repository instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the repository</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this ProductRepository value, bool indented = false)
        => value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a ProductRepository instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized ProductRepository instance or null if JSON is null/empty or deserialization fails</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static ProductRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json) || json == "null"
            ? null
            : TryFromJson(json, out var result)
                ? result
                : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ProductRepository instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized instance</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out ProductRepository? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ProductRepository>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}