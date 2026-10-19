namespace AccountCentre.Models;

using Networking.Models;

public sealed record Notification
{
	public string? Message { get; set; }
	public bool IsSystem => User == null;
	public bool IsSenderUser => User != null;
	public bool HasQuote => Message != null;
	public bool HasTrack => Track != null;
	public string Text => Type switch
	{
		NotificationType.FriendLeaderboardPassed => "passed you on the leaderboard of",
		NotificationType.FriendRaceChallenge => "challenged you to a race on",
		NotificationType.FriendRequestAccepted => "accepted your friend request.",
		NotificationType.FriendRequestReceived => "sent you a friend request.",
		NotificationType.SubscribedTrackPublish => "just published",
		NotificationType.TrackCommentMention => "mentioned you in a comment on",
		NotificationType.TrackLeaderboardPassed => "passed you on the leaderboard of",
		_ => $"Unrecognized notification type: {Type}"
	};
	public string Time { get; set; } = "";
	public long Timestamp { get; set; }
	public Track? Track { get; set; }
	public CommentData? TrackComment { get; set; }
	public NotificationType Type { get; set; } = NotificationType.Unknown;
	public User? User { get; set; }
	public Notification(NotificationData data)
	{
		this.Time = data.Time;
		this.Timestamp = data.Timestamp;
		this.Type = data.Type;

		this.Message = data.Message;

		if (data.Track != null)
			this.Track = new Track(data.Track);

		if (data.TrackComment != null)
			this.TrackComment = data.TrackComment;

		if (data.User != null)
			this.User = new User(data.User);
	}
}