# QueryBuilderTests
The `QueryBuilderTests` class is designed to test the functionality of a query builder, ensuring that it behaves correctly under various scenarios. This includes testing for invalid input, filtering, and asynchronous operations. The tests in this class help guarantee the reliability and robustness of the query builder.

## API
* `public void Take_ZeroCount_ThrowsArgumentException`: Tests that an `ArgumentException` is thrown when attempting to take zero items from a query. This method takes no parameters and returns no value. It throws an `ArgumentException` when the count is zero.
* `public void Skip_NegativeCount_ThrowsArgumentException`: Verifies that an `ArgumentException` is thrown when trying to skip a negative number of items in a query. This method takes no parameters and returns no value. It throws an `ArgumentException` when the count is negative.
* `public async Task ToListAsync_WithActiveFilter_ReturnsOnlyActiveProducts`: Asynchronously tests that a query with an active filter returns only active products. This method takes no parameters and returns a `Task`. It does not throw any exceptions as part of its normal operation.
* `public void Validate_NegativePrice_ContainsPriceError`: Tests that validating a product with a negative price results in an error related to the price. This method takes no parameters and returns no value. It does not throw any exceptions as part of its normal operation.

## Usage
The following examples demonstrate how to utilize the `QueryBuilderTests` class in a testing context:
```csharp
[TestMethod]
public void TestTakeZeroCount()
{
    // Arrange
    var queryBuilder = new QueryBuilder();

    // Act and Assert
    Assert.ThrowsException<ArgumentException>(() => queryBuilder.Take(0));
}

[TestMethod]
public async Task TestToListAsyncWithActiveFilter()
{
    // Arrange
    var queryBuilder = new QueryBuilder();
    queryBuilder.FilterByActive();

    // Act
    var result = await queryBuilder.ToListAsync();

    // Assert
    Assert.IsTrue(result.All(p => p.IsActive));
}
```

## Notes
When using the `QueryBuilderTests` class, consider the following edge cases and thread-safety remarks:
- The `Take_ZeroCount_ThrowsArgumentException` and `Skip_NegativeCount_ThrowsArgumentException` tests may not be thread-safe if the query builder instance is shared across multiple threads, as the tests modify the query builder's state.
- The `ToListAsync_WithActiveFilter_ReturnsOnlyActiveProducts` test is designed to be asynchronous, allowing for non-blocking I/O operations. However, if the underlying data source is not thread-safe, this test may still encounter issues when run concurrently with other tests.
- The `Validate_NegativePrice_ContainsPriceError` test assumes that the product validation logic is correctly implemented and does not account for potential errors in the validation process itself.
