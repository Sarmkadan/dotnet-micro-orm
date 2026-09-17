# Caching

DotnetMicroOrm exposes a pluggable caching layer through the `ICacheProvider` abstraction. It is used internally by `QueryPlanCache` and `PreparedStatementPool` to cache query plans and prepared statements, and can be used by application code for any caching need.

## Overview

All caching goes through the `ICacheProvider` interface in the `DotnetMicroOrm.Caching` namespace. The interface is `IAsyncDisposable` and supports:

- Expiration (per-entry `TimeSpan`)
- Pattern-based invalidation (`*` wildcard)
- Async operations throughout
- A single-flight `GetOrSetAsync` that prevents cache stampede (thundering herd) when an entry expires

Two built-in implementations are provided:

| Provider | Scope | Description |
|----------|-------|-------------|
| `MemoryCacheProvider` | Single server | In-memory cache backed by `ConcurrentDictionary` with LRU eviction, memory-pressure detection, and automatic expiration cleanup |
| `RedisCacheProvider` | Distributed | Shared cache backed by Redis, enabling horizontal scaling across multiple application instances |

## ICacheProvider

The interface defines the contract every provider implements:

| Method | Description |
|--------|-------------|
| `GetAsync<T>(key, ct)` | Returns the cached value or `null` if not found |
| `SetAsync<T>(key, value, expiration, ct)` | Stores a value with optional expiration |
| `GetOrSetAsync<T>(key, factory, expiration, ct)` | Returns the cached value, or creates it via `factory` if absent. Uses a single-flight lock so only one concurrent caller runs the factory for a given key |
| `RemoveAsync(key, ct)` | Removes a single key |
| `RemoveByPatternAsync(pattern, ct)` | Removes all keys matching a pattern (supports `*` wildcard) |
| `ClearAsync(ct)` | Removes all entries |
| `ExistsAsync(key, ct)` | Checks whether a key exists |
| `GetCountAsync(ct)` | Returns the number of entries, or `-1` if the count cannot be determined |

### CacheKey helper

The `CacheKey` static class builds consistent, namespaced keys:

```csharp
CacheKey.Create("user", 42, "profile");        // "user:42:profile"
CacheKey.CreatePattern("user", 42);            // "user:42:*"
CacheKey.ForUser(42, "profile");               // "user:42:profile"
CacheKey.ForProduct(7, "details");             // "product:7:details"
CacheKey.ForOrder(99, "status");               // "order:99:status"
CacheKey.ForQuery("GetOrders", 42, "active");  // "query:getorders:42_active"
CacheKey.ForConfig("featureFlags");            // "config:featureflags"
```

Note that `Create` lowercases each part, while `CreatePattern` preserves the original casing.

## MemoryCacheProvider

An in-memory implementation suitable for single-server applications. It is the default provider registered in DI.

### Features

- **LRU eviction** — when the entry count exceeds `maxEntries`, the least-recently-used entries are evicted first.
- **Memory-pressure eviction** — when enabled, the provider monitors the GC heap and aggressively evicts entries when memory load crosses 90%, stopping once it drops back below 70%.
- **Automatic expiration** — a background timer scans for expired entries every 30 seconds, and per-entry timers evict entries at their exact expiration.
- **Pattern-based removal** — wildcard patterns are converted to regex for matching.
- **Size estimation** — approximate per-entry byte size is tracked (strings by length, byte arrays by length, collections by count, everything else defaults to 1 KB).

### Constructors

```csharp
// Defaults: 10,000 max entries, 500 MB memory limit, pressure eviction enabled
var cache = new MemoryCacheProvider();

// Custom configuration
var cache = new MemoryCacheProvider(
    maxEntries: 5000,
    maxMemoryMb: 256,
    enableMemoryPressureEviction: true);
```

| Parameter | Default | Description |
|-----------|---------|-------------|
| `maxEntries` | `10000` | Maximum entries retained before LRU eviction begins |
| `maxMemoryMb` | `500` | Approximate memory limit for cache entries |
| `enableMemoryPressureEviction` | `true` | Whether to enable automatic GC memory-pressure eviction |

