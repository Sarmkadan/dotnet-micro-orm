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