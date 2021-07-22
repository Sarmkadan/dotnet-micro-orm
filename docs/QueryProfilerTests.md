# QueryProfilerTests

Unit‑test class that validates the behavior of the `QueryProfiler` component in the *dotnet-micro-orm* project. Each test method exercises a specific scenario—successful profiling, disabled profiling, failure handling, aggregation, clearing, and eviction policy—ensuring the profiler records durations correctly, respects its enabled flag, and maintains internal limits.

## API

### ProfileAsync_SuccessfulOperation_RecordsProfileWithCorrectDuration
- **Purpose**: Verifies that when `QueryProfiler.ProfileAsync` wraps a successful operation, a profile entry is created and its `Duration` matches the elapsed time within an acceptable tolerance.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if the recorded profile is missing, the duration deviates beyond tolerance, or any assertion inside the test fails (e.g., `AssertionException`).

### ProfileAsync_WhenDisabled_DoesNotRecordProfiles
- **Purpose**: Confirms that when the profiler is disabled, invoking `ProfileAsync` does not create any profile entries, regardless of the operation outcome.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if any profile is recorded while the profiler is disabled or if an assertion fails.

### ProfileAsync_FailingOperation_RecordsFailedProfileAndRethrows
- **Purpose**: Ensures that when the wrapped operation throws an exception, the profiler still records a profile (marked as failed) and then rethrows the original exception.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if the profile is not recorded, the failure flag is not set, or the original exception is not propagated; also throws on assertion failures.

### GetSummary_MultipleQueries_ReturnsCorrectAggregates
- **Purpose**: Validates that `GetSummary` returns correct aggregate statistics (count, total duration, average, min, max) after multiple profiled queries have been recorded.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if the aggregated values do not match the expected totals or if any assertion fails.

### Clear_RemovesAllProfiles
- **Purpose**: Checks that calling `Clear` removes all previously recorded profiles, leaving the profiler in an empty state.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if any profiles remain after `Clear` or if an assertion fails.

### Constructor_MaxProfilesExceeded_EvictsOldEntries
- **Purpose**: Asserts that when the profiler is instantiated with a maximum profile limit and more profiles are added than allowed, the oldest entries are evicted to stay within the limit.
- **Parameters**: None.
- **Return Value**: `Task` – completes when the test finishes.
- **Throws**: Throws if the eviction behavior is incorrect (e.g., wrong entries retained) or if an assertion fails.

## Usage

```csharp
// Example 1: Verify successful operation profiling
var profilerTests = new QueryProfoterTests();
await profilerTests.ProfileAsync_SuccessfulOperation_RecordsProfileWithCorrectDuration();
```

```csharp
// Example 2: Verify that a disabled profiler records nothing
var profilerTests = new QueryProfilerTests();
await profilerTests.ProfileAsync_WhenDisabled_DoesNotRecordProfiles();
```

## Notes

- Each test method is stateless and assumes a fresh instance of `QueryProfilerTests`; calling multiple test methods on the same instance without resetting internal state may lead to unexpected results.
- The class itself does not contain any static or shared mutable state, but the individual test methods are not thread‑safe. Concurrent execution of two or more test methods on the same instance could interfere with each other's assertions and produce nondeterministic outcomes. External synchronization is required if parallel invocation is needed.
- Edge cases covered implicitly by the tests include:
  - Profiling disabled via the profiler's configuration flag.
  - Exceptions thrown by the profiled operation are captured and rethrown.
  - Aggregation calculations ignore incomplete or failed profiles unless explicitly included.
  - The `Clear` method resets all internal collections, ensuring no leakage between test runs.
  - When the maximum number of stored profiles is exceeded, the profiler follows a FIFO eviction policy, discarding the oldest entries first.
