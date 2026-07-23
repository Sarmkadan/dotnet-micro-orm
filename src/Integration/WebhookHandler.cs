#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Handles incoming webhooks with signature verification and event processing.
/// Supports multiple webhook types (user events, order events, etc).
/// Verifies authenticity using HMAC-SHA256 signatures with timestamp-based anti-replay protection.
/// </summary>
public sealed class WebhookHandler : IAsyncDisposable
{
    private readonly Dictionary<string, List<Func<WebhookPayload, Task>>> _handlers = [];
    private readonly WebhookSignatureValidator _signatureValidator;
    private readonly IHttpClient _httpClient;
    private readonly IWebhookDeadLetterStore _deadLetterStore;
    private readonly Dictionary<string, ICircuitBreakerPolicy> _circuitBreakers = [];
    private readonly Dictionary<string, int> _retryCounts = [];
    private readonly Dictionary<string, TimeSpan> _retryDelays = [];
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of the WebhookHandler class
    /// </summary>
    /// <param name="secret">Secret for signature verification</param>
    /// <param name="httpClient">HTTP client for outbound requests (optional)</param>
    /// <param name="deadLetterStore">Dead letter store for failed deliveries (optional)</param>
    /// <param name="timestampTolerance">Maximum allowed time difference between webhook timestamp and current time (default: 5 minutes)</param>
    public WebhookHandler(
        string secret,
        IHttpClient? httpClient = null,
        IWebhookDeadLetterStore? deadLetterStore = null,
        TimeSpan? timestampTolerance = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        _signatureValidator = new WebhookSignatureValidator(secret, timestampTolerance);
        _httpClient = httpClient ?? new DefaultHttpClient();
        _deadLetterStore = deadLetterStore ?? new InMemoryWebhookDeadLetterStore();
    }

