# Database migrations

DotnetMicroOrm migrations are explicit, versioned database changes. You write each change as an `IMigration`, register it with dependency injection, and run it through `IMigrationRunner` or `MigrationRunner`.

The migration system does not compare schemas or generate migrations automatically. Migration bodies issue commands through `IDatabaseContext`.

## Main types

- `IMigration` describes one reversible change. `Version` controls ordering, `Description` is stored in the history table, `UpAsync` applies the change, and `DownAsync` reverses it.
- `MigrationRunner` sorts registered migrations by version, skips successfully applied versions, executes pending changes, and manages migration history. Applications normally resolve it through `IMigrationRunner`.
- `MigrationRecord` represents a row read from the history table. It contains the row ID, version, description, UTC application time, success flag, and an optional error message.

Versions are compared as strings using an ordinal, case-insensitive comparison. Use a fixed-width convention such as `YYYYMMdd_seq` so lexical and chronological order agree—for example, `20260913_001` followed by `20260913_002`. Every migration must have a unique version.

## Write a migration

Implement `IMigration` and keep both directions consistent. This example makes the rollback remove the object created by the forward migration:

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Migrations;

public sealed class AddProductsTable : IMigration
{
    public string Version => "20260913_001";

    public string Description => "Create the Products table";

    public Task UpAsync(IDatabaseContext context) =>
        context.ExecuteNonQueryAsync("""
            CREATE TABLE [dbo].[Products] (
                [Id]   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name] NVARCHAR(200) NOT NULL
            )
            """);

    public Task DownAsync(IDatabaseContext context) =>
        context.ExecuteNonQueryAsync("DROP TABLE [dbo].[Products]");
}
```

Choose SQL that is valid for the configured database provider. Keep migrations deterministic, avoid changing an existing migration after it has been applied, and test `DownAsync` as carefully as `UpAsync` because rollback can destroy data.

## Register and run migrations

`AddDotnetMicroOrm` registers `IMigrationRunner`. Register every migration separately with `AddMigration<TMigration>()`, then resolve the runner from a dependency-injection scope:

```csharp
using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Migrations;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services
    .AddDotnetMicroOrm(connectionString, DatabaseProvider.SqlServer)
    .AddMigration<AddProductsTable>();

await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();

var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
await runner.MigrateAsync();
```

`MigrateAsync()` creates the history table if necessary and applies every pending migration in ascending version order. To stop at a particular version, inclusive, use:

```csharp
await runner.MigrateToAsync("20260913_001");
```

To undo all applied migrations newer than a target, use `RollbackToAsync`. The target itself remains applied, and migrations are reverted in descending version order:

```csharp
await runner.RollbackToAsync("20260913_001");
```

## Inspect and preview

The runner can report current state and capture the SQL produced by migration methods:

```csharp
IReadOnlyList<string> pending = await runner.GetPendingMigrationsAsync();
IReadOnlyList<MigrationRecord> applied = await runner.GetAppliedMigrationsAsync();

IReadOnlyList<IMigrationRunner.PendingMigration> details =
    await runner.GetPendingMigrationsWithDetailsAsync();

string upSql = await runner.GenerateUpSqlAsync("20260913_001");
string downSql = await runner.GenerateDownSqlAsync("20260913_001");

IReadOnlyList<string> migrateScripts = await runner.MigrateDryRunAsync();
IReadOnlyList<string> rollbackScripts =
    await runner.RollbackDryRunAsync("20260913_001");
```

Dry runs invoke `UpAsync` or `DownAsync` with a context that captures database commands instead of sending them to the configured database. Keep migration methods limited to `IDatabaseContext` operations: unrelated side effects in migration code are not isolated by dry-run mode.

## Migration history and failures

`MigrationRunner` maintains `[dbo].[_MigrationHistory]` with one unique row per version. Successful rows determine which migrations are skipped. `GetAppliedMigrationsAsync()` returns all history rows, including failures, ordered by version; inspect `Success` and `ErrorMessage` to distinguish their outcomes.

If `UpAsync` throws, the runner wraps the exception in an `OrmException` with code `MIGRATION_FAILED` and records the failed attempt. If `DownAsync` throws, it raises `MIGRATION_ROLLBACK_FAILED` and leaves the history row in place. A successful rollback deletes that migration's history row.

The built-in history-table DDL currently targets SQL Server conventions: the `dbo` schema, bracketed identifiers, `IDENTITY`, `NVARCHAR`, and `INFORMATION_SCHEMA.TABLES`. Account for that when selecting a provider or supplying a database-compatible migration strategy.
