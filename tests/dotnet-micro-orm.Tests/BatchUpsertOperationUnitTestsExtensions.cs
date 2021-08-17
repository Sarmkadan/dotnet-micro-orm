#nullable enable

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

public static class BatchUpsertOperationUnitTestsExtensions
{
    /// <summary>
    /// Configures the test instance with a mocked database context that returns successful upsert results.
    /// </summary>
    /// <param name="tests">The test instance to configure.</param>
    /// <param name="setupMock">Optional action to further configure the mock database context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
    public static BatchUpsertOperationUnitTests WithSuccessfulMockContext(
        this BatchUpsertOperationUnitTests tests,
        Action<Mock<IDatabaseContext>>? setupMock = null)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var contextMock = new Mock<IDatabaseContext>();

        // Setup default successful behavior
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "__action", "INSERT" } }
            });

        // Apply additional setup if provided
        setupMock?.Invoke(contextMock);

        // Use reflection to set the private mock field
        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(tests, contextMock);

        // Reinitialize the batchUpsert field
        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(tests, new BatchUpsertOperation<User>(contextMock.Object));

        return tests;
    }

    /// <summary>
    /// Configures the test instance to throw <see cref="EntityValidationException"/>
    /// for validation-related testing scenarios.
    /// </summary>
    /// <param name="tests">The test instance to configure.</param>
    /// <param name="validationErrorMessage">Optional custom validation error message.</param>
    /// <returns>Configured <see cref="BatchUpsertOperationUnitTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
    public static BatchUpsertOperationUnitTests WithValidationFailure(
        this BatchUpsertOperationUnitTests tests,
        string? validationErrorMessage = null)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var contextMock = new Mock<IDatabaseContext>();

        // Setup to throw validation exception
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .ThrowsAsync(new EntityValidationException(validationErrorMessage ?? "Validation failed"));

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(tests, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(tests, new BatchUpsertOperation<User>(contextMock.Object));

        return tests;
    }

    /// <summary>
    /// Configures the test instance to return specific upsert results for testing different scenarios.
    /// </summary>
    /// <param name="tests">The test instance to configure.</param>
    /// <param name="results">List of result dictionaries to return.</param>
    /// <returns>Configured <see cref="BatchUpsertOperationUnitTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
    public static BatchUpsertOperationUnitTests WithCustomResults(
        this BatchUpsertOperationUnitTests tests,
        List<Dictionary<string, object>> results)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(results);

        var contextMock = new Mock<IDatabaseContext>();
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(results);

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(tests, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(tests, new BatchUpsertOperation<User>(contextMock.Object));

        return tests;
    }

    /// <summary>
    /// Configures the test instance to simulate database failures for testing error handling scenarios.
    /// </summary>
    /// <param name="tests">The test instance to configure.</param>
    /// <param name="exceptionToThrow">Exception to throw (defaults to generic database exception).</param>
    /// <returns>Configured <see cref="BatchUpsertOperationUnitTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
    public static BatchUpsertOperationUnitTests WithDatabaseFailure(
        this BatchUpsertOperationUnitTests tests,
        Exception? exceptionToThrow = null)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var contextMock = new Mock<IDatabaseContext>();

        exceptionToThrow ??= new Exception("Database operation failed");
        contextMock.Setup(c => c.ExecuteQueryAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .ThrowsAsync(exceptionToThrow);

        var field = typeof(BatchUpsertOperationUnitTests).GetField(
            "_contextMock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(tests, contextMock);

        var batchUpsertField = typeof(BatchUpsertOperationUnitTests).GetField(
            "_batchUpsert",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        batchUpsertField?.SetValue(tests, new BatchUpsertOperation<User>(contextMock.Object));

        return tests;
    }
}