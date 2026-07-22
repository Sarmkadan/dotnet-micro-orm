#nullable enable

using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the OrmExceptionValidation class.
/// </summary>
public sealed class OrmExceptionValidationTests
{
    /// <summary>
    /// Tests that Validate returns empty list for a valid OrmException (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidOrmException_ReturnsEmptyList()
    {
        var exception = new OrmException("Valid error message", "VALID_CODE");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns empty list for a valid DatabaseConnectionException (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidDatabaseConnectionException_ReturnsEmptyList()
    {
        var exception = new DatabaseConnectionException("Connection failed");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns empty list for a valid EntityMappingException (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidEntityMappingException_ReturnsEmptyList()
    {
        var exception = new EntityMappingException("Mapping failed");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns empty list for a valid QueryExecutionException (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidQueryExecutionException_ReturnsEmptyList()
    {
        var exception = new QueryExecutionException("Query failed");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns empty list for a valid EntityValidationException with errors (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidEntityValidationExceptionWithErrors_ReturnsEmptyList()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var exception = new EntityValidationException("Validation failed", errors);

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns empty list for a valid ConcurrencyException (happy path).
    /// </summary>
    [Fact]
    public void Validate_ValidConcurrencyException_ReturnsEmptyList()
    {
        var exception = new ConcurrencyException("Concurrency conflict");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that IsValid returns true for a valid OrmException (happy path).
    /// </summary>
    [Fact]
    public void IsValid_ValidOrmException_ReturnsTrue()
    {
        var exception = new OrmException("Valid error message", "VALID_CODE");

        var result = exception.IsValid();

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsValid returns false for an invalid OrmException with empty message.
    /// </summary>
    [Fact]
    public void IsValid_InvalidOrmExceptionWithEmptyMessage_ReturnsFalse()
    {
        var exception = new OrmException(string.Empty, "VALID_CODE");

        var result = exception.IsValid();

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsValid returns false for an invalid OrmException with empty error code.
    /// </summary>
    [Fact]
    public void IsValid_InvalidOrmExceptionWithEmptyErrorCode_ReturnsFalse()
    {
        var exception = new OrmException("Valid message", "");

        var result = exception.IsValid();

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that EnsureValid does not throw for a valid OrmException (happy path).
    /// </summary>
    [Fact]
    public void EnsureValid_ValidOrmException_DoesNotThrow()
    {
        var exception = new OrmException("Valid error message", "VALID_CODE");

        Action act = () => exception.EnsureValid();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that EnsureValid does not throw for a valid EntityValidationException (happy path).
    /// </summary>
    [Fact]
    public void EnsureValid_ValidEntityValidationException_DoesNotThrow()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var exception = new EntityValidationException("Validation failed", errors);

        Action act = () => exception.EnsureValid();

        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentNullException when passed null.
    /// </summary>
    [Fact]
    public void EnsureValid_NullException_ThrowsArgumentNullException()
    {
        OrmException? exception = null;

        Action act = () => exception.EnsureValid();

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Validate throws ArgumentNullException when passed null (error path).
    /// </summary>
    [Fact]
    public void Validate_NullException_ThrowsArgumentNullException()
    {
        OrmException? exception = null;

        Action act = () => exception.Validate();

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that IsValid throws ArgumentNullException when passed null (error path).
    /// </summary>
    [Fact]
    public void IsValid_NullException_ThrowsArgumentNullException()
    {
        OrmException? exception = null;

        Action act = () => exception.IsValid();

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Validate detects empty message in OrmException.
    /// </summary>
    [Fact]
    public void Validate_OrmExceptionWithEmptyMessage_ReturnsError()
    {
        var exception = new OrmException(string.Empty, "VALID_CODE");

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("Message cannot be null, empty, or whitespace.");
    }


    /// <summary>
    /// Tests that Validate detects null error code in OrmException.
    /// </summary>
    [Fact]
    public void Validate_OrmExceptionWithNullErrorCode_ReturnsError()
    {
        var exception = new OrmException("Valid message", null);

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("ErrorCode must be a non-empty string.");
    }

    /// <summary>
    /// Tests that Validate detects empty error code in OrmException.
    /// </summary>
    [Fact]
    public void Validate_OrmExceptionWithEmptyErrorCode_ReturnsError()
    {
        var exception = new OrmException("Valid message", "");

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("ErrorCode must be a non-empty string.");
    }

    /// <summary>
    /// Tests that Validate accepts whitespace-only error code in OrmException (validation doesn't check whitespace).
    /// </summary>
    [Fact]
    public void Validate_OrmExceptionWithWhitespaceErrorCode_ReturnsEmptyList()
    {
        var exception = new OrmException("Valid message", "   ");

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate detects invalid EntityValidationException with empty ValidationErrors.
    /// </summary>
    [Fact]
    public void Validate_EntityValidationExceptionWithEmptyValidationErrors_ReturnsError()
    {
        var exception = new EntityValidationException("Validation failed");

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("EntityValidationException must have at least one validation error in ValidationErrors list.");
    }

    /// <summary>
    /// Tests that Validate detects invalid EntityMappingException with whitespace property name.
    /// </summary>
    [Fact]
    public void Validate_EntityMappingExceptionWithWhitespacePropertyName_ReturnsError()
    {
        var exception = new EntityMappingException("Mapping failed", "   ");

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("EntityMappingException.Property context value cannot be null, empty, or whitespace.");
    }

    /// <summary>
    /// Tests that Validate detects invalid QueryExecutionException with whitespace query.
    /// </summary>
    [Fact]
    public void Validate_QueryExecutionExceptionWithWhitespaceQuery_ReturnsError()
    {
        var exception = new QueryExecutionException("Query failed", "   ");

        var result = exception.Validate();

        result.Should().ContainSingle().Which.Should().Be("QueryExecutionException.Query context value cannot be null, empty, or whitespace.");
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentException when exception has validation errors.
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidException_ThrowsArgumentException()
    {
        var exception = new OrmException(null!, "");

        Action act = () => exception.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*OrmException is invalid. Validation errors:*");
    }

    /// <summary>
    /// Tests that EnsureValid throws ArgumentException with validation errors when exception is invalid.
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidException_ThrowsArgumentExceptionWithValidationErrors()
    {
        var exception = new OrmException(null!, "");

        Action act = () => exception.EnsureValid();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*OrmException is invalid. Validation errors:*" +
                        "*ErrorCode must be a non-empty string*");
    }

    /// <summary>
    /// Tests that Validate returns multiple errors for an OrmException with multiple issues.
    /// </summary>
    [Fact]
    public void Validate_OrmExceptionWithMultipleIssues_ReturnsMultipleErrors()
    {
        var exception = new OrmException(string.Empty, string.Empty);

        var result = exception.Validate();

        result.Should().HaveCount(2);
        result.Should().Contain("Message cannot be null, empty, or whitespace.");
        result.Should().Contain("ErrorCode must be a non-empty string.");
    }

    /// <summary>
    /// Tests that Validate works correctly with EntityValidationException that has errors.
    /// </summary>
    [Fact]
    public void Validate_EntityValidationExceptionWithErrors_ReturnsEmptyList()
    {
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var exception = new EntityValidationException("Validation failed", errors);

        var result = exception.Validate();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that IsValid returns false for EntityValidationException with empty ValidationErrors.
    /// </summary>
    [Fact]
    public void IsValid_EntityValidationExceptionWithEmptyValidationErrors_ReturnsFalse()
    {
        var exception = new EntityValidationException("Validation failed");

        var result = exception.IsValid();

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Validate returns empty list for EntityValidationException with populated ValidationErrors.
    /// </summary>
    [Fact]
    public void Validate_EntityValidationExceptionWithPopulatedValidationErrors_ReturnsEmptyList()
    {
        var errors = new List<string> { "Error 1" };
        var exception = new EntityValidationException("Validation failed", errors);

        var result = exception.Validate();

        result.Should().BeEmpty();
    }
}
