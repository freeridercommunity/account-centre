using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class TrackAPIResponse : APIResponse<TrackResponseData>
{
}

public sealed class TrackResponseData
{
	[JsonPropertyName("track")]
	public TrackData? Track { get; init; }
}