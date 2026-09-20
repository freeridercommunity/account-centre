using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record FriendRequestData
{
	[JsonPropertyName("user")]
	public required FriendRequestUser User { get; init; }

	[JsonPropertyName("request")]
	public required FriendRequestRequest Request { get; init; }
}

public sealed class FriendRequestUser
{
	[JsonPropertyName("u_id")]
	public int ID { get; init; }

	[JsonPropertyName("u_name")]
	public string Name { get; init; } = "";

	[JsonPropertyName("d_name")]
	public string DisplayName { get; init; } = "";

	[JsonPropertyName("img_url_small")]
	public string? AvatarURL { get; init; }
}

public sealed class FriendRequestRequest
{
	[JsonPropertyName("u_id")]
	public int ReceiverID { get; init; }

	[JsonPropertyName("r_uid")]
	public int SenderID { get; init; }

	[JsonPropertyName("time_ago")]
	public string? TimeAgo { get; init; }
}