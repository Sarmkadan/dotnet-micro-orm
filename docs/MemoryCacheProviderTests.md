# MemoryCacheProviderTests

Unit tests for the `MemoryCacheProvider` class, verifying behavior of in-memory caching operations including set, get, remove, pattern-based removal, existence checks, and TTL management.

## API

### `GetAsync_WithNonExistentKey_ReturnsNull`
Ensures that retrieving a non-existent key returns `null` without throwing an exception. Validates the provider's behavior when the cache is empty or the key is absent.

### `SetAsync_WithValidKeyValue_StoresInCache`
Verifies that a valid key-value pair is stored in the cache and can be retrieved immediately afterward. Confirms successful insertion and basic persistence.

### `SetAsync_WithNullKey_ThrowsArgumentException`
Confirms that attempting to store a value with a `null` key throws an `ArgumentException`. Validates input validation for key parameters.

### `SetAsync_WithEmptyKey_ThrowsArgumentException`
Ensures that an empty string key throws an `ArgumentException`. Validates that empty keys are treated as invalid input.

### `SetAsync_WithNullValue_RemovesKey`
Checks that setting a value with a `null` value removes the key from the cache if it exists. Validates the provider's handling of null values as a deletion signal.

### `GetAsync_WithExpiredEntry_ReturnsNull`
Validates that retrieving an expired cache entry returns `null`. Ensures TTL enforcement and automatic expiration handling.

### `RemoveAsync_WithValidKey_DeletesFromCache`
Confirms that removing an existing key deletes it from the cache. Validates successful removal and absence of the key afterward.

### `RemoveAsync_WithNullKey_DoesNotThrow`
Ensures that calling remove with a `null` key does not throw an exception. Validates graceful handling of invalid keys in removal operations.

### `RemoveByPatternAsync_WithWildcardPattern_RemovesMatchingKeys`
Verifies that removing keys matching a wildcard pattern (e.g., `user:*`) removes all matching entries. Confirms pattern-based bulk removal functionality.

### `RemoveByPatternAsync_WithEmptyPattern_DoesNotThrow`
Ensures that calling remove with an empty pattern string does not throw an exception. Validates robustness against empty pattern inputs.

### `ClearAsync_RemovesAllEntries`
Confirms that clearing the cache removes all entries. Validates complete cache reset functionality.

### `ExistsAsync_WithExistingKey_ReturnsTrue`
Verifies that checking existence of an existing key returns `true`. Validates accurate presence detection.

### `ExistsAsync_WithNonExistentKey_ReturnsFalse`
Ensures that checking existence of a non-existent key returns `false`. Validates absence detection.

### `ExistsAsync_WithExpiredKey_ReturnsFalse`
Confirms that checking existence of an expired key returns `false`. Validates that expiration affects existence checks.

### `GetOrSetAsync_WithCachedValue_ReturnsCached`
Verifies that retrieving a value that exists in the cache returns the cached value without invoking the factory function. Validates cache hit behavior.

### `GetOrSetAsync_WithMissingValue_CallsFactory`
Ensures that retrieving a non-existent value invokes the provided factory function and stores the result. Validates cache miss behavior and factory invocation.

### `GetOrSetAsync_WithExpiration_StoresWithTTL`
Confirms that values set via `GetOrSetAsync` respect expiration parameters and are stored with the specified TTL. Validates TTL enforcement in factory-driven scenarios.

### `GetCountAsync_ReturnsAccurateCount`
Verifies that the count of non-expired entries is returned accurately. Validates the provider's ability to track active entries.

### `GetCountAsync_WithExpiredEntries_ExcludesExpired`
Ensures that expired entries are excluded from the count returned by `GetCountAsync`. Validates proper exclusion of expired data in metrics.

### `CleanupAsync_RemovesExpiredEntries`
Confirms that explicitly invoking cleanup removes all expired entries from the cache. Validates manual expiration cleanup functionality.

## Usage
