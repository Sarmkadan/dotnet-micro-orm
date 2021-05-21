# DotnetMicroOrmException

`DotnetMicroOrmException` is the specialized exception type utilized by the `dotnet-micro-orm` library to provide robust, structured error reporting for database and ORM operations. It enables developers to distinguish between different failure scenarios through a dedicated `ErrorCode` property and provides diagnostic capabilities by allowing the attachment of arbitrary contextual data via a `Dictionary<string, object>` accessible through `ErrorContext`.

## API

### `ErrorCode`
*   **Purpose:** Gets the string-based error code associated with this exception, allowing for programmatic handling of specific error conditions.
*   **Type:** `string?`

### `ErrorContext`
*   **Purpose:** Gets a dictionary containing additional key-value pairs that provide context about the failure, such as database query parameters, entity identifiers, or table names.
*   **Type:** `Dictionary<string, object>?`

### `DotnetMicroOrmException`
*   **Purpose:** Initializes a new instance of the `DotnetMicroOrmException` class.
*   **Usage:** Used to throw exceptions when a database operation fails or when library invariants are violated.

### `WithContext`
*   **Purpose:** A fluent builder method that adds a key-value pair to the `ErrorContext` and returns the same exception instance, allowing for a chainable configuration of exception details at the point of throwing.
*   **Parameters:**
    *   `key`: The string key for the diagnostic data.
    *   `value`: The object value to associate with the key.
*   **Return Value:** The current `DotnetMicroOrmException` instance.

### `ToString`
*   **Purpose:** Returns a string representation of the exception, including the message, stack trace, `ErrorCode`, and a formatted representation of the `ErrorContext` to aid in debugging.
*   **Return Value:** A string containing the diagnostic information.

## Usage

### Throwing with a Specific Error Code
```csharp
if (string.IsNullOrEmpty(entityId))
{
    throw new DotnetMicroOrmException("Entity ID cannot be null or empty.")
    {
        ErrorCode = "VALIDATION_FAILED"
    };
}
```

### Fluent Exception Building
```csharp
try
{
    // Database operation logic
}
catch (Exception ex)
{
    throw new DotnetMicroOrmException("Database connection failure", ex)
        .WithContext("Server", "prod-db-01")
        .WithContext("QueryTimeout", 30);
}
```

## Notes

*   **Thread-Safety:** `DotnetMicroOrmException` instances are generally intended to be immutable once thrown. While `WithContext` modifies the `ErrorContext` dictionary, it should only be called during the exception object's construction or before it is thrown.
*   **Serialization:** As it inherits from `System.Exception`, `DotnetMicroOrmException` is serializable. Ensure that the objects stored in `ErrorContext` are also serializable if this exception is expected to cross process or application domain boundaries.
*   **Error Context:** Use the `ErrorContext` sparingly to avoid overhead. Store only essential diagnostic information required to reproduce or troubleshoot the reported issue.
