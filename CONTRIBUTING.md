// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Contributing

Thank you for your interest in contributing! This guide walks you through everything you need to get started.

## Table of Contents

- [Development Requirements](#development-requirements)
- [Building Locally](#building-locally)
- [Running Tests](#running-tests)
- [Code Style](#code-style)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Reporting Issues](#reporting-issues)

## Development Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or .NET 8.0)
- Docker (optional, for integration tests against a real database)
- Any IDE that supports C# (e.g., VS Code with C# extension, Rider, Visual Studio)

## Building Locally

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Build a NuGet package
dotnet pack --configuration Release --output ./nupkg
```

Using the provided scripts:

```bash
# Linux / macOS
./scripts/build.sh

# Windows PowerShell
./scripts/build.ps1
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run with detailed output and save a TRX report
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/dotnet-micro-orm.Tests/dotnet-micro-orm.Tests.csproj --verbosity normal
```

Run integration tests with a local database:

```bash
# Start supporting services
docker-compose up -d

# Run integration tests
dotnet test --filter Category=Integration --verbosity normal
```

## Code Style

- Follow the conventions already established in `.editorconfig`.
- Use 4 spaces for indentation in C# files.
- Provide XML documentation comments (`///`) for all public types and members.
- Keep author headers intact — do **not** remove them.
- Prefer `var` when the type is obvious from the right-hand side.
- Avoid magic numbers; use named constants from `src/Constants/`.
- Write one class per file; file name must match the type name.

## Pull Request Guidelines

1. **Fork** the repository and create a branch from `main`.
2. Branch naming: `feature/<short-description>`, `fix/<short-description>`, or `docs/<short-description>`.
3. Ensure all tests pass (`dotnet test`).
4. Add or update tests to cover your changes.
5. Keep commits focused — one logical change per commit.
6. Write a clear PR description: what changed, why, and how to test it.
7. Link any related issues with `Closes #<issue-number>`.

## Reporting Issues

Please use [GitHub Issues](https://github.com/sarmkadan/dotnet-micro-orm/issues) to report bugs or request features. When reporting a bug, include:

- .NET version (`dotnet --version`)
- Operating system
- Minimal reproduction steps
- Expected vs. actual behaviour

## License

All contributions are licensed under the [MIT License](LICENSE).

