#nullable enable

using System;

namespace DotnetMicroOrm.Data;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/> to provide convenient service resolution
/// and type-safe service retrieval.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Gets the requested service from the service provider.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <param name="serviceProvider">The service provider instance.</param>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The requested service could not be resolved.</exception>
    public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return (T)serviceProvider.GetService(typeof(T))
            ?? throw new InvalidOperationException($"Required service {typeof(T).FullName} not found in service provider");
    }

    /// <summary>
    /// Gets the requested service from the service provider or returns null if not found.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <param name="serviceProvider">The service provider instance.</param>
    /// <returns>The requested service instance, or <see langword="null"/> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/></exception>
    public static T? GetService<T>(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return (T?)serviceProvider.GetService(typeof(T));
    }
}