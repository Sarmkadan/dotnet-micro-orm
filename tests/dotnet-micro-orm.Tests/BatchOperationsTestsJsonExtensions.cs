#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="BatchOperationsTests"/> to enable JSON serialization/deserialization.
/// </summary>
public static class BatchOperationsTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers = { JsonContextModifier }
        }
    };

    /// <summary>
    /// Converts a <see cref="BatchOperationsTests"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BatchOperationsTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="BatchOperationsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="BatchOperationsTests"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static BatchOperationsTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<BatchOperationsTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BatchOperationsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out BatchOperationsTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        value = default;

        try
        {
            value = JsonSerializer.Deserialize<BatchOperationsTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void JsonContextModifier(JsonTypeInfo typeInfo)
    {
        // This modifier ensures proper handling of the class structure
        // including any nested classes like BatchOperationsUpsertTests
        if (typeInfo.Type == typeof(BatchOperationsTests) || typeInfo.Type == typeof(BatchOperationsUpsertTests))
        {
            // Ensure all properties are included in serialization
            foreach (var property in typeInfo.Properties)
            {
                property.ShouldSerialize = (_, _) => true;
            }
        }
    }
}