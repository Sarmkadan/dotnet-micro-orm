# QueryProfiler

The `QueryProfiler` is a utility class designed to track and analyze database query execution within the dotnet-micro-orm ecosystem. It enables profiling of asynchronous query operations, collecting metrics such as execution time and query details, which can be used for performance analysis and optimization.

## API

### `public bool IsEnabled`
Gets a value indicating whether profiling is currently enabled. When `false`, profiling operations are skipped, reducing runtime overhead.

### `public QueryProfiler`
Initializes a new instance of the `QueryProfiler` class. This constructor has no parameters and prepares the profiler for collecting query execution data.

### `public async Task<T> ProfileAsync<T>(Func<Task<T>> query, string context = null)`
Executes the provided asynchronous query function while measuring its execution time and capturing details for profiling.

- **Parameters**
  - `query`: A `Func<Task<T>>` representing the asynchronous query operation to profile.
  - `context` (optional): A string providing additional context about the query, such as the method or operation name where the query is executed.

- **Return Value**
  Returns a `Task<T>` that represents the original query operation, ensuring profiling does not alter the expected behavior.

- **Exceptions**
  Throws `ArgumentNullException` if `query` is `null`.

### `public IReadOnlyList<QueryProfile> GetProfiles()`
Retrieves a read-only collection of all recorded query profiles. Each profile contains metadata about a single profiled query execution.

- **Return Value**
  Returns an `IReadOnlyList<QueryProfile>` containing all collected profiling data. The list is immutable to prevent external modification of profiling records.

### `public QueryProfilerSummary GetSummary()`
Generates a summary of all recorded profiling data, aggregating metrics such as total execution time, average query duration, and the number of queries profiled.

- **Return Value**
  Returns a `QueryProfilerSummary` object containing aggregated profiling statistics. If no profiles exist, the summary reflects zero values.

### `public void Clear()`
Removes all recorded profiling data from the current instance. This resets the profiler to its initial state, allowing new queries to be profiled without legacy data interference.

## Usage
