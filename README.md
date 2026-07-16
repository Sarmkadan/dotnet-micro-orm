// ... (rest of the file remains the same)

## CryptoHelper

The `CryptoHelper` class provides cryptographic operations including password hashing, encryption, and secure token generation using modern algorithms like PBKDF2-SHA256 and AES-256-CBC. It's designed for secure password storage and sensitive data protection.

### Example Usage

```csharp
// Password hashing and verification
string password = "SecurePassword123!";
string hashedPassword = CryptoHelper.HashPassword(password);

bool isValid = CryptoHelper.VerifyPassword(password, hashedPassword);
Console.WriteLine(isValid); // Output: True

// Secure token generation for API keys
string apiToken = CryptoHelper.GenerateSecureToken();
Console.WriteLine(apiToken.Length); // Output: 44 (base64 encoded)

// SHA256 hashing for data integrity
string fileContent = "important data";
string checksum = CryptoHelper.ComputeSha256(fileContent);
Console.WriteLine(checksum); // Output: 64-character hex string

// AES-256 encryption for sensitive data
string secretKey = "ThisIsA32CharacterKeyForAES256Encryption!";
string sensitiveData = "Credit card: 4111-1111-1111-1111";
string encrypted = CryptoHelper.EncryptAes256(sensitiveData, secretKey);
string decrypted = CryptoHelper.DecryptAes256(encrypted, secretKey);
Console.WriteLine(decrypted); // Output: Credit card: 4111-1111-1111-1111
```

## Result

The `Result` type provides a standardized way to represent operation outcomes, supporting both synchronous and asynchronous workflows. It allows you to handle success and failure cases explicitly, making your code more expressive and error-handling friendly. The `Result` type is often used in scenarios where you need to return a value or an error from a method.

### Example Usage

```csharp
var result = Result.Ok("Operation successful");

var data = result.Match(
    onSuccess: value => value,
    onFailure: error => throw new Exception(error)
);

var asyncResult = await Result.OkAsync(42).MapAsync(async value => 
{
    await Task.Delay(100);
    return value * 2;
});

Console.WriteLine(asyncResult.IsSuccess); // Output: True
Console.WriteLine(asyncResult.IsFailure); // Output: False
```

The `Result` type can also be used with generics to represent typed results:

```csharp
Result<int> intResult = Result.Ok(42);
Result<string> stringResult = Result.Fail<string>("Something went wrong");

int value = intResult.Match(
    onSuccess: v => v,
    onFailure: _ => 0
);

Console.WriteLine(value); // Output: 42

string error = stringResult.Match(
    onSuccess: _ => "",
    onFailure: e => e
);

Console.WriteLine(error); // Output: Something went wrong
```

The `PagedResult<T>` type represents a paginated result set with metadata:

```csharp
var pagedResult = new PagedResult<Product>(
    items: new List<Product> { new Product { Id = 1, Name = "Product 1" } },
    pageNumber: 1,
    pageSize: 10,
    totalCount: 100
);

Console.WriteLine(pagedResult.TotalPages); // Output: 10
Console.WriteLine(pagedResult.HasNextPage); // Output: True
```

The `BatchOperationResult` type represents the result of a batch operation:

```csharp
var batchResult = new BatchOperationResult(
    totalProcessed: 100,
    successCount: 90,
    failureCount: 10,
    failures: new List<(int, string)> { (1, "Error 1"), (2, "Error 2") }
);

Console.WriteLine(batchResult.SuccessRate); // Output: 90
Console.WriteLine(batchResult.HasFailures); // Output: True
```

## PerformanceMonitor

The `PerformanceMonitor` class provides a comprehensive way to track and analyze the performance characteristics of code execution. It measures execution time, memory allocation, and custom metrics, making it ideal for performance profiling, benchmarking, and identifying performance regressions in your application.

### Example Usage

