#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization helpers for TableAttribute
/// </summary>
public static class TableAttributeJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes a TableAttribute to a JSON string
	/// </summary>
	/// <param name="value">The TableAttribute to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation</param>
	/// <returns>A JSON string representation of the TableAttribute</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this TableAttribute value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions)
				{ WriteIndented = true }
				: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a TableAttribute from a JSON string
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>The deserialized TableAttribute, or null if JSON is null or empty</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static TableAttribute? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json) || json == "null")
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<TableAttribute>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a TableAttribute from a JSON string
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">The deserialized TableAttribute, or null if deserialization fails</param>
	/// <returns>True if deserialization succeeded; otherwise, false</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out TableAttribute? value)
	{
		value = null;

		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json) || json == "null")
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<TableAttribute>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
