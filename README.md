// ... (rest of the README content remains the same)

## OrmException

The `OrmException` class is the base exception for all ORM-related errors. It provides a way to handle and propagate errors that occur during database operations.

### Example Usage

```csharp
try
{
    // Perform a database operation
    var repository = new MyRepository();
    var entity = await repository.GetByIdAsync(1);
}
catch (OrmException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex is EntityValidationException eve)
    {
        Console.WriteLine($"Validation errors: {string.Join(", ", eve.ValidationErrors)}");
    }
    else if (ex is DatabaseConnectionException dce)
    {
        Console.WriteLine("Database connection error");
    }
    else if (ex is QueryExecutionException qee)
    {
        Console.WriteLine($"Query execution error: {qee.Query}");
    }
}
```

// ... (rest of the README content remains the same)
