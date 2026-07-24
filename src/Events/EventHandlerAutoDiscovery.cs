#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Automatically discovers and registers event handlers from assemblies.
/// </summary>
public sealed class EventHandlerAutoDiscovery
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventDispatcherOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventHandlerAutoDiscovery"/> class.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider</param>
    /// <param name="options">The dispatcher configuration options</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider or options is null</exception>
    public EventHandlerAutoDiscovery(IServiceProvider serviceProvider, IEventDispatcherOptions options)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <summary>
    /// Discovers and registers all event handlers from the specified assemblies.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher to register handlers with</param>
    /// <exception cref="ArgumentNullException">Thrown when dispatcher is null</exception>
    public void DiscoverAndRegisterHandlers(IEventDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        if (!_options.EnableAutoDiscovery)
        {
            return;
        }

        var assembliesToScan = GetAssembliesToScan();

        foreach (var assembly in assembliesToScan)
        {
            DiscoverHandlersInAssembly(dispatcher, assembly);
        }
    }

    /// <summary>
    /// Gets the list of assemblies to scan for event handlers.
    /// </summary>
    private IEnumerable<Assembly> GetAssembliesToScan()
    {
        var assemblies = new List<Assembly>();

        if (_options.ScanAssemblies.Count == 0)
        {
            // Scan all loaded assemblies
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
        }
        else
        {
            // Scan specific assemblies by name
            foreach (var assemblyName in _options.ScanAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is BadImageFormatException || ex is ArgumentException)
                {
                    // Skip assemblies that can't be loaded
                    Console.WriteLine($"Warning: Could not load assembly '{assemblyName}' for event handler discovery: {ex.Message}");
                }
            }
        }

        return assemblies;
    }

    /// <summary>
    /// Discovers event handlers in a specific assembly and registers them with the dispatcher.
    /// </summary>
    /// <param name="dispatcher">The event dispatcher</param>
    /// <param name="assembly">The assembly to scan</param>
    private void DiscoverHandlersInAssembly(IEventDispatcher dispatcher, Assembly assembly)
    {
        try
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .Select(t => new
                {
                    Type = t,
                    Interfaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                        .ToList()
                })
                .Where(t => t.Interfaces.Count > 0)
                .ToList();

            foreach (var item in handlerTypes)
            {
                foreach (var handlerInterface in item.Interfaces)
                {
                    var eventType = handlerInterface.GetGenericArguments()[0];
                    var registerMethod = typeof(IEventDispatcher).GetMethod(
                        "RegisterHandler",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        Type.EmptyTypes,
                        null);

                    if (registerMethod != null)
                    {
                        var genericMethod = registerMethod.MakeGenericMethod(eventType, item.Type);
                        genericMethod.Invoke(dispatcher, null);
                    }
                }
            }
        }
        catch (Exception ex) when (ex is ReflectionTypeLoadException || ex is TargetInvocationException)
        {
            // Skip assemblies with reflection errors
            Console.WriteLine($"Warning: Could not scan assembly '{assembly.FullName}' for event handlers: {ex.Message}");
        }
    }
}
