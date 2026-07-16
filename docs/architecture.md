# DotnetMicroOrm Architecture

This document describes the architecture as it exists in the code today. If something is listed here, you can find it under `src/`.

## Overview

DotnetMicroOrm is a small ADO.NET-based data access layer (a "micro ORM" in the Dapper spirit, not an EF competitor). It maps entities to tables via attributes and reflection, generates plain parameterized SQL strings for CRUD, and layers on a set of supporting infrastructure: unit of work, batch upsert, query plan cache, prepared statement pool, migrations, profiling, and caching.

The solution is one executable project (`DotnetMicroOrm.csproj`, `src/Program.cs` is a runnable demo of the API) plus `tests/` (xUnit) and `benchmarks/` (BenchmarkDotNet, including comparison benchmarks).

## Project layout

```
src/
  Data/            core ORM: DatabaseContext, Repository<T>, UnitOfWork,
                   QueryBuilder<T>, Specification<T>, BatchUpsertOperation<T>,
                   QueryPlanCache, PreparedStatementPool, PagedResult<T>
  Domain/Models/   BaseEntity + demo entities (User, Product, Order, OrderItem,
                   Category, Inventory, AuditLog) and mapping attributes
  Configuration/   ServiceCollectionExtensions (DI), OrmConfiguration,
                   ApplicationBuilder, AppSettings
  Services/        demo application services (UserService, ProductService,
                   OrderService, AuditService, NotificationService, AnalyticsService)
  Caching/         ICacheProvider + MemoryCacheProvider
  Migrations/      IMigration, MigrationRunner, MigrationRecord
  Profiling/       IQueryProfiler, QueryProfiler, QueryProfile
  Exceptions/      OrmException hierarchy
  Events/          in-process EventBus + example handlers
  Middleware/      example middleware (auth, logging, rate limiting, errors)
  Pipeline/        PipelineBuilder (generic step pipeline)
  BackgroundJobs/  JobScheduler + IBackgroundJob
  Formatters/      IOutputFormatter (json/csv/xml/markdown) + FormatterFactory
  Cli/             CommandParser/CommandHandler for the demo CLI
  Integration/     IHttpClient wrapper + WebhookHandler
  Utils/           Result<T>, ApiResponse<T>, validation and reflection helpers
```

Only `Data/`, `Domain/`, `Configuration/`, `Caching/`, `Migrations/`, `Profiling/` and `Exceptions/` are the ORM proper. `Services/`, `Events/`, `Middleware/`, `Formatters/`, `Cli/` etc. exist to demonstrate the ORM in a realistic application shape and to give the tests something end-to-end to chew on.

## Core components

### DatabaseContext (`src/Data/DatabaseContext.cs`)

The single owner of the physical connection. Wraps a `DbConnection` (created via `Microsoft.Data.SqlClient` or `Microsoft.Data.Sqlite` depending on `DatabaseProvider`) and one optional `DbTransaction`.

- `ExecuteQueryAsync` / `ExecuteNonQueryAsync` / `ExecuteScalarAsync` take raw SQL plus a parameter dictionary; parameters are always bound as `DbParameter`s, never concatenated.
- `BeginTransactionAsync(TransactionIsolationLevel)` / `CommitAsync` / `RollbackAsync` manage one transaction at a time; a second `Begin` throws.
- Registered as a **singleton** in DI, which means one connection is shared per container. That is a deliberate simplification (no connection-per-scope plumbing), and the trade-off is that the context is not safe for concurrent transactions across threads. Rely on ADO.NET connection pooling if you need per-operation connections and construct contexts yourself.

Supported providers: `SqlServer` and `Sqlite` are real (concrete `DbConnection` implementations are created); the SQL generation uses `[bracketed]` identifiers, so SQL Server is the primary dialect and SQLite works because it tolerates brackets.

### Repository<T> (`src/Data/Repository.cs`)

Generic CRUD for any `T : BaseEntity, new()`.

