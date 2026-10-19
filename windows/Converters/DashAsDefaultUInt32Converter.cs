using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountCentre.Converters;

public sealed class DashAsDefaultUInt32Converter : JsonConverter<uint>
{
	public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
			return reader.GetUInt32();

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (value == "--")
				return 0;

			if (uint.TryParse(value, out var result))
				return result;
		}

		throw new JsonException();
	}

	public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
		=> writer.WriteNumberValue(value);
}