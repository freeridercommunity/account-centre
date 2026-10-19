using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class UserPageRacesResponse : UserPageCategoryResponse<TrackData>
{
	[JsonInclude]
	[JsonPropertyName("recently_ghosted_tracks")]
	private TracksData<TrackData>? RecentlyCompletedTracks
	{
		init
		{
			if (value != null)
				Data = value.Data;
		}
	}
}