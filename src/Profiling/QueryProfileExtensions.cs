#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Profiling;

/// <summary>
/// Extension methods for <see cref="QueryProfile"/> to aid formatting and
/// slow-query detection.
/// </summary>
public static class QueryProfileExtensions
{
    /// <summary>
    /// Formats the profile's duration as a human-readable string, e.g. "12.3 ms".
    /// </summary>
    public static string FormatDuration(this QueryProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return FormatDuration(profile.Duration);
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a human-readable string, e.g. "12.3 ms".
    /// </summary>
    public static string FormatDuration(this TimeSpan duration)
    {
        if (duration.TotalMilliseconds < 1)
            return $"{duration.TotalMicroseconds:0.##} µs";

        if (duration.TotalSeconds < 1)
            return $"{duration.TotalMilliseconds:0.##} ms";

        if (duration.TotalMinutes < 1)
            return $"{duration.TotalSeconds:0.##} s";

        return $"{duration.TotalMinutes:0.##} min";
    }

    /// <summary>
    /// Returns <c>true</c> when the profile's duration exceeds the given threshold.
    /// </summary>
    public static bool IsSlow(this QueryProfile profile, TimeSpan threshold)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return profile.Duration > threshold;
    }

    /// <summary>
    /// Returns <c>true</c> when the profile's duration exceeds the given threshold
    /// expressed in milliseconds.
    /// </summary>
    public static bool IsSlow(this QueryProfile profile, double thresholdMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return profile.Duration.TotalMilliseconds > thresholdMilliseconds;
    }
}