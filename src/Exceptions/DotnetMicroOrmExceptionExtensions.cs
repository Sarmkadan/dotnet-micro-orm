using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMicroOrm.Exceptions
{
    /// <summary>
    /// Extension methods that add useful functionality to <see cref="DotnetMicroOrmException"/>.
    /// </summary>
    public static class DotnetMicroOrmExceptionExtensions
    {
        /// <summary>
        /// Adds multiple key/value pairs to the exception's error context.
        /// </summary>
        /// <param name="ex">The exception to enrich. Cannot be <see langword="null"/>.</param>
        /// <param name="contexts">A collection of context entries to add. Cannot be <see langword="null"/>.</param>
        /// <returns>The same exception instance, allowing method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> or <paramref name="contexts"/> is <see langword="null"/>.</exception>
        public static DotnetMicroOrmException WithContexts(
            this DotnetMicroOrmException ex,
            IDictionary<string, object> contexts)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentNullException.ThrowIfNull(contexts);

            foreach (var kvp in contexts)
            {
                // The original exception provides a WithContext method that adds a single entry.
                ex.WithContext(kvp.Key, kvp.Value);
            }

            return ex;
        }

        /// <summary>
        /// Retrieves a value from the exception's error context, cast to the requested type.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="ex">The exception containing the context. Cannot be <see langword="null"/>.</param>
        /// <param name="key">The key of the context entry. Cannot be <see langword="null"/>, empty, or whitespace.</param>
        /// <returns>The value cast to <typeparamref name="T"/>, or <see langword="default"/> if the key is missing or the cast fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static T? GetContextValue<T>(this DotnetMicroOrmException ex, string key)
        {
            ArgumentNullException.ThrowIfNull(ex);
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

            return ex.ErrorContext?.TryGetValue(key, out var value) == true &&
                   value is T typedValue
                ? typedValue
                : default;
        }

        /// <summary>
        /// Produces a detailed string representation that includes the base <c>ToString()</c>
        /// output and all error‑context entries.
        /// </summary>
        /// <param name="ex">The exception to format. Cannot be <see langword="null"/>.</param>
        /// <returns>A string containing the exception message and its context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is <see langword="null"/>.</exception>
        public static string ToDetailedString(this DotnetMicroOrmException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);

            var baseString = ex.ToString();

            return ex.ErrorContext is { Count: > 0 }
                ? $"{baseString} | Context: {{ {string.Join(", ", ex.ErrorContext.Select(kvp => $"{kvp.Key}={kvp.Value}"))} }}"
                : baseString;
        }
    }
}
