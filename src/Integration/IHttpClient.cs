// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Abstraction for HTTP client operations with built-in retry support,
/// timeout handling, and request/response logging for integration testing.
/// </summary>
public interface IHttpClient : IAsyncDisposable
{
    /// <summary>
    /// Performs a GET request
    /// </summary>
    Task<HttpResponseData> GetAsync(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a POST request with a body
    /// </summary>
    Task<HttpResponseData> PostAsync(string url, string body, string contentType = "application/json", Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a PUT request
    /// </summary>
    Task<HttpResponseData> PutAsync(string url, string body, string contentType = "application/json", Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a DELETE request
    /// </summary>
    Task<HttpResponseData> DeleteAsync(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Performs a request with automatic retry on transient failures
    /// </summary>
    Task<HttpResponseData> GetWithRetryAsync(string url, int maxRetries = 3, TimeSpan? retryDelay = null);
}

/// <summary>
/// Response data from HTTP requests
/// </summary>
public class HttpResponseData
{
    /// <summary>HTTP status code</summary>
    public int StatusCode { get; set; }

    /// <summary>Response body as string</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Response headers</summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>Request duration</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Exception if request failed</summary>
    public Exception? Exception { get; set; }

    /// <summary>Indicates if response is successful (2xx status code)</summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    /// <summary>Indicates if response is a client error (4xx status code)</summary>
    public bool IsClientError => StatusCode >= 400 && StatusCode < 500;

    /// <summary>Indicates if response is a server error (5xx status code)</summary>
    public bool IsServerError => StatusCode >= 500;
}

/// <summary>
/// Configuration for HTTP client behavior
/// </summary>
public class HttpClientConfig
{
    /// <summary>Default request timeout</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>User agent string</summary>
    public string UserAgent { get; set; } = "DotnetMicroOrm/1.0";

    /// <summary>Maximum redirects to follow</summary>
    public int MaxRedirects { get; set; } = 5;

    /// <summary>Enable automatic decompression</summary>
    public bool AutoDecompression { get; set; } = true;

    /// <summary>Pool connections for better performance</summary>
    public bool PoolConnections { get; set; } = true;
}
