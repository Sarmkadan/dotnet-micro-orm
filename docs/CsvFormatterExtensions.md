# CsvFormatterExtensions

Provides static extension methods for formatting collections of data into CSV, TSV, PSV, or custom-delimited string representations. These methods are designed to produce flat-file outputs from in-memory data structures such as dictionaries, dynamic objects, or strongly-typed collections, commonly used with the dotnet-micro-orm library for export or logging purposes.

## API

### `public static string FormatDictionary`

Formats a collection of dictionaries into a CSV string. Each dictionary represents a row, with keys used as column headers (first row) and values as cell data.

- **Parameters**  
  `data` – An `IEnumerable<IDictionary<string, object>>` containing the rows.  
  (Optional) Configuration for delimiter, quoting, or header inclusion may be accepted via overloads.

- **Returns**  
  A `string` containing the formatted CSV output.

- **Throws**  
  `ArgumentNullException` if `data` is `null`.  
  `InvalidOperationException` if any dictionary contains a key that is not a valid column name (e.g., empty or contains the delimiter).

### `public static string FormatDynamicCollection`

Formats a collection of `dynamic` objects into a CSV string. Property names of the first object are used as column headers; subsequent objects must expose the same set of properties.

- **Parameters**  
  `data` – An `IEnumerable<dynamic>` containing the rows.

- **Returns**  
  A `string` containing the formatted CSV output.

- **Throws**  
  `ArgumentNullException` if `data` is `null`.  
  `InvalidOperationException` if the collection is empty or if any object lacks a property present in the first object.

### `public static string FormatWithProperties`

Formats a collection of strongly-typed objects into a CSV string using their public readable properties. Property names become column headers.

- **Parameters**  
  `data` – An `IEnumerable<T>` containing the rows.  
  (Optional) A selector or attribute to filter which properties are included.

- **Returns**  
  A `string` containing the formatted CSV output.

- **Throws**  
  `ArgumentNullException` if `data` is `null`.  
  `InvalidOperationException` if the collection is empty or if any object has a property that cannot be read.

### `public static string FormatWithDelimiter<T>`

Formats a collection of objects of type `T` into a delimited string using a custom delimiter. Property names of `T` are used as headers.

- **Parameters**  
  `data` – An `IEnumerable<T>` containing the rows.  
  `delimiter` – A `string` or `char` specifying the field separator.

- **Returns**  
  A `string` containing the formatted delimited output.

- **Throws**  
  `ArgumentNullException` if `data` or `delimiter` is `null`.  
  `ArgumentException` if `delimiter` is empty or contains characters that conflict with quoting rules.

### `public static string FormatAsTsv<T>`

Formats a collection of objects of type `T` into a tab-separated values (TSV) string. Equivalent to calling `FormatWithDelimiter<T>` with a tab character.

- **Parameters**  
  `data` – An `IEnumerable<T>` containing the rows.

- **Returns**  
  A `string` containing the formatted TSV output.

- **Throws**  
  `ArgumentNullException` if `data` is `null`.

### `public static string FormatAsPsv<T>`

Formats a collection of objects of type `T` into a pipe-separated values (PSV) string. Equivalent to calling `FormatWithDelimiter<T>` with a pipe character (`|`).

- **Parameters**  
  `data` – An `IEnumerable<T>` containing the rows.

- **Returns**  
  A `string` containing the formatted PSV output.

- **Throws**  
  `ArgumentNullException` if `data` is `null`.

## Usage

### Example 1: Exporting a list of dictionaries to CSV

```csharp
using DotNetMicroOrm;
using System.Collections.Generic;

var records = new List<Dictionary<string, object>>
{
    new() { ["Id"] = 1, ["Name"] = "Alice", ["Score"] = 95.5 },
    new() { ["Id"] = 2, ["Name"] = "Bob",   ["Score"] = 87.3 }
};

string csv = CsvFormatterExtensions.FormatDictionary(records);
Console.WriteLine(csv);
// Output:
// Id,Name,Score
// 1,Alice,95.5
// 2,Bob,87.3
```

### Example 2: Formatting a POCO collection as TSV

```csharp
using DotNetMicroOrm;
using System.Collections.Generic;

public class Product
{
    public int ProductId { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

var products = new List<Product>
{
    new() { ProductId = 101, Description = "Widget", Price = 9.99m },
    new() { ProductId = 102, Description = "Gadget", Price = 24.99m }
};

string tsv = CsvFormatterExtensions.FormatAsTsv(products);
Console.WriteLine(tsv);
// Output:
// ProductId	Description	Price
// 101	Widget	9.99
// 102	Gadget	24.99
```

## Notes

- **Null values** – If a cell value is `null`, the output contains an empty field. No exception is thrown.
- **Special characters** – Values containing the delimiter, double-quotes, or newlines are automatically quoted according to RFC 4180 rules (for CSV) or equivalent escaping for TSV/PSV.
- **Empty collections** – All methods throw `InvalidOperationException` when the input collection is empty, because no header row can be inferred.
- **Thread safety** – These are static methods that operate only on their input parameters. They do not modify any shared state and are safe to call concurrently from multiple threads.
- **Property order** – For `FormatWithProperties`, `FormatWithDelimiter<T>`, `FormatAsTsv<T>`, and `FormatAsPsv<T>`, the order of columns follows the order of public properties as returned by reflection (typically declaration order). For `FormatDictionary` and `FormatDynamicCollection`, the order is determined by the keys of the first dictionary or the properties of the first dynamic object, respectively.
