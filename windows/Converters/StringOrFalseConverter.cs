using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

public sealed class StringOrFalseConverter : JsonConverter<string?>
{
	public override string? Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.String => reader.GetString(),
			JsonTokenType.False => null,
			JsonTokenType.Null => null,
			_ => throw new JsonException(
				$"Expected a string, false, or null but got {reader.TokenType}")
		};
	}

	public override void Write(
		Utf8JsonWriter writer,
		string? value,
		JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteBooleanValue(false);
			return;
		}

		writer.WriteStringValue(value);
	}
}