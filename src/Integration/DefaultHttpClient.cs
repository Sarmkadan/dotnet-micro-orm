#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Default HTTP client implementation using System.Net.Http.HttpClient.
/// Provides retry logic, timeout handling, and logging capabilities.
/// Thread-safe and designed for reuse across multiple requests.
/// </summary>
public sealed class DefaultHttpClient : IHttpClient
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly HttpClientConfig _config;
    private readonly IRetryPolicy _retryPolicy;

    public DefaultHttpClient(HttpClientConfig? config = null, IRetryPolicy? retryPolicy = null)
    {
        _config = config ?? new HttpClientConfig();
        _retryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy();

        _httpClient = new System.Net.Http.HttpClient
        {
            Timeout = _config.Timeout
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);
    }

    public async Task<HttpResponseData> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        return await SendAsync(System.Net.Http.HttpMethod.Get, url, null, "application/json", headers).ConfigureAwait(false);
    }

    public async Task<HttpResponseData> PostAsync(string url, string body, string contentType = "application/json", Dictionary<string, string>? headers = null)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        return await SendAsync(System.Net.Http.HttpMethod.Post, url, body, contentType, headers).ConfigureAwait(false);
    }

    public async Task<HttpResponseData> PutAsync(string url, string body, string contentType = "application/json", Dictionary<string, string>? headers = null)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        return await SendAsync(System.Net.Http.HttpMethod.Put, url, body, contentType, headers).ConfigureAwait(false);
    }

    public async Task<HttpResponseData> DeleteAsync(string url, Dictionary<string, string>? headers = null)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        return await SendAsync(System.Net.Http.HttpMethod.Delete, url, null, "application/json", headers).ConfigureAwait(false);
    }

    public async Task<HttpResponseData> GetWithRetryAsync(string url, int maxRetries = 3, TimeSpan? retryDelay = null)
    {
        int attempt = 0;

        while (attempt < maxRetries)
        {
            var response = await GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccess)
                return response;

            // Retry on server errors (5xx) but not client errors (4xx)
            if (response.IsClientError)
                return response;

            attempt++;

            if (attempt < maxRetries)
            {
                var delay = retryDelay ?? _retryPolicy.GetDelay(attempt);
                await Task.Delay(delay).ConfigureAwait(false);
            }
        }

        return await GetAsync(url).ConfigureAwait(false);
    }

    private async Task<HttpResponseData> SendAsync(
        System.Net.Http.HttpMethod method,
        string url,
        string? body,
        string contentType,
        Dictionary<string, string>? headers)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var request = new System.Net.Http.HttpRequestMessage(method, url);

            // Add headers
            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // Add body for POST/PUT
            if (!string.IsNullOrEmpty(body) && (method == System.Net.Http.HttpMethod.Post || method == System.Net.Http.HttpMethod.Put))
            {
                request.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, contentType);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            var responseHeaders = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                responseHeaders[header.Key] = string.Join(",", header.Value);
            }

            // Read response with size limit (10MB default)
            var maxResponseSize = _config.MaxResponseSizeBytes ?? 10 * 1024 * 1024;
            var responseBody = await ReadResponseBodyWithLimitAsync(response.Content, maxResponseSize).ConfigureAwait(false);

            return new HttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                Body = responseBody,
                Headers = responseHeaders,
                Duration = DateTime.UtcNow - startTime,
                IsBodyTruncated = responseBody.Length >= maxResponseSize
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseData
            {
                StatusCode = 0,
                Body = string.Empty,
                Duration = DateTime.UtcNow - startTime,
                Exception = ex
            };
        }
    }

    private async Task<string> ReadResponseBodyWithLimitAsync(System.Net.Http.HttpContent content, int maxSizeBytes)
    {
        using var responseStream = await content.ReadAsStreamAsync().ConfigureAwait(false);
        using var limitedStream = new System.IO.MemoryStream(maxSizeBytes + 1); // +1 to detect overflow

        var buffer = new byte[8192];
        var totalRead = 0;
        var isTruncated = false;

        try
        {
            int bytesRead;
            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                if (totalRead + bytesRead > maxSizeBytes)
                {
                    // Write only up to maxSizeBytes
                    var remaining = maxSizeBytes - totalRead;
                    await limitedStream.WriteAsync(buffer, 0, remaining).ConfigureAwait(false);
                    totalRead += remaining;
                    isTruncated = true;
                    break;
                }

                await limitedStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                totalRead += bytesRead;
            }

            return System.Text.Encoding.UTF8.GetString(limitedStream.ToArray());
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("request timed out"))
        {
            throw;
        }
        catch
        {
            // If we can't read the response, return empty string
            return string.Empty;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        await Task.CompletedTask.ConfigureAwait(false);
    }
}

/// <summary>
/// Interface for retry policy strategies
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Gets the delay before the next retry attempt
    /// </summary>
    TimeSpan GetDelay(int attemptNumber);
}

/// <summary>
/// Exponential backoff retry strategy
/// Delays increase exponentially: 1s, 2s, 4s, 8s, etc (capped at 30s)
/// </summary>
public sealed class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(30);

    public TimeSpan GetDelay(int attemptNumber)
    {
        var delaySeconds = Math.Min(Math.Pow(2, attemptNumber - 1), _maxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(delaySeconds);
    }
}

/// <summary>
/// Linear retry strategy with fixed delays
/// </summary>
public sealed class LinearRetryPolicy : IRetryPolicy
{
    private readonly TimeSpan _delayBetweenRetries;

    public LinearRetryPolicy(TimeSpan delayBetweenRetries)
    {
        _delayBetweenRetries = delayBetweenRetries;
    }

    public TimeSpan GetDelay(int attemptNumber) => _delayBetweenRetries;
}