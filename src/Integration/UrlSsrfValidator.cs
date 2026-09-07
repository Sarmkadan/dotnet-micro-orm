#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Net;
using System.Net.Sockets;

namespace DotnetMicroOrm.Integration;

/// <summary>
/// Provides comprehensive SSRF (Server-Side Request Forgery) protection for URLs.
/// Validates that URLs are safe to access by checking:
/// - Scheme (must be HTTPS)
/// - Host (must not be private, loopback, link-local, or multicast)
/// - DNS resolution (resolves to allowed IP ranges)
/// - DNS rebinding protection (re-resolves at connection time)
/// </summary>
public static class UrlSsrfValidator
{
    private static readonly HashSet<string> _allowedSchemes = new(StringComparer.OrdinalIgnoreCase) { "https" };
    private static readonly TimeSpan _dnsResolutionTimeout = TimeSpan.FromSeconds(5);
    private static readonly int _maxRedirects = 5;
    private static readonly int _maxResponseSizeBytes = 10 * 1024 * 1024; // 10MB
    private static readonly int _requestTimeoutSeconds = 30;

    /// <summary>
    /// Validates a URL for SSRF safety at registration time.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <param name="allowLocalhost">Whether to allow localhost addresses (default: false)</param>
    /// <param name="allowPrivateNetworks">Whether to allow private network addresses (default: false)</param>
    /// <returns>True if the URL is safe, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if url is null</exception>
    /// <exception cref="ArgumentException">Thrown if url is empty or invalid</exception>
    public static bool IsUrlSafe(string url, bool allowLocalhost = false, bool allowPrivateNetworks = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        // Parse URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("URL must be a valid absolute URI", nameof(url));
        }

        // Validate scheme
        if (!_allowedSchemes.Contains(uri.Scheme))
        {
            throw new ArgumentException($"URL scheme '{uri.Scheme}' is not allowed. Only HTTPS is supported.", nameof(url));
        }

