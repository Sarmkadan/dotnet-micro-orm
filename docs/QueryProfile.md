# QueryProfile

`QueryProfile` is a record type used to capture and analyze query execution metrics in `dotnet-micro-orm`. It tracks individual query performance, success status, and aggregated statistics across multiple queries, enabling profiling and diagnostics for performance tuning.

## API

### `Id`
A unique identifier (`Guid`) assigned to this query profile instance. Used to correlate profiling data with specific operations.

### `Query`
A `string` containing the raw query text executed. May include parameter placeholders or fully interpolated values depending on configuration.

### `Parameters`
An optional `Dictionary<string, object>` mapping parameter names to their values. `null` if no parameters were used. Values are serialized as provided by the ORM.

### `Duration`
A `TimeSpan` representing the wall-clock duration of this specific query execution. Includes network and serialization overhead where applicable.

### `ExecutedAt`
A `DateTime` indicating when the query was initiated. Uses the system clock at the start of execution.

### `Succeeded`
A `bool` indicating whether the query completed without throwing an exception. `true` if successful, `false` otherwise.

### `ErrorMessage`
An optional `string` containing the exception message if `Succeeded` is `false`. `null` if the query succeeded or no exception was captured.

### `CallerMemberName`
An optional `string` containing the name of the calling member (e.g., method or property) where the query was initiated. Used for stack trace correlation. `null` if not provided by the caller.

### `RowsAffected`
An optional `int` indicating the number of rows affected by the query (e.g., for `INSERT`, `UPDATE`, or `DELETE`). `null` for queries that do not return row counts.

### `TotalQueries`
An `int` representing the total number of queries executed in the current profiling session. Always ≥ 1.

### `TotalDuration`
A `TimeSpan` representing the cumulative duration of all queries in the profiling session.

### `AverageDuration`
A `TimeSpan` representing the average duration per query in the profiling session. Calculated as `TotalDuration / TotalQueries`.

### `MaxDuration`
A `TimeSpan` representing the longest query duration observed in the profiling session.

### `MinDuration`
A `TimeSpan` representing the shortest query duration observed in the profiling session.

### `FailedQueries`
An `int` representing the number of queries that failed (i.e., `Succeeded` is `false`) in the profiling session.

### `SlowestQuery`
An optional `QueryProfile` reference to the slowest query in the session. `null` if no queries were executed or all durations are equal.

## Usage
