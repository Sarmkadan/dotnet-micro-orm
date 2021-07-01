# MemoryCacheProvider

The `MemoryCacheProvider` is an asynchronous, in‑memory cache implementation designed for use with the dotnet‑micro‑orm library. It provides a simple key‑value store with optional expiration, pattern‑based removal, and cleanup capabilities, all exposed through `Task`‑based APIs to integrate naturally with asynchronous code paths.

## API

### GetAsync<T>
```csharp
public async Task<T?> GetAsync<T>(string key);
```
- **Purpose:** Retrieves the cached value associated with `key` and attempts to cast it to `T`. Returns `null` if the key is missing, the value has expired, or the cast fails.
- **Parameters:**  
  - `key` – The cache key to look up; must not be `null`.
- **Return Value:** A `Task<T?>` yielding the cached value or `null`.
- **Exceptions:**  
  - `ArgumentNullException` if `key` is `null`.  
  - `OperationCanceledException` if the underlying token is canceled (if any).  
  - May propagate exceptions from internal synchronization primitives.

### SetAsync<T>
```csharp
public async Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null);
```
- **Purpose:** Inserts or updates a cache entry for `key` with the supplied `value`. An optional sliding expiration can be provided; otherwise the entry lives until evicted by cleanup or manual removal.
- **Parameters:**  
  - `key` – Cache key; must not be `null`.  
  - `value` – Object to store; may be `null`.  
  - `slidingExpiration` – Optional `TimeSpan` defining how long the entry remains valid after the last access.
- **Return Value:** A completed `Task`.
- **Exceptions:**  
  - `ArgumentNullException` if `key` is `null`.  
  - `ArgumentOutOfRangeException` if `slidingExpiration` is negative.  
  - `OperationCanceledException` on cancellation.

### GetOrSetAsync<T>
```csharp
public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? slidingExpiration = null);
```
- **Purpose:** Returns the cached value for `key` if present and valid; otherwise invokes `factory` to produce a value, stores it with the given expiration, and returns the result. Guarantees that the factory is executed only once per missing key even under concurrent calls.
- **Parameters:**  
  - `key` – Cache key; must not be `null`.  
  - `factory` – Asynchronous delegate that creates the value when missing; must not be `null`.  
  - `slidingExpiration` – Optional expiration for the newly set entry.
- **Return Value:** A `Task<T>` yielding the cached or newly created value.
- **Exceptions:**  
  - `ArgumentNullException` if `key` or `factory` is `null`.  
  - `ArgumentOutOfRangeException` if `slidingExpiration` is negative.  
  - Any exception thrown by `factory` is propagated.  
  - `OperationCanceledException` on cancellation.

### RemoveAsync
```csharp
public async Task RemoveAsync(string key);
```
- **Purpose:** Deletes the entry associated with `key` if it exists.
- **Parameters:**  
  - `key` – Cache key to remove; must not be `null`.
- **Return Value:** A completed `Task`.
- **Exceptions:**  
  - `ArgumentNullException` if `key` is `null`.  
  - `OperationCanceledException` on cancellation.

### RemoveByPatternAsync
```csharp
public async Task RemoveByPatternAsync(string pattern);
```
- **Purpose:** Removes all entries whose keys match the supplied regular expression `pattern`.
- **Parameters:**  
  - `pattern` – Regular expression pattern; must not be `null` or empty.
- **Return Value:** A completed `Task`.
- **Exceptions:**  
  - `ArgumentNullException` or `ArgumentException` if `pattern` is invalid.  
  - `OperationCanceledException` on cancellation.

