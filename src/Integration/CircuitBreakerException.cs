#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Exception thrown when a circuit breaker is open and prevents execution
/// </summary>
public sealed class CircuitBreakerOpenException : Exception
{
    /// <summary>Gets the time when the circuit breaker will next allow requests</summary>
    public DateTime RetryAfter { get; }

    /// <summary>Gets the current state of the circuit breaker</summary>
    public CircuitBreakerState State { get; }

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerOpenException class
    /// </summary>
    /// <param name="retryAfter">Time when the circuit breaker will next allow requests</param>
    /// <param name="state">Current state of the circuit breaker</param>
    /// <param name="message">Error message</param>
    public CircuitBreakerOpenException(DateTime retryAfter, CircuitBreakerState state, string message)
        : base(message)
    {
        RetryAfter = retryAfter;
        State = state;
    }

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerOpenException class
    /// </summary>
    /// <param name="retryAfter">Time when the circuit breaker will next allow requests</param>
    /// <param name="state">Current state of the circuit breaker</param>
    public CircuitBreakerOpenException(DateTime retryAfter, CircuitBreakerState state)
        : this(retryAfter, state, $"Circuit breaker is {state}. Retry after {retryAfter:O}")
    {
    }
}