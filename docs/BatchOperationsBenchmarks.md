# BatchOperationsBenchmarks

Benchmark suite for evaluating the performance of batch operations in Dapper.FastCrud. This class measures the throughput of bulk insert, update, and delete operations across varying entity counts and scenarios, including operations with complex predicates and entity relations.

## API

### `GlobalSetup`
Initializes the benchmark environment before any benchmarks are run. Sets up the database schema, seeds test data, and prepares any required services or configurations. Throws if the setup fails or if the environment cannot be prepared.

### `GlobalCleanup`
Cleans up the benchmark environment after all benchmarks have completed. Removes test data, drops temporary tables, and releases any resources allocated during `GlobalSetup`. Throws if cleanup fails or if resources cannot be released.

### `AddRangeAsync_1000_Entities`
Measures the performance of adding 1,000 entities in a single batch operation using `AddRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be persisted.

### `AddRangeAsync_5000_Entities`
Measures the performance of adding 5,000 entities in a single batch operation using `AddRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be persisted.

### `UpdateRangeAsync_1000_Entities`
Measures the performance of updating 1,000 entities in a single batch operation using `UpdateRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be updated.

### `UpdateRangeAsync_5000_Entities`
Measures the performance of updating 5,000 entities in a single batch operation using `UpdateRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be updated.

### `DeleteRangeAsync_1000_Entities`
Measures the performance of deleting 1,000 entities in a single batch operation using `DeleteRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be removed.

### `DeleteRangeAsync_5000_Entities`
Measures the performance of deleting 5,000 entities in a single batch operation using `DeleteRangeAsync`. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be removed.

### `BulkInsert_100_Entities`
Measures the performance of bulk-inserting 100 entities using a dedicated bulk-insert mechanism. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be inserted.

### `BulkInsert_10000_Entities`
Measures the performance of bulk-inserting 10,000 entities using a dedicated bulk-insert mechanism. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities cannot be inserted.

### `BatchInsert_1000_Entities_With_Relations`
Measures the performance of inserting 1,000 entities along with their related entities in a single batch operation. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities or their relations cannot be persisted.

### `BatchUpdate_Complex_Predicate`
Measures the performance of updating entities using a complex predicate in a single batch operation. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities matching the predicate cannot be updated.

### `BatchDelete_With_Where_Clause`
Measures the performance of deleting entities using a `WHERE` clause in a single batch operation. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the entities matching the clause cannot be removed.

## Usage

### Example 1: Benchmarking Bulk Insert Performance