### Additional members

- `CleanupAsync(ct)` — runs an explicit cleanup of expired entries (useful for periodic maintenance).
- `GetTotalSizeBytes()` / `GetTotalSizeMb()` — approximate total size of all cached entries.

## RedisCacheProvider

A distributed implementation backed by Redis. It shares cache state across all application instances, enabling horizontal scaling and distributed query-plan caching for web farms.

### Features

- **JSON serialization** — values are serialized with `System.Text.Json` using camelCase naming.
- **Key prefixing** — all keys are prefixed (default `dotnet-micro-orm:`) to avoid cross-tenant collisions.
- **Resilience** — all Redis failures are swallowed so a Redis outage degrades caching but never crashes the application; failed reads return `null`, failed writes are ignored.
- **Single-flight** — `GetOrSetAsync` uses a per-key lock to prevent cache stampede.
- **Pattern-based removal** — implemented with a Redis `EVAL` script using `KEYS` + `DEL`.

### Constructors

```csharp
// From an existing connection multiplexer
var cache = new RedisCacheProvider(connectionMultiplexer, keyPrefix: "my-app");

// From a connection string
var cache = new RedisCacheProvider("localhost:6379", keyPrefix: "my-app");
```

| Parameter | Default | Description |
|-----------|---------|-------------|
| `connectionMultiplexer` | — | A `StackExchange.Redis` `IConnectionMultiplexer` |
| `connectionString` | — | Redis connection string |
| `keyPrefix` | `dotnet-micro-orm` | Prefix applied to all keys to avoid cross-tenant collisions |

### Notes

- `GetCountAsync` always returns `-1` because an accurate count would require a `SCAN` across the whole database.
- `DisposeAsync` clears the single-flight locks but does not close the underlying connection multiplexer (the caller owns it).

## Selecting a provider

### Default (DI)

`ServiceCollectionExtensions` registers `MemoryCacheProvider` as the default singleton for `ICacheProvider`:

```csharp
services.AddSingleton<Caching.ICacheProvider, Caching.MemoryCacheProvider>();
```

To use Redis instead, replace that registration with your own:

```csharp
var redis = ConnectionMultiplexer.Connect("localhost:6379");
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddSingleton<Caching.ICacheProvider, Caching.RedisCacheProvider>();
```

### ApplicationBuilder

`ApplicationBuilder` exposes `WithCacheProvider(ICacheProvider)` and a `CacheProvider` property. If none is set, it defaults to `MemoryCacheProvider`:

```csharp
var builder = new ApplicationBuilder()
    .WithCacheProvider(new RedisCacheProvider("localhost:6379"));
```

### Direct construction

`QueryPlanCache` and `PreparedStatementPool` accept an optional `ICacheProvider`; when omitted they fall back to `MemoryCacheProvider`:

```csharp
var queryPlanCache = new QueryPlanCache(options, logger, new RedisCacheProvider("localhost:6379"));
```

### Decision guide

| Scenario | Recommended provider |
|----------|----------------------|
| Single server, no shared state needed | `MemoryCacheProvider` |
| Multiple instances behind a load balancer | `RedisCacheProvider` |
| Cache must survive process restarts | `RedisCacheProvider` |
| Zero external dependencies / offline | `MemoryCacheProvider` |
| Very large cache that must respect memory limits | `MemoryCacheProvider` (LRU + pressure eviction) |
| Cross-tenant isolation in a shared Redis | `RedisCacheProvider` with a distinct `keyPrefix` per tenant |

## Example

```csharp
using DotnetMicroOrm.Caching;

var cache = new MemoryCacheProvider(maxEntries: 1000, maxMemoryMb: 128);

// Get or create with single-flight protection
var user = await cache.GetOrSetAsync(
    CacheKey.ForUser(42, "profile"),
    async () => await LoadUserAsync(42),
    expiration: TimeSpan.FromMinutes(5));

// Invalidate a user's entries
await cache.RemoveByPatternAsync(CacheKey.CreatePattern("user", 42));

await cache.DisposeAsync();
```