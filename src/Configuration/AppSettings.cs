// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Configuration;

/// <summary>
/// Application settings configuration
/// </summary>
public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public OrmSettings Orm { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

/// <summary>
/// Database configuration settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database provider type
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Enable connection pooling
    /// </summary>
    public bool EnablePooling { get; set; } = true;

    /// <summary>
    /// Minimum pool size
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Default schema
    /// </summary>
    public string DefaultSchema { get; set; } = "dbo";
}

/// <summary>
/// ORM-specific configuration settings
/// </summary>
public class OrmSettings
{
    /// <summary>
    /// Enable change tracking
    /// </summary>
    public bool ChangeTrackingEnabled { get; set; } = true;

    /// <summary>
    /// Enable audit logging
    /// </summary>
    public bool AuditLoggingEnabled { get; set; } = true;

    /// <summary>
    /// Lazy loading enabled
    /// </summary>
    public bool LazyLoadingEnabled { get; set; } = true;

    /// <summary>
    /// Compiled expression caching enabled
    /// </summary>
    public bool ExpressionCachingEnabled { get; set; } = true;

    /// <summary>
    /// Default batch size for bulk operations
    /// </summary>
    public int DefaultBatchSize { get; set; } = 1000;

    /// <summary>
    /// Maximum batch size allowed
    /// </summary>
    public int MaxBatchSize { get; set; } = 10000;

    /// <summary>
    /// Enable query optimization
    /// </summary>
    public bool QueryOptimizationEnabled { get; set; } = true;

    /// <summary>
    /// Proxy creation enabled
    /// </summary>
    public bool ProxyCreationEnabled { get; set; } = true;

    /// <summary>
    /// Validate on save
    /// </summary>
    public bool ValidateOnSave { get; set; } = true;
}

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Minimum log level
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// Enable query logging
    /// </summary>
    public bool LogQueries { get; set; } = true;

    /// <summary>
    /// Log SQL queries
    /// </summary>
    public bool LogSqlQueries { get; set; } = false;

    /// <summary>
    /// Log parameter values
    /// </summary>
    public bool LogParameterValues { get; set; } = false;

    /// <summary>
    /// Log execution time
    /// </summary>
    public bool LogExecutionTime { get; set; } = true;

    /// <summary>
    /// Slow query threshold in milliseconds
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Log file path
    /// </summary>
    public string LogFilePath { get; set; } = "logs/application.log";

    /// <summary>
    /// Maximum log file size in MB
    /// </summary>
    public int MaxLogFileSizeMb { get; set; } = 10;

    /// <summary>
    /// Number of log files to retain
    /// </summary>
    public int RetainedLogFileCount { get; set; } = 5;
}
