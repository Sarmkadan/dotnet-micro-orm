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
        public static async Task HandleWithConsoleLoggingAsync(this UserCreatedEventHandler handler, UserCreatedEvent @event)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            Console.WriteLine($"[UserCreated] Handling start for user '{@event.Username}' (Id: {@event.UserId})");
            await handler.HandleAsync(@event);
            Console.WriteLine($"[UserCreated] Handling completed for user '{@event.Username}'");
        }

        /// <summary>
        /// Returns a short description that includes the handler's priority.
        /// </summary>
        /// <param name="handler">The <see cref="UserCreatedEventHandler"/> instance.</param>
        public static string GetDescription(this UserCreatedEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return $"UserCreatedEventHandler (Priority = {handler.Priority})";
        }

        /// <summary>
        /// Handles a collection of <see cref="UserCreatedEvent"/> instances sequentially.
        /// </summary>
        /// <param name="events">The events to handle.</param>
        /// <param name="handler">The handler that will process each event.</param>
        public static async Task HandleManyAsync(this IEnumerable<UserCreatedEvent> events, UserCreatedEventHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var ev in events)
            {
                await handler.HandleAsync(ev);
            }
        }

        /// <summary>
        /// Executes the handler and returns a boolean indicating success.
        /// Any exception is caught and false is returned.
        /// </summary>
        /// <param name="handler">The <see cref="UserCreatedEventHandler"/> instance.</param>
        /// <param name="event">The <see cref="UserCreatedEvent"/> to handle.</param>
        public static async Task<bool> TryHandleAsync(this UserCreatedEventHandler handler, UserCreatedEvent @event)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            try
            {
                await handler.HandleAsync(@event);
                return true;
            }
            catch
            {
                // Swallow exception – caller can decide what to do.
                return false;
            }
        }
    }
}
