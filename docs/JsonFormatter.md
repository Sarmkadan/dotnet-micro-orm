# JsonFormatter

The `JsonFormatter` class provides a lightweight, dependency-free mechanism for serializing .NET objects, collections, and exception details into JSON strings. Designed specifically for the `dotnet-micro-orm` ecosystem, it facilitates efficient logging, debugging, and error reporting without requiring external JSON libraries like Newtonsoft.Json or System.Text.Json.

## API

### `public JsonFormatter`
Initializes a new instance of the `JsonFormatter` class. This constructor requires no parameters and prepares the formatter for immediate use. It does not perform any I/O operations or allocate significant resources upon instantiation.

### `public string Format(object? value)`
Serializes a single object instance into its JSON string representation.
*   **Parameters**:
    *   `value`: The object to serialize. Can be `null`, a primitive, a complex POCO, or an anonymous type.
*   **Returns**: A `string` containing the JSON representation of the input object. If `value` is `null`, returns the string `"null"`.
*   **Throws**: May throw a `NotSupportedException` if the object type contains circular references or unsupported types that cannot be traversed by the internal reflection logic.

### `public string FormatCollection<T>(IEnumerable<T> collection)`
Serializes a sequence of items into a JSON array string.
*   **Parameters**:
    *   `collection`: An `IEnumerable<T>` containing the items to serialize.
*   **Returns**: A `string` representing a JSON array (e.g., `"[item1,item2]"`). If the collection is `null`, returns `"null"`. If the collection is empty, returns `"[]"`.
*   **Throws**: May throw a `NotSupportedException` if an element within the collection cannot be serialized.

### `public string FormatError(Exception exception)`
Generates a JSON string specifically tailored for exception details, typically including the message, type, and stack trace.
*   **Parameters**:
    *   `exception`: The `Exception` instance to serialize.
*   **Returns**: A `string` containing the structured JSON representation of the error details.
*   **Throws**: Throws `ArgumentNullException` if the provided `exception` is `null`.

## Usage

### Serializing a Query Result Object
The following example demonstrates how to format a single domain object returned from a micro-ORM query for logging purposes.

```csharp
using MicroOrm;

public class User 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public void LogUserRetrieval()
{
    var formatter = new JsonFormatter();
    var user = new User 
    { 
        Id = 42, 
        Name = "Alice", 
        CreatedAt = DateTime.UtcNow 
    };

    // Converts the user object to a JSON string for the log output
    string jsonPayload = formatter.Format(user);
    
    Console.WriteLine($"Retrieved user: {jsonPayload}");
}
```

### Formatting a Batch of Errors
This example illustrates formatting a list of exceptions encountered during a batch database operation into a single JSON array for error reporting.

```csharp
using System;
using System.Collections.Generic;
using MicroOrm;

public void ReportBatchErrors(List<Exception> errors)
{
    var formatter = new JsonFormatter();
    
    if (errors.Count == 0) 
    {
        return;
    }

    // Serialize the entire collection of exceptions into a JSON array
    string errorReport = formatter.FormatCollection(errors);
    
    // Alternatively, format a specific critical error with full stack trace details
    string criticalErrorDetail = formatter.FormatError(errors[0]);

    SendToMonitoringService(errorReport, criticalErrorDetail);
}
```

## Notes

*   **Thread Safety**: The `JsonFormatter` class is stateless regarding the data being processed; however, the instance itself should be considered thread-safe for read-only operations (calling `Format` methods) as it does not maintain mutable internal state between calls. Multiple threads may safely call formatting methods on the same instance concurrently.
*   **Circular References**: The internal serialization logic does not automatically detect or handle circular object references. Attempting to format an object graph containing cycles will result in a `NotSupportedException` or a stack overflow depending on the depth. Ensure object graphs are acyclic before formatting.
*   **Null Handling**: All methods explicitly handle `null` inputs by returning the literal string `"null"`, except for `FormatError`, which enforces a non-null constraint via `ArgumentNullException`.
*   **Precision**: Date and time values are serialized using the ISO 8601 standard (`o` format specifier) to ensure compatibility across different parsing environments. Floating-point numbers are serialized using invariant culture settings.
