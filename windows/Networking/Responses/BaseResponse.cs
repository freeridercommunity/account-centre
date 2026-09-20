using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public abstract class BaseResponse
{
	[JsonPropertyName("app_version")]
	public string Version { get; init; } = "";
}