#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Base exception for all DotnetMicroOrm related errors
/// </summary>
public class DotnetMicroOrmException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? ErrorContext { get; set; }

    public DotnetMicroOrmException(string message, string? errorCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public DotnetMicroOrmException WithContext(string key, object value)
    {
        ErrorContext ??= new Dictionary<string, object>();
        ErrorContext[key] = value;
        return this;
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (ErrorCode is not null)
            baseString += $"\nError Code: {ErrorCode}";
        if (ErrorContext?.Count > 0)
            baseString += $"\nContext: {string.Join(", ", ErrorContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        return baseString;
    }
}
