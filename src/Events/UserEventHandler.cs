#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetMicroOrm.Services;

namespace DotnetMicroOrm.Events;

/// <summary>
/// Event handlers for user-related events.
/// Responds to user creation, updates, and deletion with appropriate actions
/// like logging, cache invalidation, and notification sending.
/// </summary>
public class sealed UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IAuditService _auditService;

    public int Priority => 10; // Execute early

    public UserCreatedEventHandler(IAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task HandleAsync(UserCreatedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            // Log user creation for audit purposes
            await _auditService.LogInsertAsync(
                "User",
                @event.UserId,
                $"{{\"username\":\"{@event.Username}\",\"email\":\"{@event.Email}\"}}",
                @event.UserId,
                @event.InitiatedBy);

            Console.WriteLine($"User created event handled: {@event.Username} ({@event.Email})");

            // In a real application, this would:
            // - Send welcome email
            // - Create default preferences
            // - Initialize user features
            // - Update analytics
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling user created event: {ex.Message}");
        }
    }
}

/// <summary>
/// Handles user update events with logging and cache invalidation
/// </summary>
public class sealed UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
{
    private readonly IAuditService _auditService;

    public int Priority => 20;

    public UserUpdatedEventHandler(IAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task HandleAsync(UserUpdatedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            // Log the update
            await _auditService.LogUpdateAsync(
                "User",
                @event.UserId,
                string.Empty,
                string.Empty,
                @event.ChangedFields,
                @event.UserId,
                @event.InitiatedBy);

            Console.WriteLine($"User updated event handled: {@event.UserId}");

            // In a real application:
            // - Invalidate user cache
            // - Notify connected clients
            // - Update search indexes
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling user updated event: {ex.Message}");
        }
    }
}

/// <summary>
/// Handles user deletion events with cleanup
/// </summary>
public class sealed UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    public int Priority => 50; // Lower priority - execute last

    public async Task HandleAsync(UserDeletedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            Console.WriteLine($"User deleted event handled: {@event.UserId}");

            // In a real application:
            // - Archive user data
            // - Anonymize personal information
            // - Cancel active orders
            // - Remove from mailing lists
            // - Trigger GDPR data deletion procedures
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling user deleted event: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
