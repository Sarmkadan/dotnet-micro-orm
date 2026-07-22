#nullable enable

using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the OrmException class and its derived exception types.
/// </summary>
public sealed class OrmExceptionTests
{
    /// <summary>
    /// Tests that OrmException can be created with just a message (happy path).
    /// </summary>
    [Fact]
    public void OrmException_WithMessage_CreatesInstance()
    {
        var exception = new OrmException("Test error message");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error message");
        exception.ErrorCode.Should().BeNull();
        exception.InnerException.Should().BeNull();
        exception.ErrorContext.Should().BeNull();
    }

    /// <summary>
    /// Tests that OrmException can be created with message and error code (happy path).
    /// </summary>
    [Fact]
    public void OrmException_WithMessageAndErrorCode_CreatesInstance()
    {
        var exception = new OrmException("Test error", "CUSTOM_ERROR");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.ErrorCode.Should().Be("CUSTOM_ERROR");
        exception.InnerException.Should().BeNull();
    }

    /// <summary>
    /// Tests that OrmException can be created with message, error code, and inner exception (happy path).
    /// </summary>
    [Fact]
    public void OrmException_WithMessageInnerExceptionAndErrorCode_CreatesInstance()
    {
        var innerException = new InvalidOperationException("Inner error");
        var exception = new OrmException("Test error", "CUSTOM_ERROR", innerException);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Test error");
        exception.ErrorCode.Should().Be("CUSTOM_ERROR");
        exception.InnerException.Should().BeSameAs(innerException);
    }

    /// <summary>
    /// Tests that OrmException.WithContext adds context entries (happy path).
    /// </summary>
    [Fact]
    public void OrmException_WithContext_AddsContextEntries()
    {
        var exception = new OrmException("Test error")
            .WithContext("Key1", "Value1")
            .WithContext("Key2", 42)
            .WithContext("Key3", true);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext.Should().HaveCount(3);
        exception.ErrorContext!["Key1"].Should().Be("Value1");
        exception.ErrorContext!["Key2"].Should().Be(42);
        exception.ErrorContext!["Key3"].Should().Be(true);
    }

    /// <summary>
    /// Tests that OrmException.WithContext returns the same exception instance for method chaining.
    /// </summary>
    [Fact]
    public void OrmException_WithContext_ReturnsSameInstance()
    {
        var exception = new OrmException("Test error");
        var result = exception.WithContext("Key", "Value");

        result.Should().BeSameAs(exception);
    }

    /// <summary>
    /// Tests that DatabaseConnectionException is created with correct error code.
    /// </summary>
    [Fact]
    public void DatabaseConnectionException_CreatesWithCorrectErrorCode()
    {
        var exception = new DatabaseConnectionException("Connection failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Connection failed");
        exception.ErrorCode.Should().Be("DB_CONNECTION_ERROR");
    }

    /// <summary>
    /// Tests that DatabaseConnectionException can include inner exception.
    /// </summary>
    [Fact]
    public void DatabaseConnectionException_IncludesInnerException()
    {
        var innerException = new InvalidOperationException("Database unavailable");
        var exception = new DatabaseConnectionException("Connection failed", innerException);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Connection failed");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.ErrorCode.Should().Be("DB_CONNECTION_ERROR");
    }

    /// <summary>
    /// Tests that EntityMappingException is created with correct error code.
    /// </summary>
    [Fact]
    public void EntityMappingException_CreatesWithCorrectErrorCode()
    {
        var exception = new EntityMappingException("Mapping failed for entity");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Mapping failed for entity");
        exception.ErrorCode.Should().Be("ENTITY_MAPPING_ERROR");
    }

    /// <summary>
    /// Tests that EntityMappingException can include property name in context.
    /// </summary>
    [Fact]
    public void EntityMappingException_IncludesPropertyNameInContext()
    {
        var exception = new EntityMappingException("Mapping failed", "User.Email");

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["Property"].Should().Be("User.Email");
    }

