#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Represents the state of a circuit breaker
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Circuit is closed and requests are allowed to pass through</summary>
    Closed,

    /// <summary>Circuit is open and requests are immediately rejected</summary>
    Open,

    /// <summary>Circuit is half-open, allowing limited requests to probe if the fault has been resolved</summary>
    HalfOpen
}