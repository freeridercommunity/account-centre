using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class AccountPageResponse : BasePageResponse
{
	[JsonPropertyName("user")]
	public ClientUserData? User { get; set; }

	[JsonPropertyName("user_info")]
	// [JsonConverter(typeof(UserInfoDataConverter))]
	public ClientUserInfoData? UserInfo { get; set; }

	[JsonPropertyName("personal")]
	public object? PersonalInfo { get; set; }

	[JsonPropertyName("username_can_change")]
	public bool HasNameChange { get; init; }

	[JsonPropertyName("num_username_changes_left")]
	public uint NameChangesRemaining { get; init; }

	[JsonPropertyName("user_photos")]
	public object[]? UserAvatars { get; init; }

	[JsonPropertyName("username_max_length")]
	public uint MaxNameLength { get; init; }

	[JsonPropertyName("about_max_length")]
	public uint MaxDescriptionLength { get; set; }
}

public sealed class ClientUserInfoData : UserInfoData
{
	[JsonPropertyName("u_id")]
	public int ID { get; init; }

	[JsonPropertyName("uname_chng")]
	public uint NameChangesRemaining { get; init; }
}