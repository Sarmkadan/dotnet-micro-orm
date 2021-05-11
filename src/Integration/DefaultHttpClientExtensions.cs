#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for DefaultHttpClient providing additional convenience
// and functionality while maintaining the same namespace.
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Extension methods for DefaultHttpClient providing additional HTTP operation utilities.
/// </summary>
public static class DefaultHttpClientExtensions
{
    /// <summary>
    /// Sends a GET request with the specified query parameters.
    /// </summary>
    /// <param name="client">The HTTP client instance</param>
    /// <param name="url">The request URL</param>
    /// <param name="queryParameters">Dictionary of query parameters to append to the URL</param>
    /// <param name="headers">Optional request headers</param>
    /// <returns>HTTP response data</returns>
    public static async Task<HttpResponseData> GetAsync(
        this DefaultHttpClient client,
        string url,
        Dictionary<string, string>? queryParameters,
        Dictionary<string, string>? headers = null)
    {
        if (queryParameters is null || queryParameters.Count == 0)
        {
            return await client.GetAsync(url, headers);
        }

        var queryString = string.Join("&",
            queryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var fullUrl = $"{url}{(url.Contains('?') ? "&" : "?")}{queryString}";

        return await client.GetAsync(fullUrl, headers);
    }

    /// <summary>
    /// Sends a POST request with form data (application/x-www-form-urlencoded).
    /// </summary>
    /// <param name="client">The HTTP client instance</param>
    /// <param name="url">The request URL</param>
    /// <param name="formData">Dictionary of form data fields</param>
    /// <param name="headers">Optional request headers</param>
    /// <returns>HTTP response data</returns>
    public static async Task<HttpResponseData> PostFormAsync(
        this DefaultHttpClient client,
        string url,
        Dictionary<string, string> formData,
        Dictionary<string, string>? headers = null)
    {
        if (formData is null || formData.Count == 0)
        {
            throw new ArgumentException("Form data cannot be empty", nameof(formData));
        }

        var formUrlEncoded = string.Join("&",
            formData.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return await client.PostAsync(url, formUrlEncoded, "application/x-www-form-urlencoded", headers);
    }

    /// <summary>
    /// Sends a request with JSON content and automatically deserializes the response body.
    /// </summary>
    /// <typeparam name="TResponse">Type to deserialize the response to</typeparam>
    /// <param name="client">The HTTP client instance</param>
    /// <param name="url">The request URL</param>
    /// <param name="method">HTTP method to use</param>
    /// <param name="requestBody">Optional request body</param>
    /// <param name="headers">Optional request headers</param>
    /// <returns>Deserialized response object</returns>
    public static async Task<TResponse?> GetFromJsonAsync<TResponse>(
        this DefaultHttpClient client,
        string url,
        HttpMethod method,
        string? requestBody = null,
        Dictionary<string, string>? headers = null)
        where TResponse : class
    {
        HttpResponseData response;

        switch (method.Method.ToUpperInvariant())
        {
            case "GET":
                response = await client.GetAsync(url, headers);
                break;
            case "POST":
                response = await client.PostAsync(url, requestBody ?? "{}", headers: headers);
                break;
            case "PUT":
                response = await client.PutAsync(url, requestBody ?? "{}", headers: headers);
                break;
            case "DELETE":
                response = await client.DeleteAsync(url, headers);
                break;
            default:
                throw new NotSupportedException($"HTTP method {method.Method} is not supported");
        }

        if (response.IsSuccess && !string.IsNullOrEmpty(response.Body))
        {
            return System.Text.Json.JsonSerializer.Deserialize<TResponse>(response.Body);
        }

        return null;
    }

    /// <summary>
    /// Sends a request with automatic retry logic using the existing GetWithRetryAsync method.
    /// </summary>
    /// <param name="client">The HTTP client instance</param>
    /// <param name="url">The request URL</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <param name="retryDelay">Optional custom retry delay</param>
    /// <returns>HTTP response data</returns>
    public static async Task<HttpResponseData> SendWithRetryAsync(
        this DefaultHttpClient client,
        string url,
        HttpMethod method,
        string? requestBody = null,
        int maxRetries = 3,
        TimeSpan? retryDelay = null)
    {
        // Use the existing GetWithRetryAsync for GET requests
        if (method.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            return await client.GetWithRetryAsync(url, maxRetries, retryDelay);
        }

        // For other methods, implement retry logic
        int attempt = 0;

        while (attempt <= maxRetries)
        {
            HttpResponseData response;

            try
            {
                switch (method.Method.ToUpperInvariant())
                {
                    case "POST":
                        response = await client.PostAsync(url, requestBody ?? "{}");
                        break;
                    case "PUT":
                        response = await client.PutAsync(url, requestBody ?? "{}");
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(url);
                        break;
                    default:
                        throw new NotSupportedException($"HTTP method {method.Method} is not supported");
                }
            }
            catch (Exception ex)
            {
                attempt++;

                if (attempt <= maxRetries)
                {
                    var delay = retryDelay ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(delay);
                    continue;
                }

                return new HttpResponseData
                {
                    StatusCode = 0,
                    Body = string.Empty,
                    Exception = ex,
                    Duration = TimeSpan.Zero
                };
            }

            // Retry on server errors (5xx) but not client errors (4xx)
            if (response.StatusCode >= 500 && response.StatusCode < 600)
            {
                attempt++;

                if (attempt <= maxRetries)
                {
                    var delay = retryDelay ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(delay);
                    continue;
                }
            }

            return response;
        }

        // Should never reach here, but provide a fallback
        return await client.SendAsync(method, url, requestBody);
    }

    /// <summary>
    /// Sends an HTTP request using the specified method and returns the response.
    /// </summary>
    /// <param name="client">The HTTP client instance</param>
    /// <param name="method">HTTP method to use</param>
    /// <param name="url">The request URL</param>
    /// <param name="body">Optional request body</param>
    /// <param name="contentType">Content type for the request</param>
    /// <param name="headers">Optional request headers</param>
    /// <returns>HTTP response data</returns>
    public static async Task<HttpResponseData> SendAsync(
        this DefaultHttpClient client,
        HttpMethod method,
        string url,
        string? body = null,
        string contentType = "application/json",
        Dictionary<string, string>? headers = null)
    {
        return method.Method.ToUpperInvariant() switch
        {
            "GET" => await client.GetAsync(url, headers),
            "POST" => await client.PostAsync(url, body ?? "{}", contentType, headers),
            "PUT" => await client.PutAsync(url, body ?? "{}", contentType, headers),
            "DELETE" => await client.DeleteAsync(url, headers),
            _ => throw new NotSupportedException($"HTTP method {method.Method} is not supported")
        };
    }
}