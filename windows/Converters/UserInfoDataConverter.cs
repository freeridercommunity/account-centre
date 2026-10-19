using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

using Networking.Models;

public sealed class UserInfoDataConverter : JsonConverter<UserInfoData?>
{
	public override UserInfoData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.False ||
			reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.StartObject)
			return JsonSerializer.Deserialize<UserInfoData>(ref reader, options);

		throw new JsonException($"Expected user_info to be an object or false, got {reader.TokenType}");
	}

	public override void Write(Utf8JsonWriter writer, UserInfoData? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteBooleanValue(false);
			return;
		}

		JsonSerializer.Serialize(writer, value, options);
	}
}