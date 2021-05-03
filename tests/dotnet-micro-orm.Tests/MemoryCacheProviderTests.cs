#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Caching;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class MemoryCacheProviderTests
{
    private readonly MemoryCacheProvider _cache = new();

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        var result = await _cache.GetAsync<string>("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithValidKeyValue_StoresInCache()
    {
        var value = "test_value";

        await _cache.SetAsync("test_key", value);
        var retrieved = await _cache.GetAsync<string>("test_key");

        retrieved.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentException()
    {
        var act = () => _cache.SetAsync<string>(null!, "value");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var act = () => _cache.SetAsync<string>("", "value");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SetAsync_WithNullValue_RemovesKey()
    {
        await _cache.SetAsync("test_key", "original_value");

        await _cache.SetAsync<string>("test_key", null!);
        var result = await _cache.GetAsync<string>("test_key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithExpiredEntry_ReturnsNull()
    {
        var expiration = TimeSpan.FromMilliseconds(100);
        await _cache.SetAsync("expired_key", "value", expiration);

        await Task.Delay(150);
        var result = await _cache.GetAsync<string>("expired_key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_WithValidKey_DeletesFromCache()
    {
        await _cache.SetAsync("to_remove", "value");
        await _cache.RemoveAsync("to_remove");

        var result = await _cache.GetAsync<string>("to_remove");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_DoesNotThrow()
    {
        var act = async () => await _cache.RemoveAsync(null!);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithWildcardPattern_RemovesMatchingKeys()
    {
        await _cache.SetAsync("user:1:name", "John");
        await _cache.SetAsync("user:1:email", "john@example.com");
        await _cache.SetAsync("user:2:name", "Jane");
        await _cache.SetAsync("product:1:name", "Laptop");

        await _cache.RemoveByPatternAsync("user:1:*");

        var name = await _cache.GetAsync<string>("user:1:name");
        var email = await _cache.GetAsync<string>("user:1:email");
        var other = await _cache.GetAsync<string>("user:2:name");
        var product = await _cache.GetAsync<string>("product:1:name");

        name.Should().BeNull();
        email.Should().BeNull();
        other.Should().Be("Jane");
        product.Should().Be("Laptop");
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithEmptyPattern_DoesNotThrow()
    {
        await _cache.SetAsync("key", "value");

        var act = async () => await _cache.RemoveByPatternAsync("");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        await _cache.ClearAsync();
        var count = await _cache.GetCountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        await _cache.SetAsync("existing", "value");

        var result = await _cache.ExistsAsync("existing");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentKey_ReturnsFalse()
    {
        var result = await _cache.ExistsAsync("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExpiredKey_ReturnsFalse()
    {
        await _cache.SetAsync("expired", "value", TimeSpan.FromMilliseconds(100));

        await Task.Delay(150);
        var result = await _cache.ExistsAsync("expired");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WithCachedValue_ReturnsCached()
    {
        var callCount = 0;

        await _cache.SetAsync("cached", "existing_value");
        var result = await _cache.GetOrSetAsync(
            "cached",
            async () =>
            {
                callCount++;
                return await Task.FromResult("new_value");
            });

        result.Should().Be("existing_value");
        callCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOrSetAsync_WithMissingValue_CallsFactory()
    {
        var callCount = 0;

        var result = await _cache.GetOrSetAsync(
            "missing",
            async () =>
            {
                callCount++;
                return await Task.FromResult("new_value");
            });

        result.Should().Be("new_value");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrSetAsync_WithExpiration_StoresWithTTL()
    {
        var expiration = TimeSpan.FromMilliseconds(150);

        await _cache.GetOrSetAsync(
            "ttl_key",
            async () => await Task.FromResult("value"),
            expiration);

        var immediate = await _cache.GetAsync<string>("ttl_key");
        await Task.Delay(200);
        var afterExpiry = await _cache.GetAsync<string>("ttl_key");

        immediate.Should().Be("value");
        afterExpiry.Should().BeNull();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsAccurateCount()
    {
        await _cache.ClearAsync();
        await _cache.SetAsync("key1", "value1");
        await _cache.SetAsync("key2", "value2");
        await _cache.SetAsync("key3", "value3");

        var count = await _cache.GetCountAsync();

        count.Should().Be(3);
    }

    [Fact]
    public async Task GetCountAsync_WithExpiredEntries_ExcludesExpired()
    {
        await _cache.ClearAsync();
        await _cache.SetAsync("key1", "value1", TimeSpan.FromMilliseconds(100));
        await _cache.SetAsync("key2", "value2");

        await Task.Delay(150);
        var count = await _cache.GetCountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task CleanupAsync_RemovesExpiredEntries()
    {
        await _cache.ClearAsync();
        await _cache.SetAsync("key1", "value1", TimeSpan.FromMilliseconds(100));
        await _cache.SetAsync("key2", "value2");

        await Task.Delay(150);
        await _cache.CleanupAsync();
        var count = await _cache.GetCountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task SetAsync_WithUpdatingExistingKey_OverwritesValue()
    {
        await _cache.SetAsync("key", "original");
        await _cache.SetAsync("key", "updated");

        var result = await _cache.GetAsync<string>("key");

        result.Should().Be("updated");
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_StoresAndRetrievesCorrectly()
    {
        var obj = new TestObject { Id = 1, Name = "Test" };

        await _cache.SetAsync("obj_key", obj);
        var retrieved = await _cache.GetAsync<TestObject>("obj_key");

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(1);
        retrieved.Name.Should().Be("Test");
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithNoMatches_DoesNotThrow()
    {
        await _cache.SetAsync("prefix:key", "value");

        var act = async () => await _cache.RemoveByPatternAsync("other:*");

        await act.Should().NotThrowAsync();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
