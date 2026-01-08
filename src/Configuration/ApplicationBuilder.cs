// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Caching;
using DotnetMicroOrm.Integration;
using DotnetMicroOrm.Middleware;
using DotnetMicroOrm.Pipeline;
using DotnetMicroOrm.Data;

namespace DotnetMicroOrm.Configuration;

/// <summary>
/// Fluent builder for configuring and initializing the ORM application.
/// Sets up database connections, middleware pipeline, caching, integration services,
/// and other components in a composable manner.
/// </summary>
public class ApplicationBuilder
{
    private IDatabaseContext? _dbContext;
    private readonly PipelineBuilder _pipelineBuilder = new();
    private ICacheProvider? _cacheProvider;
    private IHttpClient? _httpClient;
    private readonly Dictionary<string, object> _services = [];

    /// <summary>
    /// Configures the database context
    /// </summary>
    public ApplicationBuilder WithDatabase(IDatabaseContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _services["DbContext"] = _dbContext;
        return this;
    }

    /// <summary>
    /// Configures the database from connection string
    /// </summary>
    public ApplicationBuilder WithDatabaseConnection(string connectionString, DatabaseProvider provider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        _dbContext = new DatabaseContext(connectionString, provider);
        _services["DbContext"] = _dbContext;
        return this;
    }

    /// <summary>
    /// Adds middleware to the processing pipeline
    /// </summary>
    public ApplicationBuilder AddMiddleware(IMiddleware middleware)
    {
        _pipelineBuilder.Use(middleware);
        return this;
    }

    /// <summary>
    /// Configures in-memory caching
    /// </summary>
    public ApplicationBuilder WithMemoryCache()
    {
        _cacheProvider = new MemoryCacheProvider();
        _services["CacheProvider"] = _cacheProvider;
        return this;
    }

    /// <summary>
    /// Configures a custom cache provider
    /// </summary>
    public ApplicationBuilder WithCacheProvider(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _services["CacheProvider"] = _cacheProvider;
        return this;
    }

    /// <summary>
    /// Configures HTTP client for external integrations
    /// </summary>
    public ApplicationBuilder WithHttpClient(HttpClientConfig? config = null)
    {
        _httpClient = new DefaultHttpClient(config);
        _services["HttpClient"] = _httpClient;
        return this;
    }

    /// <summary>
    /// Registers a service instance
    /// </summary>
    public ApplicationBuilder RegisterService<T>(T instance) where T : class
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        _services[typeof(T).Name] = instance;
        return this;
    }

    /// <summary>
    /// Adds all default middleware (logging, error handling, auth, rate limiting)
    /// </summary>
    public ApplicationBuilder WithDefaultMiddleware()
    {
        _pipelineBuilder.Use(new ErrorHandlingMiddleware());
        _pipelineBuilder.Use(new LoggingMiddleware(new ConsoleLogger<LoggingMiddleware>()));
        _pipelineBuilder.Use(new AuthenticationMiddleware());
        _pipelineBuilder.Use(new RateLimitingMiddleware(new RateLimitConfig { MaxRequests = 100, WindowDuration = TimeSpan.FromMinutes(1) }));
        return this;
    }

    /// <summary>
    /// Builds the complete application configuration
    /// </summary>
    public ApplicationConfiguration Build()
    {
        if (_dbContext is null)
            throw new InvalidOperationException("Database context must be configured before building");

        return new ApplicationConfiguration
        {
            DatabaseContext = _dbContext,
            Pipeline = _pipelineBuilder.Build(),
            CacheProvider = _cacheProvider ?? new MemoryCacheProvider(),
            HttpClient = _httpClient ?? new DefaultHttpClient(),
            Services = new Dictionary<string, object>(_services)
        };
    }

    /// <summary>
    /// Gets a configured service
    /// </summary>
    public T? GetService<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T).Name, out var service))
            return service as T;

        return null;
    }
}

/// <summary>
/// Complete application configuration
/// </summary>
public class ApplicationConfiguration
{
    /// <summary>Database context</summary>
    public required IDatabaseContext DatabaseContext { get; set; }

    /// <summary>Middleware pipeline delegate</summary>
    public required Func<MiddlewareContext, Task> Pipeline { get; set; }

    /// <summary>Cache provider</summary>
    public required ICacheProvider CacheProvider { get; set; }

    /// <summary>HTTP client for external APIs</summary>
    public required IHttpClient HttpClient { get; set; }

    /// <summary>Registered services</summary>
    public required Dictionary<string, object> Services { get; set; }

    /// <summary>
    /// Gets a registered service
    /// </summary>
    public T? GetService<T>() where T : class
    {
        if (Services.TryGetValue(typeof(T).Name, out var service))
            return service as T;

        return null;
    }
}

/// <summary>
/// Dummy logger implementation for compilation
/// </summary>
public class ConsoleLogger<T> : ILogger<T>
{
    public void LogInformation(string message, params object?[] args) => Console.WriteLine(message, args);
    public void LogDebug(string message, params object?[] args) { } // Suppress debug
    public void LogError(string message, params object?[] args) => Console.WriteLine(message, args);
}
