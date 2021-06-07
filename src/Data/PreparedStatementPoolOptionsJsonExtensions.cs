#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="PreparedStatementPoolOptions"/>.
/// </summary>
public static class PreparedStatementPoolOptionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="PreparedStatementPoolOptions"/> to a JSON string.
    /// </summary>
    /// <param name="value">The options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the options.</returns>
    public static string ToJson(this PreparedStatementPoolOptions value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PreparedStatementPoolOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized options, or null if the JSON is null or empty.</returns>
    public static PreparedStatementPoolOptions? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PreparedStatementPoolOptions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PreparedStatementPoolOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized options if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out PreparedStatementPoolOptions? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<PreparedStatementPoolOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}