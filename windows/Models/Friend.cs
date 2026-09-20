namespace AccountCentre.Models;

using Networking;
using Networking.Models;
using Networking.Responses;

public sealed record Friend : Base
{
	public string Name { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public string AvatarURL { get; set; } = "";
	public string ActivityTimeAgo { get; set; } = "";
	public long ActivityTimestamp { get; set; }
	public Friend(FriendData data) : base(data.ID)
	{
		Name = data.Name;
		DisplayName = data.DisplayName;
		AvatarURL = data.AvatarURL;
		ActivityTimeAgo = data.ActivityTimeAgo;
		ActivityTimestamp = data.ActivityTimestamp;
	}

	public async Task Remove()
	{
		var response = await REST.PostAsync<APIResponse>(
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