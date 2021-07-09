# DateTimeHelper

Provides a collection of static utility methods for common DateTime operations including Unix timestamp conversion, business day calculations, date boundary normalization, ISO 8601 formatting and parsing, relative time representation, and business hours validation. All methods are pure functions with no shared state.

## API

### `public static DateTime FromUnixTimestamp(long timestamp, bool isMilliseconds = false)`

Converts a Unix timestamp to a UTC DateTime.

**Parameters**
- `timestamp` — Seconds (or milliseconds if `isMilliseconds` is true) since 1970-01-01T00:00:00Z.
- `isMilliseconds` — When true, treats `timestamp` as milliseconds; otherwise as seconds.

**Returns**  
A `DateTime` with `Kind == DateTimeKind.Utc` representing the instant.

**Throws**  
`ArgumentOutOfRangeException` if the resulting value falls outside `DateTime.MinValue`/`DateTime.MaxValue`.

---

### `public static long ToUnixTimestamp(DateTime dateTime, bool asMilliseconds = false)`

Converts a DateTime to a Unix timestamp.

**Parameters**
- `dateTime` — The instant to convert. If `Kind` is `Unspecified`, it is treated as UTC. If `Local`, it is converted to UTC.
- `asMilliseconds` — When true, returns milliseconds since epoch; otherwise seconds.

**Returns**  
Signed integer seconds (or milliseconds) since 1970-01-01T00:00:00Z.

**Throws**  
`ArgumentOutOfRangeException` if `dateTime` is outside the representable Unix timestamp range.

---

### `public static int GetBusinessDaysBetween(DateTime start, DateTime end)`

Calculates the number of business days (Monday–Friday) between two dates, exclusive of the end date.

**Parameters**
- `start` — Inclusive start date (time component ignored).
- `end` — Exclusive end date (time component ignored).

**Returns**  
Count of business days in `[start, end)`. Returns 0 if `start >= end`. Negative if `start > end`.

**Throws**  
None.

---

### `public static DateTime GetStartOfDay(DateTime dateTime)`

Returns a new DateTime at 00:00:00.000 of the same day, preserving `Kind`.

**Parameters**
- `dateTime` — Input date.

**Returns**  
DateTime with time component zeroed, same `Kind` as input.

**Throws**  
None.

---

### `public static DateTime GetEndOfDay(DateTime dateTime)`

Returns a new DateTime at 23:59:59.999 of the same day, preserving `Kind`.

**Parameters**
- `dateTime` — Input date.

**Returns**  
DateTime representing the last millisecond of the day, same `Kind` as input.

**Throws**  
None.

---

### `public static DateTime GetStartOfWeek(DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)`

Returns the first day of the week containing `dateTime`, at 00:00:00.000.

**Parameters**
- `dateTime` — Input date.
- `startOfWeek` — Day considered the first day of the week (default Monday).

**Returns**  
DateTime at start of week, same `Kind` as input.

**Throws**  
`ArgumentOutOfRangeException` if `startOfWeek` is not a valid `DayOfWeek` value.

---

### `public static DateTime GetStartOfMonth(DateTime dateTime)`

Returns the first day of the month at 00:00:00.000, preserving `Kind`.

**Parameters**
- `dateTime` — Input date.

**Returns**  
DateTime representing 1st of the month at midnight, same `Kind` as input.

**Throws**  
None.

---

### `public static DateTime GetEndOfMonth(DateTime dateTime)`

Returns the last day of the month at 23:59:59.999, preserving `Kind`.

**Parameters**
- `dateTime` — Input date.

**Returns**  
DateTime representing the last millisecond of the month, same `Kind` as input.

**Throws**  
None.

---

### `public static string ToIso8601(DateTime dateTime, bool includeMilliseconds = true)`

Formats a DateTime as an ISO 8601 string (UTC with 'Z' designator).

**Parameters**
- `dateTime` — Input date. Converted to UTC if `Kind` is `Local` or `Unspecified`.
- `includeMilliseconds` — When true, includes `.fff` fractional seconds.

**Returns**  
String in format `yyyy-MM-ddTHH:mm:ssZ` or `yyyy-MM-ddTHH:mm:ss.fffZ`.

**Throws**  
None.

---

### `public static DateTime ParseIso8601(string iso8601)`

Parses an ISO 8601 string to a UTC DateTime.

**Parameters**
- `iso8601` — String in ISO 8601 format (e.g., `2024-01-15T14:30:00Z` or `2024-01-15T14:30:00.000Z`).

**Returns**  
DateTime with `Kind == DateTimeKind.Utc`.

**Throws**  
`FormatException` if the string is not a valid ISO 8601 representation.  
`ArgumentNullException` if `iso8601` is null.

