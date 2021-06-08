#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides System.Text.Json serialization extensions for UserRepository
/// </summary>
public static class UserRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the UserRepository instance to a JSON string
    /// </summary>
    /// <param name="value">The repository instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the repository</returns>
    public static string ToJson(this UserRepository value, bool indented = false)
    {
        if (value is null)
        {
            return "null";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a UserRepository instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized UserRepository instance or null if JSON is null/empty</returns>
    public static UserRepository? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<UserRepository>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a UserRepository instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized instance</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out UserRepository? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<UserRepository>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}