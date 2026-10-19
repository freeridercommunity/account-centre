namespace AccountCentre.Networking;

using Endpoints;

public class API
{
	public UsersAPI Users { get; }
	public API(REST.Session rest)
	{
		Users = new UsersAPI(rest);

		throw new NotImplementedException();
	}
}