        // Validate host
        return IsHostSafe(uri.Host, allowLocalhost, allowPrivateNetworks);
    }

    /// <summary>
    /// Validates a URL for SSRF safety at delivery time.
    /// Performs DNS resolution and checks for DNS rebinding attacks.
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <param name="allowLocalhost">Whether to allow localhost addresses (default: false)</param>
    /// <param name="allowPrivateNetworks">Whether to allow private network addresses (default: false)</param>
    /// <returns>True if the URL is safe, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if url is null</exception>
    /// <exception cref="ArgumentException">Thrown if url is empty or invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown if DNS resolution fails or times out</exception>
    public static async Task<bool> IsUrlSafeAtDeliveryTimeAsync(string url, bool allowLocalhost = false, bool allowPrivateNetworks = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        // Parse URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("URL must be a valid absolute URI", nameof(url));
        }

        // Validate scheme
        if (!_allowedSchemes.Contains(uri.Scheme))
        {
            throw new ArgumentException($"URL scheme '{uri.Scheme}' is not allowed. Only HTTPS is supported.", nameof(url));
        }

        // Validate host
        if (!IsHostSafe(uri.Host, allowLocalhost, allowPrivateNetworks))
        {
            return false;
        }

        // Resolve DNS and validate IP addresses
        var ipAddresses = await ResolveHostToIpAddressesAsync(uri.Host).ConfigureAwait(false);

        if (ipAddresses.Count == 0)
        {
            throw new InvalidOperationException("DNS resolution failed - could not resolve host to any IP addresses");
        }

        // Check if any resolved IP is in a forbidden range
        foreach (var ipAddress in ipAddresses)
        {
            if (IsIpAddressForbidden(ipAddress, allowLocalhost, allowPrivateNetworks))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the maximum allowed response size in bytes (10MB)
    /// </summary>
    public static int MaxResponseSizeBytes => _maxResponseSizeBytes;

    /// <summary>
    /// Gets the default request timeout
    /// </summary>
    public static TimeSpan RequestTimeout => TimeSpan.FromSeconds(_requestTimeoutSeconds);

    /// <summary>
    /// Gets the maximum number of redirects to follow
    /// </summary>
    public static int MaxRedirects => _maxRedirects;

    private static bool IsHostSafe(string host, bool allowLocalhost, bool allowPrivateNetworks)
    {
        // Check for IP address literals
        if (IPAddress.TryParse(host, out var ipAddress))
        {
            return !IsIpAddressForbidden(ipAddress, allowLocalhost, allowPrivateNetworks);
        }

        // Check for localhost variants
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) && !allowLocalhost)
        {
            return false;
        }

        if (host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) && !allowLocalhost)
        {
            return false;
        }

        // Check for common localhost patterns
        if (host.Equals("127.0.0.1", StringComparison.Ordinal) && !allowLocalhost)
        {
            return false;
        }

        if (host.Equals("::1", StringComparison.Ordinal) && !allowLocalhost)
        {
            return false;
        }

        // Check for IPv4 private ranges
        if (host.StartsWith("10.", StringComparison.Ordinal) && !allowPrivateNetworks)
        {
            return false;
        }

        if (host.StartsWith("172.", StringComparison.Ordinal) && !allowPrivateNetworks)
        {
            var octet2 = int.Parse(host.Split('.')[1]);
            if (octet2 >= 16 && octet2 <= 31)
            {
                return false;
            }
        }

        if (host.StartsWith("192.168.", StringComparison.Ordinal) && !allowPrivateNetworks)
        {
            return false;
        }

        // Check for IPv6 loopback and private ranges
        if (host.Equals("::1", StringComparison.Ordinal) && !allowLocalhost)
        {
            return false;
        }

        if (host.StartsWith("fc", StringComparison.OrdinalIgnoreCase) && !allowPrivateNetworks) // IPv6 unique local
        {
            return false;
        }

        if (host.StartsWith("fe80:", StringComparison.OrdinalIgnoreCase)) // IPv6 link-local
        {
            return false;
        }

        if (host.StartsWith("ff0", StringComparison.OrdinalIgnoreCase)) // IPv6 multicast
        {
            return false;
        }

        return true;
    }

    private static bool IsIpAddressForbidden(IPAddress ipAddress, bool allowLocalhost, bool allowPrivateNetworks)
    {
        // Check loopback
        if (IPAddress.IsLoopback(ipAddress) && !allowLocalhost)
        {
            return true;
        }

        // Check link-local
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && ipAddress.IsIPv6LinkLocal)
        {
            return true;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork && IsIpv4LinkLocal(ipAddress))
        {
            return true;
        }

        // Check multicast
        if (ipAddress.IsIPv6Multicast || (ipAddress.AddressFamily == AddressFamily.InterNetwork && (ipAddress.GetAddressBytes()[0] & 0xF0) == 0xE0))
        {
            return true;
        }

        // Check private networks
        if (!allowPrivateNetworks)
        {
            if (IsPrivateIpAddress(ipAddress))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPrivateIpAddress(IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();

        // IPv4 private ranges
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10)
            {
                return true;
            }

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }

            // 169.254.0.0/16 (APIPA)
            if (bytes[0] == 169 && bytes[1] == 254)
            {
                return true;
            }

            // 127.0.0.0/8 (loopback)
            if (bytes[0] == 127)
            {
                return true;
            }
        }

        // IPv6 unique local addresses (fc00::/7)
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (bytes[0] == 0xfc || bytes[0] == 0xfd)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsIpv4LinkLocal(IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();
        return bytes[0] == 169 && bytes[1] == 254;
    }

    private static async Task<List<IPAddress>> ResolveHostToIpAddressesAsync(string host)
    {
        try
        {
            var dnsTask = System.Net.Dns.GetHostAddressesAsync(host);
            if (await Task.WhenAny(dnsTask, Task.Delay(_dnsResolutionTimeout)).ConfigureAwait(false) != dnsTask)
            {
                throw new TimeoutException("DNS resolution timed out");
            }

            return new List<IPAddress>(await dnsTask.ConfigureAwait(false));
        }
        catch (Exception ex) when (ex is not TimeoutException)
        {
            throw new InvalidOperationException($"DNS resolution failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates an HTTP client configuration with SSRF protection enabled
    /// </summary>
    public static HttpClientConfig CreateSsrfProtectedConfig()
    {
        return new HttpClientConfig
        {
            Timeout = RequestTimeout,
            MaxRedirects = MaxRedirects,
            UserAgent = "DotnetMicroOrm-SsrfProtected/1.0",
            MaxResponseSizeBytes = MaxResponseSizeBytes,
        };
    }
}