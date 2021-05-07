#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotnetMicroOrm.Exceptions;

namespace DotnetMicroOrm.Cli;

/// <summary>
/// Parses and validates command-line arguments with support for subcommands,
/// options, and arguments. Provides help text and error handling.
/// </summary>
public sealed class CommandParser
{
    private readonly Dictionary<string, CommandDefinition> _commands = [];
    private readonly StringBuilder _helpText = new();

    /// <summary>
    /// Registers a command with its handler and options
    /// </summary>
    public void RegisterCommand(string name, string description, Action<CommandContext> handler)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new OrmException("Command name cannot be empty");

        var definition = new CommandDefinition
        {
            Name = name,
            Description = description,
            Handler = handler,
            Options = []
        };

        _commands[name] = definition;
    }

    /// <summary>
    /// Adds an option to a previously registered command
    /// </summary>
    public void AddOption(string commandName, string optionName, string description, bool isRequired = false)
    {
        if (!_commands.TryGetValue(commandName, out var command))
            throw new OrmException($"Command '{commandName}' not found");

        command.Options[optionName] = new OptionDefinition
        {
            Name = optionName,
            Description = description,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Parses command-line arguments and returns execution context
    /// </summary>
    public CommandContext Parse(string[] args)
    {
        if (args is null || args.Length == 0)
            return new CommandContext { ShowHelp = true };

        var commandName = args[0].ToLowerInvariant();

        if (commandName == "--help" || commandName == "-h" || commandName == "help")
            return new CommandContext { ShowHelp = true };

        if (!_commands.TryGetValue(commandName, out var command))
            throw new OrmException($"Unknown command: '{commandName}'. Use 'help' to see available commands.");

        var context = new CommandContext
        {
            CommandName = commandName,
            Handler = command.Handler,
            Arguments = []
        };

        // Parse options and arguments
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--"))
            {
                var optionName = arg[2..];
                var optionDef = command.Options.FirstOrDefault(x => x.Key == optionName).Value;

                if (optionDef is null)
                    throw new OrmException($"Unknown option: '--{optionName}'");

                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    context.Arguments[optionName] = args[++i];
                }
            }
            else if (arg.StartsWith("-"))
            {
                var flagName = arg[1..];
                context.Arguments[flagName] = "true";
            }
            else
            {
                // Positional argument
                if (!context.Arguments.ContainsKey("_args"))
                    context.Arguments["_args"] = arg;
            }
        }

        // Validate required options
        foreach (var option in command.Options.Where(x => x.Value.IsRequired))
        {
            if (!context.Arguments.ContainsKey(option.Key))
                throw new OrmException($"Required option '--{option.Key}' is missing");
        }

        return context;
    }

    /// <summary>
    /// Generates and returns help text for all commands
    /// </summary>
    public string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Micro ORM Command-Line Interface ===");
        sb.AppendLine();
        sb.AppendLine("Usage: dotnet run <command> [options]");
        sb.AppendLine();
        sb.AppendLine("Commands:");

        foreach (var cmd in _commands.Values)
        {
            sb.AppendLine($"  {cmd.Name,-15} {cmd.Description}");

            if (cmd.Options.Count > 0)
            {
                foreach (var opt in cmd.Options.Values)
                {
                    var required = opt.IsRequired ? " (required)" : " (optional)";
                    sb.AppendLine($"    --{opt.Name,-20} {opt.Description}{required}");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private class CommandDefinition
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Action<CommandContext> Handler { get; set; }
        public required Dictionary<string, OptionDefinition> Options { get; set; }
    }

    private class OptionDefinition
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsRequired { get; set; }
    }
}

/// <summary>
/// Context passed to command handlers containing parsed arguments and options
/// </summary>
public sealed class CommandContext
{
    public string CommandName { get; set; } = string.Empty;
    public Action<CommandContext>? Handler { get; set; }
    public Dictionary<string, string> Arguments { get; set; } = [];
    public bool ShowHelp { get; set; }

    public string GetArgument(string name) => Arguments.TryGetValue(name, out var value) ? value : string.Empty;
    public bool HasArgument(string name) => Arguments.ContainsKey(name);
}
