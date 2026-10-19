using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Converters;
using Models;

public sealed class TrackRaceResponse : BasePageResponse
{
	[JsonPropertyName("track")]
	public TrackData? Track { get; init; }

	[JsonPropertyName("user_track_stats")]
	[JsonConverter(typeof(UserTrackStatsConverter))]
	public UserTrackStats? UserTrackStats { get; init; }

	[JsonPropertyName("race_data")]
	public RaceEntryData[] RaceData { get; private set; } = [];

	[JsonInclude]
	[JsonPropertyName("game_settings")]
	private GameSettings GameSettings
	{
		init
		{
			if (value?.RaceData == null)
				return;

			this.RaceData = value.RaceData;
		}
	}
}

public sealed class GameSettings
{
	[JsonPropertyName("raceData")]
	public RaceEntryData[]? RaceData { get; init; }

}