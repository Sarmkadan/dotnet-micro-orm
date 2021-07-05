#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetMicroOrm.Domain.Models;

namespace DotnetMicroOrm.Examples
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for ECommerceExample.
    /// Enables round-trip serialization/deserialization of e-commerce data structures.
    /// </summary>
    public static class ECommerceExampleJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes an ECommerceExample instance to a JSON string.
        /// </summary>
        /// <param name="value">The ECommerceExample instance to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability</param>
        /// <returns>A JSON string representation of the ECommerceExample</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this ECommerceExample value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an ECommerceExample instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized ECommerceExample instance, or null if JSON is invalid</returns>
        /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
        public static ECommerceExample? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ECommerceExample>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an ECommerceExample instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized ECommerceExample instance if successful</param>
        /// <returns>True if deserialization succeeds; false otherwise</returns>
        public static bool TryFromJson(string json, out ECommerceExample? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ECommerceExample>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}