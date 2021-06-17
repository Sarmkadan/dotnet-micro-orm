#nullable enable

using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the DotnetMicroOrm exceptions.
/// </summary>
public sealed class ExceptionTests
{
    /// <summary>
    /// Tests that an OrmException can be created with a message.
    /// </summary>
    [Fact]
    public void OrmException_WithMessage_CreatesInstance()
    {
        var exception = new OrmException("Test error");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.ErrorCode.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    /// <summary>
    /// Tests that an OrmException can be created with a message and an error code.
    /// </summary>
    [Fact]
    public void OrmException_WithMessageAndErrorCode_CreatesInstance()
    {
        var exception = new OrmException("Test error", "TEST_ERROR");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.ErrorCode.Should().Be("TEST_ERROR");
    }

    /// <summary>
    /// Tests that an OrmException can be created with a message, an error code, and an inner exception.
    /// </summary>
    [Fact]
    public void OrmException_WithMessageInnerExceptionAndErrorCode_CreatesInstance()
    {
        var innerException = new InvalidOperationException("Inner error");
        var exception = new OrmException("Test error", "TEST_ERROR", innerException);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.ErrorCode.Should().Be("TEST_ERROR");
        exception.InnerException.Should().Be(innerException);
    }

    /// <summary>
    /// Tests that an OrmException can be created with a context.
    /// </summary>
    [Fact]
    public void OrmException_WithContext_AddsContext()
    {
        var exception = new OrmException("Test error")
            .WithContext("Key1", "Value1")
            .WithContext("Key2", 42);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext.Should().HaveCount(2);
        exception.ErrorContext!["Key1"].Should().Be("Value1");
        exception.ErrorContext!["Key2"].Should().Be(42);
    }

    /// <summary>
    /// Tests that a DatabaseConnectionException can be created with a message.
    /// </summary>
    [Fact]
    public void DatabaseConnectionException_WithMessage_CreatesInstance()
    {
        var exception = new DatabaseConnectionException("Connection failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Connection failed");
        exception.ErrorCode.Should().Be("DB_CONNECTION_ERROR");
    }

    /// <summary>
    /// Tests that an EntityMappingException can be created with a message.
    /// </summary>
    [Fact]
    public void EntityMappingException_WithMessage_CreatesInstance()
    {
        var exception = new EntityMappingException("Mapping failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Mapping failed");
        exception.ErrorCode.Should().Be("ENTITY_MAPPING_ERROR");
    }

    /// <summary>
    /// Tests that an EntityMappingException can be created with a message and a property name.
    /// </summary>
    [Fact]
    public void EntityMappingException_WithMessageAndPropertyName_CreatesInstance()
    {
        var exception = new EntityMappingException("Mapping failed", "PropertyName");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Mapping failed");
        exception.ErrorCode.Should().Be("ENTITY_MAPPING_ERROR");
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["Property"].Should().Be("PropertyName");
    }

    /// <summary>
    /// Tests that a QueryExecutionException can be created with a message.
    /// </summary>
    [Fact]
    public void QueryExecutionException_WithMessage_CreatesInstance()
    {
        var exception = new QueryExecutionException("Query failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Query failed");
        exception.ErrorCode.Should().Be("QUERY_EXECUTION_ERROR");
    }

    /// <summary>
    /// Tests that a QueryExecutionException can be created with a message and a query.
    /// </summary>
    [Fact]
    public void QueryExecutionException_WithMessageAndQuery_CreatesInstance()
    {
        var exception = new QueryExecutionException("Query failed", "SELECT * FROM Table");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Query failed");
        exception.ErrorCode.Should().Be("QUERY_EXECUTION_ERROR");
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["Query"].Should().Be("SELECT * FROM Table");
    }

    /// <summary>
    /// Tests that an EntityValidationException can be created with a message.
    /// </summary>
    [Fact]
    public void EntityValidationException_WithMessage_CreatesInstance()
    {
        var exception = new EntityValidationException("Validation failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Validation failed");
        exception.ErrorCode.Should().Be("ENTITY_VALIDATION_ERROR");
        exception.ValidationErrors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that an EntityValidationException can be created with a message and validation errors.
    /// </summary>
    [Fact]
    public void EntityValidationException_WithMessageAndErrors_CreatesInstance()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var exception = new EntityValidationException("Validation failed", errors);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Validation failed");
        exception.ErrorCode.Should().Be("ENTITY_VALIDATION_ERROR");
        exception.ValidationErrors.Should().HaveCount(2);
        exception.ValidationErrors[0].Should().Be("Error 1");
        exception.ValidationErrors[1].Should().Be("Error 2");
    }

    /// <summary>
    /// Tests that a ConcurrencyException can be created with a message.
    /// </summary>
    [Fact]
    public void ConcurrencyException_WithMessage_CreatesInstance()
    {
        var exception = new ConcurrencyException("Concurrency conflict");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Concurrency conflict");
        exception.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
    }

    /// <summary>
    /// Tests that a ConcurrencyException can be created with a message and an entity key.
    /// </summary>
    [Fact]
    public void ConcurrencyException_WithMessageAndEntityKey_CreatesInstance()
    {
        var exception = new ConcurrencyException("Concurrency conflict", 42);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Concurrency conflict");
        exception.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["EntityKey"].Should().Be(42);
    }
}
