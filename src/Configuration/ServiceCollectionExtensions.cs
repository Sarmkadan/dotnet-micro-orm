#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Configuration;

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Services;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ORM services and repositories
    /// </summary>
    public static IServiceCollection AddDotnetMicroOrm(
        this IServiceCollection services,
        string connectionString,
        DatabaseProvider provider = DatabaseProvider.SqlServer,
        Action<OrmConfiguration>? configureOptions = null)
    {
        var config = new OrmConfiguration();
        configureOptions?.Invoke(config);

        // Register database context
        services.AddSingleton<IDatabaseContext>(sp =>
            new DatabaseContext(connectionString, provider));

        // Register unit of work
        services.AddScoped<IUnitOfWork>(sp =>
            new UnitOfWork(sp.GetRequiredService<IDatabaseContext>()));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register services
        services.AddScoped<UserService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<AuditService>();

        return services;
    }

    /// <summary>
    /// Registers a specific repository implementation
    /// </summary>
    public static IServiceCollection AddRepository<TEntity, TRepository>(
        this IServiceCollection services)
        where TEntity : Domain.Models.BaseEntity
        where TRepository : class, IRepository<TEntity>
    {
        services.AddScoped<IRepository<TEntity>, TRepository>();
        return services;
    }

    /// <summary>
    /// Registers a service factory
    /// </summary>
    public static IServiceCollection AddOrmService<TService>(
        this IServiceCollection services)
        where TService : class
    {
        services.AddScoped<TService>();
        return services;
    }
}

/// <summary>
/// ORM configuration options
/// </summary>
public class sealed OrmConfiguration
{
    /// <summary>
    /// Default command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = Constants.OrmConstants.DefaultCommandTimeout;

    /// <summary>
    /// Default batch size for bulk operations
    /// </summary>
    public int DefaultBatchSize { get; set; } = Constants.OrmConstants.DefaultBatchSize;

    /// <summary>
    /// Enable change tracking
    /// </summary>
    public bool EnableChangeTracking { get; set; } = true;

    /// <summary>
    /// Enable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Enable compiled expression caching
    /// </summary>
    public bool EnableExpressionCaching { get; set; } = true;

    /// <summary>
    /// Maximum cached expressions
    /// </summary>
    public int MaxCachedExpressions { get; set; } = 1000;

    /// <summary>
    /// Connection retry attempts
    /// </summary>
    public int ConnectionRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Connection retry delay in milliseconds
    /// </summary>
    public int ConnectionRetryDelayMs { get; set; } = 1000;
}
