# ApiResponse

`ApiResponse<T>` and `ApiPagedResponse<T>` are generic wrapper types that standardise the shape of all API-level return values across the system. They carry a success flag, an optional payload, diagnostic metadata such as a message, error code, timestamp, and request correlation ID, along with factory methods that enforce consistent construction for success, error, validation-failure, and exception-based outcomes. `ApiPagedResponse<T>` extends this pattern with a paginated item list and associated pagination metadata.

## API

### ApiResponse\<T\>

| Member | Kind | Description |
|---|---|---|
| `Success` | Property `bool` | Indicates whether the operation completed without error. `true` for success responses; `false` for error, validation-failure, or exception responses. |
| `Data` | Property `T?` | The primary payload of a successful operation. `null` when `Success` is `false` or when the operation legitimately returns no data. |
| `Message` | Property `string` | Human-readable summary. For successes this is typically an informational string; for errors it carries the error description. Never `null`. |
| `ErrorCode` | Property `string?` | Machine-readable error identifier (e.g. `"VALIDATION_ERROR"`, `"NOT_FOUND"`). `null` for successful responses. |
| `Timestamp` | Property `DateTime` | UTC instant at which the response was constructed. Set automatically by the factory methods. |
| `RequestId` | Property `string?` | Correlation identifier linking the response to a specific HTTP request or operation context. May be `null` if not supplied. |
| `Version` | Property `string` | API version string (e.g. `"1.0"`) baked into the response. Set automatically by the factory methods. |
| `CreateSuccess` | Static method | **Signature:** `static ApiResponse<T> CreateSuccess(T? data, string message = "Operation completed successfully", string? requestId = null)`  <br>**Returns:** A new `ApiResponse<T>` with `Success = true`, `Data` set to the supplied value, and `Timestamp` set to `UtcNow`. `ErrorCode` is `null`. |
| `CreateError` | Static method | **Signature:** `static ApiResponse<T> CreateError(string message, string? errorCode = null, string? requestId = null)`  <br>**Returns:** A new `ApiResponse<T>` with `Success = false`, `Data = default`, and the given message and error code. |
| `CreateFromException` | Static method | **Signature:** `static ApiResponse<T> CreateFromException(Exception ex, string? errorCode = null, string? requestId = null)`  <br>**Returns:** A new `ApiResponse<T>` with `Success = false`. `Message` is populated from `ex.Message`; `ErrorCode` defaults to the exception type name if not explicitly provided. |
| `CreateValidationError` | Static method | **Signature:** `static ApiResponse<T> CreateValidationError(string message, string? errorCode = "VALIDATION_ERROR", string? requestId = null)`  <br>**Returns:** A new `ApiResponse<T>` with `Success = false` and `ErrorCode` defaulting to `"VALIDATION_ERROR"`. Designed for model-binding or business-rule validation failures. |

### ApiPagedResponse\<T\>

| Member | Kind | Description |
|---|---|---|
| `Success` | Property `bool` | Same semantics as `ApiResponse<T>.Success`. |
| `Items` | Property `List<T>` | The page of data items. Never `null`; empty list when no items match the query. |
| `Pagination` | Property `PaginationMetadata` | Metadata describing the current page, including page number, page size, total count, and total pages. |
| `Message` | Property `string` | Same role as `ApiResponse<T>.Message`. |
| `ErrorCode` | Property `string?` | Same role as `ApiResponse<T>.ErrorCode`. |
| `Timestamp` | Property `DateTime` | Same role as `ApiResponse<T>.Timestamp`. |
| `RequestId` | Property `string?` | Same role as `ApiResponse<T>.RequestId`. |
| `CreateSuccess` | Static method | **Signature:** `static ApiPagedResponse<T> CreateSuccess(List<T> items, PaginationMetadata pagination, string message = "Operation completed successfully", string? requestId = null)`  <br>**Returns:** A new `ApiPagedResponse<T>` with `Success = true`, `Items` and `Pagination` set from the arguments. |
| `CreateError` | Static method | **Signature:** `static ApiPagedResponse<T> CreateError(string message, string? errorCode = null, string? requestId = null)`  <br>**Returns:** A new `ApiPagedResponse<T>` with `Success = false`, `Items` empty, and `Pagination` set to a default/empty metadata instance. |

## Usage

### Example 1: Single-result success and validation error

```csharp
// Success path
var customer = await customerRepository.GetByIdAsync(42);
var response = ApiResponse<Customer>.CreateSuccess(
    data: customer,
    message: "Customer retrieved",
    requestId: Activity.Current?.Id
);
return Ok(response); // HTTP 200 with standardised envelope

// Validation error path
if (string.IsNullOrWhiteSpace(request.Name))
{
    var validationResponse = ApiResponse<Customer>.CreateValidationError(
        message: "Name is required.",
        requestId: Activity.Current?.Id
    );
    return BadRequest(validationResponse);
}
```

### Example 2: Paged response and exception handling

```csharp
try
{
    var items = await orderRepository.GetPagedOrdersAsync(page: 1, pageSize: 20);
    var pagination = new PaginationMetadata
    {
        Page = 1,
        PageSize = 20,
        TotalCount = items.TotalCount,
        TotalPages = (int)Math.Ceiling(items.TotalCount / 20.0)
    };

    var pagedResponse = ApiPagedResponse<Order>.CreateSuccess(
        items: items.Results,
        pagination: pagination,
        message: "Orders retrieved",
        requestId: HttpContext.TraceIdentifier
    );
    return Ok(pagedResponse);
}
catch (Exception ex)
{
    var errorResponse = ApiPagedResponse<Order>.CreateError(
        message: "Failed to fetch orders",
        errorCode: "ORDERS_QUERY_FAILED",
        requestId: HttpContext.TraceIdentifier
    );
    return StatusCode(500, errorResponse);
}
```

## Notes

- **Null payloads:** `CreateSuccess` accepts `null` for `T? data`. Consumers should check `Success` before accessing `Data`; a successful response with `Data = null` is valid when the operation has no result body.
- **Default pagination on error:** `ApiPagedResponse<T>.CreateError` sets `Pagination` to a default metadata object (all numeric fields zeroed) and `Items` to an empty list. This prevents null-reference exceptions in clients that unconditionally iterate over `Items` or read pagination fields.
- **Timestamp consistency:** All factory methods capture `DateTime.UtcNow` at the moment of construction. This value is not adjustable after creation.
- **Thread safety:** The types are immutable after construction (all properties are read-only from the public surface). Factory methods create new instances and do not mutate shared state. They are safe to call concurrently without external synchronisation.
- **Exception message leakage:** `CreateFromException` copies `ex.Message` directly into the response `Message` field. Avoid exposing internal exception details to external clients in production; prefer `CreateError` with a sanitised message for user-facing responses.
- **Version stamping:** The `Version` property is populated from a constant or configuration value inside the factory methods. It is not a parameter, ensuring all responses within a deployed version carry the same value automatically.