### ClearAsync
```csharp
public async Task ClearAsync();
```
- **Purpose:** Removes all entries from the cache.
- **Return Value:** A completed `Task`.
- **Exceptions:**  
  - `OperationCanceledException` on cancellation.

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(string key);
```
- **Purpose:** Determines whether an entry for `key` exists and has not expired.
- **Parameters:**  
  - `key` – Cache key to test; must not be `null`.
- **Return Value:** A `Task<bool>` yielding `true` if the entry is present and valid, otherwise `false`.
- **Exceptions:**  
  - `ArgumentNullException` if `key` is `null`.  
  - `OperationCanceledException` on cancellation.

### GetCountAsync
```csharp
public async Task<long> GetCountAsync();
```
- **Purpose:** Retrieves the approximate number of entries currently stored in the cache.
- **Return Value:** A `Task<long>` yielding the count.
- **Exceptions:**  
  - `OperationCanceledException` on cancellation.

### CleanupAsync
```csharp
public async Task CleanupAsync();
```
- **Purpose:** Scansynchronously removes expired entries from the cache. This method is typically called periodically or when memory pressure is detected.
- **Return Value:** A completed `Task`.
- **Exceptions:**  
  - `OperationCanceledException` on cancellation.

### DisposeAsync
```csharp
public async ValueTask DisposeAsync();
```
- **Purpose:** Releases any resources held by the cache (e.g., timers used for automatic cleanup). After disposal, further operations may throw `ObjectDisposedException`.
- **Return Value:** A `ValueTask` representing the asynchronous dispose operation.
- **Exceptions:**  
  - `ObjectDisposedException` if called after the instance has already been disposed.  
  - `OperationCanceledException` on cancellation.

### Value
```csharp
public required object Value { get; set; }
```
- **Purpose:** Gets or sets the cached object associated with the most recent cache operation (e.g., the value returned by `GetAsync` or supplied to `SetAsync`). This property is provided for scenarios where direct access to the raw cached object is needed outside the generic methods.
- **Remarks:** The property is `required`, meaning it must be initialized after object creation. Its value is undefined until a cache operation populates it.

### ExpiresAt
```csharp
public DateTime? ExpiresAt { get; set; }
```
- **Purpose:** Gets or sets the absolute expiration timestamp for the cached value represented by `Value`. A `null` indicates no absolute expiration (only sliding expiration may apply).
- **Remarks:** When set, the cache treats the entry as expired once `UtcNow` exceeds this timestamp.

### CreatedAt
```csharp
public DateTime CreatedAt { get; set; }
```
- **Purpose:** Gets or sets the UTC timestamp when the cached value represented by `Value` was inserted into the cache.
- **Remarks:** This timestamp is useful for implementing custom eviction policies based on age.

## Usage

### Basic get/set pattern
```csharp
var cache = new MemoryCacheProvider();

// Store a value with a sliding expiration of 5 minutes
await cache.SetAsync("user:42", new User { Id = 42, Name = "Ada" }, TimeSpan.FromMinutes(5));

// Retrieve the value; returns null if missing or expired
var user = await cache.GetAsync<User>("user:42");
if (user != null)
{
    Console.WriteLine($"Hello, {user.Name}!");
}
```

### GetOrSet with pattern‑based removal
```csharp
var cache = new MemoryCacheProvider();

// Ensure a value exists, creating it lazily if needed
var config = await cache.GetOrSetAsync(
    "app:config",
    async () =>
    {
        // Simulate expensive I/O or computation
        return await LoadConfigurationFromDiskAsync();
    },
    TimeSpan.FromHours(1));

// Later, remove all cache entries related to configuration
await cache.RemoveByPatternAsync(@"^app:config.*");

// Clean up expired entries periodically (e.g., in a background timer)
await cache.CleanupAsync();

await cache.DisposeAsync();
```

## Notes

- **Thread safety:** All public instance methods are safe to call concurrently from multiple threads. Internal synchronization ensures that `GetOrSetAsync` invokes the supplied factory only once per missing key, even under race conditions.
- **Expiration semantics:** Sliding expiration is reset on each successful `GetAsync` or `GetOrSetAsync` call. Absolute expiration (`ExpiresAt`) is checked independently; if either condition indicates expiry, the entry is treated as missing.
- **Value, ExpiresAt, CreatedAt:** These properties reflect the state of the last value interacted with via the generic methods. They are not automatically updated by bulk operations such as `ClearAsync` or `RemoveByPatternAsync`. Consumers should treat them as informational and not rely on them for synchronization.
- **Disposal:** After `DisposeAsync` completes, any further call to the cache members will throw `ObjectDisposedException`. It is recommended to await `DisposeAsync` before discarding the instance.
- **Cancellation:** Members that accept a `CancellationToken` (not shown in the signatures but supported by the underlying implementation) will propagate `OperationCanceledException` when cancellation is requested. If no token is supplied, the operations are not cancelable.
- **Exception safety:** The cache does not throw exceptions for missing keys; instead, it returns `false`/`null` as appropriate. All other failure conditions (invalid arguments, internal errors, cancellation) are communicated via exceptions.
