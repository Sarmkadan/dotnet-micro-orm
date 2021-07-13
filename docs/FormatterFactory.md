# FormatterFactory

The `FormatterFactory` class serves as a central registry and provider for output formatters in the `dotnet-micro-orm` project. It manages the creation, registration, and retrieval of formatters responsible for converting data into specific output formats (e.g., console, file) and handles formatting operations for objects, collections, and errors.

## API

### `public FormatterFactory()`
Constructs a new instance of `FormatterFactory`. Initializes default formatters (console and file) and registers them internally.

**Parameters**: None.
**Returns**: A new `FormatterFactory` instance.
**Throws**: None.

---

### `public IOutputFormatter GetFormatter()`
Retrieves the default formatter. The default formatter is determined by the factory's internal logic, typically the first registered formatter if no explicit default is set.

**Parameters**: None.
**Returns**: An `IOutputFormatter` instance representing the default formatter.
**Throws**: `InvalidOperationException` if no formatters are registered.

---

### `public IOutputFormatter GetFormatterByContentType(string contentType)`
Retrieves a formatter registered for the specified content type.

**Parameters**:
- `contentType` (`string`): The content type identifier (e.g., `"text/plain"`, `"application/json"`).

**Returns**: An `IOutputFormatter` instance associated with the given content type.
**Throws**:
- `ArgumentNullException` if `contentType` is `null` or empty.
- `KeyNotFoundException` if no formatter is registered for the specified content type.

---

### `public void RegisterFormatter(string contentType, IOutputFormatter formatter)`
Registers a formatter with the specified content type. Overwrites any existing formatter for the same content type.

**Parameters**:
- `contentType` (`string`): The content type identifier to associate with the formatter.
- `formatter` (`IOutputFormatter`): The formatter instance to register.

**Returns**: Void.
**Throws**:
- `ArgumentNullException` if `contentType` or `formatter` is `null`.

---

### `public IEnumerable<OutputFormat> GetRegisteredFormats()`
Retrieves all registered content types and their associated formatters.

**Parameters**: None.
**Returns**: An `IEnumerable<OutputFormat>` where each `OutputFormat` contains a `ContentType` (`string`) and `Formatter` (`IOutputFormatter`).
**Throws**: None.

---

### `public IOutputFormatter CreateConsoleFormatter()`
Creates and returns a new console-specific formatter. The formatter is not automatically registered.

**Parameters**: None.
**Returns**: An `IOutputFormatter` instance configured for console output.
**Throws**: None.

---

### `public IOutputFormatter CreateFileFormatter()`
Creates and returns a new file-specific formatter. The formatter is not automatically registered.

**Parameters**: None.
**Returns**: An `IOutputFormatter` instance configured for file output.
**Throws**: None.

---

### `public string Format(object value)`
Formats a single object using the default formatter.

**Parameters**:
- `value` (`object`): The object to format. Can be `null`.

**Returns**: A `string` representing the formatted output.
**Throws**:
- `InvalidOperationException` if no default formatter is available.
- Exceptions thrown by the underlying formatter (e.g., serialization errors).

---

### `public string FormatCollection<T>(IEnumerable<T> collection)`
Formats a collection of objects using the default formatter. Each item in the collection is formatted individually, and the results are combined.

**Parameters**:
- `collection` (`IEnumerable<T>`): The collection to format. Can be `null` or empty.

**Returns**: A `string` representing the formatted collection.
**Throws**:
- `InvalidOperationException` if no default formatter is available.
- Exceptions thrown by the underlying formatter.

---

### `public string FormatError(Exception exception)`
Formats an exception into a human-readable error message using the default formatter.

**Parameters**:
- `exception` (`Exception`): The exception to format. Cannot be `null`.

**Returns**: A `string` representing the formatted error message.
**Throws**:
- `ArgumentNullException` if `exception` is `null`.
- `InvalidOperationException` if no default formatter is available.
- Exceptions thrown by the underlying formatter.

## Usage

### Example 1: Registering and Using a Custom Formatter
