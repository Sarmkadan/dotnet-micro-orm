using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DotnetMicroOrm.Cli;

public static class CommandHandlerExtensions
{
    /// <summary>
    /// Executes the command handler with a standard parser.
    /// </summary>
    /// <param name="commandHandler">The command handler to execute.</param>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The exit code.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandler"/> is null.</exception>
    public static async Task<int> ExecuteWithStandardParserAsync(this CommandHandler commandHandler, string[] args)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);

        var parser = commandHandler.CreateStandardParser();
        var context = parser.Parse(args);
        return await commandHandler.ExecuteAsync(context);
    }

    /// <summary>
    /// Gets a service of type <typeparamref name="T"/> from the command handler.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <param name="commandHandler">The command handler.</param>
    /// <returns>The service instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="commandHandler"/> is null.</exception>
    public static T GetService<T>(this CommandHandler commandHandler)
    {
        ArgumentNullException.ThrowIfNull(commandHandler);

        return commandHandler.GetService<T>();
    }
}
