#nullable enable
using DotnetMicroOrm.Integration;
using FluentAssertions;
using Xunit;

public sealed class DefaultHttpClientTests : IDisposable
{
    private readonly DefaultHttpClient _client;
    private readonly HttpClientConfig _config;

    public DefaultHttpClientTests()
    {
        _config = new HttpClientConfig
        {
            Timeout = TimeSpan.FromSeconds(10),
            UserAgent = "TestClient/1.0"
        };
        _client = new DefaultHttpClient(_config);
    }

    public void Dispose()
    {
        _client.DisposeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Constructor_WithNullConfig_UsesDefaults()
    {
        var client = new DefaultHttpClient(config: null);
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullRetryPolicy_UsesDefault()
    {
        var client = new DefaultHttpClient(config: null, retryPolicy: null);
        client.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that GetAsync throws on null or empty URL.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetAsync_WithInvalidUrl_Throws(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetAsync(url!));
    }

    /// <summary>
    /// Tests that PostAsync throws on null or empty URL.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PostAsync_WithInvalidUrl_Throws(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.PostAsync(url!, "body"));
    }

    /// <summary>
    /// Tests that PutAsync throws on null or empty URL.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PutAsync_WithInvalidUrl_Throws(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.PutAsync(url!, "body"));
    }

