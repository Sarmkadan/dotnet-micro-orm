#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// JSON formatter using System.Text.Json for efficient serialization.
/// Provides configurable indentation and property naming conventions.
/// </summary>
public class sealed JsonFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _options;

    public string ContentType => "application/json";

    public JsonFormatter(bool indented = true)
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = new[] { new JsonStringEnumConverter() }
        };
    }

    public string Format(object? data)
    {
        if (data is null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(data, _options);
        }
        catch (JsonException ex)
        {
            return FormatError("JSON_SERIALIZATION_ERROR", ex.Message, Guid.NewGuid().ToString());
        }
    }

    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null)
            return "[]";

        try
        {
            var list = items.ToList();
            return JsonSerializer.Serialize(list, _options);
        }
        catch (JsonException ex)
        {
            return FormatError("JSON_SERIALIZATION_ERROR", ex.Message, Guid.NewGuid().ToString());
        }
    }

    public string FormatError(string code, string message, string requestId)
    {
        var error = new
        {
            code,
            message,
            requestId,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(error, _options);
    }
}
