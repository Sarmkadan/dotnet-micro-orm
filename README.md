// ... (rest of the README content remains the same)

## CsvFormatterExtensions

The `CsvFormatterExtensions` class provides a set of extensions for formatting data into CSV strings. It allows you to easily format dictionaries, dynamic collections, and objects with specific properties.

### Example Usage

```csharp
var person = new { Name = "John Doe", Age = 30 };
var formatted = CsvFormatterExtensions.FormatWithProperties(person);
Console.WriteLine(formatted); // Output: Name,Age
                              //          John Doe,30

var data = new List<dynamic> { new { Name = "John Doe", Age = 30 }, new { Name = "Jane Doe", Age = 25 } };
var formattedCollection = CsvFormatterExtensions.FormatDynamicCollection(data);
Console.WriteLine(formattedCollection); // Output: Name,Age
                                        //          John Doe,30
                                        //          Jane Doe,25

var dictionary = new Dictionary<string, string> { { "Name", "John Doe" }, { "Age", "30" } };
var formattedDictionary = CsvFormatterExtensions.FormatDictionary(dictionary);
Console.WriteLine(formattedDictionary); // Output: Name,Age
                                        //          John Doe,30

var tsv = CsvFormatterExtensions.FormatAsTsv(person);
Console.WriteLine(tsv); // Output: Name    Age
                       //          John Doe    30

var psv = CsvFormatterExtensions.FormatAsPsv(person);
Console.WriteLine(psv); // Output: Name|Age
                       //          John Doe|30
```

// ... (rest of the README content remains the same)
