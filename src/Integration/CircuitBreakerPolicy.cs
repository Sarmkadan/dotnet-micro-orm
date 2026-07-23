#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Circuit breaker policy implementation that monitors failures and opens the circuit when a threshold is reached
/// </summary>
public sealed class CircuitBreakerPolicy : ICircuitBreakerPolicy
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;
    private readonly int _halfOpenAttempts;
    private readonly object _lock = new object();

    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount;
    private int _successCount;
    private DateTime _lastStateChange = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerPolicy class
    /// </summary>
    /// <param name="failureThreshold">Number of failures before opening the circuit (default: 5)</param>
    /// <param name="breakDuration">Duration to keep the circuit open before transitioning to half-open (default: 30 seconds)</param>
    /// <param name="halfOpenAttempts">Number of attempts in half-open state before determining final state (default: 3)</param>
    public CircuitBreakerPolicy(
        int failureThreshold = 5,
        TimeSpan? breakDuration = null,
        int halfOpenAttempts = 3)
    {
        _failureThreshold = failureThreshold > 0 ? failureThreshold : throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be positive");
        _breakDuration = breakDuration ?? TimeSpan.FromSeconds(30);
        _halfOpenAttempts = halfOpenAttempts > 0 ? halfOpenAttempts : throw new ArgumentOutOfRangeException(nameof(halfOpenAttempts), "Half-open attempts must be positive");
    }

    /// <inheritdoc/>
    public CircuitBreakerState CurrentState => _state;

    /// <inheritdoc/>
    public int FailureCount => _failureCount;

    /// <inheritdoc/>
    public int SuccessCount => _successCount;

    /// <inheritdoc/>
    public async Task ExecuteAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        await ExecuteInternalAsync(async () =>
        {
            await action().ConfigureAwait(false);
            return true;
        }).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return await ExecuteInternalAsync(func).ConfigureAwait(false);
    }

    private async Task<T> ExecuteInternalAsync<T>(Func<Task<T>> func)
    {
        lock (_lock)
        {
            if (_state == CircuitBreakerState.Open)
            {
                // Check if break duration has elapsed
                if (DateTime.UtcNow >= _lastStateChange + _breakDuration)
                {
                    _state = CircuitBreakerState.HalfOpen;
                    _successCount = 0;
                }
                else
                {
                    throw new CircuitBreakerOpenException(
                        _lastStateChange + _breakDuration,
                        _state,
                        $"Circuit breaker is open. Retry after {_lastStateChange + _breakDuration:O}");
                }
            }
        }

        try
        {
            var result = await func().ConfigureAwait(false);

            lock (_lock)
            {
                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _successCount++;
                    if (_successCount >= _halfOpenAttempts)
                    {
                        // Half-open state succeeded, close the circuit
                        _state = CircuitBreakerState.Closed;
                        _failureCount = 0;
                        _successCount = 0;
                    }
                }
                else if (_state == CircuitBreakerState.Closed)
                {
                    _successCount++;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
            throw;
        }
    }

    private void HandleFailure(Exception exception)
    {
        lock (_lock)
        {
            _failureCount++;

            // Only track failures in closed state
            if (_state == CircuitBreakerState.Closed && _failureCount >= _failureThreshold)
            {
                _state = CircuitBreakerState.Open;
                _lastStateChange = DateTime.UtcNow;
                _failureCount = 0;
            }
        }
    }
}