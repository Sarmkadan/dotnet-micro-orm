# ExpressionAndCachingBenchmarks

A benchmarking suite designed to measure the performance impact of expression compilation, caching, and query execution in micro-ORM scenarios. This class evaluates the overhead of first-time expression compilation versus subsequent cached invocations, as well as the efficiency of various query operations (e.g., predicates, ordering, pagination) when executed through expression trees. Results help identify optimization opportunities in expression-heavy ORM workflows.

## API

### `GlobalSetup`
