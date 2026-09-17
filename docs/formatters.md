# Formatters

The `dotnet-micro-orm` formatters provide a flexible system for outputting data in various formats (JSON, CSV, XML, Markdown, plain text) through a common `IOutputFormatter` interface. Each formatter handles serialization with proper escaping, formatting, and error handling.

## API Overview

### `IOutputFormatter`
Interface defining the contract for all formatters:
- `string ContentType { get; }` - MIME type of the formatted output
- `string Format(object? data)` - Formats a single object
- `string FormatCollection<T>(IEnumerable<T> items)` - Formats a collection of objects
- `string FormatError(string code, string message, string requestId)` - Formats error responses

### `OutputFormat`
Enum specifying supported formats:
- `Json` - JSON format
- `Csv` - Comma-separated values
- `Xml` - XML format
- `PlainText` - Plain text

### `FormatterFactory`
Factory for creating formatter instances with centralized configuration and custom formatter registration.

## JsonFormatter

The `JsonFormatter` class provides JSON serialization using `System.Text.Json` with camelCase property naming, indented output by default, and enum serialization as strings.

### Features
- Configurable indentation (default: true)
- CamelCase property naming
- Null value omission (`JsonIgnoreCondition.WhenWritingNull`)
- Enum serialization as strings
- Error formatting with code, message, requestId, and timestamp

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

// Single object formatting
var jsonFormatter = new JsonFormatter(indented: true);
var user = new User 
{ 
    Id = 1, 
    Name = "John Doe", 
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

string json = jsonFormatter.Format(user);
// Output:
// {
//   "id": 1,
//   "name": "John Doe",
//   "createdAt": "2026-09-17T10:30:00.0000000Z",
//   "isActive": true
// }

// Collection formatting
var users = new List<User> { user };
string jsonArray = jsonFormatter.FormatCollection(users);
// Output:
// [
//   {
//     "id": 1,
//     "name": "John Doe",
//     "createdAt": "2026-09-17T10:30:00.0000000Z",
//     "isActive": true
//   }
// ]

// Error formatting
string errorJson = jsonFormatter.FormatError("VALIDATION_ERROR", "Invalid email format", "req-123");
// Output:
// {
//   "code": "VALIDATION_ERROR",
//   "message": "Invalid email format",
//   "requestId": "req-123",
//   "timestamp": "2026-09-17T10:30:00.0000000Z"
// }
```

## CsvFormatter

The `CsvFormatter` class outputs data in CSV format with proper field escaping, header inclusion, and support for custom delimiters.

### Features
- Automatic quoting of fields containing commas, quotes, or newlines
- Configurable delimiter (default: comma)
- Optional header row (default: included)
- Proper escaping of special characters
- DateTime serialization in ISO 8601 format
- Boolean serialization as "true"/"false"

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime ReleaseDate { get; set; }
}

// Single object with header
var csvFormatter = new CsvFormatter();
var product = new Product 
{ 
    Id = 101, 
    Name = "Laptop, Pro Edition", 
    Price = 1299.99m,
    ReleaseDate = new DateTime(2026, 9, 15)
};

string csv = csvFormatter.Format(product);
// Output:
// Id,Name,Price,ReleaseDate
// 101,"Laptop, Pro Edition",1299.99,2026-09-15T00:00:00.0000000Z

// Collection formatting
var products = new List<Product> { product };
string csvCollection = csvFormatter.FormatCollection(products);
// Output:
// Id,Name,Price,ReleaseDate
// 101,"Laptop, Pro Edition",1299.99,2026-09-15T00:00:00.0000000Z

// Error formatting
string errorCsv = csvFormatter.FormatError("PROCESSING_ERROR", "Price cannot be negative", "req-456");
// Output:
// Error Code,Message,Request ID,Timestamp
// PROCESSING_ERROR,"Price cannot be negative",req-456,2026-09-17T10:30:00.0000000Z

// Custom delimiter (semicolon)
var csvSemicolon = new CsvFormatter(delimiter: ";");
string csvWithSemicolon = csvSemicolon.Format(product);
// Output:
// Id;Name;Price;ReleaseDate
// 101;"Laptop, Pro Edition";1299.99;2026-09-15T00:00:00.0000000Z
```

## XmlFormatter

The `XmlFormatter` class outputs data in XML format with proper element naming, escaping, and hierarchical structure for complex objects.

### Features
- Configurable root and item element names
- Proper XML escaping of special characters
- Support for nested objects and collections
- Indented output by default
- Null values represented as empty elements
- Error formatting with structured XML elements

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}

