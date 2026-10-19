namespace AccountCentre.Models;

using Networking;
using Networking.Models;
using Networking.Responses;

public sealed record Friend : User
{
	public string ActivityTimeAgo { get; set; } = "";
	public long ActivityTimestamp { get; set; }
	public Friend(FriendData data) : base(data)
	{
		ActivityTimeAgo = data.ActivityTimeAgo;
		ActivityTimestamp = data.ActivityTimestamp;
	}

	public async Task Remove(REST.Session rest)
	{
		var response = await rest.PostAsync<APIResponse>(
			"friends/remove_friend",
			new
			{
				u_id = ID
			}
		);

		if (response?.Result is false)
			throw new Exception(response.Message);
	}
}