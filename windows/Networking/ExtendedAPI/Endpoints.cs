namespace AccountCentre.Networking.ExtendedAPI;

public static class Endpoints
{
	public static string Email()
		=> "user/email";

	public static string OfficialAuthor()
		=> $"user/oa";

	public static string Race(int trackId)
		=> $"{Track(trackId)}/races";

	public static string Track(int trackId)
		=> $"tracks/{trackId}";
}