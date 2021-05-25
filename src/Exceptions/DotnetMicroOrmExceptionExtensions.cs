using System;
using System.Collections.Generic;

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
        /// <param name="ex">The exception to enrich.</param>
        /// <param name="contexts">A collection of context entries to add.</param>
        /// <returns>The same exception instance, allowing method chaining.</returns>
        public static DotnetMicroOrmException WithContexts(
            this DotnetMicroOrmException ex,
            IDictionary<string, object> contexts)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (contexts == null) throw new ArgumentNullException(nameof(contexts));

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
        /// <param name="ex">The exception containing the context.</param>
        /// <param name="key">The key of the context entry.</param>
        /// <returns>The value cast to <typeparamref name="T"/>, or <c>default</c> if the key is missing or the cast fails.</returns>
        public static T? GetContextValue<T>(this DotnetMicroOrmException ex, string key)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (ex.ErrorContext != null &&
                ex.ErrorContext.TryGetValue(key, out var value) &&
                value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        /// <summary>
        /// Produces a detailed string representation that includes the base <c>ToString()</c>
        /// output and all error‑context entries.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>A string containing the exception message and its context.</returns>
        public static string ToDetailedString(this DotnetMicroOrmException ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            var baseString = ex.ToString();

            if (ex.ErrorContext == null || ex.ErrorContext.Count == 0)
            {
                return baseString;
            }

            var parts = new List<string>(ex.ErrorContext.Count);
            foreach (var kvp in ex.ErrorContext)
            {
                parts.Add($"{kvp.Key}={kvp.Value}");
            }

            var contextString = string.Join(", ", parts);
            return $"{baseString} | Context: {{ {contextString} }}";
        }
    }
}
