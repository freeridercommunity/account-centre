using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class AccountPageResponse : BasePageResponse
{
	[JsonPropertyName("user")]
	public ClientUserData? User { get; init; }

	[JsonPropertyName("user_info")]
	// [JsonConverter(typeof(UserInfoDataConverter))]
	public ClientUserInfoData? UserInfo { get; init; }

	[JsonPropertyName("personal")]
	public object? PersonalInfo { get; init; }

	[JsonPropertyName("username_can_change")]
	public bool HasNameChange { get; init; }

	[JsonPropertyName("num_username_changes_left")]
	public uint NameChangesRemaining { get; init; }

	[JsonPropertyName("user_photos")]
	public UserPhoto[] UserAvatars { get; init; } = [];

	[JsonPropertyName("username_max_length")]
	public uint MaxNameLength { get; init; }

	[JsonPropertyName("about_max_length")]
	public uint MaxDescriptionLength { get; init; }
}

public sealed class ClientUserInfoData : UserInfoData
{
	[JsonPropertyName("u_id")]
	public int ID { get; init; }

	// # of times username has been changed
	[JsonPropertyName("uname_chng")]
	public uint NameChanges { get; init; }
}

public sealed record UserPhoto
{
	[JsonPropertyName("selected")]
	public bool Selected { get; init; }

	[JsonPropertyName("img_url")]
	public required string URL { get; init; }

	[JsonPropertyName("name")]
	public string Name { get; init; } = "Unknown";

	[JsonPropertyName("type")]
	public string Type { get; init; } = "unknown";
}