    /// <summary>
    /// Tests that EntityMappingException with null property name doesn't add context.
    /// </summary>
    [Fact]
    public void EntityMappingException_WithNullPropertyName_DoesNotAddContext()
    {
        var exception = new EntityMappingException("Mapping failed", null);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryExecutionException is created with correct error code.
    /// </summary>
    [Fact]
    public void QueryExecutionException_CreatesWithCorrectErrorCode()
    {
        var exception = new QueryExecutionException("Query execution failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Query execution failed");
        exception.ErrorCode.Should().Be("QUERY_EXECUTION_ERROR");
    }

    /// <summary>
    /// Tests that QueryExecutionException can include query in context.
    /// </summary>
    [Fact]
    public void QueryExecutionException_IncludesQueryInContext()
    {
        var query = "SELECT * FROM Users WHERE Id = @id";
        var exception = new QueryExecutionException("Query failed", query);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["Query"].Should().Be(query);
    }

    /// <summary>
    /// Tests that QueryExecutionException with null query doesn't add context.
    /// </summary>
    [Fact]
    public void QueryExecutionException_WithNullQuery_DoesNotAddContext()
    {
        var exception = new QueryExecutionException("Query failed", null);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryExecutionException can include inner exception.
    /// </summary>
    [Fact]
    public void QueryExecutionException_IncludesInnerException()
    {
        var innerException = new InvalidOperationException("Syntax error in SQL");
        var exception = new QueryExecutionException("Query failed", "SELECT *", innerException);

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Query failed");
        exception.InnerException.Should().BeSameAs(innerException);
        exception.ErrorContext!["Query"].Should().Be("SELECT *");
    }

    /// <summary>
    /// Tests that EntityValidationException initializes empty ValidationErrors list.
    /// </summary>
    [Fact]
    public void EntityValidationException_InitializesEmptyValidationErrors()
    {
        var exception = new EntityValidationException("Validation failed");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Validation failed");
        exception.ErrorCode.Should().Be("ENTITY_VALIDATION_ERROR");
        exception.ValidationErrors.Should().NotBeNull();
        exception.ValidationErrors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that EntityValidationException can be created with validation errors.
    /// </summary>
    [Fact]
    public void EntityValidationException_WithErrors_AddsToValidationErrors()
    {
        var errors = new List<string> { "Email is required", "Name is too short" };
        var exception = new EntityValidationException("Validation failed", errors);

        exception.Should().NotBeNull();
        exception.ValidationErrors.Should().HaveCount(2);
        exception.ValidationErrors[0].Should().Be("Email is required");
        exception.ValidationErrors[1].Should().Be("Name is too short");
    }

    /// <summary>
    /// Tests that EntityValidationException with null errors doesn't throw.
    /// </summary>
    [Fact]
    public void EntityValidationException_WithNullErrors_DoesNotThrow()
    {
        var exception = new EntityValidationException("Validation failed", null);

        exception.Should().NotBeNull();
        exception.ValidationErrors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that EntityValidationException can add errors after construction.
    /// </summary>
    [Fact]
    public void EntityValidationException_CanAddErrorsAfterConstruction()
    {
        var exception = new EntityValidationException("Validation failed");
        exception.ValidationErrors.Add("New error");

        exception.Should().NotBeNull();
        exception.ValidationErrors.Should().HaveCount(1);
        exception.ValidationErrors[0].Should().Be("New error");
    }

    /// <summary>
    /// Tests that ConcurrencyException is created with correct error code.
    /// </summary>
    [Fact]
    public void ConcurrencyException_CreatesWithCorrectErrorCode()
    {
        var exception = new ConcurrencyException("Concurrency conflict detected");

        exception.Should().NotBeNull();
        exception.Message.Should().Be("Concurrency conflict detected");
        exception.ErrorCode.Should().Be("CONCURRENCY_CONFLICT");
    }

    /// <summary>
    /// Tests that ConcurrencyException can include entity key in context.
    /// </summary>
    [Fact]
    public void ConcurrencyException_IncludesEntityKeyInContext()
    {
        var entityKey = Guid.NewGuid();
        var exception = new ConcurrencyException("Conflict", entityKey);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().NotBeNull();
        exception.ErrorContext!["EntityKey"].Should().Be(entityKey);
    }

    /// <summary>
    /// Tests that ConcurrencyException with null entity key doesn't add context.
    /// </summary>
    [Fact]
    public void ConcurrencyException_WithNullEntityKey_DoesNotAddContext()
    {
        var exception = new ConcurrencyException("Conflict", null);

        exception.Should().NotBeNull();
        exception.ErrorContext.Should().BeNull();
    }

    /// <summary>
    /// Tests that ConcurrencyException with numeric entity key adds context.
    /// </summary>
    [Fact]
    public void ConcurrencyException_WithNumericEntityKey_AddsContext()
    {
        var entityKey = 123;
        var exception = new ConcurrencyException("Conflict", entityKey);

        exception.Should().NotBeNull();
        exception.ErrorContext!["EntityKey"].Should().Be(entityKey);
    }

    /// <summary>
    /// Tests that all exception types inherit from OrmException.
    /// </summary>
    [Fact]
    public void AllExceptionTypes_InheritFromOrmException()
    {
        var ormException = new OrmException("Base exception");
        var dbException = new DatabaseConnectionException("DB error");
        var mappingException = new EntityMappingException("Mapping error");
        var queryException = new QueryExecutionException("Query error");
        var validationException = new EntityValidationException("Validation error");
        var concurrencyException = new ConcurrencyException("Concurrency error");

        ormException.Should().BeAssignableTo<OrmException>();
        dbException.Should().BeAssignableTo<OrmException>();
        mappingException.Should().BeAssignableTo<OrmException>();
        queryException.Should().BeAssignableTo<OrmException>();
        validationException.Should().BeAssignableTo<OrmException>();
        concurrencyException.Should().BeAssignableTo<OrmException>();
    }

    /// <summary>
    /// Tests that exception ToString() includes error code and context when present.
    /// </summary>
    [Fact]
    public void Exception_ToString_IncludesErrorCodeAndContext()
    {
        var exception = new OrmException("Test error", "TEST_CODE")
            .WithContext("Param1", "Value1");

        var toString = exception.ToString();

        toString.Should().Contain("Test error");
        toString.Should().Contain("TEST_CODE");
        toString.Should().Contain("Param1=Value1");
    }
}