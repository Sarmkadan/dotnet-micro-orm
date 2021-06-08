using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetMicroORM.Controllers;

public static class HealthControllerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(this HealthController value, bool indented = false)
    {
        if (value == null)
        {
            return "{}";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    public static HealthController? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<HealthController>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out HealthController? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<HealthController>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}