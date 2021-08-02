#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for DefaultHttpClient providing additional convenience
// and functionality while maintaining the same namespace.
// =============================================================================

using System.Net.Http;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Extension methods for <see cref="DefaultHttpClient"/> providing additional HTTP operation utilities.
/// </summary>
public static class DefaultHttpClientExtensions
{
    /// <summary>
    /// Sends a GET request with the specified query parameters.
    /// </summary>
    /// <param name="client">The HTTP client instance. Cannot be null.</param>
    /// <param name="url">The request URL. Cannot be null or empty.</param>
    /// <param name="queryParameters">Dictionary of query parameters to append to the URL. Can be null or empty.</param>
    /// <param name="headers">Optional request headers. Can be null.</param>
    /// <returns>HTTP response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null or empty.</exception>
    public static async Task<HttpResponseData> GetAsync(
        this DefaultHttpClient client,
        string url,
        Dictionary<string, string>? queryParameters,
        Dictionary<string, string>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(url);

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
    /// <param name="client">The HTTP client instance. Cannot be null.</param>
    /// <param name="url">The request URL. Cannot be null or empty.</param>
    /// <param name="formData">Dictionary of form data fields. Cannot be null.</param>
    /// <param name="headers">Optional request headers. Can be null.</param>
    /// <returns>HTTP response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="formData"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null or empty.</exception>
    public static async Task<HttpResponseData> PostFormAsync(
        this DefaultHttpClient client,
        string url,
        Dictionary<string, string> formData,
        Dictionary<string, string>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(formData);

        if (formData.Count == 0)
        {
            return await client.PostAsync(url, string.Empty, "application/x-www-form-urlencoded", headers);
        }

        var formUrlEncoded = string.Join("&",
            formData.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return await client.PostAsync(url, formUrlEncoded, "application/x-www-form-urlencoded", headers);
    }

    /// <summary>
    /// Sends a request with JSON content and automatically deserializes the response body.
    /// </summary>
    /// <typeparam name="TResponse">Type to deserialize the response to</typeparam>
    /// <param name="client">The HTTP client instance. Cannot be null.</param>
    /// <param name="url">The request URL. Cannot be null or empty.</param>
    /// <param name="method">HTTP method to use.</param>
    /// <param name="requestBody">Optional request body.</param>
    /// <param name="headers">Optional request headers. Can be null.</param>
    /// <returns>Deserialized response object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown if the HTTP method is not supported.</exception>
    public static async Task<TResponse?> GetFromJsonAsync<TResponse>(
        this DefaultHttpClient client,
        string url,
        HttpMethod method,
        string? requestBody = null,
        Dictionary<string, string>? headers = null)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(method);

        HttpResponseData response;

        switch (method.Method)
        {
            case "GET" when method == HttpMethod.Get:
                response = await client.GetAsync(url, headers);
                break;
            case "POST" when method == HttpMethod.Post:
                response = await client.PostAsync(url, requestBody ?? "{}", headers: headers);
                break;
            case "PUT" when method == HttpMethod.Put:
                response = await client.PutAsync(url, requestBody ?? "{}", headers: headers);
                break;
            case "DELETE" when method == HttpMethod.Delete:
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
    /// <param name="client">The HTTP client instance. Cannot be null.</param>
    /// <param name="url">The request URL. Cannot be null or empty.</param>
    /// <param name="method">HTTP method to use.</param>
    /// <param name="requestBody">Optional request body.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="retryDelay">Optional custom retry delay.</param>
    /// <returns>HTTP response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null or empty.</exception>
    public static async Task<HttpResponseData> SendWithRetryAsync(
        this DefaultHttpClient client,
        string url,
        HttpMethod method,
        string? requestBody = null,
        int maxRetries = 3,
        TimeSpan? retryDelay = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentNullException.ThrowIfNull(method);

        // Use the existing GetWithRetryAsync for GET requests
        if (method == HttpMethod.Get)
        {
            return await client.GetWithRetryAsync(url, maxRetries, retryDelay);
        }

        // For other methods, implement retry logic
        int attempt = 0;

        while (attempt < maxRetries)
        {
            HttpResponseData response;

            try
            {
                switch (method.Method)
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

                if (attempt < maxRetries)
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
            if (response.IsServerError)
            {
                attempt++;

                if (attempt < maxRetries)
                {
                    var delay = retryDelay ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(delay);
                    continue;
                }
            }

            return response;
        }

        // This line is unreachable but kept for completeness
        throw new InvalidOperationException("Retry loop completed without returning a response");
    }

    /// <summary>
    /// Sends an HTTP request using the specified method and returns the response.
    /// </summary>
    /// <param name="client">The HTTP client instance. Cannot be null.</param>
    /// <param name="method">HTTP method to use. Cannot be null.</param>
    /// <param name="url">The request URL. Cannot be null or empty.</param>
    /// <param name="body">Optional request body.</param>
    /// <param name="contentType">Content type for the request.</param>
    /// <param name="headers">Optional request headers. Can be null.</param>
    /// <returns>HTTP response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="method"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown if the HTTP method is not supported.</exception>
    public static async Task<HttpResponseData> SendAsync(
        this DefaultHttpClient client,
        HttpMethod method,
        string url,
        string? body = null,
        string contentType = "application/json",
        Dictionary<string, string>? headers = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrEmpty(url);

        return method.Method switch
        {
            "GET" => await client.GetAsync(url, headers),
            "POST" => await client.PostAsync(url, body ?? "{}", contentType, headers),
            "PUT" => await client.PutAsync(url, body ?? "{}", contentType, headers),
            "DELETE" => await client.DeleteAsync(url, headers),
            _ => throw new NotSupportedException($"HTTP method {method.Method} is not supported")
        };
    }
}