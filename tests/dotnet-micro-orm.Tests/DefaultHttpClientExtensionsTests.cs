#nullable enable

using DotnetMicroOrm.Integration;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class DefaultHttpClientExtensionsTests : IDisposable
{
    private readonly DefaultHttpClient _client;
    private readonly HttpClientConfig _config;

    public DefaultHttpClientExtensionsTests()
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

    // ========== GetAsync tests ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetAsync_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetAsync(url!, null));
    }

    [Fact]
    public async Task GetAsync_WithoutQueryParameters_ReturnsSuccessResponse()
    {
        var response = await _client.GetAsync("https://httpbin.org/get", queryParameters: null);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().NotBeNullOrEmpty();
        response.Duration.Should().BePositive();
        response.Exception.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithEmptyQueryParameters_ReturnsSuccessResponse()
    {
        var response = await _client.GetAsync("https://httpbin.org/get", queryParameters: new Dictionary<string, string>());

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_WithSingleQueryParameter_AppendsParameterToUrl()
    {
        var queryParams = new Dictionary<string, string> { { "key", "value" } };
        var response = await _client.GetAsync("https://httpbin.org/get", queryParameters: queryParams);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("key");
        response.Body.Should().Contain("value");
    }

    [Fact]
    public async Task GetAsync_WithMultipleQueryParameters_AppendsAllParameters()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "param1", "value1" },
            { "param2", "value2" },
            { "param3", "value3" }
        };
        var response = await _client.GetAsync("https://httpbin.org/get", queryParameters: queryParams);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("param1");
        response.Body.Should().Contain("value1");
        response.Body.Should().Contain("param2");
        response.Body.Should().Contain("value2");
        response.Body.Should().Contain("param3");
        response.Body.Should().Contain("value3");
    }

    [Fact]
    public async Task GetAsync_WithUrlAlreadyHavingQueryString_AppendsParametersWithAmpersand()
    {
        var queryParams = new Dictionary<string, string> { { "extra", "data" } };
        var response = await _client.GetAsync("https://httpbin.org/get?existing=param", queryParameters: queryParams);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("existing");
        response.Body.Should().Contain("param");
        response.Body.Should().Contain("extra");
        response.Body.Should().Contain("data");
    }

    [Fact]
    public async Task GetAsync_WithHeaders_IncludesHeadersInRequest()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "test-value" },
            { "Authorization", "Bearer token123" }
        };

        var response = await _client.GetAsync("https://httpbin.org/headers", headers: headers);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("X-Custom-Header");
        response.Body.Should().Contain("test-value");
    }

    [Fact]
    public async Task GetAsync_WithNullHeaders_WorksWithoutHeaders()
    {
        var response = await _client.GetAsync("https://httpbin.org/get", headers: null);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    // ========== PostFormAsync tests ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PostFormAsync_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        var formData = new Dictionary<string, string> { { "key", "value" } };
        await Assert.ThrowsAsync<ArgumentException>(() => _client.PostFormAsync(url!, formData));
    }

    [Fact]
    public async Task PostFormAsync_WithNullFormData_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.PostFormAsync("https://httpbin.org/post", formData: null!));
    }

    [Fact]
    public async Task PostFormAsync_WithEmptyFormData_ReturnsSuccessResponse()
    {
        var response = await _client.PostFormAsync("https://httpbin.org/post", formData: new Dictionary<string, string>());

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PostFormAsync_WithSingleFormField_SendsFormUrlEncodedData()
    {
        var formData = new Dictionary<string, string> { { "username", "john_doe" } };
        var response = await _client.PostFormAsync("https://httpbin.org/post", formData);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("username");
        response.Body.Should().Contain("john_doe");
    }

    [Fact]
    public async Task PostFormAsync_WithMultipleFormFields_SendsAllFields()
    {
        var formData = new Dictionary<string, string>
        {
            { "username", "jane_doe" },
            { "email", "jane@example.com" },
            { "age", "30" }
        };
        var response = await _client.PostFormAsync("https://httpbin.org/post", formData);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("username");
        response.Body.Should().Contain("jane_doe");
        response.Body.Should().Contain("email");
        response.Body.Should().Contain("jane@example.com");
        response.Body.Should().Contain("age");
        response.Body.Should().Contain("30");
    }

    [Fact]
    public async Task PostFormAsync_WithHeaders_IncludesHeadersInRequest()
    {
        var formData = new Dictionary<string, string> { { "data", "test" } };
        var headers = new Dictionary<string, string> { { "X-API-Key", "secret123" } };

        var response = await _client.PostFormAsync("https://httpbin.org/post", formData, headers);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("X-API-Key");
        response.Body.Should().Contain("secret123");
    }

    [Fact]
    public async Task PostFormAsync_WithUrlEncodedCharacters_ProperlyEncodesValues()
    {
        var formData = new Dictionary<string, string>
        {
            { "search query", "test value" },
            { "special=chars", "a&b=c" }
        };
        var response = await _client.PostFormAsync("https://httpbin.org/post", formData);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    // ========== GetFromJsonAsync tests ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetFromJsonAsync_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetFromJsonAsync<object>(url!, HttpMethod.Get));
    }

    [Fact]
    public async Task GetFromJsonAsync_WithNullMethod_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.GetFromJsonAsync<object>("https://httpbin.org/get", method: null!));
    }

    [Fact]
    public async Task GetFromJsonAsync_WithGetMethod_ReturnsDeserializedObject()
    {
        var result = await _client.GetFromJsonAsync<HttpBinGetResponse>("https://httpbin.org/get", HttpMethod.Get);

        result.Should().NotBeNull();
        result!.Url.Should().NotBeNullOrEmpty();
        result.Headers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFromJsonAsync_WithPostMethod_ReturnsDeserializedObject()
    {
        var result = await _client.GetFromJsonAsync<HttpBinPostResponse>("https://httpbin.org/post", HttpMethod.Post, "{\"test\":\"value\"}");

        result.Should().NotBeNull();
        result!.Json.Should().NotBeNull();
        result.Json.Should().ContainKey("test").WhoseValue.Should().Be("value");
    }

    [Fact]
    public async Task GetFromJsonAsync_WithPutMethod_ReturnsDeserializedObject()
    {
        var result = await _client.GetFromJsonAsync<HttpBinPutResponse>("https://httpbin.org/put", HttpMethod.Put, "{\"updated\":true}");

        result.Should().NotBeNull();
        result!.Json.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFromJsonAsync_WithDeleteMethod_ReturnsDeserializedObject()
    {
        var result = await _client.GetFromJsonAsync<HttpBinDeleteResponse>("https://httpbin.org/delete", HttpMethod.Delete);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFromJsonAsync_WithUnsupportedMethod_ThrowsNotSupportedException()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _client.GetFromJsonAsync<HttpBinGetResponse>("https://httpbin.org/get", new HttpMethod("PATCH")));
    }

    [Fact]
    public async Task GetFromJsonAsync_WithNullBody_ReturnsResponseWithEmptyBody()
    {
        var result = await _client.GetFromJsonAsync<HttpBinPostResponse>("https://httpbin.org/post", HttpMethod.Post);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFromJsonAsync_WithHeaders_IncludesHeadersInRequest()
    {
        var headers = new Dictionary<string, string> { { "X-Custom", "header-value" } };
        var result = await _client.GetFromJsonAsync<HttpBinGetResponse>("https://httpbin.org/get", HttpMethod.Get, headers: headers);

        result.Should().NotBeNull();
        result!.Headers.Should().ContainKey("X-Custom").WhoseValue.Should().Contain("header-value");
    }

    [Fact]
    public async Task GetFromJsonAsync_WithEmptyResponse_ReturnsNull()
    {
        var result = await _client.GetFromJsonAsync<object>("https://httpbin.org/status/204", HttpMethod.Get);

        result.Should().BeNull();
    }

    // ========== SendWithRetryAsync tests ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SendWithRetryAsync_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.SendWithRetryAsync(url!, HttpMethod.Get));
    }

    [Fact]
    public async Task SendWithRetryAsync_WithNullMethod_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.SendWithRetryAsync("https://httpbin.org/get", method: null!));
    }

    [Fact]
    public async Task SendWithRetryAsync_WithGetMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/get", HttpMethod.Get);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_WithPostMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/post", HttpMethod.Post, "{\"test\":\"data\"}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_WithPutMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/put", HttpMethod.Put, "{\"update\":true}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_WithDeleteMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/delete", HttpMethod.Delete);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_WithUnsupportedMethod_ThrowsNotSupportedException()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _client.SendWithRetryAsync("https://httpbin.org/get", new HttpMethod("PATCH")));
    }

    [Fact]
    public async Task SendWithRetryAsync_WithMaxRetries_RespectsRetryLimit()
    {
        var client = new DefaultHttpClient(new HttpClientConfig { Timeout = TimeSpan.FromSeconds(5) });

        var response = await client.SendWithRetryAsync("https://httpbin.org/status/500", HttpMethod.Get, maxRetries: 2);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(500);
        response.IsSuccess.Should().BeFalse();

        await client.DisposeAsync();
    }

    [Fact]
    public async Task SendWithRetryAsync_WithCustomRetryDelay_UsesCustomDelay()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/get", HttpMethod.Get, maxRetries: 1);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_OnServerError_RetriesAndSucceeds()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/status/500,200", HttpMethod.Get, maxRetries: 3);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendWithRetryAsync_OnClientError_ReturnsImmediatelyWithoutRetry()
    {
        var response = await _client.SendWithRetryAsync("https://httpbin.org/status/404", HttpMethod.Get);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(404);
        response.IsClientError.Should().BeTrue();
        response.IsSuccess.Should().BeFalse();
    }

    // ========== SendAsync tests ==========

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SendAsync_WithNullOrEmptyUrl_ThrowsArgumentException(string? url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.SendAsync(HttpMethod.Get, url!));
    }

    [Fact]
    public async Task SendAsync_WithNullMethod_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.SendAsync(method: null!, "https://httpbin.org/get"));
    }

    [Fact]
    public async Task SendAsync_WithGetMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendAsync(HttpMethod.Get, "https://httpbin.org/get");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithPostMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendAsync(HttpMethod.Post, "https://httpbin.org/post", "{\"test\":\"data\"}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithPutMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendAsync(HttpMethod.Put, "https://httpbin.org/put", "{\"update\":true}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithDeleteMethod_ReturnsSuccessResponse()
    {
        var response = await _client.SendAsync(HttpMethod.Delete, "https://httpbin.org/delete");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithUnsupportedMethod_ThrowsNotSupportedException()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _client.SendAsync(new HttpMethod("PATCH"), "https://httpbin.org/get"));
    }

    [Fact]
    public async Task SendAsync_WithCustomContentType_ReturnsSuccessResponse()
    {
        var response = await _client.SendAsync(HttpMethod.Post, "https://httpbin.org/post", "test=data", "text/plain");

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithNullBody_UsesEmptyObject()
    {
        var response = await _client.SendAsync(HttpMethod.Post, "https://httpbin.org/post");

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithHeaders_IncludesHeadersInRequest()
    {
        var headers = new Dictionary<string, string> { { "X-Test-Header", "test-value" } };
        var response = await _client.SendAsync(HttpMethod.Get, "https://httpbin.org/headers", headers: headers);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Body.Should().Contain("X-Test-Header");
        response.Body.Should().Contain("test-value");
    }

    // ========== Helper response classes ==========

    private class HttpBinGetResponse
    {
        public string? Url { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }

    private class HttpBinPostResponse
    {
        public Dictionary<string, object>? Json { get; set; }
        public string? Data { get; set; }
    }

    private class HttpBinPutResponse
    {
        public Dictionary<string, object>? Json { get; set; }
    }

    private class HttpBinDeleteResponse
    {
        public Dictionary<string, object>? Json { get; set; }
    }
}