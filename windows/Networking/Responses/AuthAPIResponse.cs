using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public class AuthAPIResponse : APIResponse<AuthResponseData>
{
	[JsonPropertyName("app_signed_request")]
	public string Token { get; init; } = "";
}

public sealed class AuthResponseData
{
	[JsonPropertyName("user")]
	public ClientUserData? User { get; init; }
	[JsonPropertyName("user_stats")]
	public UserStatsData? UserStats { get; init; }
}