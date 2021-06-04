#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public static class BatchUpsertOperationUnitTestsExtensions
{
    /// <summary>
    /// Creates a pre-configured BatchUpsertOperationUnitTests instance with a mocked database context
    /// that returns successful upsert results for testing purposes.
    /// </summary>
    /// <param name="setupMock">Optional action to further configure the mock database context</param>
    /// <returns>Configured BatchUpsertOperationUnitTests instance</returns>
    public static BatchUpsertOperationUnitTests WithSuccessfulMockContext(this BatchUpsertOperationUnitTests tests, Action<Mock<IDatabaseContext>>? setupMock = null)
    {
        var contextMock = new Mock<IDatabaseContext>();

        // Setup default successful behavior
        contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } }
            });

        // Apply additional setup if provided
        setupMock?.Invoke(contextMock);

        // Create new instance with our mock
        var newInstance = new BatchUpsertOperationUnitTests();

        // Use reflection to set the private mock field
        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(newInstance, contextMock);

        // Reinitialize the batchUpsert field
        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(newInstance, new BatchUpsertOperation<User>(contextMock.Object));

        return newInstance;
    }

    /// <summary>
    /// Creates a BatchUpsertOperationUnitTests instance configured to throw EntityValidationException
    /// for validation-related testing scenarios.
    /// </summary>
    /// <param name="validationErrorMessage">Optional custom validation error message</param>
    /// <returns>Configured BatchUpsertOperationUnitTests instance</returns>
    public static BatchUpsertOperationUnitTests WithValidationFailure(this BatchUpsertOperationUnitTests tests, string? validationErrorMessage = null)
    {
        var contextMock = new Mock<IDatabaseContext>();

        // Setup to throw validation exception
        contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ThrowsAsync(new EntityValidationException(validationErrorMessage ?? "Validation failed"));

        var newInstance = new BatchUpsertOperationUnitTests();

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(newInstance, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(newInstance, new BatchUpsertOperation<User>(contextMock.Object));

        return newInstance;
    }

    /// <summary>
    /// Creates a BatchUpsertOperationUnitTests instance configured to return specific upsert results
    /// for testing different scenarios like inserts vs updates.
    /// </summary>
    /// <param name="results">List of result dictionaries to return</param>
    /// <returns>Configured BatchUpsertOperationUnitTests instance</returns>
    public static BatchUpsertOperationUnitTests WithCustomResults(this BatchUpsertOperationUnitTests tests, List<Dictionary<string, object>> results)
    {
        var contextMock = new Mock<IDatabaseContext>();
        contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(results);

        var newInstance = new BatchUpsertOperationUnitTests();

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(newInstance, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(newInstance, new BatchUpsertOperation<User>(contextMock.Object));

        return newInstance;
    }

    /// <summary>
    /// Creates a BatchUpsertOperationUnitTests instance configured to simulate database failures
    /// for testing error handling scenarios.
    /// </summary>
    /// <param name="exceptionToThrow">Exception to throw (defaults to generic database exception)</param>
    /// <returns>Configured BatchUpsertOperationUnitTests instance</returns>
    public static BatchUpsertOperationUnitTests WithDatabaseFailure(this BatchUpsertOperationUnitTests tests, Exception? exceptionToThrow = null)
    {
        var contextMock = new Mock<IDatabaseContext>();

        exceptionToThrow ??= new Exception("Database operation failed");
        contextMock.Setup(c => c.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ThrowsAsync(exceptionToThrow);

        var newInstance = new BatchUpsertOperationUnitTests();

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(newInstance, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(newInstance, new BatchUpsertOperation<User>(contextMock.Object));

        return newInstance;
    }
}