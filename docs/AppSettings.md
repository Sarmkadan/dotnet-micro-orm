# AppSettings

Configuration class for `dotnet-micro-orm` settings, providing centralized access to database, ORM, and logging configurations. Designed to simplify application configuration management while maintaining flexibility for advanced scenarios.

## API

### `public DatabaseSettings Database`
Gets or sets the database-specific settings. Contains properties like `DatabaseName`, `DefaultSchema`, and behavioral flags such as `ChangeTrackingEnabled` and `LazyLoadingEnabled`. Never returns `null`.

### `public OrmSettings Orm`
Gets or sets the ORM-specific settings. Includes flags like `ExpressionCachingEnabled`, `QueryOptimizationEnabled`, and batch size controls such as `DefaultBatchSize` and `MaxBatchSize`. Never returns `null`.

### `public LoggingSettings Logging`
Gets or sets the logging configuration. Used to control log levels, output formats, and destinations. Never returns `null`.

### `public string ConnectionString`
Gets or sets the database connection string. Must be a valid connection string for the configured `DatabaseProvider`. Changing this value does not automatically reconnect existing sessions.

### `public DatabaseProvider Provider`
Gets or sets the database provider type (e.g., `SqlServer`, `PostgreSql`, `MySql`). Must be set before establishing connections. Changing this after connections are created may result in undefined behavior.

### `public int ConnectionTimeout`
Gets or sets the connection timeout in seconds. Default is typically 30. Must be a non-negative integer. Values of 0 may disable timeout depending on provider behavior.

### `public int CommandTimeout`
Gets or sets the command execution timeout in seconds. Default is typically 30. Must be a non-negative integer. Values of 0 may disable timeout depending on provider behavior.

### `public bool EnablePooling`
Gets or sets whether connection pooling is enabled. Default is `true`. Disabling pooling may impact performance and resource usage. Changes take effect on next connection creation.

### `public int MinPoolSize`
Gets or sets the minimum number of connections in the pool. Must be a non-negative integer. Ignored if `EnablePooling` is `false`. Increasing this value may improve performance under high load but increases memory usage.

### `public int MaxPoolSize`
Gets or sets the maximum number of connections in the pool. Must be a positive integer greater than or equal to `MinPoolSize`. Ignored if `EnablePooling` is `false`. Exceeding this limit may result in connection wait or failures.

### `public string DatabaseName`
Gets or sets the target database name. Used for multi-tenant scenarios or explicit database targeting. May be ignored by some providers if not applicable.

### `public string DefaultSchema`
Gets or sets the default schema name. Used to qualify unqualified table or view names. Changing this may require application restart for schema resolution to take effect.

### `public bool ChangeTrackingEnabled`
Gets or sets whether automatic change tracking is enabled. When `true`, tracks entity changes for update operations. Disabling may improve performance but requires manual change detection.

### `public bool AuditLoggingEnabled`
Gets or sets whether audit logging is enabled. When `true`, logs all data modifications (inserts, updates, deletes) with timestamps and user context. May impact performance under high write loads.

### `public bool LazyLoadingEnabled`
Gets or sets whether lazy loading of navigation properties is enabled. When `true`, related entities are loaded on first access. Disabling may reduce memory usage and improve performance but requires explicit eager loading.

### `public bool ExpressionCachingEnabled`
Gets or sets whether LINQ expression caching is enabled. When `true`, caches compiled query expressions to improve repeated query performance. Disabling may increase CPU usage under heavy query loads.

### `public int DefaultBatchSize`
Gets or sets the default batch size for bulk operations. Must be a positive integer. Larger values may improve performance for bulk inserts/updates but increase memory usage.

### `public int MaxBatchSize`
Gets or sets the maximum batch size for bulk operations. Must be a positive integer greater than or equal to `DefaultBatchSize`. Limits the size of individual batch operations to prevent timeouts or memory issues.

### `public bool QueryOptimizationEnabled`
Gets or sets whether query optimization features (e.g., query plan caching, parameter sniffing) are enabled. Disabling may reduce performance but provides more predictable behavior in some environments.

### `public bool ProxyCreationEnabled`
Gets or sets whether dynamic proxy creation is enabled for lazy loading and change tracking. Disabling may improve performance but prevents lazy loading and automatic change detection.

## Usage

### Example 1: Basic Configuration
