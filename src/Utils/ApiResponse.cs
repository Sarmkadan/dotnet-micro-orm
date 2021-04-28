#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Generic API response wrapper for consistent response formatting across all endpoints.
/// Provides a unified structure for success, error, and paginated responses.
/// </summary>
public class sealed ApiResponse<T>
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>The actual response data</summary>
    public T? Data { get; set; }

    /// <summary>Success or error message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error code if operation failed</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Timestamp when response was generated</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Unique request ID for tracing</summary>
    public string? RequestId { get; set; }

    /// <summary>Server version</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Creates a successful response</summary>
    public static ApiResponse<T> CreateSuccess(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>Creates an error response</summary>
    public static ApiResponse<T> CreateError(string message, string errorCode = "ERROR", T? data = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = data,
            Message = message,
            ErrorCode = errorCode
        };
    }

    /// <summary>Creates a response from an exception</summary>
    public static ApiResponse<T> CreateFromException(Exception ex, string? requestId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = ex.Message,
            ErrorCode = ex.GetType().Name,
            RequestId = requestId
        };
    }

    /// <summary>Creates a validation error response</summary>
    public static ApiResponse<T> CreateValidationError(Dictionary<string, string> errors)
    {
        var message = string.Join("; ", errors.Select(x => $"{x.Key}: {x.Value}"));
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            ErrorCode = "VALIDATION_ERROR",
            Data = default
        };
    }
}

/// <summary>
/// Paginated API response
/// </summary>
public class sealed ApiPagedResponse<T>
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>The data items</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Pagination metadata</summary>
    public PaginationMetadata Pagination { get; set; } = new();

    /// <summary>Success or error message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error code if operation failed</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Response timestamp</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Request ID for tracing</summary>
    public string? RequestId { get; set; }

    /// <summary>Creates a successful paginated response</summary>
    public static ApiPagedResponse<T> CreateSuccess(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount,
        string message = "Operation successful")
    {
        return new ApiPagedResponse<T>
        {
            Success = true,
            Items = items.ToList(),
            Pagination = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (totalCount + pageSize - 1) / pageSize,
                HasNextPage = pageNumber < (totalCount + pageSize - 1) / pageSize,
                HasPreviousPage = pageNumber > 1
            },
            Message = message
        };
    }

    /// <summary>Creates an error paginated response</summary>
    public static ApiPagedResponse<T> CreateError(string message, string errorCode = "ERROR")
    {
        return new ApiPagedResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Pagination metadata for paginated responses
/// </summary>
public class sealed PaginationMetadata
{
    /// <summary>Current page number (1-based)</summary>
    public int PageNumber { get; set; }

    /// <summary>Items per page</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of items</summary>
    public int TotalCount { get; set; }

    /// <summary>Total number of pages</summary>
    public int TotalPages { get; set; }

    /// <summary>Whether there's a next page</summary>
    public bool HasNextPage { get; set; }

    /// <summary>Whether there's a previous page</summary>
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Simple success response with no data
/// </summary>
public class sealed ApiResponse
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>Response message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error code if failed</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Response timestamp</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Request ID for tracing</summary>
    public string? RequestId { get; set; }

    /// <summary>Creates a successful response</summary>
    public static ApiResponse CreateSuccess(string message = "Operation successful") =>
        new() { Success = true, Message = message };

    /// <summary>Creates an error response</summary>
    public static ApiResponse CreateError(string message, string errorCode = "ERROR") =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}
