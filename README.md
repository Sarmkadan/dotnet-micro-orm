// entire file content ...

// ... goes in between

## MemoryCacheProviderTests

The `MemoryCacheProviderTests` class provides comprehensive unit tests for the `MemoryCacheProvider` class, ensuring its caching functionality works as expected. This class contains various test methods to validate different scenarios, such as cache hits, misses, expiration, and removal.

### Example Usage

```csharp
using DotnetMicroOrm.Caching;
using DotnetMicroOrm.Tests;

var cache = new MemoryCacheProvider();

// Test setting and getting a value from the cache
await cache.SetAsync("key", "value");
var result = await cache.GetAsync<string>("key");
Console.WriteLine(result); // Output: value

// Test removing a key from the cache
await cache.RemoveAsync("key");
result = await cache.GetAsync<string>("key");
Console.WriteLine(result); // Output: 

// Test clearing all entries from the cache
await cache.SetAsync("key1", "value1");
await cache.SetAsync("key2", "value2");
await cache.ClearAsync();
var count = await cache.GetCountAsync();
Console.WriteLine(count); // Output: 0
```

// ... goes in between
