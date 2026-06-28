// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Constants;

namespace DotnetMicroOrm.Examples;

/// <summary>
/// Basic usage example showing how to initialize the database context
/// and execute a simple query.
/// </summary>
public class BasicUsage
{
    public async Task RunExampleAsync(string connectionString)
    {
        // 1. Initialize the DatabaseContext
        // We specify the provider (e.g., SqlServer)
        var context = new DatabaseContext(connectionString, DatabaseProvider.SqlServer);

        // 2. Open connection
        await context.OpenAsync();

        // 3. Execute a simple query
        var query = "SELECT TOP 10 * FROM Users";
        var users = await context.ExecuteQueryAsync(query);

        Console.WriteLine($"Found {users.Count} users.");

        foreach (var user in users)
        {
            Console.WriteLine($"User: {user["Username"]}, Email: {user["Email"]}");
        }

        // 4. Dispose
        await context.DisposeAsync();
    }
}
