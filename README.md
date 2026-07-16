// ... (rest of the file remains the same)

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
