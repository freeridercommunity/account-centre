using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public abstract class UserPageCategoryResponse<T> : BasePageResponse
{
	[JsonPropertyName("data")]
	public T[]? Data { get; init; }
}