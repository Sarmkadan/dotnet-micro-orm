#nullable enable

using System;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Extension methods for IServiceProvider to provide convenient service resolution
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Gets the requested service from the service provider
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Requested service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when service cannot be resolved</exception>
    public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        return (T)serviceProvider.GetService(typeof(T))
            ?? throw new InvalidOperationException($"Required service {typeof(T).FullName} not found in service provider");
    }

    /// <summary>
    /// Gets the requested service from the service provider or returns null
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Requested service instance or null</returns>
    public static T? GetService<T>(this IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        return (T?)serviceProvider.GetService(typeof(T));
    }
}