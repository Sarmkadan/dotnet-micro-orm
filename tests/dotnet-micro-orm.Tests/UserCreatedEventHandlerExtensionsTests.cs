#nullable enable

using DotnetMicroOrm.Events;
using DotnetMicroOrm.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotnetMicroOrm.Tests;

/// <summary>
/// Tests for <see cref="UserCreatedEventHandlerExtensions"/> extension methods.
/// </summary>
public sealed class UserCreatedEventHandlerExtensionsTests
{
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly UserCreatedEventHandler _handler;

    public UserCreatedEventHandlerExtensionsTests()
    {
        _handler = new UserCreatedEventHandler(_auditServiceMock.Object);
    }

    /// <summary>
    /// Tests that HandleWithConsoleLoggingAsync successfully handles a valid event.
    /// </summary>
    [Fact]
    public async Task HandleWithConsoleLoggingAsync_WithValidEvent_ExecutesHandler()
    {
        // Arrange
        var @event = new UserCreatedEvent
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            InitiatedBy = "System"
        };

        // Act
        await _handler.HandleWithConsoleLoggingAsync(@event);

        // Assert
        _auditServiceMock.Verify(x => x.LogInsertAsync(
            "User",
            1,
            It.Is<string>(s => s.Contains("testuser") && s.Contains("test@example.com")),
            1,
            "System"),
            Times.Once);
    }

    /// <summary>
    /// Tests that HandleWithConsoleLoggingAsync throws ArgumentNullException for null handler.
    /// </summary>
    [Fact]
    public async Task HandleWithConsoleLoggingAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        UserCreatedEventHandler? handler = null;
        var @event = new UserCreatedEvent
        {
            UserId = 1,
            Username = "testuser"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler!.HandleWithConsoleLoggingAsync(@event));
    }

    /// <summary>
    /// Tests that HandleWithConsoleLoggingAsync throws ArgumentNullException for null event.
    /// </summary>
    [Fact]
    public async Task HandleWithConsoleLoggingAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = (UserCreatedEvent?)null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.HandleWithConsoleLoggingAsync(@event!));
    }

    /// <summary>
    /// Tests that GetDescription returns correct description with priority.
    /// </summary>
    [Fact]
    public void GetDescription_WithValidHandler_ReturnsDescription()
    {
        // Arrange
        var expectedPriority = _handler.Priority;

        // Act
        var description = _handler.GetDescription();

        // Assert
        description.Should().Be($"UserCreatedEventHandler (Priority = {expectedPriority})");
    }

    /// <summary>
    /// Tests that GetDescription throws ArgumentNullException for null handler.
    /// </summary>
    [Fact]
    public void GetDescription_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        UserCreatedEventHandler? handler = null;

        // Act & Assert
        Action act = () => handler!.GetDescription();
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that HandleManyAsync processes multiple events sequentially.
    /// </summary>
    [Fact]
    public async Task HandleManyAsync_WithMultipleEvents_ProcessesAllEvents()
    {
        // Arrange
        var events = new List<UserCreatedEvent>
        {
            new UserCreatedEvent { UserId = 1, Username = "user1", Email = "user1@example.com" },
            new UserCreatedEvent { UserId = 2, Username = "user2", Email = "user2@example.com" },
            new UserCreatedEvent { UserId = 3, Username = "user3", Email = "user3@example.com" }
        };

        // Act
        await events.HandleManyAsync(_handler);

        // Assert
        _auditServiceMock.Verify(x => x.LogInsertAsync(
            "User",
            1,
            It.Is<string>(s => s.Contains("user1")),
            1,
            It.IsAny<string>()),
            Times.Once);

        _auditServiceMock.Verify(x => x.LogInsertAsync(
            "User",
            2,
            It.Is<string>(s => s.Contains("user2")),
            2,
            It.IsAny<string>()),
            Times.Once);

        _auditServiceMock.Verify(x => x.LogInsertAsync(
            "User",
            3,
            It.Is<string>(s => s.Contains("user3")),
            3,
            It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that HandleManyAsync throws ArgumentNullException for null handler.
    /// </summary>
    [Fact]
    public async Task HandleManyAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var events = new List<UserCreatedEvent> { new UserCreatedEvent { UserId = 1 } };
        UserCreatedEventHandler? handler = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => events.HandleManyAsync(handler!));
    }

    /// <summary>
    /// Tests that HandleManyAsync throws ArgumentNullException for null events collection.
    /// </summary>
    [Fact]
    public async Task HandleManyAsync_WithNullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<UserCreatedEvent>? events = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => events!.HandleManyAsync(_handler));
    }

    /// <summary>
    /// Tests that HandleManyAsync handles empty collection without errors.
    /// </summary>
    [Fact]
    public async Task HandleManyAsync_WithEmptyCollection_DoesNotThrow()
    {
        // Arrange
        var events = Enumerable.Empty<UserCreatedEvent>();

        // Act
        await events.HandleManyAsync(_handler);

        // Assert - no exceptions thrown
        _auditServiceMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Tests that TryHandleAsync returns true when handling succeeds.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_WhenHandlingSucceeds_ReturnsTrue()
    {
        // Arrange
        var @event = new UserCreatedEvent { UserId = 1, Username = "successuser" };

        // Act
        var result = await _handler.TryHandleAsync(@event);

        // Assert
        result.Should().BeTrue();
        _auditServiceMock.Verify(x => x.LogInsertAsync(
            "User",
            1,
            It.Is<string>(s => s.Contains("successuser")),
            1,
            It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that TryHandleAsync returns true even when handler catches exceptions internally.
    /// The UserCreatedEventHandler catches all exceptions, so TryHandleAsync will always return true.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_WhenHandlerCatchesExceptionsInternally_ReturnsTrue()
    {
        // Arrange
        var @event = new UserCreatedEvent { UserId = 1, Username = "failinguser" };

        // Make the handler throw an exception (which will be caught internally by UserCreatedEventHandler)
        _auditServiceMock.Setup(x => x.LogInsertAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act
        var result = await _handler.TryHandleAsync(@event);

        // Assert
        // UserCreatedEventHandler catches all exceptions internally, so TryHandleAsync will return true
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that TryHandleAsync throws ArgumentNullException for null handler.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        UserCreatedEventHandler? handler = null;
        var @event = new UserCreatedEvent { UserId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler!.TryHandleAsync(@event));
    }

    /// <summary>
    /// Tests that TryHandleAsync throws ArgumentNullException for null event.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var @event = (UserCreatedEvent?)null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.TryHandleAsync(@event!));
    }
}
