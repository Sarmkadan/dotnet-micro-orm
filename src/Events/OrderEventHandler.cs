#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Handles order-related domain events.
/// Responds to order creation, shipping, and cancellation with appropriate actions.
/// Coordinates with inventory, notifications, and billing systems.
/// </summary>
public sealed class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public int Priority => 10;

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            Console.WriteLine($"Order created event handled: Order {@event.OrderId} for User {@event.UserId}");

            // In a real application, this would:
            // - Reserve inventory items
            // - Create payment intent
            // - Generate invoice
            // - Send order confirmation email
            // - Update inventory status
            // - Initialize shipping tracking
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling order created event: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Handles order shipped events with notification
/// </summary>
public sealed class OrderShippedEventHandler : IEventHandler<OrderShippedEvent>
{
    public int Priority => 15;

    public async Task HandleAsync(OrderShippedEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            Console.WriteLine($"Order shipped event handled: Order {@event.OrderId}, Tracking: {@event.TrackingNumber}");

            // In a real application:
            // - Send shipment notification email
            // - Update order status
            // - Update tracking information
            // - Notify warehouse management system
            // - Trigger customer notifications
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling order shipped event: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Handles order cancellation events
/// </summary>
public sealed class OrderCancelledEventHandler : IEventHandler<OrderCancelledEvent>
{
    public int Priority => 20;

    public async Task HandleAsync(OrderCancelledEvent @event)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            Console.WriteLine($"Order cancelled event handled: Order {@event.OrderId}, Reason: {@event.Reason}");

            // In a real application:
            // - Release reserved inventory
            // - Process refund
            // - Update billing records
            // - Send cancellation confirmation
            // - Cancel logistics/shipment
            // - Update analytics
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling order cancelled event: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
