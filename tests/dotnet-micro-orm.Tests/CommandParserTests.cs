#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Cli;
using DotnetMicroOrm.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public sealed class CommandParserTests
{
    private readonly CommandParser _parser;

    public CommandParserTests()
    {
        _parser = new CommandParser();
    }

    [Fact]
    public void RegisterCommand_WithValidParameters_RegistersCommand()
    {
        // Arrange
        var handlerCalled = false;
        Action<CommandContext> handler = ctx => handlerCalled = true;

        // Act
        _parser.RegisterCommand("test", "Test command description", handler);

        // Assert - We can't directly access the registered command, but we can test via Parse
        var context = _parser.Parse(["test"]);
        context.Should().NotBeNull();
        context.CommandName.Should().Be("test");
        context.ShowHelp.Should().BeFalse();
    }

    [Fact]
    public void RegisterCommand_WithNullName_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };

        // Act
        Action act = () => _parser.RegisterCommand(null!, "description", handler);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Command name cannot be empty");
    }

    [Fact]
    public void RegisterCommand_WithEmptyName_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };

        // Act
        Action act = () => _parser.RegisterCommand("", "description", handler);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Command name cannot be empty");
    }

    [Fact]
    public void RegisterCommand_WithWhitespaceName_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };

        // Act
        Action act = () => _parser.RegisterCommand("   ", "description", handler);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Command name cannot be empty");
    }

    [Fact]
    public void AddOption_WithValidParameters_AddsOptionToCommand()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("test", "Test command", handler);

        // Act
        _parser.AddOption("test", "verbose", "Enable verbose output");

        // Assert - Test that the option is recognized during parsing
        var context = _parser.Parse(["test", "--verbose", "true"]);
        context.Should().NotBeNull();
        context.HasArgument("verbose").Should().BeTrue();
        context.GetArgument("verbose").Should().Be("true");
    }

    [Fact]
    public void AddOption_WithNonExistentCommand_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };

        // Act
        Action act = () => _parser.AddOption("nonexistent", "option", "description");

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Command 'nonexistent' not found");
    }

    [Fact]
    public void Parse_WithNullArgs_ReturnsHelpContext()
    {
        // Act
        var context = _parser.Parse(null!);

        // Assert
        context.Should().NotBeNull();
        context.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithEmptyArgs_ReturnsHelpContext()
    {
        // Act
        var context = _parser.Parse([]);

        // Assert
        context.Should().NotBeNull();
        context.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithHelpFlag_ReturnsHelpContext()
    {
        // Act
        var context = _parser.Parse(["--help"]);

        // Assert
        context.Should().NotBeNull();
        context.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithShortHelpFlag_ReturnsHelpContext()
    {
        // Act
        var context = _parser.Parse(["-h"]);

        // Assert
        context.Should().NotBeNull();
        context.ShowHelp.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithUnknownCommand_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("valid", "Valid command", handler);

        // Act
        Action act = () => _parser.Parse(["invalid"]);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Unknown command: 'invalid'. Use 'help' to see available commands.");
    }

    [Fact]
    public void Parse_WithValidCommand_ReturnsCommandContext()
    {
        // Arrange
        var handlerCalled = false;
        Action<CommandContext> handler = ctx => handlerCalled = true;
        _parser.RegisterCommand("valid", "Valid command", handler);

        // Act
        var context = _parser.Parse(["valid"]);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("valid");
        context.ShowHelp.Should().BeFalse();
        context.Handler.Should().NotBeNull();
        handlerCalled.Should().BeFalse(); // Handler not called yet
    }

    [Fact]
    public void Parse_WithValidCommandAndOptions_ParsesOptionsCorrectly()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("build", "Build project", handler);
        _parser.AddOption("build", "config", "Build configuration", true);
        _parser.AddOption("build", "output", "Output directory");

        // Act
        var context = _parser.Parse(["build", "--config", "Release", "--output", "bin/Release"]);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("build");
        context.HasArgument("config").Should().BeTrue();
        context.GetArgument("config").Should().Be("Release");
        context.HasArgument("output").Should().BeTrue();
        context.GetArgument("output").Should().Be("bin/Release");
    }

    [Fact]
    public void Parse_WithShortFlags_ParsesFlagsCorrectly()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("deploy", "Deploy application", handler);
        _parser.AddOption("deploy", "force", "Force deployment");
        _parser.AddOption("deploy", "dry-run", "Dry run mode");

        // Act
        var context = _parser.Parse(["deploy", "-f", "-d"]);

        // Assert
        context.Should().NotBeNull();
        context.HasArgument("f").Should().BeTrue();
        context.GetArgument("f").Should().Be("true");
        context.HasArgument("d").Should().BeTrue();
        context.GetArgument("d").Should().Be("true");
    }

    [Fact]
    public void Parse_WithRequiredOptionMissing_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("build", "Build project", handler);
        _parser.AddOption("build", "config", "Build configuration", true); // Required

        // Act
        Action act = () => _parser.Parse(["build"]);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Required option '--config' is missing");
    }

    [Fact]
    public void Parse_WithUnknownOption_ThrowsOrmException()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("test", "Test command", handler);

        // Act
        Action act = () => _parser.Parse(["test", "--unknown-option"]);

        // Assert
        act.Should().Throw<OrmException>()
            .WithMessage("Unknown option: '--unknown-option'");
    }

    [Fact]
    public void Parse_WithPositionalArgument_ParsesArgumentCorrectly()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("run", "Run command", handler);

        // Act
        var context = _parser.Parse(["run", "my-arg"]);

        // Assert
        context.Should().NotBeNull();
        context.HasArgument("_args").Should().BeTrue();
        context.GetArgument("_args").Should().Be("my-arg");
    }

    [Fact]
    public void Parse_WithMultiplePositionalArguments_UsesFirstArgument()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("run", "Run command", handler);

        // Act
        var context = _parser.Parse(["run", "first", "second", "third"]);

        // Assert
        context.Should().NotBeNull();
        context.GetArgument("_args").Should().Be("first");
    }

    [Fact]
    public void Parse_WithOptionValueContainingEqualsSign_ParsesCorrectly()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("test", "Test command", handler);
        _parser.AddOption("test", "path", "File path");

        // Act
        var context = _parser.Parse(["test", "--path", "/path/to/file=with=equals"]);

        // Assert
        context.Should().NotBeNull();
        context.GetArgument("path").Should().Be("/path/to/file=with=equals");
    }

    [Fact]
    public void GetHelpText_ReturnsNonEmptyString()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("test", "Test command description", handler);
        _parser.AddOption("test", "option1", "First option", true);

        // Act
        var helpText = _parser.GetHelpText();

        // Assert
        helpText.Should().NotBeNullOrEmpty();
        helpText.Should().Contain("Micro ORM Command-Line Interface");
        helpText.Should().Contain("test");
        helpText.Should().Contain("Test command description");
        helpText.Should().Contain("--option1");
        helpText.Should().Contain("(required)");
    }

    [Fact]
    public void GetHelpText_WithNoCommands_ReturnsBasicHelp()
    {
        // Act
        var helpText = _parser.GetHelpText();

        // Assert
        helpText.Should().NotBeNullOrEmpty();
        helpText.Should().Contain("Micro ORM Command-Line Interface");
    }

    [Fact]
    public void CommandContext_GetArgument_WithExistingArgument_ReturnsValue()
    {
        // Arrange
        var context = new CommandContext
        {
            CommandName = "test",
            Arguments = new Dictionary<string, string> { { "key", "value" } }
        };

        // Act
        var result = context.GetArgument("key");

        // Assert
        result.Should().Be("value");
    }

    [Fact]
    public void CommandContext_GetArgument_WithNonExistentArgument_ReturnsEmptyString()
    {
        // Arrange
        var context = new CommandContext
        {
            CommandName = "test",
            Arguments = new Dictionary<string, string>()
        };

        // Act
        var result = context.GetArgument("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CommandContext_HasArgument_WithExistingArgument_ReturnsTrue()
    {
        // Arrange
        var context = new CommandContext
        {
            CommandName = "test",
            Arguments = new Dictionary<string, string> { { "key", "value" } }
        };

        // Act
        var result = context.HasArgument("key");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CommandContext_HasArgument_WithNonExistentArgument_ReturnsFalse()
    {
        // Arrange
        var context = new CommandContext
        {
            CommandName = "test",
            Arguments = new Dictionary<string, string>()
        };

        // Act
        var result = context.HasArgument("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Parse_WithMixedOptionsAndPositionalArguments_ParsesCorrectly()
    {
        // Arrange
        Action<CommandContext> handler = _ => { };
        _parser.RegisterCommand("deploy", "Deploy application", handler);
        _parser.AddOption("deploy", "env", "Environment", true);
        _parser.AddOption("deploy", "version", "Version");

        // Act
        var context = _parser.Parse(["deploy", "--env", "production", "--version", "1.0.0", "my-service"]);

        // Assert
        context.Should().NotBeNull();
        context.HasArgument("env").Should().BeTrue();
        context.GetArgument("env").Should().Be("production");
        context.HasArgument("version").Should().BeTrue();
        context.GetArgument("version").Should().Be("1.0.0");
        context.HasArgument("_args").Should().BeTrue();
        context.GetArgument("_args").Should().Be("my-service");
    }
}