---

### `public static string ToRelativeTime(DateTime dateTime, DateTime? relativeTo = null)`

Returns a human-readable relative time string (e.g., "2 hours ago", "in 3 days").

**Parameters**
- `dateTime` — The target instant.
- `relativeTo` — Reference instant (defaults to `DateTime.UtcNow`). If `Kind` differs, both are normalized to UTC.

**Returns**  
Localized-style string describing the offset. Past tense for past dates, future tense for future dates. Returns "just now" for differences under 30 seconds.

**Throws**  
None.

---

### `public static DateTime AddBusinessDays(DateTime start, int businessDays)`

Adds or subtracts business days (Monday–Friday) from a date.

**Parameters**
- `start` — Starting date (time component preserved).
- `businessDays` — Number of business days to add (positive) or subtract (negative).

**Returns**  
DateTime offset by the specified business days, same `Kind` as input.

**Throws**  
None.

---

### `public static bool IsBusinessHours(DateTime dateTime, TimeSpan? start = null, TimeSpan? end = null, DayOfWeek[]? businessDays = null)`

Determines whether a DateTime falls within configured business hours.

**Parameters**
- `dateTime` — Instant to test. Time component evaluated in the DateTime's own `Kind` (no conversion).
- `start` — Inclusive start of business day (default 09:00).
- `end` — Exclusive end of business day (default 17:00).
- `businessDays` — Days considered business days (default Monday–Friday).

**Returns**  
`true` if `dateTime`'s time-of-day is within `[start, end)` and its day-of-week is in `businessDays`; otherwise `false`.

**Throws**  
`ArgumentException` if `start >= end`.  
`ArgumentOutOfRangeException` if `businessDays` contains invalid `DayOfWeek` values.

---

## Usage

```csharp
// Calculate settlement date: 3 business days from trade date, then format for API
var tradeDate = new DateTime(2024, 3, 15, 14, 30, 0, DateTimeKind.Utc);
var settlement = DateTimeHelper.AddBusinessDays(tradeDate, 3);
var payload = new
{
    TradeDate = DateTimeHelper.ToIso8601(tradeDate),
    SettlementDate = DateTimeHelper.ToIso8601(settlement),
    UnixTimestamp = DateTimeHelper.ToUnixTimestamp(settlement)
};
// payload: { TradeDate: "2024-03-15T14:30:00Z", SettlementDate: "2024-03-20T14:30:00Z", UnixTimestamp: 1710945000 }
```

```csharp
// Generate a daily report window and check if current time is within business hours
var today = DateTime.UtcNow;
var windowStart = DateTimeHelper.GetStartOfDay(today);
var windowEnd = DateTimeHelper.GetEndOfDay(today);
var isOpen = DateTimeHelper.IsBusinessHours(
    DateTime.UtcNow,
    start: TimeSpan.FromHours(9),
    end: TimeSpan.FromHours(17),
    businessDays: [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]
);
var reportPeriod = $"{DateTimeHelper.ToIso8601(windowStart)}/{DateTimeHelper.ToIso8601(windowEnd)}";
// reportPeriod: "2024-03-18T00:00:00Z/2024-03-18T23:59:59.999Z"
```

---

## Notes

- **Time zone handling**: Methods that accept `DateTime` preserve the input `Kind` unless explicitly documented (e.g., `ToIso8601` and `ParseIso8601` always operate in UTC). Callers should ensure consistent `Kind` usage across related operations.
- **Unix timestamp range**: `FromUnixTimestamp` and `ToUnixTimestamp` support the full `DateTime` range (year 1–9999). Values outside the traditional Unix range (1970–2038 for 32-bit) are handled correctly using 64-bit arithmetic.
- **Business day logic**: `GetBusinessDaysBetween`, `AddBusinessDays`, and `IsBusinessHours` use a fixed Monday–Friday definition unless overridden. Holidays are not considered; integrate a holiday calendar separately if required.
- **Boundary precision**: `GetEndOfDay` and `GetEndOfMonth` use 23:59:59.999 (millisecond precision). For `DateTime` comparisons covering a full day, prefer half-open intervals `[start, end)` using `GetStartOfDay` and `GetStartOfDay(nextDay)`.
- **Thread safety**: All methods are stateless and thread-safe. No synchronization is required for concurrent calls.
- **Relative time granularity**: `ToRelativeTime` switches units at thresholds (seconds → minutes → hours → days → weeks → months → years). The output is not localized; it uses English phrases suitable for logging or UI tooltips.
- **ISO 8601 strictness**: `ParseIso8601` accepts the common subset used in web APIs (UTC 'Z' designator, optional fractional seconds). It does not support offset designators (e.g., `+02:00`) or week/ordinal date formats.
