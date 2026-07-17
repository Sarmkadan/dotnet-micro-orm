#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="global::DotnetMicroOrm.Domain.Models.Product"/> type.
/// </summary>
public static class ProductModelTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes a <see cref="global::DotnetMicroOrm.Domain.Models.Product"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The product instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the product.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this global::DotnetMicroOrm.Domain.Models.Product value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options);

    /// <summary>
    /// Deserializes a <see cref="global::DotnetMicroOrm.Domain.Models.Product"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must be valid JSON representing a product.</param>
    /// <returns>The deserialized product instance, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is not valid JSON or cannot be deserialized into a <see cref="global::DotnetMicroOrm.Domain.Models.Product"/>.</exception>
    public static global::DotnetMicroOrm.Domain.Models.Product? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<global::DotnetMicroOrm.Domain.Models.Product>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="global::DotnetMicroOrm.Domain.Models.Product"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must be valid JSON representing a product.</param>
    /// <param name="value">Receives the deserialized product if deserialization succeeds; otherwise, <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out global::DotnetMicroOrm.Domain.Models.Product? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<global::DotnetMicroOrm.Domain.Models.Product>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}