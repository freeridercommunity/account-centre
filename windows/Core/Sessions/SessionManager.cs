namespace AccountCentre.Core.Sessions;

using Core.Settings;
using Models;

public static class SessionManager
{
	private static readonly Dictionary<Guid, Session> sessions = [];
	public static Session Create(string token, ClientUser user)
	{
		if (!SettingsManager.Current.AllowConcurrentSessions &&
			sessions.Values.FirstOrDefault(s => s.UserID == (ulong)user.ID) is Session existing)
			return existing;

		Session session = new(token)
		{
			User = user,
			UserID = (ulong)user.ID,
			Username = user.Name
		};
		sessions.Add(session.ID, session);
		return session;
	}
}