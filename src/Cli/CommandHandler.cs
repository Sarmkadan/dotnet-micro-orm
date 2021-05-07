#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Data;
using DotnetMicroOrm.Services;

namespace DotnetMicroOrm.Cli;

/// <summary>
/// Executes CLI commands with proper exception handling and output formatting.
/// Coordinates between command parser and business logic services.
/// </summary>
public sealed class CommandHandler
{
    private readonly IDatabaseContext _dbContext;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandler(IDatabaseContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Executes a command context with error handling and output formatting
    /// </summary>
    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            if (context.ShowHelp)
            {
                // Help text would be printed by caller
                return 0;
            }

            if (context.Handler is null)
                throw new InvalidOperationException("No handler defined for command");

            context.Handler(context);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException is not null)
                Console.Error.WriteLine($"Details: {ex.InnerException.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Gets a service instance from the service provider
    /// </summary>
    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    /// <summary>
    /// Registers standard CLI commands for data operations
    /// </summary>
    public CommandParser CreateStandardParser()
    {
        var parser = new CommandParser();

        // User commands
        parser.RegisterCommand("user-list", "List all users", ctx => HandleUserList(ctx));
        parser.AddOption("user-list", "active-only", "Show only active users", false);

        parser.RegisterCommand("user-create", "Create a new user", ctx => HandleUserCreate(ctx));
        parser.AddOption("user-create", "username", "Username", true);
        parser.AddOption("user-create", "email", "Email address", true);

        parser.RegisterCommand("user-get", "Get user details", ctx => HandleUserGet(ctx));
        parser.AddOption("user-get", "id", "User ID", true);

        // Product commands
        parser.RegisterCommand("product-list", "List all products", ctx => HandleProductList(ctx));
        parser.AddOption("product-list", "active-only", "Show only active products", false);

        parser.RegisterCommand("product-create", "Create a new product", ctx => HandleProductCreate(ctx));
        parser.AddOption("product-create", "name", "Product name", true);
        parser.AddOption("product-create", "price", "Product price", true);

        parser.RegisterCommand("product-get", "Get product details", ctx => HandleProductGet(ctx));
        parser.AddOption("product-get", "id", "Product ID", true);

        // Order commands
        parser.RegisterCommand("order-list", "List all orders", ctx => HandleOrderList(ctx));
        parser.RegisterCommand("order-get", "Get order details", ctx => HandleOrderGet(ctx));
        parser.AddOption("order-get", "id", "Order ID", true);

        // Audit commands
        parser.RegisterCommand("audit-summary", "Show audit summary", ctx => HandleAuditSummary(ctx));

        return parser;
    }

    private void HandleUserList(CommandContext context)
    {
        Console.WriteLine("User list command executed");
        // Implementation would interact with UserService
    }

    private void HandleUserCreate(CommandContext context)
    {
        var username = context.GetArgument("username");
        var email = context.GetArgument("email");
        Console.WriteLine($"Creating user: {username} ({email})");
    }

    private void HandleUserGet(CommandContext context)
    {
        var id = context.GetArgument("id");
        Console.WriteLine($"Fetching user: {id}");
    }

    private void HandleProductList(CommandContext context)
    {
        Console.WriteLine("Product list command executed");
    }

    private void HandleProductCreate(CommandContext context)
    {
        var name = context.GetArgument("name");
        var price = context.GetArgument("price");
        Console.WriteLine($"Creating product: {name} at ${price}");
    }

    private void HandleProductGet(CommandContext context)
    {
        var id = context.GetArgument("id");
        Console.WriteLine($"Fetching product: {id}");
    }

    private void HandleOrderList(CommandContext context)
    {
        Console.WriteLine("Order list command executed");
    }

    private void HandleOrderGet(CommandContext context)
    {
        var id = context.GetArgument("id");
        Console.WriteLine($"Fetching order: {id}");
    }

    private void HandleAuditSummary(CommandContext context)
    {
        Console.WriteLine("Audit summary command executed");
    }
}
