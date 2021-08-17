using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Formatters;

/// <summary>Provides System.Text.Json serialization helpers for <see cref="MarkdownFormatter"/>.</summary>
public static class MarkdownFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>Serializes the <see cref="MarkdownFormatter"/> to a JSON string.</summary>
    /// <param name="value">The <see cref="MarkdownFormatter"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="MarkdownFormatter"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this MarkdownFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            { WriteIndented = true, }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>Deserializes a JSON string to a <see cref="MarkdownFormatter"/> instance.</summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="MarkdownFormatter"/> instance, or null if the JSON is null or whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static MarkdownFormatter? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<MarkdownFormatter>(json, _jsonOptions);
    }

    /// <summary>Attempts to deserialize a JSON string to a <see cref="MarkdownFormatter"/> instance.</summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="MarkdownFormatter"/> instance if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; false if the JSON is null, whitespace, or invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out MarkdownFormatter? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<MarkdownFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}