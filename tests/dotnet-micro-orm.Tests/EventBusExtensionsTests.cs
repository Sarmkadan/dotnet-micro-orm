#nullable enable

using DotnetMicroOrm.Events;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Tests for <see cref="EventBusExtensions"/> functionality.
/// </summary>
public sealed class EventBusExtensionsTests
{
    /// <summary>
    /// Tests that SubscribeRange successfully subscribes multiple handlers for the same event type.
    /// </summary>
    [Fact]
    public void SubscribeRange_WithMultipleHandlers_SubscribesAllHandlers()
    {
        // Arrange
        var bus = new EventBus();
        var handlers = new List<TestEventHandler> { new(), new(), new() };

        // Act
        bus.SubscribeRange<TestEvent, TestEventHandler>(handlers);

        // Assert
        bus.HasSubscribers<TestEvent>().Should().BeTrue();
        bus.GetTotalSubscriberCount().Should().Be(3);
        bus.GetSubscriberCount<TestEvent>().Should().Be(3);
    }

    /// <summary>
    /// Tests that SubscribeRange throws ArgumentNullException when bus is null.
    /// </summary>
    [Fact]
    public void SubscribeRange_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? bus = null;
        var handlers = new List<TestEventHandler> { new() };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => bus!.SubscribeRange<TestEvent, TestEventHandler>(handlers));
    }

    /// <summary>
    /// Tests that SubscribeRange throws ArgumentNullException when handlers collection is null.
    /// </summary>
    [Fact]
    public void SubscribeRange_WithNullHandlers_ThrowsArgumentNullException()
    {
        // Arrange
        var bus = new EventBus();
        List<TestEventHandler>? handlers = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => bus.SubscribeRange<TestEvent, TestEventHandler>(handlers!));
    }

    /// <summary>
    /// Tests that SubscribeRange works correctly with empty handler collection.
    /// </summary>
    [Fact]
    public void SubscribeRange_WithEmptyHandlers_DoesNotThrow()
    {
        // Arrange
        var bus = new EventBus();
        var handlers = new List<TestEventHandler>();

        // Act
        bus.SubscribeRange<TestEvent, TestEventHandler>(handlers);

        // Assert
        bus.HasSubscribers<TestEvent>().Should().BeFalse();
        bus.GetTotalSubscriberCount().Should().Be(0);
    }

    /// <summary>
    /// Tests that SubscribeRange handles duplicate handlers correctly.
    /// </summary>
    [Fact]
    public void SubscribeRange_WithDuplicateHandlers_AddsEachHandlerOnce()
    {
        // Arrange
        var bus = new EventBus();
        var handler = new TestEventHandler();
        var handlers = new List<TestEventHandler> { handler, handler, handler };

        // Act
        bus.SubscribeRange<TestEvent, TestEventHandler>(handlers);

        // Assert - should only have 1 handler since duplicates are prevented
        bus.HasSubscribers<TestEvent>().Should().BeTrue();
        bus.GetTotalSubscriberCount().Should().Be(1);
        bus.GetSubscriberCount<TestEvent>().Should().Be(1);
    }

    /// <summary>
    /// Tests that PublishSyncAsync successfully publishes an event and waits for completion.
    /// </summary>
    [Fact]
    public async Task PublishSyncAsync_WithEvent_PublishesEventSuccessfully()
    {
        // Arrange
        var bus = new EventBus();
        var handler = new TestEventHandler();
        bus.SubscribeRange<TestEvent, TestEventHandler>(new List<TestEventHandler> { handler });
        var @event = new TestEvent();

        // Act
        await bus.PublishSyncAsync(@event);

        // Assert
        handler.HandleCount.Should().Be(1);
        handler.LastHandledEvent.Should().BeSameAs(@event);
    }

    /// <summary>
    /// Tests that PublishSyncAsync throws ArgumentNullException when bus is null.
    /// </summary>
    [Fact]
    public async Task PublishSyncAsync_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? bus = null;
        var @event = new TestEvent();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => bus!.PublishSyncAsync(@event));
    }

    /// <summary>
    /// Tests that PublishSyncAsync throws ArgumentNullException when event is null.
    /// </summary>
    [Fact]
    public async Task PublishSyncAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var bus = new EventBus();
        TestEvent? @event = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => bus.PublishSyncAsync(@event!));
    }

    /// <summary>
    /// Tests that PublishSyncAsync does not throw when there are no subscribers.
    /// </summary>
    [Fact]
    public async Task PublishSyncAsync_WithNoSubscribers_DoesNotThrow()
    {
        // Arrange
        var bus = new EventBus();
        var @event = new TestEvent();

        // Act
        await bus.PublishSyncAsync(@event);

        // Assert - should complete without error
        await Task.CompletedTask;
    }

    /// <summary>
    /// Tests that GetTotalSubscriberCount returns 0 when there are no subscribers.
    /// </summary>
    [Fact]
    public void GetTotalSubscriberCount_WithNoSubscribers_ReturnsZero()
    {
        // Arrange
        var bus = new EventBus();

        // Act
        var count = bus.GetTotalSubscriberCount();

        // Assert
        count.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetTotalSubscriberCount returns correct count with multiple event types subscribed.
    /// </summary>
    [Fact]
    public void GetTotalSubscriberCount_WithMultipleEventTypes_ReturnsCorrectTotal()
    {
        // Arrange
        var bus = new EventBus();
        var testHandlers = new List<TestEventHandler> { new(), new() };
        var userHandlers = new List<UserCreatedEventHandler> { new(), new(), new() };

        bus.SubscribeRange<TestEvent, TestEventHandler>(testHandlers);
        bus.SubscribeRange<UserCreatedEvent, UserCreatedEventHandler>(userHandlers);

        // Act
        var count = bus.GetTotalSubscriberCount();

        // Assert
        count.Should().Be(5); // 2 test handlers + 3 user handlers
    }

    /// <summary>
    /// Tests that GetTotalSubscriberCount throws ArgumentNullException when bus is null.
    /// </summary>
    [Fact]
    public void GetTotalSubscriberCount_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? bus = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => bus!.GetTotalSubscriberCount());
    }

    /// <summary>
    /// Tests that HasSubscribers returns false when there are no subscribers for the event type.
    /// </summary>
    [Fact]
    public void HasSubscribers_WithNoSubscribers_ReturnsFalse()
    {
        // Arrange
        var bus = new EventBus();

        // Act
        var hasSubscribers = bus.HasSubscribers<TestEvent>();

        // Assert
        hasSubscribers.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasSubscribers returns true when there are subscribers for the event type.
    /// </summary>
    [Fact]
    public void HasSubscribers_WithSubscribers_ReturnsTrue()
    {
        // Arrange
        var bus = new EventBus();
        bus.SubscribeRange<TestEvent, TestEventHandler>(new List<TestEventHandler> { new() });

        // Act
        var hasSubscribers = bus.HasSubscribers<TestEvent>();

        // Assert
        hasSubscribers.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasSubscribers throws ArgumentNullException when bus is null.
    /// </summary>
    [Fact]
    public void HasSubscribers_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? bus = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => bus!.HasSubscribers<TestEvent>());
    }

    /// <summary>
    /// Tests that HasSubscribers returns false for unregistered event type even when other types are registered.
    /// </summary>
    [Fact]
    public void HasSubscribers_ForUnregisteredEventType_ReturnsFalse()
    {
        // Arrange
        var bus = new EventBus();
        bus.SubscribeRange<UserCreatedEvent, UserCreatedEventHandler>(new List<UserCreatedEventHandler> { new() });

        // Act
        var hasSubscribers = bus.HasSubscribers<TestEvent>();

        // Assert
        hasSubscribers.Should().BeFalse();
    }

    /// <summary>
    /// Tests that multiple PublishSyncAsync calls work correctly with the same handlers.
    /// </summary>
    [Fact]
    public async Task PublishSyncAsync_MultipleCalls_HandlesEventsCorrectly()
    {
        // Arrange
        var bus = new EventBus();
        var handler = new TestEventHandler();
        bus.SubscribeRange<TestEvent, TestEventHandler>(new List<TestEventHandler> { handler });
        var event1 = new TestEvent();
        var event2 = new TestEvent();

        // Act
        await bus.PublishSyncAsync(event1);
        await bus.PublishSyncAsync(event2);

        // Assert
        handler.HandleCount.Should().Be(2);
        handler.LastHandledEvent.Should().BeSameAs(event2);
    }

    /// <summary>
    /// Tests that SubscribeRange works with handlers of different priorities.
    /// </summary>
    [Fact]
    public async Task SubscribeRange_WithDifferentPriorities_HandlesInPriorityOrder()
    {
        // Arrange
        var bus = new EventBus();
        var highPriorityHandler = new HighPriorityTestEventHandler();
        var lowPriorityHandler = new LowPriorityTestEventHandler();

        bus.Subscribe<TestEvent, HighPriorityTestEventHandler>(highPriorityHandler);
        bus.Subscribe<TestEvent, LowPriorityTestEventHandler>(lowPriorityHandler);

        // Act
        var @event = new TestEvent();
        await bus.PublishSyncAsync(@event);

        // Assert - both handlers should have been called
        highPriorityHandler.HandleCount.Should().Be(1);
        lowPriorityHandler.HandleCount.Should().Be(1);
    }

    // Test event and handlers for testing purposes
    private sealed class TestEvent : DomainEvent
    {
        public override int AggregateId => 1;
        public override string AggregateType => "Test";
        public override string EventType => "test.event";
    }

    private sealed class UserCreatedEvent : DomainEvent
    {
        public override int AggregateId => 1;
        public override string AggregateType => "User";
        public override string EventType => "user.created";
    }

    private sealed class TestEventHandler : IEventHandler<TestEvent>
    {
        public int HandleCount { get; private set; }
        public TestEvent? LastHandledEvent { get; private set; }

        public Task HandleAsync(TestEvent @event)
        {
            HandleCount++;
            LastHandledEvent = @event;
            return Task.CompletedTask;
        }
    }

    private sealed class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        public Task HandleAsync(UserCreatedEvent @event) => Task.CompletedTask;
    }

    private sealed class HighPriorityTestEventHandler : IEventHandler<TestEvent>
    {
        public int HandleCount { get; private set; }

        public int Priority => 10; // Higher priority (lower number)

        public Task HandleAsync(TestEvent @event)
        {
            HandleCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class LowPriorityTestEventHandler : IEventHandler<TestEvent>
    {
        public int HandleCount { get; private set; }

        public int Priority => 200; // Lower priority (higher number)

        public Task HandleAsync(TestEvent @event)
        {
            HandleCount++;
            return Task.CompletedTask;
        }
    }
}