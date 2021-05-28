# CommandParser

A lightweight command-line argument parser for .NET applications that supports named commands, options, and positional arguments with built-in help generation.

## API

### `void RegisterCommand(string commandName, Action<CommandContext> handler)`

Registers a new command with the parser. The command can later be invoked via its name.

- **Parameters**
  - `commandName`: The unique identifier for the command (case-sensitive).
  - `handler`: The delegate invoked when the command is parsed and executed.
- **Throws**
  - `ArgumentNullException`: If `commandName` or `handler` is `null`.
  - `InvalidOperationException`: If a command with the same `commandName` already exists.

---

### `void AddOption(string optionName, string description, bool isRequired = false)`

Adds a global option that applies to all commands. Options are prefixed with `--` when parsed.

- **Parameters**
  - `optionName`: The name of the option (case-sensitive, must not start with `--`).
  - `description`: Human-readable explanation of the option’s purpose.
  - `isRequired`: Whether the option must be present for parsing to succeed.
- **Throws**
  - `ArgumentNullException`: If `optionName` or `description` is `null`.
  - `InvalidOperationException`: If an option with the same `optionName` already exists.

---

### `CommandContext Parse(string[] args)`

Parses the provided command-line arguments into a structured context containing the command, options, and arguments.

- **Parameters**
  - `args`: The raw command-line arguments (typically `Environment.GetCommandLineArgs()`).
- **Returns**
  - A `CommandContext` instance populated with the parsed command, options, and arguments.
- **Throws**
  - `CommandParserException`: If parsing fails due to missing required options, invalid syntax, or unknown commands/options.
  - `ArgumentNullException`: If `args` is `null`.

---

### `string GetHelpText()`

Generates a formatted help text summarizing all registered commands and global options.

- **Returns**
  - A multi-line string containing command descriptions, option details, and usage examples.
- **Throws**
  - No exceptions.

---

### `public required string Name` (in `CommandDefinition`)

The unique identifier for the command (case-sensitive).

- **Constraints**
  - Must not be `null` or empty.
  - Must be unique among all registered commands.

---

### `public required string Description` (in `CommandDefinition`)

A human-readable explanation of the command’s purpose.

- **Constraints**
  - Must not be `null` or empty.

---
### `public required Action<CommandContext> Handler` (in `CommandDefinition`)

The delegate invoked when the command is executed.

- **Constraints**
  - Must not be `null`.

---
### `public required Dictionary<string, OptionDefinition> Options` (in `CommandDefinition`)

A dictionary of options specific to this command, where keys are option names and values are their definitions.

- **Constraints**
  - Must not be `null`.
  - Must not contain duplicate keys.

---
### `public required string Name` (in `OptionDefinition`)

The unique identifier for the option (case-sensitive, must not start with `--`).

- **Constraints**
  - Must not be `null` or empty.
  - Must be unique among all options (global and command-specific).

---
### `public required string Description` (in `OptionDefinition`)

A human-readable explanation of the option’s purpose.

- **Constraints**
  - Must not be `null` or empty.

---
### `public required bool IsRequired` (in `OptionDefinition`)

Indicates whether the option must be present for parsing to succeed.

- **Default**
  - `false`.

---
### `public string CommandName` (in `CommandContext`)

The name of the command being executed, or `null` if no command was specified.

---
### `public Action<CommandContext>? Handler` (in `CommandContext`)

The delegate associated with the parsed command, or `null` if no command was found.

---
### `public Dictionary<string, string> Arguments` (in `CommandContext`)

A dictionary of positional arguments, where keys are argument names (e.g., `input`, `output`) and values are their provided values.

- **Note**
  - Arguments are parsed in the order they appear on the command line, unless explicitly named.

---
### `public bool ShowHelp` (in `CommandContext`)

Indicates whether the user requested help (e.g., via `--help` or `-h`).

- **Default**
  - `false`.

---
### `public string GetArgument(string name)`

Retrieves the value of a positional argument by name.

- **Parameters**
  - `name`: The name of the argument to retrieve.
- **Returns**
  - The value of the argument, or `null` if the argument was not provided.
- **Throws**
  - `ArgumentNullException`: If `name` is `null`.

---
### `public bool HasArgument(string name)`

Checks whether a positional argument with the given name was provided.

- **Parameters**
  - `name`: The name of the argument to check.
- **Returns**
  - `true` if the argument exists; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `name` is `null`.

## Usage

### Example 1: Basic Command with Options