    /// <summary>
    /// Registers a handler for a specific webhook event type
    /// </summary>
    public void Subscribe(string eventType, Func<WebhookPayload, Task> handler)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _handlers[eventType] = handlers;
        }

        handlers.Add(handler);
    }

    /// <summary>
    /// Processes an incoming webhook request with signature verification
    /// </summary>
    /// <param name="payload">Webhook payload to process</param>
    /// <param name="signatureHeader">Signature header in format "t={timestamp},v1={signature}"</param>
    /// <returns>Result of webhook processing</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload is null</exception>
    public async Task<WebhookResult> ProcessAsync(WebhookPayload payload, string signatureHeader)
    {
        ArgumentNullException.ThrowIfNull(payload);

        // Verify signature with timestamp validation
        if (!_signatureValidator.ValidateSignature(payload, signatureHeader))
        {
            return new WebhookResult
            {
                Success = false,
                Error = "Invalid signature",
                ProcessedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Find handlers for this event type
            if (!_handlers.TryGetValue(payload.EventType, out var handlers) || handlers.Count == 0)
            {
                return new WebhookResult
                {
                    Success = true,
                    Message = "No handlers registered for this event type",
                    ProcessedAt = DateTime.UtcNow
                };
            }

            // Execute all handlers
            var tasks = handlers.Select(h => h(payload));
            await Task.WhenAll(tasks).ConfigureAwait(false);

            return new WebhookResult
            {
                Success = true,
                Message = $"Processed {handlers.Count} handler(s)",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new WebhookResult
            {
                Success = false,
                Error = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Generates a signature header for a webhook payload (for testing and outbound deliveries)
    /// </summary>
    /// <param name="payload">Webhook payload to sign</param>
    /// <returns>Signature header string in format "t={timestamp},v1={signature}"</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload is null</exception>
    public string GenerateSignatureHeader(WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return _signatureValidator.GenerateSignatureHeader(payload);
    }

    /// <summary>
    /// Generates a signature for a webhook payload (legacy method for backward compatibility)
    /// </summary>
    /// <param name="payload">Webhook payload to sign</param>
    /// <returns>Hex-encoded HMAC-SHA256 signature</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload is null</exception>
    [Obsolete("Use GenerateSignatureHeader instead. This method does not include timestamp-based anti-replay protection.")]
    public string GenerateSignature(WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var secretBytes = Encoding.UTF8.GetBytes(_signatureValidator.GetSecretForBackwardCompatibility());

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Asynchronously releases all resources used by the WebhookHandler
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _httpClient.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Processes an outgoing webhook delivery with circuit breaker, retry, and dead-letter support
    /// </summary>
    /// <param name="payload">Webhook payload to deliver</param>
    /// <param name="url">Destination URL for the webhook</param>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
    /// <param name="initialRetryDelay">Initial delay between retries (default: 1 second)</param>
    /// <param name="circuitBreakerThreshold">Number of failures before opening circuit (default: 5)</param>
    /// <param name="circuitBreakerDuration">Duration circuit stays open (default: 30 seconds)</param>
    /// <returns>Webhook delivery result with attempt tracking</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload or url is null</exception>
    /// <exception cref="ArgumentException">Thrown if url is empty</exception>
    public async Task<WebhookResult> DeliverAsync(
        WebhookPayload payload,
        string url,
        int maxRetries = 3,
        TimeSpan? initialRetryDelay = null,
        int circuitBreakerThreshold = 5,
        TimeSpan? circuitBreakerDuration = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(url);

        var startTime = DateTime.UtcNow;
        var attemptCount = 0;
        WebhookResult? lastResult = null;
        Exception? lastException = null;
        bool shouldStoreInDeadLetter = false;
        string finalDisposition = "success";

        // Get or create circuit breaker for this URL
        var circuitBreaker = GetCircuitBreaker(url, circuitBreakerThreshold, circuitBreakerDuration);

        // Apply retry delays based on event type
        var retryDelay = GetRetryDelay(payload.EventType, initialRetryDelay);

        while (attemptCount <= maxRetries)
        {
            attemptCount++;

            try
            {
                // Check circuit breaker before attempting delivery
                if (circuitBreaker.CurrentState == CircuitBreakerState.Open)
                {
                    lastResult = new WebhookResult
                    {
                        Success = false,
                        Error = "Circuit breaker is open",
                        ProcessedAt = DateTime.UtcNow,
                        AttemptCount = attemptCount,
                        FinalDisposition = "circuit_open",
                        Duration = DateTime.UtcNow - startTime
                    };
                    shouldStoreInDeadLetter = true;
                    finalDisposition = "circuit_open";
                    break;
                }

                // Execute delivery with circuit breaker protection
                lastResult = await circuitBreaker.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.PostAsync(url, System.Text.Json.JsonSerializer.Serialize(payload))
                        .ConfigureAwait(false);

                    return new WebhookResult
                    {
                        Success = response.IsSuccess,
                        Message = response.IsSuccess ? "Webhook delivered successfully" : $"HTTP {(int)response.StatusCode}",
                        Error = response.IsSuccess ? null : response.Body,
                        ProcessedAt = DateTime.UtcNow,
                        AttemptCount = attemptCount,
                        FinalDisposition = response.IsSuccess ? "success" : "http_error",
                        Duration = response.Duration,
                        HttpStatusCode = response.StatusCode
                    };
                }).ConfigureAwait(false);

                if (lastResult.Success)
                {
                    // Reset retry count on success
                    lock (_lock)
                    {
                        _retryCounts[url] = 0;
                    }
                    return lastResult;
                }

                // Check if this is a retryable error (5xx or timeout)
                if ((lastResult.HttpStatusCode ?? 0) >= 500 || lastResult.Exception is not null)
                {
		lastException = lastResult.Exception ?? new HttpRequestException(lastResult.Error ?? "Unknown error");

                    // Exponential backoff
                    if (attemptCount <= maxRetries)
                    {
                        await Task.Delay(retryDelay).ConfigureAwait(false);
                        retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks * 2, TimeSpan.FromMinutes(5).Ticks));
                    }
                }
                else
                {
                    // Non-retryable error (4xx), don't retry
                    shouldStoreInDeadLetter = true;
                    finalDisposition = "client_error";
                    break;
                }
            }
            catch (CircuitBreakerOpenException cboEx)
            {
                lastResult = new WebhookResult
                {
                    Success = false,
                    Error = cboEx.Message,
                    ProcessedAt = DateTime.UtcNow,
                    AttemptCount = attemptCount,
                    FinalDisposition = "circuit_open",
                    Duration = DateTime.UtcNow - startTime
                };
                shouldStoreInDeadLetter = true;
                finalDisposition = "circuit_open";
                break;
            }
            catch (Exception ex)
            {
                lastException = ex;
                lastResult = new WebhookResult
                {
                    Success = false,
                    Error = ex.Message,
                    ProcessedAt = DateTime.UtcNow,
                    AttemptCount = attemptCount,
                    FinalDisposition = "exception",
                    Duration = DateTime.UtcNow - startTime,
                    Exception = ex
                };

                // Exponential backoff on exceptions
                if (attemptCount <= maxRetries)
                {
                    await Task.Delay(retryDelay).ConfigureAwait(false);
                    retryDelay = TimeSpan.FromTicks(Math.Min(retryDelay.Ticks * 2, TimeSpan.FromMinutes(5).Ticks));
                }
            }
        }

        // If we exhausted all retries or got a non-retryable error, store in dead letter
        if ((!lastResult?.Success ?? true) && shouldStoreInDeadLetter)
        {
            try
            {
                await _deadLetterStore.StoreAsync(
                    payload,
                    lastResult?.Error ?? lastException?.Message ?? "Unknown error",
                    attemptCount,
                    finalDisposition).ConfigureAwait(false);
            }
            catch
            {
                // Don't fail the operation if dead letter store fails
            }

            lastResult ??= new WebhookResult
            {
                Success = false,
                Error = "All retry attempts failed",
                ProcessedAt = DateTime.UtcNow,
                AttemptCount = maxRetries + 1,
                FinalDisposition = "retry_exhausted",
                Duration = DateTime.UtcNow - startTime
            };
        }

        return lastResult!;
    }

    private ICircuitBreakerPolicy GetCircuitBreaker(string url, int threshold, TimeSpan? duration)
    {
        lock (_lock)
        {
            if (!_circuitBreakers.TryGetValue(url, out var circuitBreaker))
            {
                circuitBreaker = new CircuitBreakerPolicy(
                    failureThreshold: threshold,
                    breakDuration: duration ?? TimeSpan.FromSeconds(30));
                _circuitBreakers[url] = circuitBreaker;
            }
            return circuitBreaker;
        }
    }

    private TimeSpan GetRetryDelay(string eventType, TimeSpan? initialDelay)
    {
        lock (_lock)
        {
            var key = $"retry_delay_{eventType}";
            if (!_retryDelays.TryGetValue(key, out var delay))
            {
                delay = initialDelay ?? TimeSpan.FromSeconds(1);
                _retryDelays[key] = delay;
            }
            return delay;
        }
    }
}

/// <summary>
/// Represents a webhook payload
/// </summary>
public sealed class WebhookPayload
{
    /// <summary>Unique identifier for this webhook</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Type of event that triggered this webhook</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>When the event occurred</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>The actual event data</summary>
    public Dictionary<string, object> Data { get; set; } = [];

    /// <summary>Version of the webhook format</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Result of processing a webhook
/// </summary>
public sealed class WebhookResult
{
    /// <summary>Whether processing was successful</summary>
    public bool Success { get; set; }

    /// <summary>Success message</summary>
    public string? Message { get; set; }

    /// <summary>Error message if processing failed</summary>
    public string? Error { get; set; }

    /// <summary>When the webhook was processed</summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>Number of delivery attempts made</summary>
    public int AttemptCount { get; set; }

    /// <summary>Final disposition of the webhook delivery</summary>
    /// <remarks>Values: "success", "failed", "circuit_open", "retry_exhausted", "timeout", "http_error", "client_error", "exception"</remarks>
    public string? FinalDisposition { get; set; }

    /// <summary>Duration of the last attempt</summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>HTTP status code if applicable</summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>Exception if one occurred during processing</summary>
    public Exception? Exception { get; set; }

    /// <summary>Indicates if the result represents a circuit breaker open state</summary>
    public bool IsCircuitBreakerOpen => string.Equals(FinalDisposition, "circuit_open", StringComparison.OrdinalIgnoreCase);

    /// <summary>Indicates if all retry attempts were exhausted</summary>
    public bool IsRetryExhausted => string.Equals(FinalDisposition, "retry_exhausted", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Webhook event types
/// </summary>
public static class WebhookEvents
{
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string UserDeleted = "user.deleted";
    public const string OrderCreated = "order.created";
    public const string OrderShipped = "order.shipped";
    public const string OrderCancelled = "order.cancelled";
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string ProductDeleted = "product.deleted";
}