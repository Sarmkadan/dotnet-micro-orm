// ... (rest of the README content remains the same)

## ApiResponseExtensions

The `ApiResponseExtensions` class provides a set of extensions for working with API responses. It allows you to easily add request IDs and context to responses, convert responses to JSON, and create success and error responses.

### Example Usage

```csharp
var response = new ApiResponse();
var responseWithRequestId = response.WithRequestId("12345");
var json = responseWithRequestId.ToJson();

var successResponse = ApiResponse.ToSuccessResponse("Hello, World!");
var errorResponse = ApiResponse.ToErrorResponse("Error message");

var pagedResponse = new ApiPagedResponse<string>
{
    Items = new List<string> { "Item1", "Item2" },
    PageNumber = 1,
    PageSize = 10,
    TotalCount = 20
};

var pagedResponseWithRequestId = pagedResponse.WithRequestId("67890");
var pagedJson = pagedResponseWithRequestId.ToJson();
```

// ... (rest of the README content remains the same)
