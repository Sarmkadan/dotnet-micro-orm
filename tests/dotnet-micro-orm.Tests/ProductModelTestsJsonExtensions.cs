#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Provides System.Text.Json serialization helpers for ProductModelTests type
/// </summary>
public static class ProductModelTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes a ProductModelTests instance to a JSON string
    /// </summary>
    /// <param name="value">The ProductModelTests instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the ProductModelTests</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ProductModelTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? new JsonSerializerOptions(_options)
        {
            WriteIndented = true
        } : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a ProductModelTests instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized ProductModelTests instance, or null if JSON is null/empty</returns>
    public static ProductModelTests? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ProductModelTests>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a ProductModelTests instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized ProductModelTests if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out ProductModelTests? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ProductModelTests>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}