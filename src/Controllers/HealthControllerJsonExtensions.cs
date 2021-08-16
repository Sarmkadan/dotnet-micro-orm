using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetMicroORM.Controllers;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="HealthController"/>.
/// </summary>
public static class HealthControllerJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes a <see cref="HealthController"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="HealthController"/> instance to serialize. Can be <see langword="null"/>.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the <see cref="HealthController"/>. Returns "{}" if <paramref name="value"/> is <see langword="null"/>.</returns>
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
	/// Deserializes a JSON string to a <see cref="HealthController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be <see langword="null"/> or empty.</param>
	/// <returns>The deserialized <see cref="HealthController"/> instance, or <see langword="null"/> if deserialization fails.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
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
	/// Attempts to deserialize a JSON string to a <see cref="HealthController"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be <see langword="null"/> or empty.</param>
	/// <param name="value">Receives the deserialized <see cref="HealthController"/> instance if successful; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
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