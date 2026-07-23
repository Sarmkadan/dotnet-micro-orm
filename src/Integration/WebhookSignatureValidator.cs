#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Validates webhook signatures with timestamp-based anti-replay protection.
/// Provides constant-time comparison to prevent timing attacks.
/// </summary>
public sealed class WebhookSignatureValidator
{
    private readonly string _secret;
    private readonly TimeSpan _timestampTolerance;
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the WebhookSignatureValidator class
    /// </summary>
    /// <param name="secret">Secret for signature verification</param>
    /// <param name="timestampTolerance">Maximum allowed time difference between webhook timestamp and current time (default: 5 minutes)</param>
    /// <param name="clock">Clock for testing (optional)</param>
    /// <exception cref="ArgumentException">Thrown if secret is null or whitespace</exception>
    public WebhookSignatureValidator(
        string secret,
        TimeSpan? timestampTolerance = null,
        IClock? clock = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        _secret = secret;
        _timestampTolerance = timestampTolerance ?? TimeSpan.FromMinutes(5);
        _clock = clock ?? SystemClock.Instance;
    }

    /// <summary>
    /// Validates a webhook signature with timestamp verification
    /// </summary>
    /// <param name="payload">Webhook payload to validate</param>
    /// <param name="signatureHeader">Signature header value (format: "t={timestamp},v1={signature}")</param>
    /// <returns>True if signature is valid and timestamp is within tolerance; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload is null</exception>
    public bool ValidateSignature(WebhookPayload payload, string? signatureHeader)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        // Parse signature header (format: "t={timestamp},v1={signature}")
        var (timestamp, signature) = ParseSignatureHeader(signatureHeader);
        if (timestamp == null || signature == null)
        {
            return false;
        }

        // Verify timestamp is within tolerance window
        if (!IsTimestampValid(timestamp.Value))
        {
            return false;
        }

        // Verify signature
        return VerifySignature(payload, signature, timestamp.Value);
    }

    /// <summary>
    /// Generates a signature header for a webhook payload
    /// </summary>
    /// <param name="payload">Webhook payload to sign</param>
    /// <returns>Signature header string (format: "t={timestamp},v1={signature}")</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload is null</exception>
    public string GenerateSignatureHeader(WebhookPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        // Use payload timestamp if available, otherwise use current time
        var timestamp = payload.Timestamp > DateTime.MinValue
            ? payload.Timestamp
            : _clock.UtcNow;

        var unixTimestamp = timestamp.ToUnixTimeSeconds();
        var signature = GenerateSignature(payload, unixTimestamp);
        return $"t={unixTimestamp},v1={signature}";
    }

    /// <summary>
    /// Verifies the timestamp is within the allowed tolerance window
    /// </summary>
    /// <param name="timestamp">Unix timestamp to verify</param>
    /// <returns>True if timestamp is valid; otherwise false</returns>
    private bool IsTimestampValid(long timestamp)
    {
        var timestampDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        var now = _clock.UtcNow;
        var timeDifference = now - timestampDateTime;

        return Math.Abs(timeDifference.TotalSeconds) <= _timestampTolerance.TotalSeconds;
    }

    /// <summary>
    /// Verifies the HMAC signature using constant-time comparison
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Expected signature</param>
    /// <param name="timestamp">Timestamp used in signature</param>
    /// <returns>True if signature matches; otherwise false</returns>
    private bool VerifySignature(WebhookPayload payload, string signature, long timestamp)
    {
        var expectedSignature = GenerateSignature(payload, timestamp);

        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature));
    }

    /// <summary>
    /// Generates HMAC-SHA256 signature for a webhook payload at a specific timestamp
    /// </summary>
    /// <param name="payload">Webhook payload to sign</param>
    /// <param name="timestamp">Timestamp to include in signature</param>
    /// <returns>Hex-encoded HMAC-SHA256 signature</returns>
    private string GenerateSignature(WebhookPayload payload, long timestamp)
    {
        // Convert timestamp to DateTime
        var timestampDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

        // Serialize payload to JSON
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

        // Create signature string: timestamp.payloadJson
        var signatureString = $"{timestamp}.{payloadJson}";

        // Compute HMAC-SHA256
        var secretBytes = Encoding.UTF8.GetBytes(_secret);
        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureString));

        // Return hex-encoded signature
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Gets the secret for backward compatibility with legacy signature generation
    /// </summary>
    /// <returns>Secret value</returns>
    internal string GetSecretForBackwardCompatibility()
    {
        return _secret;
    }

    /// <summary>
    /// Parses a signature header into timestamp and signature components
    /// </summary>
    /// <param name="signatureHeader">Signature header string</param>
    /// <returns>Tuple of (timestamp, signature) or (null, null) if parsing fails</returns>
    private static (long? timestamp, string? signature) ParseSignatureHeader(string signatureHeader)
    {
        try
        {
            // Split by comma to get individual components
            var parts = signatureHeader.Split(',');
            if (parts.Length < 2)
            {
                return (null, null);
            }

            long? timestamp = null;
            string? signature = null;

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length != 2)
                {
                    continue;
                }

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                if (key.Equals("t", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(value, out var parsedTimestamp))
                    {
                        timestamp = parsedTimestamp;
                    }
                }
                else if (key.Equals("v1", StringComparison.OrdinalIgnoreCase))
                {
                    signature = value;
                }
            }

            return (timestamp, signature);
        }
        catch
        {
            return (null, null);
        }
    }
}

/// <summary>
/// Represents a clock interface for testing purposes
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }
}

/// <summary>
/// System clock implementation
/// </summary>
internal sealed class SystemClock : IClock
{
    public static readonly IClock Instance = new SystemClock();

    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>
/// Extension methods for DateTimeOffset to convert to Unix timestamp
/// </summary>
internal static class DateTimeOffsetExtensions
{
    public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset)
    {
        return (long)dateTimeOffset.Subtract(DateTimeOffset.UnixEpoch).TotalSeconds;
    }

    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return ToUnixTimeSeconds(DateTimeOffset.FromUnixTimeSeconds((long)(dateTime - DateTime.UnixEpoch).TotalSeconds));
    }
}