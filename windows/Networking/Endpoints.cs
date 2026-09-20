namespace AccountCentre.Networking;

public static class Endpoints
{
	public static string Account()
		=> "account/settings";

	public static string RaceData(int tid, int uid)
		=> $"track_api/load_races?t_id={tid}&u_ids={uid}";

	public static string Track(int id)
		=> $"t/{id}";

	public static string TrackData(int id, params string[] fields)
	{
		var endpoint = $"track_api/load_track?id={id}";

		foreach (var field in fields)
		{
			endpoint += $"&fields[]={Uri.EscapeDataString(field)}";
		}

		return endpoint;
	}

	public static string TrackRace(int tid, string username)
		=> $"t/{tid}/r/{username}";

	public static string User(string username)
		=> $"u/{Uri.EscapeDataString(username)}";

	public static string UserFriends(string username)
		=> $"{User(username)}/friends";

	public static string UserRaces(string username)
		=> $"{User(username)}/ghosted";

	public static string UserTracks(string username)
		=> $"{User(username)}/created";
}