- Table/schema/column names come from mapping attributes on the entity (`src/Domain/Models/MappingAttributes.cs`) resolved by reflection, with sensible fallbacks (type name pluralization is not attempted; the attribute is authoritative).
- `AddAsync`/`UpdateAsync`/`DeleteAsync` generate `INSERT`/`UPDATE`/`DELETE` strings with `@parameter` placeholders and run them through `DatabaseContext`. Updates include optimistic-concurrency handling when the entity carries a version column.
- Entities self-validate before writes (`BaseEntity.Validate`); validation failures throw `EntityValidationException` before any SQL runs.
- **Predicate queries are evaluated client-side.** `GetAsync(predicate)` does `SELECT *`, materializes all rows, then applies the LINQ expression in memory. Same for `FirstOrDefaultAsync` and `ExistsAsync`-via-`CountAsync`. This is the biggest honest limitation of the library: there is no expression-to-SQL translator. It keeps the code auditable and dialect-free, but it means filtered queries over large tables pull the whole table. If a table is big, write raw SQL through `DatabaseContext` or a custom repository (see `Data/Repositories/ProductRepository.cs` for examples that build real `WHERE` clauses by hand).

### UnitOfWork (`src/Data/UnitOfWork.cs`)

Repository factory plus transaction facade over one `DatabaseContext`. Caches one `Repository<T>` per entity type in a `ConcurrentDictionary` and forwards `Begin/Commit/Rollback` to the context, tracking whether a transaction is active so misuse fails fast instead of at the database.

### QueryBuilder<T> and Specification<T>

Two complementary querying styles, both ultimately executed by `Repository<T>`:

- `QueryBuilder<T>` (`src/Data/QueryBuilder.cs`) - fluent, imperative: `Where` (multiple calls are AND-combined by rewriting expression trees), `OrderBy`, `Skip`/`Take`. Good for one-off call-site queries.
- `Specification<T>` (`src/Data/Specification.cs`) - the classic specification pattern: named, reusable query objects carrying `Criteria`, includes, ordering and paging. `SpecificationCombinators.cs` provides `And`/`Or`/`Not` composition. Good for queries that are business rules ("active premium users") rather than ad-hoc filters.

Both inherit the client-side evaluation caveat above.

### BatchUpsertOperation<T> (`src/Data/BatchUpsertOperation.cs`)

Chunked bulk insert-or-update. Splits the input into batches (size from `OrmConstants.DefaultBatchSize`, overridable), generates multi-row statements, and runs each batch inside the ambient transaction. Exists because looping `AddAsync` is O(rows) round-trips; the batch path collapses that to O(rows/batchSize).

### QueryPlanCache (`src/Data/QueryPlanCache.cs`)

LRU + TTL cache keyed by a hash of normalized SQL text (whitespace/parameter-literal normalization via regex, SHA-256 key). Capacity and default TTL come from `QueryPlanCacheOptions` (1000 entries / 1h by default). Note this caches *plan metadata* the library records about a query, not result sets - result caching is `ICacheProvider`'s job and is opt-in at the service layer.

### PreparedStatementPool (`src/Data/PreparedStatementPool.cs`)

Thread-safe pool of prepared-statement entries (SQL + parameter shape) with least-used eviction at `MaxPoolSize` (200 default). Saves re-deriving `DbCommand` parameter shapes on hot paths.

### Caching (`src/Caching/`)

`ICacheProvider` is a small async get/set/remove abstraction; `MemoryCacheProvider` implements it with a `ConcurrentDictionary` plus per-key `Timer`s for expiration. It is intentionally not distributed - swap in your own `ICacheProvider` if you need Redis or similar. The repository does not cache implicitly; caching is explicit at call sites/services so invalidation stays understandable.

### Migrations (`src/Migrations/`)

`IMigration` implementations are registered in DI (`AddMigration<T>()`), discovered by `MigrationRunner`, executed in version order, and recorded in a migrations table via `MigrationRecord`. Down/rollback is supported per migration. No automatic diffing - migrations are hand-written SQL/commands, which is the appropriate level of magic for a micro ORM.

### Profiling (`src/Profiling/`)

`QueryProfiler` (registered as a singleton `IQueryProfiler`) records per-query timings into `QueryProfile` aggregates - counts, total/min/max duration - so slow queries can be found without attaching external tooling.

### Exceptions (`src/Exceptions/`)

All library errors derive from `OrmException` (which carries an error code string): `DatabaseConnectionException`, `EntityValidationException`, `ConfigurationException`, `ValidationException`, plus the umbrella `DotnetMicroOrmException`. Callers can catch the base type at the boundary and switch on codes.

## Dependency injection

