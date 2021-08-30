#nullable enable

using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class DatabaseContextTests
{
    [Fact]
    public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
    {
        var act = () => new DatabaseContext(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("connectionString");
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ThrowsArgumentException()
    {
        var act = () => new DatabaseContext("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithValidConnectionString_CreatesInstance()
    {
        var context = new DatabaseContext("Data Source=:memory:");

        context.Should().NotBeNull();
    }

    [Fact]
    public async Task OpenAsync_WithValidConnection_OpensSuccessfully()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);

        var result = await context.OpenAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task OpenAsync_WithInvalidConnection_ThrowsDatabaseConnectionException()
    {
        var context = new DatabaseContext("InvalidConnectionString");

        var act = () => context.OpenAsync();

        await act.Should().ThrowAsync<DatabaseConnectionException>();
    }

    [Fact]
    public async Task CloseAsync_WithOpenConnection_ClosesSuccessfully()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();

        var result = await context.CloseAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithValidConnection_ReturnsTrue()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);

        var result = await context.TestConnectionAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithInvalidConnection_ReturnsFalse()
    {
        var context = new DatabaseContext("InvalidConnectionString");

        var result = await context.TestConnectionAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteScalarAsync_WithValidQuery_ReturnsResult()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();

        var result = await context.ExecuteScalarAsync("SELECT 1");

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteScalarAsync_WithParameters_ReturnsCorrectResult()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();

        var parameters = new Dictionary<string, object> { { "@value", 42 } };
        var result = await context.ExecuteScalarAsync("SELECT @value", parameters);

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteScalarAsync_WithInvalidQuery_ThrowsQueryExecutionException()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();

        var act = () => context.ExecuteScalarAsync("INVALID SQL");

        await act.Should().ThrowAsync<QueryExecutionException>();
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithValidQuery_ExecutesSuccessfully()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();

        var result = await context.ExecuteNonQueryAsync("CREATE TABLE Test (Id INTEGER PRIMARY KEY, Name TEXT)");

        // DDL statements report zero rows affected; success is verified by the table being usable afterwards.
        result.Should().Be(0);

        var insertResult = await context.ExecuteNonQueryAsync("INSERT INTO Test (Id, Name) VALUES (1, 'Test')");
        insertResult.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_WithParameters_ExecutesSuccessfully()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();
        await context.ExecuteNonQueryAsync("CREATE TABLE Test (Id INTEGER PRIMARY KEY, Name TEXT)");

        var parameters = new Dictionary<string, object> { { "@id", 1 }, { "@name", "Test" } };
        var result = await context.ExecuteNonQueryAsync("INSERT INTO Test (Id, Name) VALUES (@id, @name)", parameters);

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithValidQuery_ReturnsResults()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();
        await context.ExecuteNonQueryAsync("CREATE TABLE Test (Id INTEGER PRIMARY KEY, Name TEXT)");
        await context.ExecuteNonQueryAsync("INSERT INTO Test (Id, Name) VALUES (1, 'Test')");

        var results = await context.ExecuteQueryAsync("SELECT * FROM Test");

        results.Should().HaveCount(1);
        results[0]["Id"].Should().Be(1);
        results[0]["Name"].Should().Be("Test");
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithParameters_ReturnsFilteredResults()
    {
        var context = new DatabaseContext("Data Source=:memory:", DatabaseProvider.Sqlite);
        await context.OpenAsync();
        await context.ExecuteNonQueryAsync("CREATE TABLE Test (Id INTEGER PRIMARY KEY, Name TEXT, Value INTEGER)");
        await context.ExecuteNonQueryAsync("INSERT INTO Test (Id, Name, Value) VALUES (1, 'Test1', 10)");
        await context.ExecuteNonQueryAsync("INSERT INTO Test (Id, Name, Value) VALUES (2, 'Test2', 20)");

        var parameters = new Dictionary<string, object> { { "@value", 10 } };
        var results = await context.ExecuteQueryAsync("SELECT * FROM Test WHERE Value = @value", parameters);

        results.Should().HaveCount(1);
        results[0]["Name"].Should().Be("Test1");
    }

    [Fact]
    public async Task GetDatabaseProvider_ReturnsCorrectProvider()
    {
        var context = new DatabaseContext("Data Source=:memory:");

        var provider = context.GetDatabaseProvider();

        provider.Should().Be(DatabaseProvider.SqlServer);
    }

    [Fact]
    public void GetDatabaseProvider_AfterConstruction_ReturnsDefault()
    {
        var context = new DatabaseContext("Data Source=:memory:");

        var provider = context.GetDatabaseProvider();

        provider.Should().Be(DatabaseProvider.SqlServer);
    }
}
