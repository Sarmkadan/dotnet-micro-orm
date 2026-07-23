#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Circuit breaker policy that monitors failures and opens the circuit when a threshold is reached
/// </summary>
public interface ICircuitBreakerPolicy
{
    /// <summary>
    /// Executes an action with circuit breaker protection
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Task representing the operation</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit is open</exception>
    Task ExecuteAsync(Func<Task> action);

    /// <summary>
    /// Executes a function with circuit breaker protection
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="func">Function to execute</param>
    /// <returns>Task with result</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit is open</exception>
    Task<T> ExecuteAsync<T>(Func<Task<T>> func);

    /// <summary>
    /// Gets the current state of the circuit breaker
    /// </summary>
    CircuitBreakerState CurrentState { get; }

    /// <summary>
    /// Gets the number of failures since the circuit was last closed
    /// </summary>
    int FailureCount { get; }

    /// <summary>
    /// Gets the number of successful calls since the circuit was last closed
    /// </summary>
    int SuccessCount { get; }
}