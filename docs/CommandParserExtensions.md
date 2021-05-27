# CommandParserExtensions

The `CommandParserExtensions` class provides a set of static extension methods designed to streamline the configuration, parsing, and interrogation of command-line arguments within the `dotnet-micro-orm` ecosystem. It acts as a fluent builder for constructing command definitions and offers utility methods to extract specific arguments or generate help documentation from a parsed context, reducing boilerplate code associated with manual argument processing.

## API

### RegisterCommand
Registers a new command definition with the parser, typically serving as the entry point for configuring a specific CLI operation.
*   **Parameters**: Accepts the command name and an optional description.
*   **Returns**: A `string` representing the registered command identifier or confirmation token.
*   **Throws**: Throws an exception if the command name is null, empty, or already registered within the current scope.

### AddOption
Appends a configurable option (flag or parameter) to an existing `CommandParser` instance, enabling fluent chaining.
*   **Parameters**: Takes the target `CommandParser`, the option name, a description, and optionally a default value or requirement flag.
*   **Returns**: The modified `CommandParser` instance to allow method chaining.
*   **Throws**: Throws if the option name conflicts with an existing option or if the parser instance is null.

### Parse
Executes the parsing logic against the provided raw command-line arguments based on the configured `CommandParser` schema.
*   **Parameters**: Accepts the configured `CommandParser` and the string array of raw arguments (e.g., `args`).
*   **Returns**: A `CommandContext` object containing the resolved values, flags, and command state.
*   **Throws**: Throws a parsing exception if required arguments are missing, unknown options are encountered, or type conversion fails.

### GetHelpText
Generates a formatted help string displaying available commands, options, and usage examples based on the current parser configuration.
*   **Parameters**: Takes the configured `CommandParser` instance.
*   **Returns**: A `string` containing the formatted help documentation.
*   **Throws**: Generally does not throw unless the parser configuration is internally inconsistent.

### HasArgument
Checks for the presence of a specific argument or option within the parsed `CommandContext`.
*   **Parameters**: Accepts the `CommandContext` and the name of the argument to check.
*   **Returns**: A `bool` indicating whether the argument exists and was provided.
*   **Throws**: Throws if the `CommandContext` is null.

### GetArgument (Boolean Return)
Retrieves a specific argument value from the `CommandContext`, returning a default or empty state if not found without throwing.
*   **Parameters**: Accepts the `CommandContext` and the argument name.
*   **Returns**: A `string` containing the argument value, or `null`/empty if the argument is absent.
*   **Throws**: Does not throw for missing arguments; only throws if the context is invalid.

### GetArgument (String Return - Overload)
Retrieves a specific argument value with strict validation, ensuring the argument exists before returning.
*   **Parameters**: Accepts the `CommandContext`, the argument name, and an optional error message for missing values.
*   **Returns**: A `string` containing the verified argument value.
*   **Throws**: Throws a specific exception if the requested argument is not present in the context, using the provided error message or a default one.

## Usage

### Example 1: Fluent Command Configuration and Parsing
This example demonstrates how to define a migration command with required and optional parameters, parse the input, and retrieve values safely.

```csharp
using DotnetMicroOrm.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure the parser
        var parser = new CommandParser()
            .RegisterCommand("migrate")
            .AddOption("--connection", "Database connection string", required: true)
            .AddOption("--env", "Target environment", defaultValue: "Development");

        try
        {
            // Parse the arguments
            var context = parser.Parse(args);

            // Retrieve arguments
            string connectionString = context.GetArgument("--connection", "Connection string is required.");
            string environment = context.GetArgument("--env");

            Console.WriteLine($"Running migration for {environment} on {connectionString}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(CommandParserExtensions.GetHelpText(parser));
        }
    }
}
```

### Example 2: Conditional Logic and Help Generation
This example shows how to check for optional flags and dynamically generate help text if the user provides invalid input or requests assistance.

```csharp
using DotnetMicroOrm.Cli;

public class CliHandler
{
    public static void Handle(string[] args)
    {
        var parser = new CommandParser()
            .RegisterCommand("seed")
            .AddOption("--force", "Overwrite existing data", required: false);

        // Check for help flag manually or via logic
        if (args.Contains("--help") || args.Length == 0)
        {
            Console.WriteLine(CommandParserExtensions.GetHelpText(parser));
            return;
        }

        var context = parser.Parse(args);

        if (context.HasArgument("--force"))
        {
            Console.WriteLine("Force mode enabled. Existing data will be overwritten.");
        }
        
        // Proceed with seeding logic
    }
}
```

## Notes

*   **Thread Safety**: As `CommandParserExtensions` consists entirely of static methods operating on passed-in instances (`CommandParser`, `CommandContext`), the class itself is thread-safe. However, the `CommandParser` and `CommandContext` instances passed to these methods are not inherently thread-safe; they should not be mutated by multiple threads simultaneously during configuration or parsing phases.
*   **Argument Precedence**: When using the overloaded `GetArgument` methods, be aware that the strict overload (throwing on missing) should be used for mandatory business logic parameters, while the non-throwing overload is suitable for optional feature flags.
*   **Parsing State**: The `Parse` method creates a new `CommandContext` snapshot. Subsequent modifications to the original `CommandParser` configuration after parsing will not reflect in the already created `CommandContext`.
*   **Exception Handling**: While `HasArgument` and the non-strict `GetArgument` overload handle missing data gracefully, the strict `GetArgument` and `Parse` methods will terminate execution flow via exceptions if constraints are violated. Callers should wrap parsing logic in try-catch blocks to capture `DotnetMicroOrmException` or standard parsing errors.
