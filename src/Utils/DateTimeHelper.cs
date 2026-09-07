#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Provides utility methods for common DateTime operations.
/// Handles timezone conversions, formatting, and datetime calculations
/// with proper handling of edge cases and performance considerations.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Gets current UTC time with millisecond precision (consistent across app)
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Converts Unix timestamp (seconds since epoch) to DateTime
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="seconds"/> is outside the range supported by <see cref="DateTimeOffset"/>.
    /// </exception>
    public static DateTime FromUnixTimestamp(long seconds)
    {
        const long minUnixSeconds = -62_135_596_800;
        const long maxUnixSeconds = 253_402_300_799;

        if (seconds < minUnixSeconds || seconds > maxUnixSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                seconds,
                $"Unix timestamp must be between {minUnixSeconds} and {maxUnixSeconds} seconds, inclusive.");
        }

        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch)
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            dateTime = dateTime.ToUniversalTime();

        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)dateTime.Subtract(epoch).TotalSeconds;
    }

    /// <summary>
    /// Calculates business days between two dates (excludes weekends)
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="end"/> is earlier than <paramref name="start"/>.
    /// </exception>
    public static int GetBusinessDaysBetween(DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentException(
                $"End date '{end:O}' must be greater than or equal to start date '{start:O}'.",
                nameof(end));
        }

        int businessDays = 0;
        var current = start;

        while (current < end)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// Gets the start of day (00:00:00) for given date
    /// </summary>
    public static DateTime GetStartOfDay(this DateTime dateTime)
        => dateTime.Date;

    /// <summary>
    /// Gets the end of day (23:59:59.999) for given date
    /// </summary>
    public static DateTime GetEndOfDay(this DateTime dateTime)
        => dateTime.Date.AddDays(1).AddMilliseconds(-1);

    /// <summary>
    /// Gets the start of week (Monday) for given date
    /// </summary>
    public static DateTime GetStartOfWeek(this DateTime dateTime)
    {
        var daysToMonday = (int)dateTime.DayOfWeek - 1;
        if (daysToMonday < 0)
            daysToMonday = 6; // If Sunday, go back 6 days

        return dateTime.AddDays(-daysToMonday).Date;
    }

    /// <summary>
    /// Gets the start of month for given date
    /// </summary>
    public static DateTime GetStartOfMonth(this DateTime dateTime)
        => new(dateTime.Year, dateTime.Month, 1);

    /// <summary>
    /// Gets the end of month for given date
    /// </summary>
    public static DateTime GetEndOfMonth(this DateTime dateTime)
    {
        var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        return new DateTime(dateTime.Year, dateTime.Month, lastDay).GetEndOfDay();
    }

    /// <summary>
    /// Formats DateTime to RFC3339 string (ISO 8601)
    /// </summary>
    public static string ToIso8601(this DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Utc
            ? dateTime.ToString("O") // "2023-05-15T14:30:45.1234567Z"
            : dateTime.ToUniversalTime().ToString("O");

    /// <summary>
    /// Parses ISO 8601 string to DateTime
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="value"/> is not a valid round-trip ISO 8601 value.</exception>
    public static DateTime ParseIso8601(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (!DateTime.TryParseExact(value, "O", null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            throw new FormatException($"Invalid ISO 8601 format: {value}");

        return result;
    }

    /// <summary>
    /// Gets human-readable relative time (e.g., "2 hours ago", "in 3 minutes")
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = UtcNow;
        var diff = now - dateTime;

        if (diff.TotalSeconds < 30)
            return "just now";
        if (diff.TotalSeconds < 60)
            return "a moment ago";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes != 1 ? "s" : "")} ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours != 1 ? "s" : "")} ago";
        if (diff.TotalDays < 30)
            return $"{(int)diff.TotalDays} day{((int)diff.TotalDays != 1 ? "s" : "")} ago";

        return dateTime.ToString("G");
    }

    /// <summary>
    /// Adds business days (skipping weekends) to a date
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="days"/> is <see cref="int.MinValue"/>, whose absolute value cannot be represented by an <see cref="int"/>.
    /// </exception>
    public static DateTime AddBusinessDays(this DateTime dateTime, int days)
    {
        if (days == int.MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(days),
                days,
                $"Business-day count cannot be {int.MinValue} because its absolute value cannot be represented.");
        }

        int direction = days > 0 ? 1 : -1;
        int count = Math.Abs(days);
        var result = dateTime;

        while (count > 0)
        {
            result = result.AddDays(direction);

            if (result.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                count--;
        }

        return result;
    }

    /// <summary>
    /// Checks if a date is within business hours (9 AM - 5 PM)
    /// </summary>
    public static bool IsBusinessHours(this DateTime dateTime)
    {
        if (dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return false;

        return dateTime.Hour >= 9 && dateTime.Hour < 17;
    }
}
