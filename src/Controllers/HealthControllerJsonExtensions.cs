using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetMicroORM.Controllers;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="HealthController"/>.
/// </summary>
public static class HealthControllerJsonExtensions
{
	/// <summary>
	/// Serializes a <see cref="HealthController"/> instance to a JSON string using the predefined
	/// <see cref="_jsonOptions"/>. If <paramref name="value"/> is <c>null</c>, the method returns an
	/// empty JSON object (<c>{}</c>).
	/// </summary>
	/// <param name="value">The <see cref="HealthController"/> instance to serialize. May be <c>null</c>.</param>
	/// <param name="indented">
	/// When <c>true</c>, the output JSON is formatted with indentation for readability; otherwise,
	/// the JSON is written in a compact form.
	/// </param>
	/// <returns>
	/// A JSON string representation of <paramref name="value"/>. Returns <c>"{}"</c> when
	/// <paramref name="value"/> is <c>null</c>.
	/// </returns>
	public static string ToJson(this HealthController? value, bool indented = false)
	{
		if (value is null)
		{
			return "{}";
		}

		var options = indented
			? new JsonSerializerOptions(_jsonOptions)
			{
				WriteIndented = true
			}
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="HealthController"/> instance using the predefined
	/// <see cref="_jsonOptions"/>.
	/// </summary>
	/// <param name="json">
	/// The JSON string to deserialize. Must not be <c>null</c> or consist only of whitespace.
	/// </param>
	/// <returns>
	/// The deserialized <see cref="HealthController"/> instance, or <c>null</c> if deserialization fails
	/// due to a <see cref="JsonException"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
	public static HealthController? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

		try
		{
			return JsonSerializer.Deserialize<HealthController>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="HealthController"/> instance without
	/// propagating any <see cref="JsonException"/> that may occur.
	/// </summary>
	/// <param name="json">
	/// The JSON string to deserialize. Must not be <c>null</c> or consist only of whitespace.
	/// </param>
	/// <param name="value">
	/// When the method returns <c>true</c>, receives the deserialized <see cref="HealthController"/>
	/// instance; otherwise, receives <c>null</c>.
	/// </param>
	/// <returns>
	/// <c>true</c> if deserialization succeeds; otherwise, <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
	public static bool TryFromJson(string json, out HealthController? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

		try
		{
			value = JsonSerializer.Deserialize<HealthController>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