    /// <summary>
    /// Tests that DeleteAsync throws on null or empty URL.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteAsync_WithInvalidUrl_Throws(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.DeleteAsync(url!));
    }

    /// <summary>
    /// Tests that GetAsync returns success response for valid URL.
    /// </summary>
    [Fact]
    public async Task GetAsync_WithValidUrl_ReturnsSuccessResponse()
    {
        // Use a known public endpoint that returns 200
        var response = await _client.GetAsync("https://httpbin.org/get");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.IsClientError.Should().BeFalse();
        response.IsServerError.Should().BeFalse();
        response.Body.Should().NotBeNullOrEmpty();
        response.Duration.Should().BePositive();
        response.Exception.Should().BeNull();
        response.Headers.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that PostAsync sends request with body and returns response.
    /// </nsummary>
    [Fact]
    public async Task PostAsync_WithBody_ReturnsResponseWithBody()
    {
        var testData = "{\"test\": \"value\"}";
        var response = await _client.PostAsync("https://httpbin.org/post", testData);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().NotBeNullOrEmpty();
        response.Body.Should().Contain("test");
        response.Duration.Should().BePositive();
    }

    /// <summary>
    /// Tests that PutAsync sends request with body and returns response.
    /// </summary>
    [Fact]
    public async Task PutAsync_WithBody_ReturnsResponseWithBody()
    {
        var testData = "{\"updated\": true}";
        var response = await _client.PutAsync("https://httpbin.org/put", testData);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().NotBeNullOrEmpty();
        response.Duration.Should().BePositive();
    }

    /// <summary>
    /// Tests that DeleteAsync returns success response.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithValidUrl_ReturnsSuccessResponse()
    {
        var response = await _client.DeleteAsync("https://httpbin.org/delete");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Duration.Should().BePositive();
    }

    /// <summary>
    /// Tests that PostAsync with custom content type works.
    /// </summary>
    [Fact]
    public async Task PostAsync_WithCustomContentType_ReturnsResponse()
    {
        var testData = "test=value";
        var response = await _client.PostAsync("https://httpbin.org/post", testData, "application/x-www-form-urlencoded");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that requests include custom headers.
    /// </summary>
    [Fact]
    public async Task GetAsync_WithHeaders_IncludesHeadersInRequest()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "test-value" },
            { "Authorization", "Bearer token123" }
        };

        var response = await _client.GetAsync("https://httpbin.org/headers", headers);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("X-Custom-Header");
        response.Body.Should().Contain("test-value");
    }

    /// <summary>
    /// Tests that GetWithRetryAsync retries on server errors.
    /// </summary>
    [Fact]
    public async Task GetWithRetryAsync_OnServerError_RetriesAndSucceeds()
    {
        // This endpoint will fail with 500 initially, then succeed
        var response = await _client.GetWithRetryAsync("https://httpbin.org/status/500,200");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetWithRetryAsync returns client error immediately without retry.
    /// </summary>
    [Fact]
    public async Task GetWithRetryAsync_OnClientError_ReturnsImmediately()
    {
        var response = await _client.GetWithRetryAsync("https://httpbin.org/status/404");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(404);
        response.IsClientError.Should().BeTrue();
        response.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetWithRetryAsync respects maxRetries parameter.
    /// </summary>
    [Fact]
    public async Task GetWithRetryAsync_WithMaxRetries_RespectsLimit()
    {
        // Configure client to fail always
        var client = new DefaultHttpClient(new HttpClientConfig { Timeout = TimeSpan.FromSeconds(5) });

        var response = await client.GetWithRetryAsync("https://httpbin.org/status/500", maxRetries: 2);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(500); // Should return the final failed response
        response.IsSuccess.Should().BeFalse();

        await client.DisposeAsync();
    }

    /// <summary>
    /// Tests that DisposeAsync properly cleans up resources.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_CleansUpResources()
    {
        var client = new DefaultHttpClient();

        // Should not throw
        await client.DisposeAsync();

        // Second dispose should also not throw
        await client.DisposeAsync();
    }

    /// <summary>
    /// Tests that LinearRetryPolicy uses fixed delay.
    /// </summary>
    [Fact]
    public void LinearRetryPolicy_GetDelay_ReturnsFixedDelay()
    {
        var delay = TimeSpan.FromSeconds(2);
        var policy = new LinearRetryPolicy(delay);

        policy.GetDelay(1).Should().Be(delay);
        policy.GetDelay(5).Should().Be(delay);
        policy.GetDelay(100).Should().Be(delay);
    }

    /// <summary>
    /// Tests that ExponentialBackoffRetryPolicy increases delay exponentially.
    /// </summary>
    [Fact]
    public void ExponentialBackoffRetryPolicy_GetDelay_IncreasesExponentially()
    {
        var policy = new ExponentialBackoffRetryPolicy();

        policy.GetDelay(1).Should().Be(TimeSpan.FromSeconds(1));
        policy.GetDelay(2).Should().Be(TimeSpan.FromSeconds(2));
        policy.GetDelay(3).Should().Be(TimeSpan.FromSeconds(4));
        policy.GetDelay(4).Should().Be(TimeSpan.FromSeconds(8));
        policy.GetDelay(5).Should().Be(TimeSpan.FromSeconds(16));
        policy.GetDelay(6).Should().Be(TimeSpan.FromSeconds(30)); // Capped at 30s
    }

    /// <summary>
    /// Tests that HttpResponseData properties are correctly set.
    /// </summary>
    [Fact]
    public void HttpResponseData_Properties_SetCorrectly()
    {
        var exception = new InvalidOperationException("Test exception");
        var headers = new Dictionary<string, string> { { "Content-Type", "application/json" } };
        var duration = TimeSpan.FromMilliseconds(123);

        var response = new HttpResponseData
        {
            StatusCode = 201,
            Body = "test body",
            Headers = headers,
            Duration = duration,
            Exception = exception
        };

        response.StatusCode.Should().Be(201);
        response.Body.Should().Be("test body");
        response.Headers.Should().BeSameAs(headers);
        response.Duration.Should().Be(duration);
        response.Exception.Should().BeSameAs(exception);
        response.IsSuccess.Should().BeTrue();
        response.IsClientError.Should().BeFalse();
        response.IsServerError.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HttpResponseData handles 2xx status codes as success.
    /// </summary>
    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(202)]
    [InlineData(299)]
    public void HttpResponseData_IsSuccess_For2xxStatusCodes(int statusCode)
    {
        var response = new HttpResponseData { StatusCode = statusCode };
        response.IsSuccess.Should().BeTrue();
        response.IsClientError.Should().BeFalse();
        response.IsServerError.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HttpResponseData handles 4xx status codes as client errors.
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(499)]
    public void HttpResponseData_IsClientError_For4xxStatusCodes(int statusCode)
    {
        var response = new HttpResponseData { StatusCode = statusCode };
        response.IsSuccess.Should().BeFalse();
        response.IsClientError.Should().BeTrue();
        response.IsServerError.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HttpResponseData handles 5xx status codes as server errors.
    /// </summary>
    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(599)]
    public void HttpResponseData_IsServerError_For5xxStatusCodes(int statusCode)
    {
        var response = new HttpResponseData { StatusCode = statusCode };
        response.IsSuccess.Should().BeFalse();
        response.IsClientError.Should().BeFalse();
        response.IsServerError.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HttpClientConfig uses sensible defaults.
    /// </summary>
    [Fact]
    public void HttpClientConfig_UsesSensibleDefaults()
    {
        var config = new HttpClientConfig();

        config.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        config.UserAgent.Should().Be("DotnetMicroOrm/1.0");
        config.MaxRedirects.Should().Be(5);
        config.AutoDecompression.Should().BeTrue();
        config.PoolConnections.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HttpClientConfig can override defaults.
    /// </summary>
    [Fact]
    public void HttpClientConfig_CanOverrideDefaults()
    {
        var config = new HttpClientConfig
        {
            Timeout = TimeSpan.FromSeconds(60),
            UserAgent = "CustomAgent/2.0",
            MaxRedirects = 10,
            AutoDecompression = false,
            PoolConnections = false
        };

        config.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        config.UserAgent.Should().Be("CustomAgent/2.0");
        config.MaxRedirects.Should().Be(10);
        config.AutoDecompression.Should().BeFalse();
        config.PoolConnections.Should().BeFalse();
    }
}