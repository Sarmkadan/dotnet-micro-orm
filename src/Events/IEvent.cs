#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Events;

/// <summary>
/// Base interface for domain events that represent something important that happened.
/// Events are immutable and used for notification, auditing, and eventual consistency.
/// </summary>
public interface IEvent
{
    /// <summary>Unique event identifier</summary>
    string EventId { get; }

    /// <summary>When the event occurred</summary>
    DateTime OccurredAt { get; }

    /// <summary>Who or what caused the event</summary>
    string InitiatedBy { get; }

    /// <summary>Aggregate root ID this event relates to</summary>
    int AggregateId { get; }

    /// <summary>Type of aggregate (User, Product, Order, etc)</summary>
    string AggregateType { get; }

    /// <summary>Event type identifier</summary>
    string EventType { get; }
}

/// <summary>
/// Handler interface for processing events
/// </summary>
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the event asynchronously
    /// </summary>
    Task HandleAsync(TEvent @event);

    /// <summary>
    /// Priority for handler execution (lower = higher priority)
    /// </summary>
    int Priority => 100;
}

/// <summary>
/// Base class for domain events with common fields
/// </summary>
public abstract class DomainEvent : IEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string InitiatedBy { get; set; } = "System";
    public abstract int AggregateId { get; }
    public abstract string AggregateType { get; }
    public abstract string EventType { get; }
}

/// <summary>
/// User-related domain events
/// </summary>
public sealed class UserCreatedEvent : DomainEvent
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override int AggregateId => UserId;
    public override string AggregateType => "User";
    public override string EventType => "user.created";
}

public sealed class UserUpdatedEvent : DomainEvent
{
    public int UserId { get; set; }
    public string ChangedFields { get; set; } = string.Empty;

    public override int AggregateId => UserId;
    public override string AggregateType => "User";
    public override string EventType => "user.updated";
}

public sealed class UserDeletedEvent : DomainEvent
{
    public int UserId { get; set; }

    public override int AggregateId => UserId;
    public override string AggregateType => "User";
    public override string EventType => "user.deleted";
}

/// <summary>
/// Product-related domain events
/// </summary>
public sealed class ProductCreatedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override int AggregateId => ProductId;
    public override string AggregateType => "Product";
    public override string EventType => "product.created";
}

public sealed class ProductStockChangedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public int OldStock { get; set; }
    public int NewStock { get; set; }

    public override int AggregateId => ProductId;
    public override string AggregateType => "Product";
    public override string EventType => "product.stock_changed";
}

/// <summary>
/// Order-related domain events
/// </summary>
public sealed class OrderCreatedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }

    public override int AggregateId => OrderId;
    public override string AggregateType => "Order";
    public override string EventType => "order.created";
}

public sealed class OrderShippedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;

    public override int AggregateId => OrderId;
    public override string AggregateType => "Order";
    public override string EventType => "order.shipped";
}

public sealed class OrderCancelledEvent : DomainEvent
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;

    public override int AggregateId => OrderId;
    public override string AggregateType => "Order";
    public override string EventType => "order.cancelled";
}
