# PreparedStatementPoolOptionsExtensions

Provides extension methods for configuring `PreparedStatementPoolOptions` instances with common pooling strategies. These methods simplify the creation of option objects by offering predefined configurations for pool sizing and eviction behavior, eliminating the need to manually set individual properties.

## API

### WithMaxPoolSize

```csharp
public static PreparedStatementPoolOptions WithMaxPoolSize(this PreparedStatementPoolOptions options, int maxSize)
```

Sets the maximum number of prepared statements the pool can hold. Once the pool reaches this limit, additional statements may trigger eviction of existing entries depending on the eviction policy in effect.

**Parameters:**
- `options` — The `PreparedStatementPoolOptions` instance to configure.
- `maxSize` — The absolute maximum capacity of the pool. Must be greater than zero.

**Returns:** The same `PreparedStatementPoolOptions` instance, enabling fluent chaining.

**Throws:** `ArgumentOutOfRangeException` when `maxSize` is less than or equal to zero.

---

### WithMemoryBasedMaxPoolSize

```csharp
public static PreparedStatementPoolOptions WithMemoryBasedMaxPoolSize(this PreparedStatementPoolOptions options, long maxMemoryBytes)
```

Configures the pool to limit its total size based on estimated memory consumption rather than a fixed statement count. The pool will track the approximate memory used by cached prepared statements and evict entries when the specified threshold is exceeded.

**Parameters:**
- `options` — The `PreparedStatementPoolOptions` instance to configure.
- `maxMemoryBytes` — The maximum estimated memory in bytes the pool may consume. Must be greater than zero.

**Returns:** The same `PreparedStatementPoolOptions` instance, enabling fluent chaining.

**Throws:** `ArgumentOutOfRangeException` when `maxMemoryBytes` is less than or equal to zero.

---

### WithNoEviction

```csharp
public static PreparedStatementPoolOptions WithNoEviction(this PreparedStatementPoolOptions options)
```

Disables automatic eviction of prepared statements from the pool. Once added, statements remain cached indefinitely. This is suitable for applications with a bounded, known set of queries where memory growth is not a concern.

**Parameters:**
- `options` — The `PreparedStatementPoolOptions` instance to configure.

**Returns:** The same `PreparedStatementPoolOptions` instance, enabling fluent chaining.

**Throws:** Nothing.

---

### WithDefaultSize

```csharp
public static PreparedStatementPoolOptions WithDefaultSize(this PreparedStatementPoolOptions options)
```

Applies a sensible default pool size suitable for most applications. The exact value is determined by the library and balances memory usage with caching effectiveness.

**Parameters:**
- `options` — The `PreparedStatementPoolOptions` instance to configure.

**Returns:** The same `PreparedStatementPoolOptions` instance, enabling fluent chaining.

**Throws:** Nothing.

## Usage

### Example 1: Fixed-size pool with no eviction

```csharp
var poolOptions = new PreparedStatementPoolOptions()
    .WithMaxPoolSize(100)
    .WithNoEviction();

var connection = new PreparedStatementConnection(connectionString, poolOptions);

// The pool will hold up to 100 prepared statements and never evict them.
// Once the limit is reached, additional prepare attempts will be rejected
// until statements are explicitly removed.
```

### Example 2: Memory-bound pool for a high-throughput service

```csharp
var poolOptions = new PreparedStatementPoolOptions()
    .WithMemoryBasedMaxPoolSize(50 * 1024 * 1024) // 50 MB limit
    .WithMaxPoolSize(10_000);                     // Hard cap as safety net

var connection = new PreparedStatementConnection(connectionString, poolOptions);

// The pool grows freely until estimated memory reaches 50 MB or the
// absolute cap of 10,000 statements is hit. Beyond those thresholds,
// the least recently used statements are evicted to make room.
```

## Notes

- All methods return the same `PreparedStatementPoolOptions` instance passed to them, making them fully chainable. The order of calls matters only when multiple methods set overlapping properties; the last call wins for any given property.
- `WithMemoryBasedMaxPoolSize` and `WithMaxPoolSize` can be combined. When both are set, eviction occurs when either limit is breached.
- `WithNoEviction` overrides any previously configured eviction policy. Calling it after `WithMemoryBasedMaxPoolSize` or `WithMaxPoolSize` will disable eviction entirely, though the size limits remain as hard caps that reject new additions.
- These extension methods are not thread-safe by themselves. The returned `PreparedStatementPoolOptions` instance should be fully configured before being shared across threads or passed to a connection. Concurrent modification of options after they are in use by a pool may lead to inconsistent behavior.
- `WithDefaultSize` applies a library-defined default. Applications with specific performance or memory requirements should prefer explicit sizing via `WithMaxPoolSize` or `WithMemoryBasedMaxPoolSize`.
