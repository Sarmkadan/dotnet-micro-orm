#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// Factory for creating output formatters based on requested format.
/// Provides a centralized way to instantiate formatters with consistent configuration.
/// Supports registration of custom formatter implementations.
/// </summary>
public sealed class FormatterFactory
{
    private readonly Dictionary<OutputFormat, Func<IOutputFormatter>> _formatters = [];

    public FormatterFactory()
    {
        RegisterDefaultFormatters();
    }

    /// <summary>
    /// Registers the default formatters
    /// </summary>
    private void RegisterDefaultFormatters()
    {
        _formatters[OutputFormat.Json] = () => new JsonFormatter(indented: true);
        _formatters[OutputFormat.Csv] = () => new CsvFormatter(delimiter: ",", includeHeader: true);
        _formatters[OutputFormat.Xml] = () => new XmlFormatter("root", "item", indented: true);
        _formatters[OutputFormat.PlainText] = () => new PlainTextFormatter();
    }

    /// <summary>
    /// Gets a formatter for the specified output format
    /// </summary>
    public IOutputFormatter GetFormatter(OutputFormat format)
    {
        if (_formatters.TryGetValue(format, out var factory))
            return factory();

        throw new InvalidOperationException($"No formatter registered for format: {format}");
    }

    /// <summary>
    /// Gets a formatter by content type MIME string
    /// </summary>
    public IOutputFormatter GetFormatterByContentType(string contentType)
    {
        var format = contentType switch
        {
            "application/json" => OutputFormat.Json,
            "text/csv" => OutputFormat.Csv,
            "application/xml" or "text/xml" => OutputFormat.Xml,
            _ => OutputFormat.Json // Default to JSON
        };

        return GetFormatter(format);
    }

    /// <summary>
    /// Registers a custom formatter
    /// </summary>
    public void RegisterFormatter(OutputFormat format, Func<IOutputFormatter> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        _formatters[format] = factory;
    }

    /// <summary>
    /// Gets all registered formatters
    /// </summary>
    public IEnumerable<OutputFormat> GetRegisteredFormats()
        => _formatters.Keys;

    /// <summary>
    /// Creates a formatter configured for console output
    /// </summary>
    public IOutputFormatter CreateConsoleFormatter()
        => new JsonFormatter(indented: true);

    /// <summary>
    /// Creates a formatter configured for file output
    /// </summary>
    public IOutputFormatter CreateFileFormatter(string fileExtension)
    {
        return fileExtension?.ToLowerInvariant() switch
        {
            ".csv" => GetFormatter(OutputFormat.Csv),
            ".xml" => GetFormatter(OutputFormat.Xml),
            ".json" => GetFormatter(OutputFormat.Json),
            ".txt" => GetFormatter(OutputFormat.PlainText),
            _ => GetFormatter(OutputFormat.Json)
        };
    }
}

/// <summary>
/// Plain text formatter for simple string output
/// </summary>
public sealed class PlainTextFormatter : IOutputFormatter
{
    public string ContentType => "text/plain";

    public string Format(object? data)
    {
        if (data is null)
            return "null";

        return data.ToString() ?? string.Empty;
    }

    public string FormatCollection<T>(IEnumerable<T> items)
    {
        if (items is null)
            return string.Empty;

        var lines = items.Select(item => item?.ToString() ?? "null");
        return string.Join(Environment.NewLine, lines);
    }

    public string FormatError(string code, string message, string requestId)
    {
        return $"Error [{code}]: {message} (Request ID: {requestId})";
    }
}
