#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetMicroOrm.Cli;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for CommandHandler.
/// </summary>
public static class CommandHandlerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };


    /// <summary>
    /// Serializes a CommandHandler instance to a JSON string.
    /// </summary>
    /// <param name="value">The CommandHandler instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the CommandHandler</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this CommandHandler value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a CommandHandler instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A CommandHandler instance, or null if the JSON is null or empty</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    public static CommandHandler? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json) || json == "null"
            ? null
            : JsonSerializer.Deserialize<CommandHandler>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CommandHandler instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The resulting CommandHandler instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CommandHandler? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<CommandHandler>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}