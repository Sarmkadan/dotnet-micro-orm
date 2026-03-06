# Migration Guide: v1.x to v2.0

This document covers all breaking changes and required steps to upgrade from DotnetMicroOrm 1.x to 2.0.

## Overview

Version 2.0 introduces query interceptors, distributed caching via Redis, database sharding, and a migration toolkit. The repository and Unit of Work interfaces have been updated to support these features.

## Breaking Changes

### 1. IRepository<T> interface changes

The `IRepository<T>` interface now requires async-only methods. Synchronous overloads have been removed.

**Before (v1.x):**

```csharp
public interface IRepository<T>
{
    T GetById(int id);
    Task<T> GetByIdAsync(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

**After (v2.0):**

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    IAsyncEnumerable<T> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

All methods now accept `CancellationToken`. Return types are nullable where applicable.

### 2. DbContext configuration

`MicroOrmDbContext` constructor no longer accepts a raw connection string. Use `MicroOrmOptions` instead.

**Before (v1.x):**

```csharp
var context = new MicroOrmDbContext("Server=localhost;Database=MyDb;...");
```

**After (v2.0):**

```csharp
var options = new MicroOrmOptions
{
    ConnectionString = "Server=localhost;Database=MyDb;...",
    DatabaseType = DatabaseType.SqlServer,
    EnableCaching = true,
    CacheTTL = TimeSpan.FromMinutes(5)
};
var context = new MicroOrmDbContext(options);
```

### 3. Specification pattern namespace

Specifications moved from `DotnetMicroOrm.Specifications` to `DotnetMicroOrm.Query.Specifications`.

```csharp
// Before
using DotnetMicroOrm.Specifications;

// After
using DotnetMicroOrm.Query.Specifications;
```

### 4. Batch operations API

`BatchInsert`, `BatchUpdate`, `BatchDelete` replaced with a unified `BatchExecuteAsync` method.

**Before (v1.x):**

```csharp
await repo.BatchInsert(entities);
await repo.BatchUpdate(entities);
await repo.BatchDelete(entities);
```

**After (v2.0):**

```csharp
await repo.BatchExecuteAsync(BatchOperation.Insert, entities);
await repo.BatchExecuteAsync(BatchOperation.Update, entities);
await repo.BatchExecuteAsync(BatchOperation.Delete, entities);
```

### 5. Change tracking

Change tracking is now opt-in per query instead of a global setting.

```csharp
// v1.x - global
options.EnableChangeTracking = true;

// v2.0 - per query
var entity = await repo.GetByIdAsync(1, tracking: true);

// or via context
context.TrackingBehavior = TrackingBehavior.TrackAll; // global fallback
```

### 6. Caching - Redis support

In-memory caching remains the default. To use Redis distributed caching:

```csharp
var options = new MicroOrmOptions
{
    CacheProvider = new RedisCacheProvider("localhost:6379"),
    CacheTTL = TimeSpan.FromMinutes(10)
};
```

The `ICacheProvider` interface is new in v2.0 and replaces the internal caching implementation.

## New Features (non-breaking)

### Query Interceptors

```csharp
context.AddInterceptor(new SlowQueryLogInterceptor(threshold: TimeSpan.FromSeconds(1)));
context.AddInterceptor(new AuditInterceptor());
```

### Database Sharding

```csharp
var options = new MicroOrmOptions
{
    ShardingStrategy = new HashShardingStrategy(shardCount: 4),
    ShardConnectionStrings = new Dictionary<int, string>
    {
        [0] = "Server=shard0;...",
        [1] = "Server=shard1;...",
        [2] = "Server=shard2;...",
        [3] = "Server=shard3;..."
    }
};
```

### Migration Toolkit

```bash
dotnet micro-orm migrations add InitialCreate
dotnet micro-orm migrations apply
dotnet micro-orm migrations rollback --steps 1
```

## Step-by-step Migration

1. Update the NuGet package:
   ```bash
   dotnet add package Zaiets.dotnet.micro.orm --version 2.0.0
   ```

2. Replace synchronous repository calls with async equivalents.

3. Update `MicroOrmDbContext` instantiation to use `MicroOrmOptions`.

4. Update `using` statements for specifications namespace.

5. Replace `BatchInsert`/`BatchUpdate`/`BatchDelete` with `BatchExecuteAsync`.

6. Review change tracking usage - switch from global to per-query if needed.

7. Run tests to verify behavior.

## Docker Support

v2.0 ships with a production-ready Dockerfile and docker-compose.yml. The application runs on port 8080 by default.

```bash
# Build and run
docker compose up -d

# With PostgreSQL instead of SQL Server
docker compose --profile postgres up -d

# Health check
curl http://localhost:8080/health
```

## Compatibility

| Feature | v1.x | v2.0 |
|---------|------|------|
| .NET | 10 | 10 |
| SQL Server | Yes | Yes |
| PostgreSQL | Yes | Yes |
| MySQL | Yes | Yes |
| SQLite | Yes | Yes |
| Sync API | Yes | Removed |
| Redis Cache | No | Yes |
| Sharding | No | Yes |
| Query Interceptors | No | Yes |
| Migrations | No | Yes |

## Support

For issues during migration, open a GitHub issue with the `migration` label.
