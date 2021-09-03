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

## DotnetMicroOrmException

The `DotnetMicroOrmException` class serves as the base exception for all DotnetMicroOrm-specific errors. It extends the standard `Exception` class with additional properties for error tracking (`ErrorCode`) and contextual information (`ErrorContext`), making it easier to diagnose issues in production environments. The `WithContext` method allows adding custom diagnostic data to exceptions during error handling.


### Example Usage

```csharp
try
{
    var repository = new UserRepository();
    var user = await repository.GetByIdAsync(42);
}
catch (DotnetMicroOrmException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    
    if (ex.ErrorCode != null)
    {
        Console.WriteLine($"Error Code: {ex.ErrorCode}");
    }
    
    if (ex.ErrorContext != null)
    {
        foreach (var kvp in ex.ErrorContext)
        {
            Console.WriteLine($"Context: {kvp.Key} = {kvp.Value}");
        }
    }
}

// Add context to an exception during error handling
try
{
    await SaveUserAsync(user);
}
catch (DotnetMicroOrmException ex)
{
    var detailedEx = ex.WithContext("userId", user.Id)
                      .WithContext("timestamp", DateTime.UtcNow);
    logger.LogError(detailedEx, "Failed to save user");
    throw;
}
```

// ... (rest of the README content remains the same)
