#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Exceptions;

/// <summary>
/// Thrown when a configuration error occurs (e.g., missing connection string)
/// </summary>
public sealed class ConfigurationException : DotnetMicroOrmException
{
    public ConfigurationException(string message, Exception? innerException = null)
        : base(message, "CONFIGURATION_ERROR", innerException) { }
}
