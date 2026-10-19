using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record FriendData : UserData
{
	[JsonPropertyName("activity_time_ago")]
	public string ActivityTimeAgo { get; init; } = "";

	[JsonPropertyName("a_ts")]
	public long ActivityTimestamp { get; init; }
}