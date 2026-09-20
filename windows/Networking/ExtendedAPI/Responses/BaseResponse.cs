using System.Text.Json.Serialization;

namespace AccountCentre.Networking.ExtendedAPI.Responses;

public class BaseResponse
{
	[JsonPropertyName("message")]
	public string? Message { get; init; }

	[JsonPropertyName("status")]
	public int Status { get; init; }
}