```csharp
using System;
using System.Threading.Tasks;
using DotnetMicroOrm.Utils;

class Program
{
    static async Task Main()
    {
        // Basic performance monitoring
        using var monitor = new PerformanceMonitor("Database Query");
        
        // Simulate work
        await Task.Delay(150);
        monitor.RecordMetric("RowsAffected", 1250);
        monitor.RecordItemCount("UsersProcessed", 500);
        monitor.Checkpoint("After query execution");
        
        // Get detailed performance report
        var report = monitor.GetReport();
        Console.WriteLine(report.GetSummary());
        
        // Check if performance thresholds were exceeded
        if (monitor.ExceededTimeThreshold)
        {
            Console.WriteLine("Warning: Operation exceeded time threshold!");
        }
        
        if (monitor.ExceededMemoryThreshold)
        {
            Console.WriteLine("Warning: Operation exceeded memory threshold!");
        }
        
        // Log summary to console
        monitor.LogSummary();
        
        // Measure async operations with built-in helper
        var (result, elapsedMs) = await PerformanceMonitor.MeasureAsync(async () =>
        {
            await Task.Delay(200);
            return "Operation completed";
        });
        
        Console.WriteLine($"Async operation took {elapsedMs}ms");
        
        // Create child monitor for nested operations
        using var parentMonitor = new PerformanceMonitor("Parent Operation");
        using var childMonitor = parentMonitor.CreateChild("Child Operation");
        
        await Task.Delay(100);
        childMonitor.RecordMetric("CacheHits", 42);
        
        // Get performance grade
        char grade = parentMonitor.GetPerformanceGrade();
        Console.WriteLine($"Performance grade: {grade}");
    }
}
```

## ApiResponse

`ApiResponse<T>` and `ApiPagedResponse<T>` are lightweight wrappers that give every API endpoint a consistent shape. They expose status information (`Success`), payload (`Data` or `Items`), human‑readable messages, optional error codes, timestamps, request identifiers and versioning. Helper factories (`CreateSuccess`, `CreateError`, `CreateFromException`, `CreateValidationError`, and the paged equivalents) make constructing responses concise and error‑free.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using DotnetMicroOrm.Utils;

class Program
{
    static void Main()
    {
        // Simple success response with data
        var success = ApiResponse<string>.CreateSuccess(
            data: "Hello, world!",
            message: "Request completed successfully");

        Console.WriteLine($"Success: {success.Success}");
        Console.WriteLine($"Data: {success.Data}");
        Console.WriteLine($"Message: {success.Message}");
        Console.WriteLine($"Timestamp: {success.Timestamp:u}");
        Console.WriteLine($"Version: {success.Version}");

        // Error response
        var error = ApiResponse<string>.CreateError(
            message: "Unable to process request",
            errorCode: "ERR_INVALID_INPUT");

        Console.WriteLine($"\nSuccess: {error.Success}");
        Console.WriteLine($"ErrorCode: {error.ErrorCode}");
        Console.WriteLine($"Message: {error.Message}");

        // Exception‑based response
        try
        {
            ThrowSomething();
        }
        catch (Exception ex)
        {
            var exResponse = ApiResponse<string>.CreateFromException(
                ex,
                requestId: "req-12345");

            Console.WriteLine($"\nSuccess: {exResponse.Success}");
            Console.WriteLine($"ErrorCode: {exResponse.ErrorCode}");
            Console.WriteLine($"Message: {exResponse.Message}");
            Console.WriteLine($"RequestId: {exResponse.RequestId}");
        }

        // Validation error response
        var validationErrors = new Dictionary<string, string>
        {
            { "Email", "Invalid email format" },
            { "Password", "Password must be at least 8 characters" }
        };

        var validation = ApiResponse<string>.CreateValidationError(validationErrors);
        Console.WriteLine($"\nSuccess: {validation.Success}");
        Console.WriteLine($"Message: {validation.Message}");
        Console.WriteLine($"ErrorCode: {validation.ErrorCode}");

        // Paginated response
        var items = new[] { "Item1", "Item2", "Item3" };
        var paged = ApiPagedResponse<string>.CreateSuccess(
            items: items,
            pageNumber: 1,
            pageSize: 10,
            totalCount: 3,
            message: "Page retrieved successfully");

        Console.WriteLine($"\nPaged Success: {paged.Success}");
        Console.WriteLine($"Item count: {paged.Items.Count}");
        Console.WriteLine($"Total pages: {paged.Pagination.TotalPages}");
        Console.WriteLine($"Has next page: {paged.Pagination.HasNextPage}");
    }

