using System.Text.Json.Serialization;

namespace AccountCentre.Networking.ExtendedAPI.Responses;

public class BaseResponse
{
	[JsonPropertyName("error")]
	public ErrorData? Error { get; init; }

	[JsonPropertyName("message")]
	public string? Message { get; init; }

	[JsonPropertyName("status")]
	public ushort Status { get; init; }
}

public class ErrorData
{
	[JsonPropertyName("code")]
	public string Code { get; init; } = "";

	[JsonPropertyName("message")]
	public string Message { get; init; } = "";
}