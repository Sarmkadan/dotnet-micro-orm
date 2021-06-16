# RateLimitingMiddleware

Middleware component that enforces rate limiting using the token bucket algorithm. It tracks request counts within configurable time windows and rejects requests when the configured maximum number of requests is exceeded. The middleware is designed to be thread-safe and supports dynamic enable/disable switching.

## API

### `public RateLimitingMiddleware(int maxRequests, TimeSpan windowDuration)`

Initializes a new instance of the rate limiting middleware with the specified maximum number of requests and time window.

**Parameters**
- `maxRequests`: The maximum number of requests allowed within the time window.
- `windowDuration`: The duration of the sliding time window during which requests are counted.

**Exceptions**
- `ArgumentOutOfRangeException`: Thrown when `maxRequests` is less than or equal to zero or when `windowDuration` is negative.

---

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware to process an HTTP request. If the request exceeds the rate limit, a `429 Too Many Requests` response is returned. Otherwise, the request proceeds to the next middleware in the pipeline.

**Parameters**
- `context`: The HTTP context for the current request.
- `next`: The delegate representing the next middleware in the pipeline.

**Return Value**
- A `Task` representing the asynchronous operation.

---

### `public void CleanupExpiredBuckets()`

Removes expired rate limit tracking buckets from the internal store. This method should be called periodically to prevent unbounded memory growth.

**Remarks**
- This method is safe to call from any thread.
- It does not block concurrent invocations of `TryConsume`.

---

### `public TokenBucket Public { get; }`

Gets the token bucket instance used for rate limiting. The bucket encapsulates the current state of request counts and expiration logic.

**Remarks**
- The returned bucket is mutable and reflects real-time changes to the rate limiting state.
- Direct manipulation of the bucket may lead to inconsistent rate limiting behavior.

---

### `public bool TryConsume()`

Attempts to consume a token from the bucket, decrementing the available request count if successful.

**Return Value**
- `true` if a token was consumed and the request should be allowed; otherwise, `false`.

**Remarks**
- This method is thread-safe and may be called concurrently by multiple requests.
- It does not block and returns immediately.

---
### `public int MaxRequests { get; }`

Gets the maximum number of requests allowed within the configured time window.

---
### `public TimeSpan WindowDuration { get; }`

Gets the duration of the sliding time window during which requests are counted.
---
### `public bool Enabled { get; set; }`

Gets or sets a value indicating whether the rate limiting middleware is enabled. When set to `false`, all requests bypass rate limiting checks.

**Remarks**
- Changes to this property are not atomic; concurrent reads and writes may briefly observe inconsistent states.
- Thread-safe for individual reads and writes, but not for compound operations (e.g., check-then-set).

## Usage

### Basic Middleware Registration
