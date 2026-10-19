using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public class UserUpdatedAPIResponse : APIResponse<UserUpdatedAPIResponseData>
{
}

public class UserUpdatedAPIResponseData
{
	[JsonPropertyName("changes_left")]
	public int ChangesRemaining { get; init; }

	[JsonPropertyName("update_user")]
	public bool UpdateUser { get; init; }

	[JsonPropertyName("user")]
	public UserData? User { get; init; }

	[JsonPropertyName("user_stats")]
	public UserStatsData? UserStats { get; init; }
}