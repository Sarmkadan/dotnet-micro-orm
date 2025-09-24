// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Formatters;

/// <summary>
/// Interface for formatting output data in various formats (JSON, CSV, XML, etc).
/// Allows pluggable serialization strategies for different media types.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// The MIME type this formatter produces
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Formats an object to a string representation
    /// </summary>
    string Format(object? data);

    /// <summary>
    /// Formats a collection of objects
    /// </summary>
    string FormatCollection<T>(IEnumerable<T> items);

    /// <summary>
    /// Formats an error response
    /// </summary>
    string FormatError(string code, string message, string requestId);
}

/// <summary>
/// Supported output formats
/// </summary>
public enum OutputFormat
{
    Json,
    Csv,
    Xml,
    PlainText
}
