using System.Reflection;
using System.Text;

namespace DotnetMicroOrm.Formatters;

/// <summary>Formats data as GitHub-flavored Markdown tables.</summary>
public sealed class MarkdownFormatter : IOutputFormatter
{
    public string ContentType => "text/markdown";

    /// <summary>Single object: | Property | Value | table. Null renders as "_(null)_".</summary>
    public string Format(object? data)
    {
        if (data is null)
        {
            return "_(null)_";
        }

        var sb = new StringBuilder();
        sb.AppendLine("| Property | Value |");
        sb.AppendLine("|----------|-------|");

        var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var value = property.GetValue(data);
            sb.AppendLine($"| {EscapeCell(property.Name)} | {EscapeCell(value)} |");
        }

        return sb.ToString().Trim();
    }

    /// <summary>Collection: header from typeof(T) public properties, one row per item. Empty collection renders header only.</summary>
    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null || !items.Any())
        {
            return FormatHeader<T>();
        }

        var sb = new StringBuilder();
        sb.AppendLine(FormatHeader<T>());
        sb.AppendLine("|" + string.Join("|", Enumerable.Repeat("---", GetPropertyCount<T>())) + "|");

        foreach (var item in items)
        {
            sb.Append("|");
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(item);
                sb.Append($" {EscapeCell(value)} |");
            }
            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    /// <summary>Renders: > **Error CODE**: message (Request: requestId)</summary>
    public string FormatError(string code, string message, string requestId)
    {
        return $"> **Error {code}**: {message} (Request: {requestId})";
    }

    /// <summary>Escapes '|' and newlines in a cell value; null becomes empty string.</summary>
    private static string EscapeCell(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var str = value.ToString();
        if (str is null)
        {
            return string.Empty;
        }

        return str.Replace("|", "\\|").Replace("\n", " ").Replace("\r", " ");
    }

    private static string FormatHeader<T>()
    {
        var sb = new StringBuilder();
        sb.Append("|");
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            sb.Append($" {EscapeCell(property.Name)} |");
        }
        return sb.ToString();
    }

    private static int GetPropertyCount<T>()
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Length;
    }
}