using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Converters;
using Models;

public sealed class UserPageResponse : BasePageResponse
{
	[JsonPropertyName("user")]
	public UserData? User { get; init; }

	[JsonPropertyName("user_stats")]
	public UserStatsData? UserStats { get; init; }

	[JsonPropertyName("user_info")]
	[JsonConverter(typeof(UserInfoDataConverter))]
	public UserInfoData? UserInfo { get; init; }

	[JsonPropertyName("user_mobile_stats")]
	public UserMobileStatsData? UserMobileStats { get; init; }

	[JsonPropertyName("is_profile_owner")]
	public bool IsCurrentUser { get; init; }

	[JsonPropertyName("user_verify_reminder")]
	public bool EmailVerifyReminder { get; init; }

	[JsonPropertyName("created_tracks")]
	// public TracksData<CreatedTrackData>? CreatedTracks { get; init; }
	public TracksData<TrackData>? CreatedTracks { get; init; }

	[JsonPropertyName("friends")]
	public FriendsData<FriendData>? Friends { get; init; }

	[JsonPropertyName("friend_requests")]
	public FriendRequestsData<FriendRequestData>? FriendRequests { get; init; }

	[JsonPropertyName("recently_ghosted_tracks")]
	public TracksData<TrackData>? RecentlyCompletedTracks { get; init; }
}

public sealed class TracksData<T>
{
	[JsonPropertyName("tracks")]
	public required T[] Data { get; init; }
}

public sealed class FriendsData<T>
{
	[JsonPropertyName("friends_data")]
	public required T[] Data { get; init; }
}

public sealed class FriendRequestsData<T>
{
	[JsonPropertyName("request_data")]
	public required T[] Data { get; init; }
}