# Command-Line Interface

DotnetMicroOrm ships a small, self-contained command-line layer in the
`DotnetMicroOrm.Cli` namespace. It is built around two classes:

- `CommandParser` — parses and validates command-line arguments (subcommands,
  `--options`, positional arguments) and generates help text.
- `CommandHandler` — executes a parsed command with exception handling and
  output formatting, and coordinates with the application's services.

The CLI is intentionally lightweight: it has no external dependencies and is
driven entirely by the `CommandContext` produced by the parser.

## `CommandParser`

`CommandParser` is a sealed class that registers commands and their options,
then turns raw `string[]` arguments into a `CommandContext`.

### Registering commands

```csharp
var parser = new CommandParser();

parser.RegisterCommand("user-list", "List all users", ctx => HandleUserList(ctx));
parser.AddOption("user-list", "active-only", "Show only active users", false);

parser.RegisterCommand("user-create", "Create a new user", ctx => HandleUserCreate(ctx));
parser.AddOption("user-create", "username", "Username", true);
parser.AddOption("user-create", "email", "Email address", true);
```

- `RegisterCommand(name, description, handler)` — registers a command with a
  short description and the delegate invoked when it is parsed. Throws
  `OrmException` if the name is empty, and `ArgumentNullException` if any
  argument is `null`.
- `AddOption(commandName, optionName, description, isRequired = false)` — adds
  an option to a previously registered command. The option name is given
  without leading dashes. Throws `OrmException` if the command does not exist.

### Parsing arguments

```csharp
var context = parser.Parse(args);
```

`Parse` follows these rules:

- No arguments, or a first token of `--help`, `-h`, or `help`, produces a
  `CommandContext` with `ShowHelp = true` and no handler.
- The first token is the command name (matched case-insensitively). An unknown
  command throws `OrmException`.
- Tokens starting with `--` are options. If the next token does not start with
  `--`, it is consumed as the option's value; otherwise the option is treated
  as a flag.
- Tokens starting with a single `-` are flags and are stored with the value
  `"true"`.
- Any other token is a positional argument, stored under the reserved key
  `_args` (only the first positional argument is kept).
- After parsing, every option marked `isRequired` must be present, otherwise an
  `OrmException` is thrown.

### Help text

```csharp
string help = parser.GetHelpText();
```

`GetHelpText()` renders a formatted listing of all registered commands and
their options, marking each option as `(required)` or `(optional)`.

## `CommandContext`

`CommandContext` is the object passed to command handlers. It carries the parsed
command and arguments:

| Member | Description |
|--------|-------------|
| `CommandName` | The name of the parsed command. |
| `Handler` | The delegate registered for the command, or `null`. |
| `Arguments` | Parsed options and arguments keyed by name. |
| `ShowHelp` | `true` when help should be shown instead of executing. |
| `GetArgument(name)` | Returns the named argument's value, or `string.Empty` if absent. |
| `HasArgument(name)` | Returns `true` if the named argument is present. |

## `CommandHandler`

`CommandHandler` executes a `CommandContext` and coordinates with the
application's services. It is constructed with an `IDatabaseContext` and an
`IServiceProvider`:

```csharp
var handler = new CommandHandler(dbContext, serviceProvider);
```

### Executing a command

```csharp
int exitCode = await handler.ExecuteAsync(context);
```

`ExecuteAsync` runs the context's handler inside a `try/catch`:

- If `ShowHelp` is `true`, it returns `0` without executing (the caller is
  expected to print help).
- If no handler is defined, it throws `InvalidOperationException`.
- On success it returns `0`; on failure it writes `Error:` (and `Details:` for
  the inner exception) to standard error and returns `1`.

### Resolving services

```csharp
var userService = handler.GetService<IUserService>();
```

`GetService<T>()` resolves a service from the injected `IServiceProvider`,
throwing `InvalidOperationException` if the service is not registered.

### Standard parser

`CreateStandardParser()` returns a `CommandParser` pre-populated with the
built-in data commands described below:

```csharp
var parser = handler.CreateStandardParser();
var context = parser.Parse(args);
return await handler.ExecuteAsync(context);
```

The `CommandHandlerExtensions.ExecuteWithStandardParserAsync(handler, args)`
helper wraps exactly this flow and returns the exit code.

## Available commands

`CreateStandardParser()` registers the following commands:

### User commands

| Command | Description | Options |
|---------|-------------|---------|
| `user-list` | List all users | `--active-only` (optional) |
| `user-create` | Create a new user | `--username` (required), `--email` (required) |
| `user-get` | Get user details | `--id` (required) |

### Product commands

| Command | Description | Options |
|---------|-------------|---------|
| `product-list` | List all products | `--active-only` (optional) |
| `product-create` | Create a new product | `--name` (required), `--price` (required) |
| `product-get` | Get product details | `--id` (required) |

### Order commands

| Command | Description | Options |
|---------|-------------|---------|
| `order-list` | List all orders | — |
| `order-get` | Get order details | `--id` (required) |

### Audit commands

| Command | Description | Options |
|---------|-------------|---------|
| `audit-summary` | Show audit summary | — |

## Usage

The standard flow wires the parser to the handler and returns the process exit
code:

```csharp
var handler = new CommandHandler(dbContext, serviceProvider);
return await handler.ExecuteWithStandardParserAsync(args);
```

Invoking the CLI from a shell:

```bash
# Show help
dotnet run -- help

# List users
dotnet run -- user-list

# List only active users
dotnet run -- user-list --active-only

# Create a user (required options)
dotnet run -- user-create --username alice --email alice@example.com

# Fetch a product by id
dotnet run -- product-get --id 42
```

## JSON extensions

`CommandHandlerJsonExtensions` adds `System.Text.Json` helpers for
`CommandHandler`:

- `ToJson(this CommandHandler value, bool indented = false)` — serializes the
  handler to JSON (camelCase, nulls omitted, cycles ignored).
- `FromJson(string json)` — deserializes a JSON string to a `CommandHandler`,
  or `null` for empty/`"null"` input.
- `TryFromJson(string json, out CommandHandler? value)` — non-throwing variant
  that returns `false` on a `JsonException`.

## Notes

- **No external dependencies** — the CLI uses only the BCL and the project's
  own `OrmException`.
- **Errors** — parse and validation failures surface as `OrmException`; runtime
  handler failures are caught by `ExecuteAsync` and reported on standard error
  with a non-zero exit code.
- **Case-insensitive commands** — command names are matched case-insensitively;
  option names are matched exactly as registered.