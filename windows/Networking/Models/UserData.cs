using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public record UserData
{
	[JsonPropertyName("u_id")]
	public int ID { get; init; }
	[JsonPropertyName("u_name")]
	public string Name { get; init; } = "";
	[JsonPropertyName("d_name")]
	public string DisplayName { get; init; } = "";
	[JsonPropertyName("img_url_medium")]
	public string? AvatarURL { get; init; }
	[JsonPropertyName("forum_url")]
	public string? ForumURL { get; init; }
	[JsonPropertyName("current_user")]
	public bool IsCurrentUser { get; init; }
	[JsonPropertyName("classic")]
	public bool IsOfficialAuthor { get; init; }
	[JsonPropertyName("moderator")]
	public bool IsModerator { get; init; }
	[JsonPropertyName("admin")]
	public bool IsAdmin { get; init; }
	[JsonPropertyName("cosmetics")]
	public UserCosmeticsData? Cosmetics { get; init; }

	// AuthUser
	// [JsonPropertyName("a_ts")]
	// public long? ATimestamp { get; init; }
	// // [JsonPropertyName("campaign")]
	// // public CampaignData? Campaign { get; init; }
	// [JsonPropertyName("i_ts")]
	// public long? CreatedTimestamp { get; init; }
	// [JsonPropertyName("day")]
	// public ushort? Day { get; init; }
	// [JsonPropertyName("verified")]
	// public ushort? EmailVerified { get; init; }
	// [JsonPropertyName("friend_cnt")]
	// public ushort? FriendCount { get; init; }
	// [JsonPropertyName("plus")]
	// public bool? IsProMember { get; init; }
	// [JsonPropertyName("locale")]
	// public string? Locale { get; init; }
}

public sealed class UserCosmeticsData
{
	[JsonPropertyName("head")]
	public CosmeticData? Head { get; init; }
}

public sealed class CosmeticData
{
	[JsonPropertyName("img")]
	public string Image { get; init; } = "";
}

public sealed class UserStatsData
{
	[JsonPropertyName("u_id")]
	public int? ID { get; init; }
	[JsonPropertyName("tot_pts")]
	public uint Points { get; init; }
	[JsonPropertyName("cmpltd")]
	public uint Completed { get; init; }
	[JsonPropertyName("rtd")]
	public uint Rated { get; init; }
	[JsonPropertyName("cmmnts")]
	public uint Comments { get; init; }
	[JsonPropertyName("crtd")]
	public uint Created { get; init; }
	[JsonPropertyName("head_cnt")]
	public uint HeadCount { get; init; }
	[JsonPropertyName("total_head_cnt")]
	public uint TotalHeadCount { get; init; }

	// AuthSuccess
	[JsonPropertyName("total_cns")]
	public bool Coins { get; init; }
	[JsonPropertyName("beginner_pts")]
	public bool IsBeginner { get; init; }
}

public class UserInfoData
{
	[JsonPropertyName("about")]
	public string About { get; init; } = "";
}

public sealed class UserMobileStatsData
{
	[JsonPropertyName("lvl")]
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public uint Level { get; init; }
	[JsonPropertyName("wins")]
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public uint Wins { get; init; }
	[JsonPropertyName("headCount")]
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public uint HeadCount { get; init; }
	[JsonPropertyName("connected")]
	public ushort? Connected { get; init; }
}