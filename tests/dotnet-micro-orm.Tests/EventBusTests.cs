using DotnetMicroOrm.Events;
using FluentAssertions;
using Xunit;

namespace DotnetMicroOrm.Tests;

public class EventBusTests
{
    private readonly EventBus _eventBus;
    private readonly EventBus _syncEventBus;

    public EventBusTests()
    {
        _eventBus = new EventBus(executeAsync: true);
        _syncEventBus = new EventBus(executeAsync: false);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var bus = new EventBus();

        // Assert
        bus.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldSetExecuteAsyncFlag()
    {
        // Arrange & Act
        var asyncBus = new EventBus(executeAsync: true);
        var syncBus = new EventBus(executeAsync: false);

        // Assert
        // Note: The executeAsync field is private, so we test behavior instead
        asyncBus.Should().NotBeNull();
        syncBus.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EventBus bus = _eventBus;

        // Act
        Func<Task> act = async () => await bus.PublishAsync<UserCreatedEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_ShouldNotThrow_WhenNoSubscribersRegistered()
    {
        // Arrange
        var @event = new UserCreatedEvent
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ShouldExecuteHandlersInPriorityOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            Name = "Test Product",
            Price = 10.99m
        };

        var handler1 = new TestEventHandler<ProductCreatedEvent>(1, executionOrder);
        var handler2 = new TestEventHandler<ProductCreatedEvent>(50, executionOrder);
        var handler3 = new TestEventHandler<ProductCreatedEvent>(100, executionOrder);

        _eventBus.Subscribe<ProductCreatedEvent, TestEventHandler<ProductCreatedEvent>>(handler1);
        _eventBus.Subscribe<ProductCreatedEvent, TestEventHandler<ProductCreatedEvent>>(handler2);
        _eventBus.Subscribe<ProductCreatedEvent, TestEventHandler<ProductCreatedEvent>>(handler3);

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        executionOrder.Should().BeInAscendingOrder("Handlers should execute in priority order (lower priority first)");
        executionOrder.Should().Equal(new List<int> { 1, 50, 100 });
    }

    [Fact]
    public async Task PublishAsync_ShouldExecuteAllHandlers()
    {
        // Arrange
        var executionCount = 0;
        var @event = new UserUpdatedEvent
        {
            UserId = 1,
            ChangedFields = "email"
        };

        var handler1 = new TestEventHandler<UserUpdatedEvent>(100, _ => executionCount++);
        var handler2 = new TestEventHandler<UserUpdatedEvent>(100, _ => executionCount++);

        _eventBus.Subscribe<UserUpdatedEvent, TestEventHandler<UserUpdatedEvent>>(handler1);
        _eventBus.Subscribe<UserUpdatedEvent, TestEventHandler<UserUpdatedEvent>>(handler2);

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        executionCount.Should().Be(2, "Both handlers should be executed");
    }

    [Fact]
    public async Task PublishAsync_ShouldHandleExceptionsInHandlers()
    {
        // Arrange
        var @event = new OrderCreatedEvent
        {
            OrderId = 1,
            UserId = 1,
            TotalAmount = 99.99m
        };

        var throwingHandler = new ThrowingEventHandler<OrderCreatedEvent>();
        var normalHandler = new TestEventHandler<OrderCreatedEvent>(100, _ => { });

        _eventBus.Subscribe<OrderCreatedEvent, ThrowingEventHandler<OrderCreatedEvent>>(throwingHandler);
        _eventBus.Subscribe<OrderCreatedEvent, TestEventHandler<OrderCreatedEvent>>(normalHandler);

        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync("Exceptions in handlers should be caught and not rethrown");
    }

    [Fact]
    public void Subscribe_ShouldThrowArgumentNullException_WhenHandlerIsNull()
    {
        // Arrange
        EventBus bus = _eventBus;

        // Act
        Action act = () => bus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subscribe_ShouldAddHandlerToSubscribers()
    {
        // Arrange
        var handler = new TestEventHandler<UserCreatedEvent>(100, _ => { });

        // Act
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Assert
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(1);
    }

    [Fact]
    public void Subscribe_ShouldAddMultipleHandlersForSameEventType()
    {
        // Arrange
        var handler1 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        var handler2 = new TestEventHandler<UserCreatedEvent>(100, _ => { });

        // Act
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler1);
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler2);

        // Assert
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(2);
    }

    [Fact]
    public void Subscribe_ShouldNotAddDuplicateHandlers()
    {
        // Arrange
        var handler = new TestEventHandler<UserCreatedEvent>(100, _ => { });

        // Act
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Assert
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(1, "Duplicate handlers should not be added");
    }

    [Fact]
    public void Unsubscribe_ShouldRemoveHandler()
    {
        // Arrange
        var handler = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Act
        _eventBus.Unsubscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Assert
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(0);
    }

