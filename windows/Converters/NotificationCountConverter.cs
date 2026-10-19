using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

public sealed class NotificationCountConverter : JsonConverter<ushort?>
{
	public override ushort? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.Number)
			return reader.GetUInt16();

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (ushort.TryParse(value?.TrimStart('+'), out var count))
				return count;
		}

		throw new JsonException("Invalid notification count.");
	}

	public override void Write(Utf8JsonWriter writer, ushort? value, JsonSerializerOptions options)
	{
		if (value is null)
			writer.WriteNullValue();
		else
			writer.WriteNumberValue(value.Value);
	}
}