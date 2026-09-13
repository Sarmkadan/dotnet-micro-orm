# Circuit Breaker

The circuit breaker pattern implementation in DotnetMicroOrm provides fault tolerance for external service calls by preventing cascading failures and allowing systems to recover gracefully.

## Overview

The circuit breaker monitors for failures and temporarily stops requests to a failing service, giving it time to recover. After a configured timeout, it allows a limited number of test requests to determine if the service has recovered.

## States

The circuit breaker operates in three distinct states:

### Closed
- Normal operation state where requests are allowed to pass through
- Failure count is tracked; when it reaches the threshold, the circuit transitions to Open
- Success count is incremented on successful calls

### Open
- Requests are immediately rejected without calling the protected operation
- After the break duration elapses, the circuit transitions to HalfOpen
- Any attempt to execute throws a `CircuitBreakerOpenException`

### HalfOpen
- Limited number of requests are allowed to probe if the underlying issue has been resolved
- Success count is tracked; if it reaches the half-open attempts threshold, the circuit closes
- Any failure causes an immediate transition back to Open

## Configuration Options

### CircuitBreakerPolicy Constructor Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `failureThreshold` | `int` | `5` | Number of consecutive failures before opening the circuit |
| `breakDuration` | `TimeSpan` | `30 seconds` | Duration to keep the circuit open before transitioning to half-open |
| `halfOpenAttempts` | `int` | `3` | Number of successful attempts in half-open state required to close the circuit |

## Usage Examples

### Basic Usage

```csharp
// Create a circuit breaker with default settings (5 failures, 30s break, 3 half-open attempts)
var circuitBreaker = new CircuitBreakerPolicy();

// Execute an operation with circuit breaker protection
try
{
    var result = await circuitBreaker.ExecuteAsync(async () =>
    {
        // Your operation that might fail
        return await httpClient.GetAsync("https://api.example.com/data");
    });
    
    // Handle successful result
}
catch (CircuitBreakerOpenException ex)
{
    // Circuit is open - handle accordingly
    Console.WriteLine($"Service unavailable. Retry after: ex.RetryAfter");
}
```

### Custom Configuration

```csharp
// Create a circuit breaker with custom settings
var circuitBreaker = new CircuitBreakerPolicy(
    failureThreshold: 3,           // Open after 3 failures
    breakDuration: TimeSpan.FromSeconds(60),  // Stay open for 60 seconds
    halfOpenAttempts: 2            // Require 2 successes in half-open to close
);

// Execute a function that returns a value
try
{
    var data = await circuitBreaker.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetFromJsonAsync<User>("https://api.example.com/users/1");
        return response;
    });
}
catch (CircuitBreakerOpenException ex)
{
    // Handle open circuit
    return GetCachedUserData(); // Fallback to cached data
}
```

### Monitoring Circuit State

```csharp
// Check current state without executing
var state = circuitBreaker.CurrentState;
var failureCount = circuitBreaker.FailureCount;
var successCount = circuitBreaker.SuccessCount;

if (state == CircuitBreakerState.Open)
{
    // Log or alert about open circuit
    logger.Warning("Circuit breaker is open for {Url}", url);
}
```

## Integration with WebhookHandler

The circuit breaker is integrated into the WebhookHandler service for reliable webhook delivery:

```csharp
// In WebhookHandler configuration
var webhookHandler = new WebhookHandler(
    httpClient: httpClient,
    deadLetterStore: deadLetterStore,
    circuitBreakerThreshold: 5,    // Open circuit after 5 failures
    circuitBreakerDuration: TimeSpan.FromSeconds(30)  // Keep open for 30 seconds
);

// Each unique URL gets its own circuit breaker instance
await webhookHandler.SendWebhookAsync(webhookPayload, "https://service.example.com/webhook");
```

## Exception Handling

When the circuit breaker is open, it throws a `CircuitBreakerOpenException` which contains:

- `RetryAfter`: DateTime when the circuit will next allow requests
- `State`: Current state of the circuit breaker (Open/HalfOpen/Closed)
- `Message`: Descriptive error message including retry information

## Thread Safety

The circuit breaker implementation is thread-safe and can be used concurrently from multiple threads. Internal state is protected by locks to ensure consistent behavior under concurrent access.

## Best Practices

1. **Appropriate Thresholds**: Set failure thresholds based on your service's expected error rate
2. **Reasonable Break Duration**: Set break duration long enough for the service to recover but short enough to resume normal operation quickly
3. **Monitoring**: Monitor circuit breaker state and failure counts for operational insights
4. **Fallback Strategies**: Implement fallback mechanisms when the circuit is open (cached data, default values, etc.)
5. **Granularity**: Use separate circuit breaker instances for different services or endpoints

## Notes

- The circuit breaker only tracks failures when in the Closed state
- Success counts are reset when transitioning between states
- All state transitions are atomic and thread-safe
- The implementation follows the standard circuit breaker pattern as described by Martin Fowler