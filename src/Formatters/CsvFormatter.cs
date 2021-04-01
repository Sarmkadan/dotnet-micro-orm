// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using DotnetMicroOrm.Utils;

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// CSV formatter for outputting data in comma-separated values format.
/// Properly handles quoted fields, line breaks, and special characters.
/// Includes header row with property names.
/// </summary>
public class CsvFormatter : IOutputFormatter
{
    private readonly string _delimiter;
    private readonly string _lineEnding;
    private readonly bool _includeHeader;

    public string ContentType => "text/csv";

    public CsvFormatter(string delimiter = ",", bool includeHeader = true)
    {
        _delimiter = delimiter;
        _lineEnding = Environment.NewLine;
        _includeHeader = includeHeader;
    }

    public string Format(object? data)
    {
        if (data is null)
            return string.Empty;

        if (data is IEnumerable<object> collection)
            return FormatCollection(collection);

        // Single object - format as single row
        var sb = new StringBuilder();
        var type = data.GetType();
        var properties = ReflectionHelper.GetProperties(type);

        if (_includeHeader)
        {
            sb.Append(string.Join(_delimiter, properties.Select(p => EscapeCsvField(p.Name))));
            sb.Append(_lineEnding);
        }

        var values = properties.Select(p => EscapeCsvField(GetPropertyValueAsString(data, p)));
        sb.Append(string.Join(_delimiter, values));

        return sb.ToString();
    }

    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null)
            return string.Empty;

        var itemList = items.ToList();
        if (itemList.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var type = typeof(T);
        var properties = ReflectionHelper.GetProperties(type);

        // Write header
        if (_includeHeader)
        {
            sb.Append(string.Join(_delimiter, properties.Select(p => EscapeCsvField(p.Name))));
            sb.Append(_lineEnding);
        }

        // Write rows
        foreach (var item in itemList)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvField(value?.ToString() ?? string.Empty);
            });

            sb.Append(string.Join(_delimiter, values));
            sb.Append(_lineEnding);
        }

        return sb.ToString();
    }

    public string FormatError(string code, string message, string requestId)
    {
        var sb = new StringBuilder();
        sb.Append($"Error Code,Message,Request ID,Timestamp{_lineEnding}");
        sb.Append($"{EscapeCsvField(code)},{EscapeCsvField(message)},{EscapeCsvField(requestId)},{EscapeCsvField(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))}");

        return sb.ToString();
    }

    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private static string GetPropertyValueAsString(object obj, System.Reflection.PropertyInfo property)
    {
        try
        {
            var value = property.GetValue(obj);

            if (value is null)
                return string.Empty;

            if (value is DateTime dt)
                return dt.ToString("O");

            if (value is bool b)
                return b ? "true" : "false";

            return value.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
