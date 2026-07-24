#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Extension methods for configuring event dispatcher options.
/// </summary>
public static class EventDispatcherOptionsExtensions
{
    /// <summary>
    /// Enables automatic discovery of event handlers from assemblies.
    /// </summary>
    /// <param name="options">The options to configure</param>
    /// <param name="enable">Whether to enable auto-discovery</param>
    /// <returns>The configured options</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static IEventDispatcherOptions EnableAutoDiscovery(this IEventDispatcherOptions options, bool enable = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.EnableAutoDiscovery = enable;
        return options;
    }

    /// <summary>
    /// Adds an assembly name to scan for event handlers during auto-discovery.
    /// </summary>
    /// <param name="options">The options to configure</param>
    /// <param name="assemblyName">The assembly name to scan</param>
    /// <returns>The configured options</returns>
    /// <exception cref="ArgumentNullException">Thrown when options or assemblyName is null or whitespace</exception>
    public static IEventDispatcherOptions ScanAssembly(this IEventDispatcherOptions options, string assemblyName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        options.ScanAssemblies.Add(assemblyName);
        return options;
    }

    /// <summary>
    /// Configures event dispatcher in the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The configured service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null</exception>
    public static IServiceCollection AddEventDispatcher(
        this IServiceCollection services,
        Action<IEventDispatcherOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register dispatcher as singleton
        services.AddSingleton<IEventDispatcher, EventDispatcher>();

        // Register options with default configuration
        services.AddSingleton<IEventDispatcherOptions>(provider =>
        {
            var options = new EventDispatcherOptions();
            configureOptions?.Invoke(options);
            return options;
        });

        // Register auto-discovery service if enabled
        services.AddSingleton<EventHandlerAutoDiscovery>();

        return services;
    }
}
