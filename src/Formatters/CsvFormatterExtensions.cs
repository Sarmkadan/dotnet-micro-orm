#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections;
using System.Dynamic;
using System.Globalization;
using DotnetMicroOrm.Utils;

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// Extension methods for <see cref="CsvFormatter"/> providing convenient formatting operations
/// for common scenarios like formatting dictionaries, dynamic objects, and custom delimiters.
/// </summary>
public static class CsvFormatterExtensions
{
    /// <summary>
    /// Formats a dictionary as CSV with keys as column headers and values as rows.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="dictionary">Dictionary to format</param>
    /// <returns>CSV formatted string</returns>
    public static string FormatDictionary(this CsvFormatter formatter, IDictionary dictionary)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (dictionary is null || dictionary.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        var keys = new List<string>();
        var values = new List<object?>();

        foreach (var key in dictionary.Keys)
        {
            keys.Add(EscapeKey(key?.ToString()));
            values.Add(dictionary[key]);
        }

        // Create a simple anonymous object for each row
        var row = new { Key = string.Join(",", keys), Value = string.Join(",", values.Select(v => v?.ToString() ?? string.Empty)) };

        return formatter.Format(row);
    }

    /// <summary>
    /// Formats a collection of dynamic objects as CSV.
    /// Each dynamic object is treated as a row with properties as columns.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">Collection of dynamic objects</param>
    /// <returns>CSV formatted string</returns>
    public static string FormatDynamicCollection(this CsvFormatter formatter, IEnumerable<dynamic> items)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (items is null)
            return string.Empty;

        var itemList = items.ToList();
        if (itemList.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        var allPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Collect all unique property names from all dynamic objects
        foreach (var item in itemList)
        {
            if (item is not null)
            {
                var type = item.GetType();
                var properties = ReflectionHelper.GetProperties(type);
                foreach (var prop in properties)
                {
                    allPropertyNames.Add(prop.Name);
                }
            }
        }

        var sortedProperties = allPropertyNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        // Create a list of anonymous objects with consistent properties
        var rows = new List<object>();
        foreach (var item in itemList)
        {
            var anonObject = new ExpandoObject() as IDictionary<string, object?>;
            foreach (var propName in sortedProperties)
            {
                try
                {
                    var value = ReflectionHelper.GetPropertyValue(item, propName);
                    anonObject[propName] = value;
                }
                catch
                {
                    anonObject[propName] = null;
                }
            }
            rows.Add(anonObject);
        }

        return formatter.FormatCollection(rows);
    }

    /// <summary>
    /// Formats a single object with custom property selection.
    /// Allows specifying which properties to include in the output.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="data">Object to format</param>
    /// <param name="propertyNames">Names of properties to include</param>
    /// <returns>CSV formatted string with only specified properties</returns>
    public static string FormatWithProperties(this CsvFormatter formatter, object? data, params string[] propertyNames)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (data is null || propertyNames is null || propertyNames.Length == 0)
            return string.Empty;

        var type = data.GetType();
        var properties = propertyNames
            .Select(name => ReflectionHelper.GetProperty(type, name))
            .Where(p => p is not null)
            .ToArray();

        if (properties.Length == 0)
            return string.Empty;

        // Create an anonymous object with only the specified properties
        var anonObject = new { };
        var expando = new ExpandoObject() as IDictionary<string, object?>;

        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(data);
                expando[prop.Name] = value;
            }
            catch
            {
                expando[prop.Name] = null;
            }
        }

        return formatter.Format(expando);
    }

    /// <summary>
    /// Formats a collection with custom delimiter for this operation only.
    /// Creates a new formatter instance with the specified delimiter.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">Collection to format</param>
    /// <param name="customDelimiter">Custom delimiter to use for this formatting</param>
    /// <returns>CSV formatted string with custom delimiter</returns>
    public static string FormatWithDelimiter<T>(this CsvFormatter formatter, IEnumerable<T> items, string customDelimiter)
    {
        if (formatter is null)
            throw new ArgumentNullException(nameof(formatter));

        if (items is null || string.IsNullOrEmpty(customDelimiter))
            return string.Empty;

        // Create a new formatter with the custom delimiter
        var customFormatter = new CsvFormatter(
            delimiter: customDelimiter
        );

        return customFormatter.FormatCollection(items);
    }

    /// <summary>
    /// Formats a collection with tab delimiter (TSV format).
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">Collection to format</param>
    /// <returns>Tab-separated values formatted string</returns>
    public static string FormatAsTsv<T>(this CsvFormatter formatter, IEnumerable<T> items)
    {
        return formatter.FormatWithDelimiter(items, "\t");
    }

    /// <summary>
    /// Formats a collection with pipe delimiter.
    /// </summary>
    /// <param name="formatter">The CSV formatter instance</param>
    /// <param name="items">Collection to format</param>
    /// <returns>Pipe-delimited formatted string</returns>
    public static string FormatAsPsv<T>(this CsvFormatter formatter, IEnumerable<T> items)
    {
        return formatter.FormatWithDelimiter(items, "|");
    }

    private static string EscapeKey(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        // Keys typically don't need complex escaping
        if (key.Contains(",") || key.Contains("\n") || key.Contains("\r"))
        {
            return $"\"{key}\"";
        }

        return key;
    }
}