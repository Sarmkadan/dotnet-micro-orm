# CLAUDE.md

DotnetMicroOrm - a .NET micro-ORM library (NuGet `Zaiets.dotnet.micro.orm`) with compiled expressions, batch upsert, change tracking, query plan caching and SQL Server / SQLite support; ships with a demo app, examples and benchmarks.

## Build

- SDK: .NET 10 (`global.json` pins 10.0.100, rollForward latestMinor); CI also builds on 8.0.x.
- `dotnet restore && dotnet build -c Release` (or `make build`)
- Solution: `dotnet-micro-orm.sln` (3 projects: library, tests, benchmarks)
- Library project is `DotnetMicroOrm.csproj` at repo root; it excludes `tests/`, `examples/`, `benchmarks/`, `src/Controllers/` and `src/Program.cs` from compilation.
- `make pack` - NuGet package; `make docker` / `make docker-up` - Docker + compose (SQL Server, Redis).

## Test

- `dotnet test` (or `make test`, `make coverage`)
- Single test: `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"`
- Framework: xUnit + FluentAssertions + Moq; SQLite in-memory for DB tests.
- Tests live in `tests/dotnet-micro-orm.Tests/`, one file per unit, named `<Subject>Tests.cs`.

## Lint / Format

- `dotnet format --verify-no-changes` (`make format`), `dotnet format` to fix (`make format-fix`)
- `make lint` = build with `EnforceCodeStyleInBuild=true`
- Style rules in `.editorconfig`: 4 spaces, LF, Allman braces, `var` preferred, nullable enabled, warnings not errors.

## Key directories

- `src/Data/` - core ORM: `DatabaseContext`, `Repository<T>` / `IRepository`, `QueryBuilder`, `UnitOfWork`, `Specification`, `BatchUpsertOperation`, `QueryPlanCache`, `PreparedStatementPool`, `ConnectionRetryPolicy`
- `src/Data/Repositories/` - concrete repositories (`ProductRepository`, `UserRepository`)
- `src/Domain/Models/` - entities
- `src/Caching/` - `ICacheProvider`, `MemoryCacheProvider`, `RedisCacheProvider`
- `src/Configuration/` - `ServiceCollectionExtensions` (DI registration), `AppSettings`, `ApplicationBuilder`
- `src/Migrations/`, `src/Events/`, `src/Pipeline/`, `src/Middleware/`, `src/BackgroundJobs/`, `src/Profiling/`, `src/Cli/`, `src/Exceptions/`, `src/Utils/`, `src/Constants/OrmConstants.cs`
- `src/Program.cs` - demo entry point (excluded from the library build)
- `examples/` - usage samples; `benchmarks/` - BenchmarkDotNet; `docs/` - extended docs; `scripts/init.sql` - DB bootstrap
- `.github/workflows/` - ci, codeql, docker, nuget-publish, release

## Conventions

- Root namespace `DotnetMicroOrm`, sub-namespaces mirror folders (`DotnetMicroOrm.Data`, `DotnetMicroOrm.Caching`, ...).
- Interfaces prefixed with `I`; async methods end with `Async` and take `CancellationToken`.
- Helper logic split into partial/extension files: `XExtensions.cs`, `XJsonExtensions.cs`, `XValidation.cs` next to `X.cs`.
- Magic numbers go to named constants (`OrmConstants`, private `const` fields).
- Files start with `#nullable enable` and an author header; XML doc comments on public API (`GenerateDocumentationFile` is on, CS1591 suppressed).
- Commit messages: Conventional Commits (`feat:`, `fix:`, `refactor(scope):`, `docs:`, `chore:`).
- Do not commit `bin/`, `obj/`, `.aider*`, `*.backup` files.
