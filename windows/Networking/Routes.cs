using WinRT;

namespace AccountCentre.Networking;

public static class Routes
{
	public static string AccountPasswordChange()
		=> "account/change_password";

	public static string AccountSelectAvatar()
		=> "account/update_photo";

	public static string AccountSettings()
		=> "account/settings";

	public static string AccountUpdateProfile()
		=> "account/edit_profile";

	public static string AuthLogout()
		=> "auth/logout";
	
	public static string AuthTokenRefresh()
		=> "account/change_password";

	public static string DataPoll()
		=> "datapoll/poll_request";

	public static string ForumAccountUpdate()
		=> "account/update_forum_account";

	public static string Notifications()
		=> "notifications";

	public static string NotificationSettings()
		=> "notifications/settings";

	public static string NotificationSettingsUpdate(string method, string type, bool state)
		=> $"notifications/update_settings/{method}/{type}/{(state ? 1 : 0)}";

	public static string NotificationsLoadMore(long timestamp)
		=> $"notifications/load_more/{timestamp}";

	public static string RaceData(ulong tid, ulong uid)
		=> $"track_api/load_races?t_id={tid}&u_ids={uid}";

	public static string Track(ulong id)
		=> $"t/{id}";

	public static string TrackData(ulong id, params string[] fields)
	{
		var endpoint = $"track_api/load_track?id={id}";

		foreach (var field in fields)
		{
			endpoint += $"&fields[]={Uri.EscapeDataString(field)}";
		}

		return endpoint;
	}

	public static string TrackRace(ulong tid, string username)
		=> $"t/{tid}/r/{username}";

	public static string User(string username)
		=> $"u/{Uri.EscapeDataString(username)}";

	public static string UserFriends(string username)
		=> $"{User(username)}/friends";

	public static string UserFriendRemove()
		=> "friends/remove_friend";

	public static string UserRaces(string username)
		=> $"{User(username)}/ghosted";

	public static string UserTracks(string username)
		=> $"{User(username)}/created";
}