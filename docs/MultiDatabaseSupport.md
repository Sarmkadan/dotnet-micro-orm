# Multi-Database Support in DotnetMicroOrm

DotnetMicroOrm is designed to be database-agnostic, supporting multiple relational database providers. This document outlines how to configure the ORM for different databases, provider-specific considerations, and known behavioral differences.

## Supported Database Providers

The following database providers are officially supported:

*   **SQL Server**
*   **PostgreSQL**
*   **MySQL**
*   **SQLite**
*   **Oracle** (Limited support - further details below)

## Configuration

Database configuration is primarily handled when initializing your `DatabaseContext`. You specify the connection string and the `DatabaseProvider` enum value.

```csharp
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models; // Assuming BaseEntity is here
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotnetMicroOrm(this IServiceCollection services, string connectionString, DatabaseProvider provider)
    {
        services.AddSingleton<IDatabaseContext>(new DatabaseContext(connectionString, provider));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        // Add other services like UnitOfWork, etc.
        return services;
    }
}

// Example usage in Program.cs (or Startup.cs)
// string sqlServerConnectionString = "Server=localhost;Database=MyDb;Integrated Security=True;";
// builder.Services.AddDotnetMicroOrm(sqlServerConnectionString, DatabaseProvider.SqlServer);

// string postgreSqlConnectionString = "Host=localhost;Port=5432;Database=MyDb;Username=user;Password=password;";
// builder.Services.AddDotnetMicroOrm(postgreSqlConnectionString, DatabaseProvider.PostgreSql);

// string mySqlConnectionString = "Server=localhost;Port=3306;Database=MyDb;Uid=user;Pwd=password;";
// builder.Services.AddDotnetMicroOrm(mySqlConnectionString, DatabaseProvider.MySql);

// string sqliteConnectionString = "Data Source=MyDb.db;";
// builder.Services.AddDotnetMicroOrm(sqliteConnectionString, DatabaseProvider.Sqlite);
```

### Connection String Formats

Ensure you use the correct connection string format for your chosen provider.

*   **SQL Server:**
    `Server=your_server_name;Database=your_database_name;User Id=your_username;Password=your_password;TrustServerCertificate=True;`
    (For integrated security: `Server=localhost;Database=MyDb;Integrated Security=True;`)
*   **PostgreSQL:**
    `Host=your_host;Port=5432;Database=your_database_name;Username=your_username;Password=your_password;`
*   **MySQL:**
    `Server=your_host;Port=3306;Database=your_database_name;Uid=your_username;Pwd=your_password;`
*   **SQLite:**
    `Data Source=your_database_file.db;` (File path to the SQLite database)
*   **Oracle:**
    `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your_host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your_service_name)));User Id=your_username;Password=your_password;`

## Provider-Specific Dialect and Quirks

While DotnetMicroOrm strives for a unified API, certain SQL syntax and behaviors are inherently database-specific.

### Pagination (`OFFSET` / `FETCH NEXT`)

The ORM uses standard `OFFSET ... FETCH NEXT` for pagination, which is supported by SQL Server (2012+), PostgreSQL, and MySQL (using `LIMIT ... OFFSET`).

*   **SQL Server:** Uses `OFFSET N ROWS FETCH NEXT M ROWS ONLY`.
*   **PostgreSQL:** Uses `LIMIT M OFFSET N`.
*   **MySQL:** Uses `LIMIT M OFFSET N`.
*   **SQLite:** Uses `LIMIT M OFFSET N`.
*   **Oracle:** Oracle's pagination typically involves subqueries with `ROWNUM` or `FETCH FIRST ... ROWS ONLY` (12c+). The current ORM implementation might require manual query construction for complex Oracle pagination scenarios if `OFFSET/FETCH NEXT` is not directly translated.

### Identifier Quoting

Database systems have different ways of quoting identifiers (table names, column names) to handle special characters or reserved keywords.

*   **SQL Server:** Uses `[Identifier]`
*   **PostgreSQL:** Uses `"identifier"`
*   **MySQL:** Uses `` `identifier` ``
*   **SQLite:** Uses `"identifier"` or `[identifier]`
*   **Oracle:** Uses `"IDENTIFIER"` (case-sensitive when quoted, uppercase by default when unquoted)

DotnetMicroOrm attempts to handle quoting automatically based on the configured provider, but be mindful of this when writing raw SQL queries.

### `DateTime` Precision

`DateTime` and `DateTimeOffset` precision can vary between database providers.

*   **SQL Server:** `datetime` (3.33 ms), `datetime2` (100 ns), `smalldatetime` (1 minute). `datetime2` is generally recommended for precision.
*   **PostgreSQL:** `timestamp` and `timestamptz` (microseconds precision).
*   **MySQL:** `DATETIME` and `TIMESTAMP` (seconds precision by default, up to microseconds with `DATETIME(6)` / `TIMESTAMP(6)` in MySQL 5.6.4+).
*   **SQLite:** Stores dates as TEXT, REAL, or INTEGER. Precision depends on storage format.
*   **Oracle:** `DATE` (seconds precision), `TIMESTAMP` (nanosecond precision).

When dealing with high-precision time values, verify the specific data types used in your database schema and adjust entity property types if necessary.

### `BOOLEAN` Type Handling

*   **SQL Server:** Does not have a native `BOOLEAN` type. `BIT` is typically used (0 or 1).
*   **PostgreSQL:** Has a native `BOOLEAN` type (`true`/`false`).
*   **MySQL:** `TINYINT(1)` is commonly used (0 or 1). `BOOLEAN` is a synonym for `TINYINT(1)`.
*   **SQLite:** Does not have a native `BOOLEAN` type. `INTEGER` (0 or 1) is used.
*   **Oracle:** Does not have a native `BOOLEAN` type in SQL. `NUMBER(1)` or `VARCHAR2(1)` is often used.

The ORM will handle the mapping to .NET `bool` types, but be aware of the underlying database representation.

### Stored Procedures and Functions

Calling stored procedures and functions might require provider-specific syntax or ADO.NET objects. DotnetMicroOrm's `ExecuteNonQueryAsync`, `ExecuteScalarAsync`, and `ExecuteQueryAsync` can execute arbitrary SQL, including calls to stored procedures, but parameter handling for complex stored procedures might need manual adjustment.

### Transaction Isolation Levels

The supported `TransactionIsolationLevel` enums map to standard ADO.NET `IsolationLevel` values, which are generally consistent across providers. However, the exact behavior and concurrency guarantees can differ subtly. Consult your database's documentation for specifics.

## Advanced Scenarios

For highly provider-specific features, such as advanced spatial data types, JSON column querying, or full-text search, you might need to use raw SQL queries or extend the ORM with custom type handlers and query builders.

## Contributing

If you encounter a provider-specific quirk not documented here, please consider contributing to this documentation or opening an issue on our GitHub repository.