    static void ThrowSomething()
    {
        throw new InvalidOperationException("Something went wrong");
    }
}
```

The snippet demonstrates creating successful, error, exception‑derived, validation‑error, and paginated responses using only the public members defined in `ApiResponse.cs`.


## Extensions

The `Extensions` static class provides a collection of extension methods for common operations including entity reflection, data conversion, pagination, filtering, sorting, and utility functions. These methods simplify working with entities, queryables, and collections while maintaining type safety and clean syntax.

### Example Usage

```csharp
using System;
using System.Linq;
using System.Collections.Generic;
using DotnetMicroOrm.Utils;
using DotnetMicroOrm.Domain.Models;

class Program
{
    static void Main()
    {
        // Get table name and schema from entity type
        var userType = typeof(User);
        string tableName = userType.GetTableName();
        string schemaName = userType.GetTableSchema();
        Console.WriteLine($"Table: {schemaName}.{tableName}");

        // Get column name from property
        var nameProperty = typeof(User).GetProperty("Name");
        string columnName = nameProperty.GetColumnName();
        Console.WriteLine($"Column name for 'Name': {columnName}");

        // Get mapped properties and primary key
        var mappedProperties = typeof(User).GetMappedProperties();
        var primaryKey = typeof(User).GetPrimaryKeyProperty();
        Console.WriteLine($"Mapped properties: {mappedProperties.Count}");
        Console.WriteLine($"Primary key: {primaryKey?.Name}");

        // Clone entity with new ID
        var originalUser = new User { Id = 1, Name = "John", Email = "john@example.com" };
        var clonedUser = originalUser.CloneWithNewId();
        Console.WriteLine($"Original ID: {originalUser.Id}, Cloned ID: {clonedUser.Id}");

        // Convert entity to dictionary
        var userDict = originalUser.ToDictionary();
        Console.WriteLine($"Dictionary keys: {string.Join(", ", userDict.Keys)}");

        // Check if property has changed
        bool hasChanged = originalUser.HasPropertyChanged("Name", "Jane");
        Console.WriteLine($"Name property changed: {hasChanged}");

        // Get member name from expression
        string memberName = Extensions.GetMemberName<User, string>(u => u.Name);
        Console.WriteLine($"Member name: {memberName}");

        // Paginate a list
        var users = new List<User>
        {
            new User { Id = 1, Name = "User 1" },
            new User { Id = 2, Name = "User 2" },
            new User { Id = 3, Name = "User 3" },
            new User { Id = 4, Name = "User 4" },
        };
        
        var (paginatedItems, totalCount) = users.Paginate(pageNumber: 1, pageSize: 2);
        Console.WriteLine($"Page items: {paginatedItems.Count}, Total: {totalCount}");

        // Safely apply filter to IQueryable
        var query = users.AsQueryable().SafeWhere(u => u.Id > 2);
        Console.WriteLine($"Filtered count: {query.Count()}");

        // Apply sorting to IQueryable
        var sortedQuery = users.AsQueryable().ApplySort(u => u.Name, ascending: true);
        Console.WriteLine($"First sorted: {sortedQuery.First().Name}");

        // Convert entity to JSON string
        string json = originalUser.ToJsonString();
        Console.WriteLine($"JSON: {json}");

        // Check if string is null or empty
        bool isEmpty = "".IsNullOrEmpty();
        Console.WriteLine($"Is empty: {isEmpty}");

        // Calculate age from birth date
        int age = new DateTime(1990, 5, 15).GetAgeInYears();
        Console.WriteLine($"Age: {age}");

        // Format currency
        string formatted = 1234.56m.FormatCurrency("$");
        Console.WriteLine($"Formatted: {formatted}");
    }
}

// Example entity for demonstration
public class User : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```