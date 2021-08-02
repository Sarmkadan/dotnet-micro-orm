using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotnetMicroOrm.Benchmarks;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="ComparisonBenchmarks"/>.
/// </summary>
public static class ComparisonBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes <see cref="ComparisonBenchmarks"/> to JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ComparisonBenchmarks"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ComparisonBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes <see cref="ComparisonBenchmarks"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized <see cref="ComparisonBenchmarks"/> instance or <see langword="null"/> if JSON is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static ComparisonBenchmarks? FromJson(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? throw new ArgumentNullException(nameof(json))
            : JsonSerializer.Deserialize<ComparisonBenchmarks>(json, _jsonOptions);

    /// <summary>
    /// Attempts to deserialize <see cref="ComparisonBenchmarks"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for deserialized value.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out ComparisonBenchmarks? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ComparisonBenchmarks>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}