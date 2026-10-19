using AccountCentre.Models;

namespace AccountCentre.Networking.Endpoints;

public class UsersAPI(REST.Session rest)
{
	public Task<User?> FetchAsync(string uname)
		=> rest.GetAsync<User>($"u/{uname}");

	public Task<List<User>?> GetFriendsAsync(string uname)
		=> rest.GetAsync<List<User>>($"u/{uname}/friends");
}