#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetMicroOrm.Events
{
    /// <summary>
    /// Extension methods for <see cref="UserCreatedEventHandler"/>.
    /// </summary>
    public static class UserCreatedEventHandlerExtensions
    {
        /// <summary>
        /// Executes the handler while writing simple console log messages before and after the operation.
        /// </summary>
        /// <param name="handler">The <see cref="UserCreatedEventHandler"/> instance.</param>
        /// <param name="event">The <see cref="UserCreatedEvent"/> to handle.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> or <paramref name="event"/> is null.</exception>
        public static async Task HandleWithConsoleLoggingAsync(this UserCreatedEventHandler handler, UserCreatedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(@event);

            Console.WriteLine($"[UserCreated] Handling start for user '{@event.Username}' (Id: {@event.UserId})");
            await handler.HandleAsync(@event);
            Console.WriteLine($"[UserCreated] Handling completed for user '{@event.Username}'");
        }

        /// <summary>
        /// Returns a short description that includes the handler's priority.
        /// </summary>
        /// <param name="handler">The <see cref="UserCreatedEventHandler"/> instance.</param>
        /// <returns>A string describing the handler and its priority.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        public static string GetDescription(this UserCreatedEventHandler handler) =>
            $"UserCreatedEventHandler (Priority = {handler?.Priority ?? throw new ArgumentNullException(nameof(handler))})";

        /// <summary>
        /// Handles a collection of <see cref="UserCreatedEvent"/> instances sequentially.
        /// </summary>
        /// <param name="events">The events to handle.</param>
        /// <param name="handler">The handler that will process each event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> or <paramref name="events"/> is null.</exception>
        public static async Task HandleManyAsync(this IEnumerable<UserCreatedEvent> events, UserCreatedEventHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(events);

            foreach (var ev in events)
            {
                await handler.HandleAsync(ev);
            }
        }

        /// <summary>
        /// Executes the handler and returns a boolean indicating success.
        /// Any exception is caught, logged to stderr, and false is returned.
        /// </summary>
        /// <param name="handler">The <see cref="UserCreatedEventHandler"/> instance.</param>
        /// <param name="event">The <see cref="UserCreatedEvent"/> to handle.</param>
        /// <returns>True if the event was handled successfully; otherwise, false.</returns>
        public static async Task<bool> TryHandleAsync(this UserCreatedEventHandler handler, UserCreatedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(@event);

            try
            {
                await handler.HandleAsync(@event);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception to stderr for debugging purposes before returning false
                Console.Error.WriteLine($"Error handling user created event: {@event.Username} ({@event.UserId}): {ex}");
                return false;
            }
        }
    }
}
