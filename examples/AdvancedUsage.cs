// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Constants;
using DotnetMicroOrm.Exceptions;

namespace DotnetMicroOrm.Examples;

/// <summary>
/// Advanced usage example showing transaction management and error handling.
/// </summary>
public class AdvancedUsage
{
    public async Task RunExampleAsync(string connectionString)
    {
        var context = new DatabaseContext(connectionString, DatabaseProvider.SqlServer);

        try
        {
            // 1. Start a transaction
            await context.BeginTransactionAsync(TransactionIsolationLevel.ReadCommitted);

            // 2. Perform operations
            var updateQuery = "UPDATE Users SET LastLogin = GETDATE() WHERE Id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", 1 } };
            
            await context.ExecuteNonQueryAsync(updateQuery, parameters);

            // 3. Commit
            await context.CommitAsync();
            Console.WriteLine("Transaction committed successfully.");
        }
        catch (QueryExecutionException qex)
        {
            // 4. Handle ORM-specific exceptions
            Console.WriteLine($"Database operation failed: {qex.Message}, Query: {qex.Query}");
            await context.RollbackAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            await context.RollbackAsync();
        }
        finally
        {
            await context.DisposeAsync();
        }
    }
}
