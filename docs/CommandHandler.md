# CommandHandler
The `CommandHandler` class is designed to handle and execute commands in a .NET micro-ORM context. It provides a way to parse and execute commands, as well as retrieve services required for command execution. This class is a crucial component in the dotnet-micro-orm project, enabling the execution of database commands and queries.

## API
The `CommandHandler` class has the following public members:
* `public CommandHandler`: The constructor for the `CommandHandler` class. It initializes a new instance of the class.
* `public async Task<int> ExecuteAsync`: Executes a command asynchronously and returns the number of affected rows. The parameters for this method are not specified in the provided information, but it is expected to throw exceptions if the execution fails.
* `public T GetService<T>`: Retrieves a service of type `T`. The service is expected to be registered and available for retrieval. If the service is not found, it may throw an exception.
* `public CommandParser CreateStandardParser`: Creates a standard parser for commands. The parser is used to parse commands into a format that can be executed by the `CommandHandler`.

## Usage
Here are two examples of using the `CommandHandler` class:
```csharp
// Example 1: Execute a command
var commandHandler = new CommandHandler();
var rowsAffected = await commandHandler.ExecuteAsync("SELECT * FROM table");
Console.WriteLine($"Rows affected: {rowsAffected}");

// Example 2: Retrieve a service
var loggerService = commandHandler.GetService<ILogger>();
loggerService.Log("Command executed successfully");
```

## Notes
When using the `CommandHandler` class, consider the following edge cases and thread-safety remarks:
* The `ExecuteAsync` method is asynchronous, which means it does not block the calling thread. However, it may still throw exceptions if the execution fails, so proper error handling is necessary.
* The `GetService<T>` method retrieves a service of type `T`. If the service is not registered, it may throw an exception. It is essential to ensure that the required services are registered before attempting to retrieve them.
* The `CreateStandardParser` method creates a standard parser for commands. The parser is expected to be thread-safe, but the `CommandHandler` instance itself may not be thread-safe. Therefore, it is recommended to create a new instance of the `CommandHandler` class for each thread or use proper synchronization mechanisms to ensure thread safety.
