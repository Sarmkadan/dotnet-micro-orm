#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for CircuitBreakerState providing additional convenience
// and functionality while maintaining the same namespace.
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Extension methods for <see cref="CircuitBreakerState"/> providing convenience predicates.
/// </summary>
public static class CircuitBreakerStateExtensions
{
    /// <summary>
    /// Determines whether the circuit breaker is open and requests are being rejected.
    /// </summary>
    /// <param name="state">The circuit breaker state.</param>
    /// <returns><c>true</c> if the state is <see cref="CircuitBreakerState.Open"/>; otherwise, <c>false</c>.</returns>
    public static bool IsOpen(this CircuitBreakerState state)
    {
        return state == CircuitBreakerState.Open;
    }

    /// <summary>
    /// Determines whether the circuit breaker is closed and requests are allowed to pass through.
    /// </summary>
    /// <param name="state">The circuit breaker state.</param>
    /// <returns><c>true</c> if the state is <see cref="CircuitBreakerState.Closed"/>; otherwise, <c>false</c>.</returns>
    public static bool IsClosed(this CircuitBreakerState state)
    {
        return state == CircuitBreakerState.Closed;
    }

    /// <summary>
    /// Determines whether the circuit breaker is half-open and probing if the fault has been resolved.
    /// </summary>
    /// <param name="state">The circuit breaker state.</param>
    /// <returns><c>true</c> if the state is <see cref="CircuitBreakerState.HalfOpen"/>; otherwise, <c>false</c>.</returns>
    public static bool IsHalfOpen(this CircuitBreakerState state)
    {
        return state == CircuitBreakerState.HalfOpen;
    }
}