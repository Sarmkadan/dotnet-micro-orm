# ICacheProvider

`ICacheProvider` is a static class that exposes a set of predefined string constants used as cache keys throughout the dotnet‑micro‑orm library. These constants help ensure consistent naming of cached items for entities, operations, and configuration data, reducing the likelihood of key collisions and simplifying cache‑related logic.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Create` | Key used when caching the result of a create operation. | none | A constant string representing the create‑operation cache key. | Does not throw. |
| `CreatePattern` | Pattern string used to match and remove all create‑related cache entries during invalidation. | none | A constant string that can be used with cache‑key pattern matching (e.g., `Cache.RemoveByPattern(ICacheProvider.CreatePattern)`). | Does not throw. |
| `ForUser` | Key prefix for caching user‑specific data. | none | A constant string that forms the base of user‑related cache keys (typically combined with a user identifier). | Does not throw. |
| `ForProduct` | Key prefix for caching product‑specific data. | none | A constant string that forms the base of product‑related cache keys (typically combined with a product identifier). | Does not throw. |
| `ForOrder` | Key prefix for caching order‑specific data. | none | A constant string that forms the base of order‑related cache keys (typically combined with an order identifier). | Does not throw. |
| `ForQuery` | Key prefix for caching arbitrary query results. | none | A constant string that forms the base of query‑related cache keys (often combined with a hash of the query text and parameters). | Does not throw. |
| `ForConfig` | Key prefix for caching configuration values. | none | A constant string that forms the base of configuration‑related cache keys (typically combined with a configuration name). | Does not throw. |

All members are immutable `static string` fields; they contain hard‑coded values and are safe to read from any thread without synchronization.

## Usage

```csharp
// Example 1: Building a cache key for a specific user.
string userId = "12345";
string userCacheKey = $"{ICacheProvider.ForUser}:{userId}";
var user = await cache.GetOrAddAsync(userId, userCacheKey, () => GetUserFromDb(userId));

// Example 2: Invalidating all create‑related cache entries after a bulk insert.
await cache.RemoveByPatternAsync(ICacheProvider.CreatePattern);
```

```csharp
// Example 3: Using the query key prefix to cache a complex report.
string queryHash = ComputeHash(reportSql, reportParameters);
string reportCacheKey = $"{ICacheProvider.ForQuery}:{queryHash}";
var report = await cache.GetOrAddAsync(reportCacheKey, () => ExecuteReport(reportSql, reportParameters));
```

## Notes

- The string constants are compile‑time literals; attempting to modify them via reflection will not affect the values seen by normal code and may lead to undefined behavior.
- Because the fields are immutable, they are inherently thread‑safe; no locking is required when reading them from multiple threads.
- If these keys are used as part of a larger key (e.g., concatenated with an identifier), ensure the separator character does not appear in the identifier to avoid ambiguous keys.
- The `CreatePattern` constant is intended for pattern‑based cache invalidation; providers that do not support pattern removal should fall back to enumerating and deleting matching keys manually.
