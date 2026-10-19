namespace AccountCentre.Models;

using Networking;
using Networking.Models;
using Networking.Responses;

public sealed record ClientUser : User
{
	public string? Email { get; set; } = "";
	public ClientUser(ClientUserData data) : base(data)
	{
		Email = data.Email;
		// data.
	}

	// public Task<bool> ChangeEmail(string email)
	// {
	// 	return true;
	// }

	// public Task<bool> ChangeUsername(string username)
	// {
	// 	return true;
	// }

	public async Task<ClientUserConfig?> FetchConfig(REST.Session rest)
		=> await rest.GetAsync<ClientUserConfig>(Routes.AccountSettings());

	public async Task<uint> FetchNotificationCount(REST.Session rest)
	{
		var response = await rest.PostAsync<DataPollResponse>(
			Routes.DataPoll(),
			new
			{
				notifications = "true"
			}
		);
		if (response == null ||
			response.Notifications == null)
			throw new Exception("Something went wrong! Failed to fetch notification count");

		return (uint)response.Notifications;
	}
}