// Single object formatting
var xmlFormatter = new XmlFormatter("order", "field");
var order = new Order 
{ 
    Id = 1001, 
    CustomerName = "Jane Smith", 
    TotalAmount = 250.75m,
    OrderDate = DateTime.UtcNow
};

string xml = xmlFormatter.Format(order);
// Output:
// <order>
//   <Id>1001</Id>
//   <CustomerName>Jane Smith</CustomerName>
//   <TotalAmount>250.75</TotalAmount>
//   <OrderDate>2026-09-17T10:30:00.0000000Z</OrderDate>
// </order>

// Collection formatting
var orders = new List<Order> { order };
string xmlCollection = xmlFormatter.FormatCollection(orders);
// Output:
// <order>
//   <Id>1001</Id>
//   <CustomerName>Jane Smith</CustomerName>
//   <TotalAmount>250.75</TotalAmount>
//   <OrderDate>2026-09-17T10:30:00.0000000Z</OrderDate>
// </order>

// Custom element names
var xmlCustom = new XmlFormatter("orders", "orderItem");
string xmlCustomOutput = xmlCustom.FormatCollection(orders);
// Output:
// <orders>
//   <orderItem>
//     <Id>1001</Id>
//     <CustomerName>Jane Smith</CustomerName>
//     <TotalAmount>250.75</TotalAmount>
//     <OrderDate>2026-09-17T10:30:00.0000000Z</OrderDate>
//   </orderItem>
// </orders>

// Error formatting
string errorXml = xmlFormatter.FormatError("DB_ERROR", "Connection timeout", "req-789");
// Output:
// <error>
//   <code>DB_ERROR</code>
//   <message>Connection timeout</message>
//   <requestId>req-789</requestId>
//   <timestamp>2026-09-17T10:30:00.0000000Z</timestamp>
// </error>
```

## MarkdownFormatter

The `MarkdownFormatter` class outputs data as GitHub-flavored Markdown tables, ideal for documentation and readable reports.

### Features
- Single objects displayed as two-column tables (Property | Value)
- Collections displayed as standard Markdown tables with headers
- Proper escaping of pipe characters and newlines in cell values
- Null values rendered as "_(null)_" for single objects or empty cells for collections
- Error formatting in blockquote style with bold error code

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public bool IsAvailable { get; set; }
}

// Single object formatting
var mdFormatter = new MarkdownFormatter();
var book = new Book 
{ 
    Id = 201, 
    Title = "The Great Novel", 
    Author = "John Writer",
    IsAvailable = true
};

string markdown = mdFormatter.Format(book);
// Output:
// | Property   | Value         |
// |------------|---------------|
// | Id         | 201           |
// | Title      | The Great Novel |
// | Author     | John Writer   |
// | IsAvailable| True          |

// Collection formatting
var books = new List<Book> { book };
string markdownTable = mdFormatter.FormatCollection(books);
// Output:
// | Id | Title           | Author      | IsAvailable |
// |----|-----------------|-------------|-------------|
// | 201| The Great Novel | John Writer | True        |

// Special character escaping
var bookWithPipe = new Book 
{ 
    Id = 202, 
    Title = "Book | With | Pipes", 
    Author = "Author\nWith\nNewlines",
    IsAvailable = false
};

string markdownEscaped = mdFormatter.Format(bookWithPipe);
// Output:
// | Property   | Value                 |
// |------------|-----------------------|
// | Id         | 202                   |
// | Title      | Book \| With \| Pipes |
// | Author     | Author With Newlines  |
// | IsAvailable| False                 |

// Error formatting
string errorMd = mdFormatter.FormatError("NOT_FOUND", "Book with ID 999 not found", "req-999");
// Output:
// > **Error NOT_FOUND**: Book with ID 999 not found (Request: req-999)
```

