using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

using Networking.Models;

public sealed class UserTrackStatsConverter : JsonConverter<UserTrackStats?>
{
	public override UserTrackStats? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.False)
			return null;

		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected user_track_stats to be an object or false.");

		return JsonSerializer.Deserialize<UserTrackStats>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, UserTrackStats? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value, options);
	}
}