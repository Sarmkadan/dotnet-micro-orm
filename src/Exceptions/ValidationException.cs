#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Thrown when validation of an entity or input fails
/// </summary>
public sealed class ValidationException : DotnetMicroOrmException
{
    public List<string> ValidationErrors { get; } = [];

    public ValidationException(string message, List<string>? errors = null, Exception? innerException = null)
        : base(message, "VALIDATION_ERROR", innerException)
    {
        if (errors is not null)
            ValidationErrors.AddRange(errors);
    }
}
