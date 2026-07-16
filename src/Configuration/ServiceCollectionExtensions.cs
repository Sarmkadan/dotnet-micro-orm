#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Configuration;

using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Migrations;
using DotnetMicroOrm.Profiling;
using DotnetMicroOrm.Services;
using DotnetMicroOrm.Exceptions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all ORM services and repositories
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <param name="connectionString">Database connection string</param>
	/// <param name="provider">Database provider type</param>
	/// <param name="configureOptions">Optional configuration action</param>
	/// <exception cref="ArgumentNullException">Thrown when services or connectionString is null or whitespace</exception>
	public static IServiceCollection AddDotnetMicroOrm(
		this IServiceCollection services,
		string connectionString,
		DatabaseProvider provider = DatabaseProvider.SqlServer,
		Action<OrmConfiguration>? configureOptions = null)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

		var config = new OrmConfiguration();
		configureOptions?.Invoke(config);

		// Register configuration so consumers can resolve the effective options
		services.AddSingleton(config);

		// Register database context
		services.AddSingleton<IDatabaseContext>(_ =>
			new DatabaseContext(connectionString, provider));

		// Register unit of work
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Register repositories
		services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

		// Register batch upsert
		services.AddScoped(typeof(IBatchUpsertOperation<>), typeof(BatchUpsertOperation<>));

		// Register query profiler
		services.AddSingleton<IQueryProfiler, QueryProfiler>();

		// Register default in-memory cache provider
		services.AddSingleton<Caching.ICacheProvider, Caching.MemoryCacheProvider>();

		// Register migration runner
		services.AddScoped<IMigrationRunner, MigrationRunner>();

		// Register services
		services.AddScoped<UserService>();
		services.AddScoped<ProductService>();
		services.AddScoped<OrderService>();
		services.AddScoped<IAuditService, AuditService>();
		services.AddScoped<AuditService>();

		return services;
	}

	/// <summary>
	/// Registers a migration implementation so it is discovered by <see cref="IMigrationRunner"/>
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <typeparam name="TMigration">The migration type to register</typeparam>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	public static IServiceCollection AddMigration<TMigration>(this IServiceCollection services)
	where TMigration : class, IMigration
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddTransient<IMigration, TMigration>();
		return services;
	}

	/// <summary>
	/// Registers a specific repository implementation
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <typeparam name="TEntity">The entity type</typeparam>
	/// <typeparam name="TRepository">The repository implementation type</typeparam>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	public static IServiceCollection AddRepository<TEntity, TRepository>(
		this IServiceCollection services)
	where TEntity : Domain.Models.BaseEntity
	where TRepository : class, IRepository<TEntity>
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddScoped<IRepository<TEntity>, TRepository>();
		return services;
	}

	/// <summary>
	/// Registers a service factory
	/// </summary>
	/// <param name="services">The service collection</param>
	/// <typeparam name="TService">The service type to register</typeparam>
	/// <exception cref="ArgumentNullException">Thrown when services is null</exception>
	public static IServiceCollection AddOrmService<TService>(this IServiceCollection services)
	where TService : class
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddScoped<TService>();
		return services;
	}
}

/// <summary>
/// ORM configuration options
/// </summary>
public sealed class OrmConfiguration
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