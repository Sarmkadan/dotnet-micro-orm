# ValidationExample

The `ValidationExample` class serves as a demonstrative utility within the `dotnet-micro-orm` project, illustrating the implementation of asynchronous validation workflows. It encapsulates the state of a validation operation, exposing the final validity status and a collection of descriptive error messages, while providing an entry point for execution via an asynchronous run method and a standard console application main function.

## API

### `public ValidationExample`
Initializes a new instance of the `ValidationExample` class. This constructor sets up the internal state required to track validation results, ensuring that the `Errors` list is instantiated and the `IsValid` property is set to its default state prior to execution.

### `public async Task RunAsync`
Executes the core validation logic asynchronously. This method performs the necessary checks against the target data or configuration.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` that completes when the validation process finishes. The result of the operation is reflected in the updated `IsValid` property and the populated `Errors` list.
*   **Exceptions**: May throw exceptions if underlying asynchronous operations fail or if critical system resources are unavailable during the check.

### `public bool IsValid`
Gets a value indicating whether the validation executed by `RunAsync` was successful.
*   **Return Value**: `true` if no validation errors were encountered; otherwise, `false`. This property should only be accessed after `RunAsync` has completed.

### `public List<string> Errors`
Gets the collection of error messages generated during the validation process.
*   **Return Value**: A `List<string>` containing descriptive messages for each validation failure. If `IsValid` is `true`, this list will be empty. The list is mutable but should typically be treated as read-only after `RunAsync` completes.

### `public static async Task Main`
The static entry point for the application or demonstration script.
*   **Parameters**: None (assumes standard `string[] args` overload handling internally or none required for this specific signature).
*   **Return Value**: Returns a `Task` representing the asynchronous operation of the entire program flow.
*   **Purpose**: Instantiates `ValidationExample`, invokes `RunAsync`, and outputs the results (status and errors) to the console or configured logger.

## Usage

**Example 1: Programmatic Validation Check**
This example demonstrates instantiating the class, running the validation, and branching logic based on the `IsValid` property.

```csharp
using DotNetMicroOrm;

var validator = new ValidationExample();
await validator.RunAsync();

if (validator.IsValid)
{
    Console.WriteLine("Validation passed successfully.");
}
else
{
    Console.WriteLine($"Validation failed with {validator.Errors.Count} errors:");
    foreach (var error in validator.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

**Example 2: Execution via Entry Point**
This example shows how the class is executed directly as a standalone tool or test harness via its `Main` method.

```csharp
// Typically invoked by the runtime, but can be called explicitly for testing
// within a controlled environment if access modifiers allow.

await ValidationExample.Main();

// Post-execution inspection would require an instance if state is not static,
// but Main typically handles its own output reporting.
```

## Notes

*   **Execution Order**: The `IsValid` and `Errors` properties reflect the state of the *last* execution of `RunAsync`. Accessing these properties before awaiting `RunAsync` will return the default initialized state (typically `false` for `IsValid` and an empty list for `Errors`), which may not represent the actual data status.
*   **Thread Safety**: The `Errors` property exposes a `List<string>`, which is not thread-safe for concurrent writes. While `RunAsync` is designed to be awaited, care should be taken not to modify the `Errors` list from multiple threads simultaneously while a validation operation is in progress or being read.
*   **Statefulness**: `ValidationExample` is stateful. Reusing the same instance for multiple validation runs will overwrite the previous `Errors` list and `IsValid` status. For parallel validation of different datasets, distinct instances of `ValidationExample` must be created.
*   **Asynchronous Context**: As `RunAsync` returns a `Task`, it must be awaited. Blocking on this task (e.g., using `.Result` or `.Wait()`) in a UI or ASP.NET context may lead to deadlocks.
