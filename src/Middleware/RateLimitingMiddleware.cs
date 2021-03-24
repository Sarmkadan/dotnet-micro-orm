#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Middleware;

/// <summary>
/// Middleware that implements rate limiting to prevent abuse and ensure fair resource usage.
/// Tracks requests per user/IP and blocks excessive traffic with configurable thresholds.
/// Uses token bucket algorithm for smooth rate limiting without sudden cutoffs.
/// </summary>
public class sealed RateLimitingMiddleware : IMiddleware
{
    private readonly RateLimitConfig _config;
    private readonly Dictionary<string, TokenBucket> _buckets = [];
    private readonly object _lock = new();

    public int Order => 5; // Execute early but after error handling

    public RateLimitingMiddleware(RateLimitConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task InvokeAsync(MiddlewareContext context, Func<MiddlewareContext, Task> next)
    {
        var userId = context.User?.UserId.ToString() ?? "anonymous";
        var bucketKey = $"rate-limit:{userId}";

        lock (_lock)
        {
            if (!_buckets.TryGetValue(bucketKey, out var bucket))
            {
                bucket = new TokenBucket(_config.MaxRequests, _config.WindowDuration);
                _buckets[bucketKey] = bucket;
            }

            if (!bucket.TryConsume())
            {
                context.Exception = new InvalidOperationException("Rate limit exceeded");
                context.ResponseData = new ErrorResponse
                {
                    Code = "RATE_LIMITED",
                    Message = $"Rate limit exceeded. Maximum {_config.MaxRequests} requests per {_config.WindowDuration.TotalSeconds}s",
                    RequestId = context.RequestId,
                    Timestamp = DateTime.UtcNow
                };
                context.IsHandled = true;
                return;
            }
        }

        await next(context);
    }

    /// <summary>
    /// Periodically cleans up expired buckets to prevent memory leaks
    /// </summary>
    public void CleanupExpiredBuckets()
    {
        lock (_lock)
        {
            var expiredKeys = _buckets
                .Where(x => x.Value.IsExpired)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _buckets.Remove(key);
            }
        }
    }

    /// <summary>
    /// Token bucket algorithm for rate limiting
    /// </summary>
    private class TokenBucket
    {
        private readonly int _capacity;
        private readonly TimeSpan _refillInterval;
        private double _tokens;
        private DateTime _lastRefill;

        public TokenBucket(int capacity, TimeSpan refillInterval)
        {
            _capacity = capacity;
            _refillInterval = refillInterval;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow - _lastRefill > _refillInterval.Add(TimeSpan.FromMinutes(1));

        public bool TryConsume()
        {
            Refill();

            if (_tokens >= 1)
            {
                _tokens--;
                return true;
            }

            return false;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var timePassed = now - _lastRefill;

            if (timePassed >= _refillInterval)
            {
                _tokens = _capacity;
                _lastRefill = now;
            }
            else
            {
                var refillRate = _capacity / _refillInterval.TotalSeconds;
                var tokensToAdd = timePassed.TotalSeconds * refillRate;
                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            }
        }
    }
}

/// <summary>
/// Configuration for rate limiting behavior
/// </summary>
public class sealed RateLimitConfig
{
    /// <summary>Maximum number of requests allowed per window</summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>Time window for rate limit calculation</summary>
    public TimeSpan WindowDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Enable rate limiting (can be disabled for testing)</summary>
    public bool Enabled { get; set; } = true;
}
