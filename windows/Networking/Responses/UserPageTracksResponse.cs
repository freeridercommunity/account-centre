using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class UserPageTracksResponse : UserPageCategoryResponse<TrackData>
{
	[JsonInclude]
	[JsonPropertyName("created_tracks")]
	private TracksData<TrackData>? CreatedTracks
	{
		init
		{
			if (value != null)
				Data = value.Data;
		}
	}
}