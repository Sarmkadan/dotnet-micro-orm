using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMicroOrm.Cli
{
    public static class CommandParserExtensions
    {
        /// <summary>
        /// Registers a command with the parser and returns the command name for fluent chaining.
        /// </summary>
        /// <param name="parser">The command parser instance</param>
        /// <param name="name">Name of the command to register</param>
        /// <param name="description">Description of what the command does</param>
        /// <param name="handler">Action to execute when command is invoked</param>
        /// <returns>The command name for fluent chaining</returns>
        public static string RegisterCommand(
            this CommandParser parser,
            string name,
            string description,
            Action<CommandContext> handler)
        {
            parser.RegisterCommand(name, description, handler);
            return name;
        }

        /// <summary>
        /// Adds an option to the current command being configured and returns the parser for fluent chaining.
        /// </summary>
        /// <param name="parser">The command parser instance</param>
        /// <param name="commandName">Name of the command to add the option to</param>
        /// <param name="optionName">Name of the option (without leading dashes)</param>
        /// <param name="description">Description of the option</param>
        /// <param name="isRequired">Whether the option is required</param>
        /// <returns>The parser instance for fluent chaining</returns>
        public static CommandParser AddOption(
            this CommandParser parser,
            string commandName,
            string optionName,
            string description,
            bool isRequired = false)
        {
            parser.AddOption(commandName, optionName, description, isRequired);
            return parser;
        }

        /// <summary>
        /// Parses the command line arguments and returns a context containing the parsed command and options.
        /// </summary>
        /// <param name="parser">The command parser instance</param>
        /// <param name="args">Command line arguments</param>
        /// <returns>Parsed command context</returns>
        public static CommandContext Parse(this CommandParser parser, string[] args)
        {
            return parser.Parse(args);
        }

        /// <summary>
        /// Gets the help text for all registered commands in the parser.
        /// </summary>
        /// <param name="parser">The command parser instance</param>
        /// <returns>Formatted help text showing all commands and their options</returns>
        public static string GetHelpText(this CommandParser parser)
        {
            return parser.GetHelpText();
        }

        /// <summary>
        /// Determines if the specified argument exists in the command context.
        /// </summary>
        /// <param name="context">The command context</param>
        /// <param name="name">Name of the argument to check</param>
        /// <returns>True if the argument exists, false otherwise</returns>
        public static bool HasArgument(this CommandContext context, string name)
        {
            return context.HasArgument(name);
        }

        /// <summary>
        /// Gets the value of a command argument or returns a default value if not present.
        /// </summary>
        /// <param name="context">The command context</param>
        /// <param name="name">Name of the argument to retrieve</param>
        /// <param name="defaultValue">Default value to return if argument doesn't exist</param>
        /// <returns>The argument value or default</returns>
        public static string GetArgument(
            this CommandContext context,
            string name,
            string defaultValue = null)
        {
            var value = context.GetArgument(name);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// Gets the value of a command argument.
        /// </summary>
        /// <param name="context">The command context</param>
        /// <param name="name">Name of the argument to retrieve</param>
        /// <returns>The argument value or empty string if not found</returns>
        public static string GetArgument(this CommandContext context, string name)
        {
            return context.GetArgument(name);
        }
    }
}