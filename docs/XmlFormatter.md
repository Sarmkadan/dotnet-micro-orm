# XmlFormatter

A utility class for serializing .NET objects and collections into XML strings, typically used for logging, diagnostics, or inter-process communication scenarios where a compact, human-readable format is desired.

## API

### `public XmlFormatter()`

Initializes a new instance of the `XmlFormatter` class with default settings.

### `public string Format(object? value)`

Serializes the given object into an XML string.

- **Parameters**
  - `value` – The object to serialize; may be `null`, in which case the method returns an empty string.
- **Return value**
  - A string containing the XML representation of `value`.
- **Exceptions**
  - Throws `System.InvalidOperationException` when the object graph contains types that cannot be serialized to XML (e.g., anonymous types or types lacking parameterless constructors).

### `public string FormatCollection<T>(IEnumerable<T> collection)`

Serializes an entire collection into a single XML string with a root element named `Collection`.

- **Type parameters**
  - `T` – The type of elements in the collection.
- **Parameters**
  - `collection` – The collection to serialize; may be `null`, in which case the method returns an empty string.
- **Return value**
  - A string containing the XML representation of the collection, wrapped in a `<Collection>` root element.
- **Exceptions**
  - Throws `System.InvalidOperationException` when any element in the collection cannot be serialized to XML.

### `public string FormatError(Exception exception)`

Serializes an exception (including its stack trace and inner exceptions) into an XML string.

- **Parameters**
  - `exception` – The exception to serialize; may be `null`, in which case the method returns an empty string.
- **Return value**
  - A string containing the XML representation of the exception, including `<Message>`, `<StackTrace>`, and `<InnerException>` elements.
- **Exceptions**
  - Throws `System.ArgumentNullException` when `exception` is `null`.

## Usage
