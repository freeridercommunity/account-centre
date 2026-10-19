using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public enum NotificationType
{
	FriendLeaderboardPassed,
	FriendRaceChallenge,
	FriendRequestAccepted,
	FriendRequestReceived,
	SubscribedTrackPublish,
	TrackCommentMention,
	TrackLeaderboardPassed,
	Unknown
}

public sealed record NotificationData
{
	[JsonPropertyName("comment")]
	public CommentData? TrackComment { get; init; }

	[JsonPropertyName("msg")]
	public string? Message { get; init; }

	[JsonPropertyName("time")]
	public string Time { get; init; } = "Unknown";

	[JsonPropertyName("track")]
	public TrackData? Track { get; init; }

	[JsonPropertyName("ts")]
	public long Timestamp { get; init; }

	[JsonPropertyName("user")]
	public UserData? User { get; init; }

	public NotificationType Type { get; init; } = NotificationType.Unknown;

	// Infer types
	[JsonInclude]
	[JsonPropertyName("friend_lb_passed")]
	private bool FriendLbPassed
	{
		init
		{
			if (value)
				Type = NotificationType.FriendLeaderboardPassed;
		}
	}

	[JsonInclude]
	[JsonPropertyName("friend_t_challenge")]
	private bool FriendTChallenge
	{
		init
		{
			if (value)
				Type = NotificationType.FriendRaceChallenge;
		}
	}

	[JsonInclude]
	[JsonPropertyName("friend_req_accptd")]
	private bool FriendReqAccptd
	{
		init
		{
			if (value)
				Type = NotificationType.FriendRequestAccepted;
		}
	}

	[JsonInclude]
	[JsonPropertyName("friend_req_rcvd")]
	private bool FriendReqRcvd
	{
		init
		{
			if (value)
				Type = NotificationType.FriendRequestReceived;
		}
	}

	[JsonInclude]
	[JsonPropertyName("subscribed_t_publish")]
	private bool SubscribedTPublish
	{
		init
		{
			if (value)
				Type = NotificationType.SubscribedTrackPublish;
		}
	}

	[JsonInclude]
	[JsonPropertyName("t_uname_mention")]
	private bool TUnameMention
	{
		init
		{
			if (value)
				Type = NotificationType.TrackCommentMention;
		}
	}

	[JsonInclude]
	[JsonPropertyName("track_lb_passed")]
	private bool TrackLbPassed
	{
		init
		{
			if (value)
				Type = NotificationType.TrackLeaderboardPassed;
		}
	}
}