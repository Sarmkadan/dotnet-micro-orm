#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Cli;
using FluentAssertions;
using Moq;
using Xunit;

/// <summary>
/// Tests for the CommandHandler class.
/// </summary>
public sealed class CommandHandlerTests
{
    /// <summary>
    /// Builds a mock IServiceProvider instance.
    /// </summary>
    /// <returns>A mock IServiceProvider instance.</returns>
    private static Mock<IServiceProvider> BuildServiceProviderMock()
    {
        var mock = new Mock<IServiceProvider>();
        return mock;
    }

    /// <summary>
    /// Builds a mock IDatabaseContext instance.
    /// </summary>
    /// <returns>A mock IDatabaseContext instance.</returns>
    private static Mock<IDatabaseContext> BuildDatabaseContextMock()
    {
        var mock = new Mock<IDatabaseContext>();
        return mock;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_NullDbContext_ThrowsArgumentNullException()
    {
        var serviceProviderMock = BuildServiceProviderMock();

        Action act = () => new CommandHandler(null!, serviceProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*dbContext*");
    }

    [Fact]
    public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
    {
        var dbContextMock = BuildDatabaseContextMock();

        Action act = () => new CommandHandler(dbContextMock.Object, null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*serviceProvider*");
    }

    [Fact]
    public void Constructor_ValidParameters_Succeeds()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        Action act = () => new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);

        act.Should().NotThrow();
    }

    // ========== ExecuteAsync Tests ==========

    [Fact]
    public async Task ExecuteAsync_ShowHelp_ReturnsZero()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var context = new CommandContext { ShowHelp = true };

        var result = await handler.ExecuteAsync(context);

        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_NullHandler_ReturnsOne()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var context = new CommandContext { Handler = null };

        var result = await handler.ExecuteAsync(context);

        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ValidHandler_ReturnsZero()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var context = new CommandContext { Handler = ctx => { } };

        var result = await handler.ExecuteAsync(context);

        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionThrown_ReturnsOneAndWritesError()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var context = new CommandContext { Handler = ctx => throw new InvalidOperationException("Test error") };

        using var errorStream = new MemoryStream();
        using var errorWriter = new StreamWriter(errorStream);
        var originalError = Console.Error;
        Console.SetError(errorWriter);

        try
        {
            var result = await handler.ExecuteAsync(context);

            result.Should().Be(1);

            errorWriter.Flush();
            errorStream.Position = 0;
            using var reader = new StreamReader(errorStream);
            var errorOutput = reader.ReadToEnd();

            errorOutput.Should().Contain("Error: Test error");
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    // ========== GetService<T> Tests ==========

    [Fact]
    public void GetService_RegisteredService_ReturnsService()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var testService = new TestService();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestService)))
            .Returns(testService);

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);

        var result = handler.GetService<TestService>();

        result.Should().BeSameAs(testService);
    }

    [Fact]
    public void GetService_UnregisteredService_ThrowsInvalidOperationException()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestService)))
            .Returns((object?)null);

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);

        Action act = () => handler.GetService<TestService>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Service TestService not registered");
    }

    [Fact]
    public void GetService_WrongTypeReturned_ThrowsInvalidOperationException()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestService)))
            .Returns((object?)new object());

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);

        Action act = () => handler.GetService<TestService>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Service TestService not registered");
    }

    // ========== CreateStandardParser Tests ==========

    [Fact]
    public void CreateStandardParser_ReturnsNonNullParser()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);

        var parser = handler.CreateStandardParser();

        parser.Should().NotBeNull();
    }

    [Fact]
    public void CreateStandardParser_UserListCommand_IsRegistered()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var parser = handler.CreateStandardParser();

        var context = parser.Parse(["user-list"]);

        context.Should().NotBeNull();
        context.CommandName.Should().Be("user-list");
    }

    [Fact]
    public void CreateStandardParser_UserCreateCommand_HasRequiredOptions()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var parser = handler.CreateStandardParser();

        var context = parser.Parse(["user-create", "--username", "testuser", "--email", "test@example.com"]);

        context.Should().NotBeNull();
        context.CommandName.Should().Be("user-create");
        context.HasArgument("username").Should().BeTrue();
        context.GetArgument("username").Should().Be("testuser");
        context.HasArgument("email").Should().BeTrue();
        context.GetArgument("email").Should().Be("test@example.com");
    }

    [Fact]
    public void CreateStandardParser_ProductCreateCommand_HasRequiredOptions()
    {
        var dbContextMock = BuildDatabaseContextMock();
        var serviceProviderMock = BuildServiceProviderMock();

        var handler = new CommandHandler(dbContextMock.Object, serviceProviderMock.Object);
        var parser = handler.CreateStandardParser();

        var context = parser.Parse(["product-create", "--name", "Test Product", "--price", "99.99"]);

        context.Should().NotBeNull();
        context.CommandName.Should().Be("product-create");
        context.HasArgument("name").Should().BeTrue();
        context.GetArgument("name").Should().Be("Test Product");
        context.HasArgument("price").Should().BeTrue();
        context.GetArgument("price").Should().Be("99.99");
    }

    /// <summary>
    /// Simple test service for GetService tests.
    /// </summary>
    private class TestService
    {
    }
}