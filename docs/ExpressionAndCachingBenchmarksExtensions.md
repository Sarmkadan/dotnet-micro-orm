# ExpressionAndCachingBenchmarksExtensions

The `ExpressionAndCachingBenchmarksExtensions` class provides a set of static utility methods designed to facilitate the creation, manipulation, and inspection of LINQ expression trees within the context of micro-ORM benchmarking scenarios. These helpers streamline the generation of parameterized predicates and property accessors while offering low-level utilities for cloning expressions and extracting body nodes, ensuring consistent expression handling across performance tests without the overhead of repeated compilation or manual tree construction.

## API

### `CreateExpression<T>`
Generates a simple lambda expression that accesses a specific member or constant value of type `T`.
*   **Purpose**: Constructs a foundational `Expression<Func<T>>` typically used to represent property accessors or constant selectors in benchmark setups.
*   **Parameters**: None (generic type `T` defines the return type).
*   **Return Value**: An `Expression<Func<T>>` representing the generated lambda.
*   **Exceptions**: May throw `ArgumentException` if `T` is an invalid type for the underlying member resolution logic or if the expression tree cannot be constructed.

### `CreateComplexExpression<T>`
Constructs a lambda expression containing a more intricate logical predicate involving type `T`.
*   **Purpose**: Creates an `Expression<Func<T, bool>>` suitable for testing filter performance, often combining multiple conditions or nested property checks.
*   **Parameters**: None (generic type `T` defines the input parameter type).
*   **Return Value**: An `Expression<Func<T, bool>>` representing the complex predicate.
*   **Exceptions**: May throw `ArgumentException` if the internal logic fails to resolve members required for the complex predicate on type `T`.

### `CloneExpression`
Produces a deep copy of an existing expression tree.
*   **Purpose**: Creates a duplicate of a provided `Expression` object, ensuring that modifications to the clone do not affect the original tree structure. This is critical for benchmarks requiring isolated expression instances.
*   **Parameters**: Takes an `Expression` instance as the source to clone.
*   **Return Value**: A new `Expression` object that is a structural copy of the input.
*   **Exceptions**: Throws `ArgumentNullException` if the input expression is `null`.

### `GetBody`
Extracts the body node from a lambda expression.
*   **Purpose**: Unwraps a `LambdaExpression` to return its underlying `Expression` body, simplifying access to the core logic without the parameter definition wrapper.
*   **Parameters**: Takes an `Expression` (expected to be a `LambdaExpression`) as input.
*   **Return Value**: The `Expression` representing the body of the lambda.
*   **Exceptions**: Throws `InvalidOperationException` or `InvalidCastException` if the provided expression is not a `LambdaExpression` or if the body is inaccessible.

## Usage

### Generating Benchmark Predicates
The following example demonstrates how to generate both simple and complex expressions for use in a micro-ORM filtering benchmark.

```csharp
using System;
using System.Linq.Expressions;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
}

public class BenchmarkSetup
{
    public void InitializeExpressions()
    {
        // Create a simple expression selecting a user instance (or property accessor)
        Expression<Func<User>> simpleSelector = ExpressionAndCachingBenchmarksExtensions.CreateExpression<User>();

        // Create a complex predicate for filtering active users with specific criteria
        Expression<Func<User, bool>> complexFilter = ExpressionAndCachingBenchmarksExtensions.CreateComplexExpression<User>();

        // The expressions are now ready to be compiled or passed to the ORM query provider
        Console.WriteLine($"Simple Expression: {simpleSelector}");
        Console.WriteLine($"Complex Filter: {complexFilter}");
    }
}
```

### Cloning and Inspecting Expression Trees
This example illustrates how to safely clone an expression for isolated testing and extract its body for analysis.

```csharp
using System;
using System.Linq.Expressions;

public class ExpressionAnalysis
{
    public void AnalyzeExpressionStructure()
    {
        // Generate a base complex expression
        var originalExpression = ExpressionAndCachingBenchmarksExtensions.CreateComplexExpression<User>();

        // Clone the expression to ensure mutations in one test run do not affect others
        Expression clonedExpression = ExpressionAndCachingBenchmarksExtensions.CloneExpression(originalExpression);

        // Extract the body of the lambda to inspect the raw logical tree
        Expression bodyNode = ExpressionAndCachingBenchmarksExtensions.GetBody(originalExpression);

        // Verify the structure
        if (bodyNode is BinaryExpression binary)
        {
            Console.WriteLine($"Detected binary operation in body: {binary.NodeType}");
        }
    }
}
```

## Notes

*   **Thread Safety**: As the methods are static and primarily rely on constructing new expression tree nodes rather than maintaining mutable static state, they are generally thread-safe for concurrent invocation. However, the returned `Expression` instances are immutable by design of the .NET expression tree API, making them safe to share across threads once created.
*   **Input Validation**: The `GetBody` method strictly requires a `LambdaExpression` as input. Passing a raw `MemberExpression` or `ConstantExpression` directly to this method will result in a runtime cast failure or invalid operation exception.
*   **Generic Constraints**: The generic methods `CreateExpression<T>` and `CreateComplexExpression<T>` rely on reflection to resolve members of `T`. If `T` is an interface, abstract class, or a type lacking the expected properties assumed by the internal benchmark logic, an `ArgumentException` may be thrown at runtime.
*   **Cloning Depth**: The `CloneExpression` utility performs a deep copy of the tree structure. While the nodes are duplicated, any external objects referenced within constant expressions inside the tree (e.g., closed-over variables) will still reference the same underlying objects, consistent with standard expression tree behavior.
