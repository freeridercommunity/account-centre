using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class UserPageFriendsResponse : UserPageCategoryResponse<FriendData>
{
	[JsonInclude]
	[JsonPropertyName("friends")]
	private FriendsData<FriendData>? Friends
	{
		init
		{
			if (value != null)
				Data = value.Data;
		}
	}

	[JsonPropertyName("friend_requests")]
	public FriendRequestsData<FriendRequestData>? FriendRequests { get; init; }
}