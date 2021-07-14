# CsvFormatter

The `CsvFormatter` class provides methods to convert objects and collections into CSV-formatted strings. It is designed for simple, consistent CSV output without external dependencies, handling common formatting rules such as quoting fields that contain delimiters, newlines, or double-quote characters.

## API

### `public CsvFormatter()`

Initializes a new instance of the `CsvFormatter` class.  
No parameters.  
No return value.  
Does not throw.

### `public string Format(object value)`

Formats a single object into a CSV line.

- **Parameters**  
  `value` – The object to format. Can be `null`.

- **Returns**  
  A string representing the CSV line. If `value` is `null`, returns an empty string.

- **Throws**  
  `InvalidOperationException` – If the object’s properties cannot be read (e.g., reflection fails on a non-public type).

### `public string FormatCollection<T>(IEnumerable<T> collection)`

Formats a collection of objects into a multi-line CSV string, including a header row with property names.

- **Type parameters**  
  `T` – The type of elements in the collection.

- **Parameters**  
  `collection` – The collection to format. Must not be `null`.

- **Returns**  
  A string containing the header row followed by one CSV line per element, separated by newline characters (`\n`). Returns an empty string if the collection is empty.

- **Throws**  
  `ArgumentNullException` – If `collection` is `null`.  
  `InvalidOperationException` – If any element’s properties cannot be read.

### `public string FormatError(Exception exception)`

Formats an exception into a single CSV line containing the exception type, message, and stack trace.

- **Parameters**  
  `exception` – The exception to format. Must not be `null`.

- **Returns**  
  A string representing the CSV line with three fields: `ExceptionType`, `Message`, and `StackTrace`. Each field is quoted if necessary.

- **Throws**  
  `ArgumentNullException` – If `exception` is `null`.

## Usage

### Example 1: Formatting a collection of objects

```csharp
using System.Collections.Generic;
using DotNetMicroOrm;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var people = new List<Person>
{
    new Person { Name = "Alice", Age = 30 },
    new Person { Name = "Bob", Age = 25 }
};

var formatter = new CsvFormatter();
string csv = formatter.FormatCollection(people);
Console.WriteLine(csv);
// Output:
// Name,Age
// Alice,30
// Bob,25
```

### Example 2: Formatting an error and a single object

```csharp
using System;
using DotNetMicroOrm;

var formatter = new CsvFormatter();

// Format an exception
try
{
    throw new InvalidOperationException("Something went wrong");
}
catch (Exception ex)
{
    string errorCsv = formatter.FormatError(ex);
    Console.WriteLine(errorCsv);
    // Example output:
    // "System.InvalidOperationException","Something went wrong","   at ..."
}

// Format a single object
var data = new { Id = 42, Value = "test" };
string line = formatter.Format(data);
Console.WriteLine(line);
// Output:
// 42,test
```

## Notes

- **Null handling** – `Format` accepts `null` and returns an empty string. `FormatCollection<T>` and `FormatError` throw `ArgumentNullException` when their required parameter is `null`.
- **Empty collections** – `FormatCollection<T>` returns an empty string when the collection contains no elements; no header row is emitted.
- **Field quoting** – Fields containing commas, double quotes, newlines, or leading/trailing whitespace are automatically enclosed in double quotes. Embedded double quotes are escaped by doubling them.
- **Property order** – The header row and corresponding CSV fields are ordered according to the reflection order of the type’s public instance properties (typically the declaration order, but not guaranteed across runtimes).
- **Thread safety** – Instances of `CsvFormatter` are not thread-safe. If concurrent formatting is required, each thread should use its own instance or external synchronization must be applied.
