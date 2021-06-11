# MigrationRunner

The `MigrationRunner` class orchestrates the execution and tracking of database migrations within the `dotnet-micro-orm` framework. It provides asynchronous methods to apply pending migrations, migrate to or roll back to a specific version, and query the migration state.

## API

### `public Task MigrateAsync()`
Applies all pending migrations in the configured migration assembly, ordered by version.  
- **Parameters:** None.  
- **Return value:** A `Task` that completes when the migration process finishes.  
- **Throws:**  
  - `InvalidOperationException` if no migration assembly has been configured.  
  - `DbException` (or derived) if a migration script fails to execute or a database error occurs.  
  - `MigrationException` if a migration reports a validation failure.

### `public Task MigrateToAsync(long targetVersion)`
Migrates the database to the specified `targetVersion`. If the current version is lower than the target, pending migrations up to that version are applied; if higher, no action is taken.  
- **Parameters:**  
  - `targetVersion`: The version number to migrate to.  
- **Return value:** A `Task` that completes when the migration to the target version finishes.  
- **Throws:**  
  - `ArgumentOutOfRangeException` if `targetVersion` is negative.  
  - `InvalidOperationException` if the migration assembly is not configured.  
  - `DbException` if any migration step encounters a database error.  
  - `MigrationException` if the target version does not exist in the migration set.

### `public Task RollbackToAsync(long targetVersion)`
Rolls back the database to the specified `targetVersion`. If the current version is higher than the target, migrations are reverted in reverse order until the target version is reached; if lower, no action is taken.  
- **Parameters:**  
  - `targetVersion`: The version number to roll back to.  
- **Return value:** A `Task` that completes when the rollback finishes.  
- **Throws:**  
  - `ArgumentOutOfRangeException` if `targetVersion` is negative.  
  - `InvalidOperationException` if the migration assembly is not configured.  
  - `DbException` if a rollback script fails.  
  - `MigrationException` if the target version does not exist or cannot be rolled back.

### `public Task<IReadOnlyList<MigrationRecord>> GetAppliedMigrationsAsync()`
Retrieves the list of migrations that have been successfully applied to the database.  
- **Parameters:** None.  
- **Return value:** A `Task` whose result is an `IReadOnlyList<MigrationRecord>` containing metadata for each applied migration, ordered by application time.  
- **Throws:**  
  - `InvalidOperationException` if the migration storage table has not been created.  
  - `DbException` if querying the migration table fails.

### `public Task<IReadOnlyList<IMigration>> GetPendingMigrationsAsync()`
Enumerates migrations that are defined in the assembly but have not yet been applied to the database.  
- **Parameters:** None.  
- **Return value:** A `Task` whose result is an `IReadOnlyList<IMigration>` of pending migrations, ordered by version.  
- **Throws:**  
  - `InvalidOperationException` if the migration assembly is not configured.  
  - `DbException` if reading the migration assembly or checking applied migrations fails.

## Usage

```csharp
using DotNetMicroOrm.Migration;
using System.Threading.Tasks;

// Assume a configured DbConnectionFactory is available.
var runner = new MigrationRunner(connectionFactory, typeof(MyMigrationsAssembly).Assembly);

// Apply all pending migrations.
await runner.MigrateAsync();
```

```csharp
using DotNetMicroOrm.Migration;
using System.Threading.Tasks;

var runner = new MigrationRunner(connectionFactory, typeof(MyMigrationsAssembly).Assembly);

// Migrate to version 202309150001.
await runner.MigrateToAsync(202309150001);

// Later, roll back to version 202309100001.
await runner.RollbackToAsync(202309100001);
```

## Notes

- The class does **not** manage the lifetime of the supplied `DbConnectionFactory`; callers are responsible for disposing of any resources it creates.  
- All methods are asynchronous and should be awaited; fire‑and‑forget usage may hide exceptions.  
- `MigrationRunner` is **not thread‑safe**. Concurrent invocations of any of its methods on the same instance can lead to race conditions, duplicate migration attempts, or inconsistent state. External synchronization (e.g., locking or ensuring single‑threaded access) is required when sharing an instance across threads.  
- Migration scripts are executed within a transaction per migration step; if a step fails, the transaction for that step is rolled back, but previously applied migrations remain committed.  
- The migration history table is created automatically on first use if it does not exist; however, if the table exists with an incompatible schema, an exception will be thrown.  
- Version identifiers are treated as monotonic increasing numbers; supplying a non‑monotonic sequence may cause unexpected behavior.  
- If no migrations are pending, `MigrateAsync` and `MigrateToAsync` complete successfully without performing any work.  
- `GetAppliedMigrationsAsync` and `GetPendingMigrationsAsync` reflect the state of the database at the moment the query is executed; subsequent migrations by other processes will not be visible until the method is called again.
