using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record FriendData
{
	[JsonPropertyName("u_id")]
	public int ID { get; init; }

	[JsonPropertyName("u_name")]
	public string Name { get; init; } = "";

	[JsonPropertyName("d_name")]
	public string DisplayName { get; init; } = "";

	[JsonPropertyName("img_url_small")]
	public string AvatarURL { get; init; } = "";

	[JsonPropertyName("activity_time_ago")]
	public string ActivityTimeAgo { get; init; } = "";

	[JsonPropertyName("a_ts")]
	public long ActivityTimestamp { get; init; }
}