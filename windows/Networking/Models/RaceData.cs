using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public record RaceEntryData
{
	[JsonPropertyName("race")]
	public RaceData? Data { get; init; }

	[JsonPropertyName("track")]
	public TrackData? Track { get; set; }

	[JsonPropertyName("user")]
	public UserData? User { get; init; }
}

public class RaceData
{
	[JsonPropertyName("code")]
	public string? Code { get; init; }

	[JsonPropertyName("desktop")]
	public bool IsDesktop { get; init; }

	[JsonPropertyName("run_ticks")]
	public uint Ticks { get; init; }

	[JsonPropertyName("vehicle")]
	public string? Vehicle { get; init; }
}