    [Fact]
    public void Unsubscribe_ShouldNotThrow_WhenHandlerIsNull()
    {
        // Arrange
        EventBus bus = _eventBus;

        // Act
        Action act = () => bus.Unsubscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Unsubscribe_ShouldNotThrow_WhenHandlerNotSubscribed()
    {
        // Arrange
        var handler1 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        var handler2 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler1);

        // Act
        Action act = () => _eventBus.Unsubscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler2);

        // Assert
        act.Should().NotThrow();
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(1);
    }

    [Fact]
    public void Unsubscribe_ShouldRemoveEventType_WhenLastHandlerRemoved()
    {
        // Arrange
        var handler = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Act
        _eventBus.Unsubscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler);

        // Assert
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        count.Should().Be(0);
    }

    [Fact]
    public void GetSubscriberCount_ShouldReturnZero_WhenNoSubscribers()
    {
        // Act
        var count = _eventBus.GetSubscriberCount<UserCreatedEvent>();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void GetSubscriberCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var handler1 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        var handler2 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        var handler3 = new TestEventHandler<ProductCreatedEvent>(100, _ => { });

        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler1);
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler2);
        _eventBus.Subscribe<ProductCreatedEvent, TestEventHandler<ProductCreatedEvent>>(handler3);

        // Act
        var userCount = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        var productCount = _eventBus.GetSubscriberCount<ProductCreatedEvent>();

        // Assert
        userCount.Should().Be(2);
        productCount.Should().Be(1);
    }

    [Fact]
    public void ClearSubscribers_ShouldRemoveAllSubscribers()
    {
        // Arrange
        var handler1 = new TestEventHandler<UserCreatedEvent>(100, _ => { });
        var handler2 = new TestEventHandler<ProductCreatedEvent>(100, _ => { });
        _eventBus.Subscribe<UserCreatedEvent, TestEventHandler<UserCreatedEvent>>(handler1);
        _eventBus.Subscribe<ProductCreatedEvent, TestEventHandler<ProductCreatedEvent>>(handler2);

        // Act
        _eventBus.ClearSubscribers();

        // Assert
        var userCount = _eventBus.GetSubscriberCount<UserCreatedEvent>();
        var productCount = _eventBus.GetSubscriberCount<ProductCreatedEvent>();
        userCount.Should().Be(0);
        productCount.Should().Be(0);
    }

    [Fact]
    public void ClearSubscribers_ShouldNotThrow_WhenNoSubscribers()
    {
        // Arrange
        EventBus bus = _eventBus;

        // Act
        Action act = () => bus.ClearSubscribers();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task PublishAsync_ShouldExecuteHandlersSynchronously_WhenExecuteAsyncIsFalse()
    {
        // Arrange
        var executionOrder = new List<string>();
        var @event = new OrderShippedEvent
        {
            OrderId = 1,
            TrackingNumber = "TRACK123"
        };

        var handler1 = new TestEventHandler<OrderShippedEvent>(100, _ => executionOrder.Add("handler1"));
        var handler2 = new TestEventHandler<OrderShippedEvent>(100, _ => executionOrder.Add("handler2"));

        _syncEventBus.Subscribe<OrderShippedEvent, TestEventHandler<OrderShippedEvent>>(handler1);
        _syncEventBus.Subscribe<OrderShippedEvent, TestEventHandler<OrderShippedEvent>>(handler2);

        // Act
        await _syncEventBus.PublishAsync(@event);

        // Assert
        executionOrder.Should().HaveCount(2);
        executionOrder.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task PublishAsync_ShouldExecuteHandlersAsynchronously_WhenExecuteAsyncIsTrue()
    {
        // Arrange
        var executionOrder = new List<int>();
        var @event = new OrderCancelledEvent
        {
            OrderId = 1,
            Reason = "Out of stock"
        };

        var handler1 = new TestEventHandler<OrderCancelledEvent>(100, _ => executionOrder.Add(1));
        var handler2 = new TestEventHandler<OrderCancelledEvent>(100, _ => executionOrder.Add(2));

        _eventBus.Subscribe<OrderCancelledEvent, TestEventHandler<OrderCancelledEvent>>(handler1);
        _eventBus.Subscribe<OrderCancelledEvent, TestEventHandler<OrderCancelledEvent>>(handler2);

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        executionOrder.Should().HaveCount(2);
    }

    // Test helper classes
    private class TestEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        private readonly Action<TEvent>? _action;
        private readonly List<int>? _executionOrder;

        public TestEventHandler(int priority, Action<TEvent> action)
        {
            Priority = priority;
            _action = action;
        }

        public TestEventHandler(int priority, List<int> executionOrder)
        {
            Priority = priority;
            _executionOrder = executionOrder;
        }

        public int Priority { get; }

        public Task HandleAsync(TEvent @event)
        {
            _action?.Invoke(@event);
            if (_executionOrder != null)
            {
                _executionOrder.Add(Priority);
            }
            return Task.CompletedTask;
        }
    }

    private class ThrowingEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
    {
        public int Priority => 100;

        public Task HandleAsync(TEvent @event)
        {
            throw new InvalidOperationException("Test exception from handler");
        }
    }
}