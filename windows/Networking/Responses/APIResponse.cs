using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public class APIResponse : BaseResponse
{
	[JsonPropertyName("result")]
	public virtual bool Result { get; init; } = true;

	[JsonPropertyName("msg")]
	public string? Message { get; init; }
}

public class APIResponse<T> : APIResponse
{
	[JsonPropertyName("data")]
	public T? Data { get; init; }
}