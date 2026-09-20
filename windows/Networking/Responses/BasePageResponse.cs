using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public abstract class BasePageResponse : BaseResponse
{
	[JsonPropertyName("app_title")]
	private string Title { get; init; } = "";
	[JsonPropertyName("result")]
	public bool Result => Title != "Page Not Found | Free Rider HD";
}