#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// System.Text.Json serialization extensions for MigrationRunnerTests.
/// </summary>
public static class MigrationRunnerTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a MigrationRunnerTests instance to a JSON string.
    /// </summary>
    /// <param name="value">The MigrationRunnerTests instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the MigrationRunnerTests instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this MigrationRunnerTests value, bool indented = false)
    {
        return value is null
            ? throw new ArgumentNullException(nameof(value))
            : JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a MigrationRunnerTests instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized MigrationRunnerTests instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static MigrationRunnerTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json) || json == "null"
            ? null
            : JsonSerializer.Deserialize<MigrationRunnerTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a MigrationRunnerTests instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized MigrationRunnerTests instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out MigrationRunnerTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<MigrationRunnerTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}