## PlainTextFormatter

The `PlainTextFormatter` class provides simple string representation of objects using `ToString()` method.

### Features
- Uses `object.ToString()` for single objects
- Collections output each item on a new line using `Environment.NewLine`
- Null handling returns "null" for single objects or empty string for collections
- Error formatting in readable text format

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

// Single object formatting
var txtFormatter = new PlainTextFormatter();
var person = new Person { FirstName = "John", LastName = "Doe" };

string plainText = txtFormatter.Format(person);
// Output:
// John Doe

// Collection formatting
var people = new List<Person> 
{ 
    new Person { FirstName = "John", LastName = "Doe" },
    new Person { FirstName = "Jane", LastName = "Smith" }
};

string plainTextCollection = txtFormatter.FormatCollection(people);
// Output:
// John Doe
// Jane Smith

// Error formatting
string errorTxt = txtFormatter.FormatError("AUTH_FAILED", "Invalid credentials", "req-auth");
// Output:
// Error [AUTH_FAILED]: Invalid credentials (Request ID: req-auth)
```

## FormatterFactory

The `FormatterFactory` provides centralized creation and management of formatter instances with support for custom formatter registration.

### Features
- Pre-registered formatters for all standard formats (Json, Csv, Xml, PlainText)
- Content-type based formatter lookup (`application/json`, `text/csv`, etc.)
- Custom formatter registration
- Console and file-specific formatter creation methods
- Extension methods for CSV formatting (TXT, TSV, PSV, dictionary formatting, etc.)

### Example Usage

```csharp
using DotnetMicroOrm.Formatters;

// Basic factory usage
var factory = new FormatterFactory();

// Get formatters by enum
IOutputFormatter jsonFormatter = factory.GetFormatter(OutputFormat.Json);
IOutputFormatter csvFormatter = factory.GetFormatter(OutputFormat.Csv);

// Get formatters by content type
IOutputFormatter jsonFromContent = factory.GetFormatterByContentType("application/json");
IOutputFormatter csvFromContent = factory.GetFormatterByContentType("text/csv");

// Create console formatter (returns JSON formatter with indentation)
IOutputFormatter consoleFormatter = factory.CreateConsoleFormatter();

// Create file formatter based on extension
IOutputFormatter jsonFileFormatter = factory.CreateFileFormatter(".json");
IOutputFormatter csvFileFormatter = factory.CreateFileFormatter(".csv");
IOutputFormatter xmlFileFormatter = factory.CreateFileFormatter(".xml");
IOutputFormatter txtFileFormatter = factory.CreateFileFormatter(".txt");

// Register custom formatter
factory.RegisterFormatter(OutputFormat.Yaml, () => new YamlFormatter());

// Get all registered formats
IEnumerable<OutputFormat> formats = factory.GetRegisteredFormats();

// CSV-specific extensions (via CsvFormatterExtensions)
var csvFormatter = (CsvFormatter)factory.GetFormatter(OutputFormat.Csv);

// Format as TSV (tab-separated)
string tsv = csvFormatter.FormatAsTsv(people);

// Format as PSV (pipe-separated)
string psv = csvFormatter.FormatAsPsv(people);

// Format dictionary as CSV
var dict = new Dictionary<string, object> { {"Name", "John"}, {"Age", 30} };
string dictCsv = csvFormatter.FormatDictionary(dict);
// Output:
// Key,Value
// Name,John
// Age,30
```

## Choosing a Formatter

- **JSON**: Best for API responses, configuration, and structured data interchange
- **CSV**: Ideal for spreadsheets, data exchange, and tabular data reports
- **XML**: Suitable for document storage, configuration, and legacy system integration
- **Markdown**: Perfect for documentation, readable reports, and GitHub-friendly output
- **Plain Text**: Best for logging, simple output, and human-readable debugging

All formatters are stateless and thread-safe for concurrent use. Error formatting is consistent across all formatters, providing structured error information regardless of output format.