// entire file content ...
// ... goes in between

## FormatterFactory

The `FormatterFactory` class is a centralized factory for creating output formatters based on requested formats. It provides a way to instantiate formatters with consistent configuration and supports registration of custom formatter implementations.

### Example Usage

```csharp
using System;
using DotnetMicroOrm.Formatters;

class Program
{
    static void Main()
    {
        var factory = new FormatterFactory();
        var jsonFormatter = factory.GetFormatter(OutputFormat.Json);
        var csvFormatter = factory.GetFormatter(OutputFormat.Csv);

        Console.WriteLine(jsonFormatter.Format(new { Name = "John", Age = 30 }));
        Console.WriteLine(csvFormatter.Format(new { Name = "Jane", Age = 25 }));
    }
}
```

// ... goes in between