`ServiceCollectionExtensions.AddDotnetMicroOrm(connectionString, provider, configureOptions)` (`src/Configuration/`) is the composition root:

- `IDatabaseContext` - singleton
- `IUnitOfWork` - scoped
- open-generic `IRepository<>` -> `Repository<>` - scoped
- open-generic `IBatchUpsertOperation<>` - scoped
- `IQueryProfiler` - singleton
- `ICacheProvider` -> `MemoryCacheProvider` - singleton
- `IMigrationRunner` - scoped
- `OrmConfiguration` - singleton (populated from the `configureOptions` callback)
- demo services (`UserService`, `ProductService`, `OrderService`, `IAuditService`)

Helper extensions: `AddMigration<T>()`, `AddRepository<TEntity, TRepository>()` (override the generic repository for one entity), `AddOrmService<T>()`.

`ApplicationBuilder` (`src/Configuration/ApplicationBuilder.cs`) is an alternative manual composition path for apps that do not use `IServiceCollection`.

## Data flow

Read path (`Repository.GetAsync(predicate)`):

```
caller -> Repository<T>
  -> build "SELECT * FROM [schema].[table]"
  -> DatabaseContext.ExecuteQueryAsync (parameterized DbCommand)
  -> rows -> MapToEntity (reflection + mapping attributes)
  -> predicate applied in memory (LINQ to Objects)
  -> List<T> back to caller
```

Write path (`Repository.AddAsync` / `UpdateAsync`):

```
caller -> entity.Validate() -> entity.PreSave()
  -> SQL string from mapped columns (INSERT ... VALUES / UPDATE ... SET)
  -> parameters dictionary from entity properties
  -> DatabaseContext.ExecuteNonQueryAsync
  (inside UnitOfWork.Begin/Commit if the caller opened a transaction)
```

Bulk path: caller -> `BatchUpsertOperation<T>.ExecuteAsync` -> chunked multi-row SQL -> same context.

## Key design decisions

1. **SQL strings + reflection mapping, no expression-to-SQL compiler.** A real LINQ provider is the single most expensive component of an ORM to build and maintain. Skipping it keeps the whole library readable in an afternoon. The cost - client-side predicate evaluation - is documented above and mitigated by hand-written repositories for hot queries.
2. **Singleton context, scoped repositories.** Simple lifetime story, one transaction at a time, matches the CLI/demo usage. Web apps with high concurrency should register a scoped context instead (the types support it; only the DI default would change).
3. **Explicit caching, no implicit read-through.** Silent caching inside a repository is the classic source of stale-data bugs. `ICacheProvider` exists, but *services* decide what to cache and when to invalidate.
4. **Validation lives on entities** (`BaseEntity.Validate`) and runs before every write. Trade-off: entities carry behavior (not pure POCOs), but no invalid row can reach SQL through the repository path.
5. **Parameterized SQL everywhere.** All generated SQL binds values through `DbParameter`; identifiers come from compile-time attributes, not user input.
6. **Optimistic concurrency via version column** on updates, surfacing conflicts as exceptions rather than last-write-wins.

## Extension points

- `ICacheProvider` - plug in a distributed cache.
- `AddRepository<TEntity, TRepository>()` - replace the generic repository per entity with one that writes real SQL `WHERE` clauses.
- `IMigration` - add schema migrations.
- `IOutputFormatter` / `FormatterFactory` - additional export formats.
- `IQueryProfiler` - swap the profiler for one that exports to your metrics system.
- `Specification<T>` subclasses + combinators - reusable query vocabulary.

## Known limitations

- Predicate/specification queries load full tables before filtering (see Repository above). Fine for small/medium tables, wrong tool for large ones.
- Only SQL Server and SQLite have real provider wiring; the `DatabaseProvider` enum also lists PostgreSql/MySql/Oracle, but `DatabaseContext.CreateConnection` throws `NotSupportedException` for them. Bracketed identifiers make SQL Server the primary dialect.
- No relationship loading: `Includes` on specifications are recorded but joins are not generated; load related aggregates explicitly.
- Single connection/transaction per `DatabaseContext` instance - no parallel transactions on one context.
- `MemoryCacheProvider` allocates one `Timer` per expiring key; with very large key counts prefer a scanning-based provider.
- The change-tracking list inside `Repository<T>` records added entities but there is no deferred `SaveChanges`-style flush; writes are immediate.
