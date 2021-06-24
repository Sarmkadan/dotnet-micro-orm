# QueryPlan

Represents a cached execution plan for a SQL query, including the parameterized SQL text, estimated cost, metadata about its parameters, and usage statistics. Each plan is uniquely identified by a statement key and optionally a database type, allowing the ORM to reuse compiled query strategies across multiple executions.

## API

### `public required string Fingerprint`
A unique hash or normalized representation of the query structure, used internally to identify semantically identical queries regardless of literal values. This property must be supplied when constructing a `QueryPlan`.

### `public required string Sql`
The parameterized SQL text associated with this plan. Placeholders replace literal values, enabling safe reuse with different parameter sets. This property must be supplied when constructing a `QueryPlan`.

### `public double EstimatedCost`
An approximate relative cost of executing this query as determined by the database engine's planner or an internal heuristic. Higher values indicate more expensive operations. This value is informational and may influence caching or eviction policies.

### `public IReadOnlyList<QueryParameterDescriptor> Parameters`
An ordered, read-only collection of descriptors for each parameter expected by the parameterized `Sql`. Each descriptor includes the parameter name, its database type (`DbType`), and optional size information. The collection is immutable after the plan is created.

### `public DateTime CachedAt`
The UTC timestamp when this plan was first created and inserted into the query cache. Used to determine plan age for cache eviction or staleness checks.

### `public long HitCount`
The total number of times this cached plan has been retrieved from the cache for execution. Incremented by the caching layer on each cache hit; not directly modified by consumers.

### `public required string Name`
A human-readable or system-assigned name for this query plan, often corresponding to the method or operation it represents. This property must be supplied when constructing a `QueryPlan`.

### `public required DbType DbType`
The database provider type (e.g., SQL Server, PostgreSQL, MySQL) for which this plan was compiled. Plans are typically scoped to a specific provider because SQL dialects and parameter handling differ. This property must be supplied when constructing a `QueryPlan`.

### `public int? Size`
An optional size constraint associated with the query or its primary parameter, such as a string length or binary field size. A `null` value indicates no explicit size restriction.

### `public required string StatementKey`
A unique key identifying the logical statement, derived from the query structure and provider type. Used as the primary lookup key in the plan cache. This property must be supplied when constructing a `QueryPlan`.

### `public DateTime LastUsedAt`
The UTC timestamp of the most recent cache hit for this plan. Updated by the caching layer each time the plan is retrieved; `CachedAt` if never reused.

### `public long UseCount`
The number of times this plan has been used for actual query execution. Distinct from `HitCount` in that it may represent executions rather than cache retrievals, depending on the ORM's internal accounting.

## Usage

### Creating and Registering a Query Plan
```csharp
var plan = new QueryPlan
{
    Fingerprint = "a3f8b2c1d4e5",
    Sql = "SELECT Id, Name FROM Users WHERE Email = @Email",
    EstimatedCost = 1.2,
    Parameters = new List<QueryParameterDescriptor>
    {
        new QueryParameterDescriptor
        {
            Name = "@Email",
            DbType = DbType.String,
            Size = 256
        }
    }.AsReadOnly(),
    CachedAt = DateTime.UtcNow,
    HitCount = 0,
    Name = "GetUserByEmail",
    DbType = DbType.SqlServer,
    Size = null,
    StatementKey = "sqlserver:getuserbyemail",
    LastUsedAt = DateTime.UtcNow,
    UseCount = 0
};

queryCache.Add(plan.StatementKey, plan);
```

### Retrieving and Using a Cached Plan
```csharp
if (queryCache.TryGetValue("sqlserver:getuserbyemail", out var cachedPlan))
{
    // Update usage metadata
    cachedPlan.LastUsedAt = DateTime.UtcNow;
    cachedPlan.HitCount++;
    cachedPlan.UseCount++;

    // Execute using the parameterized SQL and parameter descriptors
    using var command = connection.CreateCommand();
    command.CommandText = cachedPlan.Sql;
    command.Parameters.Add(new SqlParameter
    {
        ParameterName = "@Email",
        SqlDbType = SqlDbType.NVarChar,
        Size = 256,
        Value = "user@example.com"
    });

    using var reader = command.ExecuteReader();
    // Process results...
}
```

## Notes

- **Immutability of Parameters**: The `Parameters` collection is exposed as `IReadOnlyList<QueryParameterDescriptor>`. While the list itself cannot be modified, the individual descriptors within it may be mutable depending on their implementation. Treat them as read-only after plan creation to avoid inconsistent caching behavior.
- **Thread Safety**: `QueryPlan` is not inherently thread-safe. Properties such as `HitCount`, `UseCount`, and `LastUsedAt` are mutable and may be updated concurrently by multiple threads retrieving the same cached plan. External synchronization (e.g., using `Interlocked.Increment` for counters or locking around updates) is required in multi-threaded scenarios.
- **Required Members**: All properties marked with the `required` keyword must be initialized during object construction. Failure to do so results in a compile-time error. This ensures every cached plan has a valid fingerprint, SQL text, name, database type, and statement key.
- **Size Semantics**: The `Size` property on `QueryPlan` represents a plan-level size constraint, while individual `QueryParameterDescriptor` entries may also carry their own `Size`. These can differ; the descriptor-level size typically takes precedence when configuring command parameters.
- **Cost Volatility**: `EstimatedCost` is a snapshot taken at plan creation time. Actual execution costs may diverge over time due to data volume changes or index modifications. Consider periodic plan recompilation for long-lived applications.
- **Timestamp Granularity**: `CachedAt` and `LastUsedAt` use `DateTime` (not `DateTimeOffset`), implying UTC values. Ensure consistency by always assigning `DateTime.UtcNow` to avoid time zone ambiguity across distributed cache layers.
