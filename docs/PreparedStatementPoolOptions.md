# PreparedStatementPoolOptions

`PreparedStatementPoolOptions` provides configuration and management for a pool of prepared SQL statements, optimizing query execution by reusing statement handles across multiple database operations. It is designed for high-throughput scenarios where repeated query compilation would otherwise degrade performance.

## API

### `MaxPoolSize`
Gets or sets the maximum number of prepared statement entries allowed in the pool. When the pool reaches this limit, new requests to `BorrowAsync` will wait until an entry is returned via `ReturnAsync` or `ReleaseAsync`.

### `PreparedStatementPool`
Gets the underlying pool instance managing the prepared statement entries. This property is primarily used for advanced scenarios where direct interaction with the pool is required.

### `Task<PreparedStatementEntry?> BorrowAsync()`
Asynchronously retrieves a prepared statement entry from the pool. If an entry is available, it is returned immediately; otherwise, the call waits until one becomes available or the pool is disposed. Returns `null` if the pool is disposed or closing.

### `Task ReturnAsync(PreparedStatementEntry entry)`
Returns a borrowed prepared statement entry to the pool, making it available for subsequent reuse. The entry must not be in use and should be in a valid state for reuse. This method does not release the underlying database resources; use `ReleaseAsync` to deallocate them.

### `Task ReleaseAsync(PreparedStatementEntry entry)`
Releases a prepared statement entry, deallocating its underlying database resources and removing it from the pool. This should be called when the entry is no longer needed or when the pool is being cleaned up. The entry must not be in use.

### `Task<(int PoolSize, double HitRatio)> GetPoolStatsAsync()`
Asynchronously retrieves statistics about the current state of the pool. Returns a tuple containing:
- `PoolSize`: the current number of entries in the pool.
- `HitRatio`: the ratio of successful borrows from the pool to total borrow attempts (a value between 0.0 and 1.0).

### `ValueTask DisposeAsync()`
Asynchronously releases all resources held by the pool, including all prepared statement entries and their underlying database handles. After disposal, any further calls to `BorrowAsync`, `ReturnAsync`, or `ReleaseAsync` will fail or return `null`. This method should be called when the pool is no longer needed.

### `static IServiceCollection AddQueryPlanCaching(IServiceCollection services)`
Registers the prepared statement pool and its dependencies with the dependency injection container. This extension method configures the pool with default settings and adds it to the service collection for use in applications leveraging dependency injection.

## Usage

### Basic Usage with Dependency Injection
