#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Extension methods for <see cref="ApiResponse"/> and <see cref="ApiPagedResponse{T}"/> classes
/// providing common utility operations for API responses.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Adds a request ID to the response for better tracing and debugging.
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="requestId">The request ID to add</param>
    /// <returns>The response with the request ID set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="requestId"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse WithRequestId(this ApiResponse response, string requestId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        response.RequestId = requestId;
        return response;
    }

    /// <summary>
    /// Adds a request ID to the typed API response for better tracing and debugging.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The API response</param>
    /// <param name="requestId">The request ID to add</param>
    /// <returns>The response with the request ID set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="requestId"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse<T> WithRequestId<T>(this ApiResponse<T> response, string requestId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        response.RequestId = requestId;
        return response;
    }

    /// <summary>
    /// Adds a request ID to the paginated API response for better tracing and debugging.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated API response</param>
    /// <param name="requestId">The request ID to add</param>
    /// <returns>The response with the request ID set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="requestId"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiPagedResponse<T> WithRequestId<T>(this ApiPagedResponse<T> response, string requestId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        response.RequestId = requestId;
        return response;
    }

    /// <summary>
    /// Adds a custom property to the response message by appending additional context.
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="additionalContext">Additional context to append to the message</param>
    /// <returns>The response with updated message</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="additionalContext"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse WithContext(this ApiResponse response, string additionalContext)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalContext);
        response.Message += " " + additionalContext;
        return response;
    }

    /// <summary>
    /// Adds a custom property to the typed API response message by appending additional context.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The API response</param>
    /// <param name="additionalContext">Additional context to append to the message</param>
    /// <returns>The response with updated message</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="additionalContext"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse<T> WithContext<T>(this ApiResponse<T> response, string additionalContext)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalContext);
        response.Message += " " + additionalContext;
        return response;
    }

    /// <summary>
    /// Adds a custom property to the paginated API response message by appending additional context.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated API response</param>
    /// <param name="additionalContext">Additional context to append to the message</param>
    /// <returns>The response with updated message</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="additionalContext"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiPagedResponse<T> WithContext<T>(this ApiPagedResponse<T> response, string additionalContext)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalContext);
        response.Message += " " + additionalContext;
        return response;
    }

    /// <summary>
    /// Converts the API response to a JSON string with consistent formatting.
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="options">Optional JSON serialization options</param>
    /// <returns>JSON string representation of the response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static string ToJson(this ApiResponse response, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        return JsonSerializer.Serialize(response, options);
    }

    /// <summary>
    /// Converts the typed API response to a JSON string with consistent formatting.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The API response</param>
    /// <param name="options">Optional JSON serialization options</param>
    /// <returns>JSON string representation of the response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static string ToJson<T>(this ApiResponse<T> response, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        return JsonSerializer.Serialize(response, options);
    }

    /// <summary>
    /// Converts the paginated API response to a JSON string with consistent formatting.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated API response</param>
    /// <param name="options">Optional JSON serialization options</param>
    /// <returns>JSON string representation of the response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static string ToJson<T>(this ApiPagedResponse<T> response, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(response);
        return JsonSerializer.Serialize(response, options);
    }

    /// <summary>
    /// Creates a new success response based on an existing response, copying all properties
    /// except the message which can be customized.
    /// </summary>
    /// <param name="response">The original response to copy from</param>
    /// <param name="newMessage">The new success message</param>
    /// <returns>A new success response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="newMessage"/> is <see langword="null"/> or empty</exception>
    public static ApiResponse ToSuccessResponse(this ApiResponse response, string newMessage)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);
        return new ApiResponse
        {
            Success = true,
            Message = newMessage,
            ErrorCode = null,
            Timestamp = DateTime.UtcNow,
            RequestId = response.RequestId
        };
    }

    /// <summary>
    /// Creates a new success response based on an existing typed response, copying all properties
    /// except the message and data which can be customized.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The original response to copy from</param>
    /// <param name="newData">The new data to include</param>
    /// <param name="newMessage">The new success message</param>
    /// <returns>A new success response with data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="newMessage"/> is <see langword="null"/> or empty</exception>
    public static ApiResponse<T> ToSuccessResponse<T>(this ApiResponse<T> response, T newData, string newMessage)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(newMessage);
        return new ApiResponse<T>
        {
            Success = true,
            Data = newData,
            Message = newMessage,
            ErrorCode = null,
            Timestamp = DateTime.UtcNow,
            RequestId = response.RequestId
        };
    }

    /// <summary>
    /// Creates a new error response based on an existing response, copying all properties
    /// except the error details which can be customized.
    /// </summary>
    /// <param name="response">The original response to copy from</param>
    /// <param name="errorMessage">The new error message</param>
    /// <param name="errorCode">The new error code</param>
    /// <returns>A new error response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is <see langword="null"/> or empty</exception>
    public static ApiResponse ToErrorResponse(this ApiResponse response, string errorMessage, string errorCode = "ERROR")
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new ApiResponse
        {
            Success = false,
            Message = errorMessage,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow,
            RequestId = response.RequestId
        };
    }

    /// <summary>
    /// Creates a new error response based on an existing typed response, copying all properties
    /// except the error details which can be customized.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The original response to copy from</param>
    /// <param name="errorMessage">The new error message</param>
    /// <param name="errorCode">The new error code</param>
    /// <returns>A new error response with default data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is <see langword="null"/> or empty</exception>
    public static ApiResponse<T> ToErrorResponse<T>(this ApiResponse<T> response, string errorMessage, string errorCode = "ERROR")
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = errorMessage,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow,
            RequestId = response.RequestId
        };
    }

    /// <summary>
    /// Adds pagination metadata to a non-paginated response for consistent response structure.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The original response</param>
    /// <param name="pagination">Pagination metadata to add</param>
    /// <returns>A new paginated response with the original data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="pagination"/> is <see langword="null"/></exception>
    public static ApiPagedResponse<T> ToPagedResponse<T>(this ApiResponse<T> response, PaginationMetadata pagination)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(pagination);

        return new ApiPagedResponse<T>
        {
            Success = response.Success,
            Items = response.Data != null ? [response.Data] : [],
            Pagination = pagination,
            Message = response.Message,
            ErrorCode = response.ErrorCode,
            Timestamp = response.Timestamp,
            RequestId = response.RequestId
        };
    }

    /// <summary>
    /// Converts a paginated response to a simple list of items, extracting the data from Items property.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated response</param>
    /// <returns>List of items from the paginated response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static List<T> ToItemList<T>(this ApiPagedResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Items ?? new List<T>();
    }

    /// <summary>
    /// Determines whether the paginated response has more items available (HasNextPage = true).
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated response</param>
    /// <returns>True if there are more items to fetch</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static bool HasMoreItems<T>(this ApiPagedResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Pagination?.HasNextPage == true;
    }

    /// <summary>
    /// Determines whether the paginated response is on the first page (PageNumber = 1).
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated response</param>
    /// <returns>True if this is the first page</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static bool IsFirstPage<T>(this ApiPagedResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Pagination?.PageNumber == 1;
    }

    /// <summary>
    /// Determines whether the paginated response is on the last page.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">The paginated response</param>
    /// <returns>True if this is the last page</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static bool IsLastPage<T>(this ApiPagedResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return response.Pagination?.HasNextPage == false;
    }
}