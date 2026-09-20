using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

public sealed class BoolConverter : JsonConverter<bool>
{
	public override bool Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True => true,
			JsonTokenType.False => false,
			JsonTokenType.Number => reader.GetUInt16() != 0,
			JsonTokenType.String => reader.GetString() switch
			{
				"0" => false,
				"1" => true,
				"true" => true,
				"false" => false,
				_ => throw new JsonException("Expected a boolean or 0/1.")
			},

			_ => throw new JsonException(
				$"Cannot convert {reader.TokenType} to bool."
			)
		};
	}

	public override void Write(
		Utf8JsonWriter writer,
		bool value,
		JsonSerializerOptions options)
	{
		writer.WriteBooleanValue(value);
	}
}