// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Configuration;
using DotnetMicroOrm.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMicroOrm.Examples;

/// <summary>
/// Shows how to integrate the library into an ASP.NET Core application
/// using the provided IServiceCollection extension methods.
/// </summary>
public class IntegrationExample
{
    public void ConfigureServices(IServiceCollection services, string connectionString)
    {
        // Add the ORM to the DI container
        services.AddDotnetMicroOrm(
            connectionString,
            DatabaseProvider.SqlServer,
            options =>
            {
                options.CommandTimeout = 60;
                options.EnableAuditLogging = true;
            });
    }
}
