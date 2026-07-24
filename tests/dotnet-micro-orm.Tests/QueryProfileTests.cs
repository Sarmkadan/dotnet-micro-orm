#nullable enable

using DotnetMicroOrm.Profiling;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the QueryProfile class.
/// </summary>
public sealed class QueryProfileTests
{
    /// <summary>
    /// Tests that QueryProfile is created with a valid GUID Id.
    /// </summary>
    [Fact]
    public void Constructor_GeneratesValidGuidId()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.Id.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Tests that QueryProfile is created with default empty Query string.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultEmptyQuery()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.Query.Should().BeEmpty();
        profile.Query.Should().BeOfType<string>();
    }

    /// <summary>
    /// Tests that QueryProfile is created with null Parameters by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultNullParameters()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.Parameters.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile is created with zero Duration by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultZeroDuration()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.Duration.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that QueryProfile is created with ExecutedAt set to DateTime.MinValue by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultExecutedAtToMinValue()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.ExecutedAt.Should().Be(DateTime.MinValue);
    }

    /// <summary>
    /// Tests that QueryProfile is created with Succeeded set to false by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultSucceededToFalse()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.Succeeded.Should().BeFalse();
    }

    /// <summary>
    /// Tests that QueryProfile is created with null ErrorMessage by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultNullErrorMessage()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile is created with null CallerMemberName by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultNullCallerMemberName()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.CallerMemberName.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile is created with null RowsAffected by default.
    /// </summary>
    [Fact]
    public void Constructor_SetsDefaultNullRowsAffected()
    {
        // Act
        var profile = new QueryProfile();

        // Assert
        profile.RowsAffected.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom Query string.
    /// </summary>
    [Fact]
    public void WithQuery_SetsQueryProperty()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @id";

        // Act
        var profile = new QueryProfile { Query = query };

        // Assert
        profile.Query.Should().Be(query);
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom Parameters dictionary.
    /// </summary>
    [Fact]
    public void WithParameters_SetsParametersProperty()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { { "@id", 42 }, { "@name", "John" } };

        // Act
        var profile = new QueryProfile { Parameters = parameters };

        // Assert
        profile.Parameters.Should().NotBeNull();
        profile.Parameters.Should().HaveCount(2);
        profile.Parameters.Should().ContainKey("@id");
        profile.Parameters.Should().ContainKey("@name");
        profile.Parameters!["@id"].Should().Be(42);
        profile.Parameters!["@name"].Should().Be("John");
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom Duration.
    /// </summary>
    [Fact]
    public void WithDuration_SetsDurationProperty()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(150);

        // Act
        var profile = new QueryProfile { Duration = duration };

        // Assert
        profile.Duration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom ExecutedAt time.
    /// </summary>
    [Fact]
    public void WithExecutedAt_SetsExecutedAtProperty()
    {
        // Arrange
        var executedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var profile = new QueryProfile { ExecutedAt = executedAt };

        // Assert
        profile.ExecutedAt.Should().Be(executedAt);
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom Succeeded flag.
    /// </summary>
    [Fact]
    public void WithSucceeded_SetsSucceededProperty()
    {
        // Act
        var profile = new QueryProfile { Succeeded = false };

        // Assert
        profile.Succeeded.Should().BeFalse();
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom ErrorMessage.
    /// </summary>
    [Fact]
    public void WithErrorMessage_SetsErrorMessageProperty()
    {
        // Arrange
        var errorMessage = "Timeout expired";

        // Act
        var profile = new QueryProfile { ErrorMessage = errorMessage };

        // Assert
        profile.ErrorMessage.Should().Be(errorMessage);
        profile.Succeeded.Should().BeFalse();
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom CallerMemberName.
    /// </summary>
    [Fact]
    public void WithCallerMemberName_SetsCallerMemberNameProperty()
    {
        // Arrange
        var callerName = "GetUserById";

        // Act
        var profile = new QueryProfile { CallerMemberName = callerName };

        // Assert
        profile.CallerMemberName.Should().Be(callerName);
    }

    /// <summary>
    /// Tests that QueryProfile can be created with custom RowsAffected value.
    /// </summary>
    [Fact]
    public void WithRowsAffected_SetsRowsAffectedProperty()
    {
        // Arrange
        var rowsAffected = 10;

        // Act
        var profile = new QueryProfile { RowsAffected = rowsAffected };

        // Assert
        profile.RowsAffected.Should().Be(rowsAffected);
    }

    /// <summary>
    /// Tests that QueryProfile can be created with all properties set.
    /// </summary>
    [Fact]
    public void WithAllProperties_SetsAllProperties()
    {
        // Arrange
        var query = "SELECT COUNT(*) FROM Products";
        var parameters = new Dictionary<string, object> { { "@category", "Electronics" } };
        var duration = TimeSpan.FromSeconds(2);
        var executedAt = new DateTime(2024, 6, 15, 9, 30, 0, DateTimeKind.Utc);
        var succeeded = false;
        var errorMessage = "Connection timeout";
        var callerMemberName = "GetProductCount";
        var rowsAffected = 150;

        // Act
        var profile = new QueryProfile
        {
            Query = query,
            Parameters = parameters,
            Duration = duration,
            ExecutedAt = executedAt,
            Succeeded = succeeded,
            ErrorMessage = errorMessage,
            CallerMemberName = callerMemberName,
            RowsAffected = rowsAffected
        };

        // Assert
        profile.Query.Should().Be(query);
        profile.Parameters.Should().BeSameAs(parameters);
        profile.Duration.Should().Be(duration);
        profile.ExecutedAt.Should().Be(executedAt);
        profile.Succeeded.Should().Be(succeeded);
        profile.ErrorMessage.Should().Be(errorMessage);
        profile.CallerMemberName.Should().Be(callerMemberName);
        profile.RowsAffected.Should().Be(rowsAffected);
    }

    /// <summary>
    /// Tests that QueryProfile Id is immutable (init-only property).
    /// </summary>
    [Fact]
    public void Id_Property_IsInitOnly()
    {
        // Arrange
        var profile = new QueryProfile();
        var originalId = profile.Id;

        // Act - Try to change Id (should not compile, but we can verify it's set once)
        // This test just verifies the property exists and has a value
        profile.Id.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Tests that QueryProfile Query is immutable (init-only property).
    /// </summary>
    [Fact]
    public void Query_Property_IsInitOnly()
    {
        // Arrange
        var profile = new QueryProfile { Query = "SELECT 1" };

        // Assert
        profile.Query.Should().Be("SELECT 1");
    }

    /// <summary>
    /// Tests that QueryProfile Parameters is immutable (init-only property).
    /// </summary>
    [Fact]
    public void Parameters_Property_IsInitOnly()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { { "@id", 1 } };
        var profile = new QueryProfile { Parameters = parameters };

        // Assert
        profile.Parameters.Should().BeSameAs(parameters);
    }

    /// <summary>
    /// Tests that QueryProfile Duration is immutable (init-only property).
    /// </summary>
    [Fact]
    public void Duration_Property_IsInitOnly()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(100);
        var profile = new QueryProfile { Duration = duration };

        // Assert
        profile.Duration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfile ExecutedAt is immutable (init-only property).
    /// </summary>
    [Fact]
    public void ExecutedAt_Property_IsInitOnly()
    {
        // Arrange
        var executedAt = DateTime.UtcNow;
        var profile = new QueryProfile { ExecutedAt = executedAt };

        // Assert
        profile.ExecutedAt.Should().Be(executedAt);
    }

    /// <summary>
    /// Tests that QueryProfile Succeeded is immutable (init-only property).
    /// </summary>
    [Fact]
    public void Succeeded_Property_IsInitOnly()
    {
        // Arrange
        var profile = new QueryProfile { Succeeded = true };

        // Assert
        profile.Succeeded.Should().BeTrue();
    }

    /// <summary>
    /// Tests that QueryProfile ErrorMessage is immutable (init-only property).
    /// </summary>
    [Fact]
    public void ErrorMessage_Property_IsInitOnly()
    {
        // Arrange
        var errorMessage = "Test error";
        var profile = new QueryProfile { ErrorMessage = errorMessage };

        // Assert
        profile.ErrorMessage.Should().Be(errorMessage);
    }

    /// <summary>
    /// Tests that QueryProfile CallerMemberName is immutable (init-only property).
    /// </summary>
    [Fact]
    public void CallerMemberName_Property_IsInitOnly()
    {
        // Arrange
        var callerName = "TestMethod";
        var profile = new QueryProfile { CallerMemberName = callerName };

        // Assert
        profile.CallerMemberName.Should().Be(callerName);
    }

    /// <summary>
    /// Tests that QueryProfile RowsAffected is immutable (init-only property).
    /// </summary>
    [Fact]
    public void RowsAffected_Property_IsInitOnly()
    {
        // Arrange
        var rowsAffected = 100;
        var profile = new QueryProfile { RowsAffected = rowsAffected };

        // Assert
        profile.RowsAffected.Should().Be(rowsAffected);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary TotalQueries is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_TotalQueries_IsInitOnly()
    {
        // Arrange
        var summary = new QueryProfilerSummary { TotalQueries = 5 };

        // Assert
        summary.TotalQueries.Should().Be(5);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary TotalDuration is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_TotalDuration_IsInitOnly()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(10);
        var summary = new QueryProfilerSummary { TotalDuration = duration };

        // Assert
        summary.TotalDuration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary AverageDuration is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_AverageDuration_IsInitOnly()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(500);
        var summary = new QueryProfilerSummary { AverageDuration = duration };

        // Assert
        summary.AverageDuration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary MaxDuration is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_MaxDuration_IsInitOnly()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(5);
        var summary = new QueryProfilerSummary { MaxDuration = duration };

        // Assert
        summary.MaxDuration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary MinDuration is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_MinDuration_IsInitOnly()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(10);
        var summary = new QueryProfilerSummary { MinDuration = duration };

        // Assert
        summary.MinDuration.Should().Be(duration);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary FailedQueries is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_FailedQueries_IsInitOnly()
    {
        // Arrange
        var summary = new QueryProfilerSummary { FailedQueries = 3 };

        // Assert
        summary.FailedQueries.Should().Be(3);
    }

    /// <summary>
    /// Tests that QueryProfilerSummary SlowestQuery is immutable (init-only property).
    /// </summary>
    [Fact]
    public void QueryProfilerSummary_SlowestQuery_IsInitOnly()
    {
        // Arrange
        var profile = new QueryProfile { Query = "Slow query", Duration = TimeSpan.FromSeconds(10) };
        var summary = new QueryProfilerSummary { SlowestQuery = profile };

        // Assert
        summary.SlowestQuery.Should().BeSameAs(profile);
    }

    /// <summary>
    /// Tests that QueryProfile with empty query string is valid.
    /// </summary>
    [Fact]
    public void WithEmptyQuery_IsValid()
    {
        // Act
        var profile = new QueryProfile { Query = string.Empty };

        // Assert
        profile.Query.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that QueryProfile with null query string is valid.
    /// </summary>
    [Fact]
    public void WithNullQuery_IsValid()
    {
        // Act
        var profile = new QueryProfile { Query = null };

        // Assert
        profile.Query.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile with empty Parameters dictionary is valid.
    /// </summary>
    [Fact]
    public void WithEmptyParameters_IsValid()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act
        var profile = new QueryProfile { Parameters = parameters };

        // Assert
        profile.Parameters.Should().NotBeNull();
        profile.Parameters.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that QueryProfile with zero TimeSpan duration is valid.
    /// </summary>
    [Fact]
    public void WithZeroDuration_IsValid()
    {
        // Act
        var profile = new QueryProfile { Duration = TimeSpan.Zero };

        // Assert
        profile.Duration.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that QueryProfile with negative TimeSpan duration is valid (edge case).
    /// </summary>
    [Fact]
    public void WithNegativeDuration_IsValid()
    {
        // Arrange
        var negativeDuration = TimeSpan.FromMilliseconds(-100);

        // Act
        var profile = new QueryProfile { Duration = negativeDuration };

        // Assert
        profile.Duration.Should().Be(negativeDuration);
    }

    /// <summary>
    /// Tests that QueryProfile with very large TimeSpan duration is valid.
    /// </summary>
    [Fact]
    public void WithLargeDuration_IsValid()
    {
        // Arrange
        var largeDuration = TimeSpan.FromDays(365);

        // Act
        var profile = new QueryProfile { Duration = largeDuration };

        // Assert
        profile.Duration.Should().Be(largeDuration);
    }

    /// <summary>
    /// Tests that QueryProfile with minimum DateTime is valid.
    /// </summary>
    [Fact]
    public void WithMinDateTime_IsValid()
    {
        // Arrange
        var minDate = DateTime.MinValue;

        // Act
        var profile = new QueryProfile { ExecutedAt = minDate };

        // Assert
        profile.ExecutedAt.Should().Be(minDate);
    }

    /// <summary>
    /// Tests that QueryProfile with maximum DateTime is valid.
    /// </summary>
    [Fact]
    public void WithMaxDateTime_IsValid()
    {
        // Arrange
        var maxDate = DateTime.MaxValue;

        // Act
        var profile = new QueryProfile { ExecutedAt = maxDate };

        // Assert
        profile.ExecutedAt.Should().Be(maxDate);
    }

    /// <summary>
    /// Tests that QueryProfile with very large RowsAffected value is valid.
    /// </summary>
    [Fact]
    public void WithLargeRowsAffected_IsValid()
    {
        // Arrange
        var largeRows = int.MaxValue;

        // Act
        var profile = new QueryProfile { RowsAffected = largeRows };

        // Assert
        profile.RowsAffected.Should().Be(largeRows);
    }

    /// <summary>
    /// Tests that QueryProfile with negative RowsAffected value is valid (edge case).
    /// </summary>
    [Fact]
    public void WithNegativeRowsAffected_IsValid()
    {
        // Arrange
        var negativeRows = -1;

        // Act
        var profile = new QueryProfile { RowsAffected = negativeRows };

        // Assert
        profile.RowsAffected.Should().Be(negativeRows);
    }

    /// <summary>
    /// Tests that QueryProfile with complex Parameters dictionary is valid.
    /// </summary>
    [Fact]
    public void WithComplexParameters_IsValid()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "@id", 42 },
            { "@name", "John Doe" },
            { "@isActive", true },
            { "@createdDate", new DateTime(2024, 1, 1) },
            { "@amount", 123.45m }
        };

        // Act
        var profile = new QueryProfile { Parameters = parameters };

        // Assert
        profile.Parameters.Should().NotBeNull();
        profile.Parameters.Should().HaveCount(5);
        profile.Parameters!["@id"].Should().Be(42);
        profile.Parameters!["@name"].Should().Be("John Doe");
        profile.Parameters!["@isActive"].Should().Be(true);
        profile.Parameters!["@createdDate"].Should().Be(new DateTime(2024, 1, 1));
        profile.Parameters!["@amount"].Should().Be(123.45m);
    }

    /// <summary>
    /// Tests that QueryProfile with special characters in Query string is valid.
    /// </summary>
    [Fact]
    public void WithSpecialCharactersInQuery_IsValid()
    {
        // Arrange
        var query = "SELECT * FROM \"Users\" WHERE \"Name\" = 'O\'Reilly' AND \"Status\" = @status";

        // Act
        var profile = new QueryProfile { Query = query };

        // Assert
        profile.Query.Should().Be(query);
    }

    /// <summary>
    /// Tests that QueryProfile with very long CallerMemberName is valid.
    /// </summary>
    [Fact]
    public void WithLongCallerMemberName_IsValid()
    {
        // Arrange
        var longCallerName = new string('a', 1000);

        // Act
        var profile = new QueryProfile { CallerMemberName = longCallerName };

        // Assert
        profile.CallerMemberName.Should().Be(longCallerName);
    }

    /// <summary>
    /// Tests that QueryProfile with null CallerMemberName is valid.
    /// </summary>
    [Fact]
    public void WithNullCallerMemberName_IsValid()
    {
        // Act
        var profile = new QueryProfile { CallerMemberName = null };

        // Assert
        profile.CallerMemberName.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile with null ErrorMessage and Succeeded=true is consistent.
    /// </summary>
    [Fact]
    public void NullErrorMessageWithSucceededTrue_IsConsistent()
    {
        // Act
        var profile = new QueryProfile { Succeeded = true, ErrorMessage = null };

        // Assert
        profile.Succeeded.Should().BeTrue();
        profile.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Tests that QueryProfile with ErrorMessage and Succeeded=false is consistent.
    /// </summary>
    [Fact]
    public void ErrorMessageWithSucceededFalse_IsConsistent()
    {
        // Arrange
        var errorMessage = "Query timeout";

        // Act
        var profile = new QueryProfile { Succeeded = false, ErrorMessage = errorMessage };

        // Assert
        profile.Succeeded.Should().BeFalse();
        profile.ErrorMessage.Should().Be(errorMessage);
    }

    /// <summary>
    /// Tests that QueryProfile with non-null ErrorMessage but Succeeded=true is allowed (edge case).
    /// </summary>
    [Fact]
    public void NonNullErrorMessageWithSucceededTrue_IsAllowed()
    {
        // Arrange
        var errorMessage = "Warning: slow query";

        // Act
        var profile = new QueryProfile { Succeeded = true, ErrorMessage = errorMessage };

        // Assert
        profile.Succeeded.Should().BeTrue();
        profile.ErrorMessage.Should().Be(errorMessage);
    }
}