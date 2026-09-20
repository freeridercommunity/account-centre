using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Converters;
using Models;

public sealed class TrackRaceResponse : BasePageResponse
{
	[JsonPropertyName("track")]
	public TrackData? Track { get; init; }

	[JsonPropertyName("user_track_stats")]
	public UserTrackStats? UserTrackStats { get; init; }

	[JsonPropertyName("game_settings")]
	public GameSettings? GameSettings { get; init; }
}

public sealed record UserTrackStats
{
	[JsonPropertyName("avg_time")]
	[JsonConverter(typeof(StringOrFalseConverter))]
	public string? AverageTime { get; init; }

	[JsonPropertyName("best_date")]
	[JsonConverter(typeof(StringOrFalseConverter))]
	public string? BestDate { get; init; }

	[JsonPropertyName("best_time")]
	[JsonConverter(typeof(StringOrFalseConverter))]
	public string? BestTime { get; init; }

	[JsonPropertyName("best_vehicle")]
	public string? BestVehicle { get; init; }

	[JsonPropertyName("bst_d")]
	public string? BestPlatform { get; init; }

	[JsonPropertyName("desktop")]
	public bool? IsDesktop { get; init; }

	[JsonPropertyName("dwn_vote")]
	public ushort DownVote { get; init; }

	[JsonPropertyName("fav")]
	public ushort IsFavorite { get; init; }

	[JsonPropertyName("flagged")]
	public bool Flagged { get; init; }

	[JsonPropertyName("last_played_date")]
	[JsonConverter(typeof(StringOrFalseConverter))]
	public string? LastPlayedDate { get; init; }

	[JsonPropertyName("plays")]
	public uint Plays { get; init; }

	[JsonPropertyName("runs")]
	public uint Runs { get; init; }

	[JsonPropertyName("t_id")]
	public uint TrackID { get; init; }

	[JsonPropertyName("u_id")]
	public uint UserID { get; init; }

	[JsonPropertyName("up_vote")]
	public ushort UpVote { get; init; }

	[JsonPropertyName("vote")]
	public short Vote { get; init; }
}

public sealed class GameSettings
{
	[JsonPropertyName("raceData")]
	public RaceData[]? Races { get; init; }

}