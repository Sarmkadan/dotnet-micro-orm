#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Configuration options for the event dispatcher.
/// </summary>
public interface IEventDispatcherOptions
{
    /// <summary>
    /// Enables automatic discovery of event handlers from assemblies.
    /// </summary>
    bool EnableAutoDiscovery { get; set; }

    /// <summary>
    /// List of assemblies to scan for event handlers when auto-discovery is enabled.
    /// If empty, all loaded assemblies will be scanned.
    /// </summary>
    List<string> ScanAssemblies { get; }
}

/// <summary>
/// Default implementation of event dispatcher configuration options.
/// </summary>
public sealed class EventDispatcherOptions : IEventDispatcherOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatcherOptions"/> class.
    /// </summary>
    public EventDispatcherOptions()
    {
        EnableAutoDiscovery = true;
    }

    /// <summary>
    /// Gets or sets whether automatic discovery of event handlers is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableAutoDiscovery { get; set; }

    /// <summary>
    /// Gets the list of assemblies to scan for event handlers.
    /// </summary>
    public List<string> ScanAssemblies { get; } = [];
}
