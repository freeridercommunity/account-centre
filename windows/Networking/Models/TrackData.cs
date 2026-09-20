using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

using Converters;

public enum TrackVehicle
{
	BMX,
	MTB
}

public record TrackData : BaseData<TrackData>
{
	[JsonPropertyName("id")]
	public int ID { get; private set; } = -1;
	[JsonPropertyName("title")]
	public string Title { get; init; } = "";
	[JsonPropertyName("descr")]
	public string Description { get; init; } = "";
	[JsonPropertyName("vehicle")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public TrackVehicle DefaultVehicle { get; init; }
	[JsonPropertyName("vehicles")]
	public string[]? AllowedVehicleNames { get; init; }
	[JsonIgnore]
	public TrackVehicle[]? AllowedVehicles =>
		AllowedVehicleNames?.Select(Enum.Parse<TrackVehicle>).ToArray();

	// Author stuff
	[JsonPropertyName("u_id")]
	public int? AuthorID { get; init; }
	[JsonPropertyName("author_is_user")]
	public bool IsAuthorUser { get; init; }
	[JsonPropertyName("u_url")]
	public string? AuthorName { get; init; }
	[JsonPropertyName("author")]
	public string AuthorDisplayName { get; init; } = "";

	[JsonPropertyName("featured")]
	public bool IsFeatured { get; init; }
	[JsonPropertyName("hide")]
	[JsonConverter(typeof(BoolConverter))]
	public bool IsHidden { get; init; }

	// Non-shared props

	[JsonInclude]
	[JsonPropertyName("url")]
	private string Url { init => InferID(value); }
	[JsonInclude]
	[JsonPropertyName("slug")]
	private string Slug { init => InferID(value); }

	[JsonPropertyName("code")]
	public string? Code { get; init; }
	[JsonPropertyName("size")]
	public int? Size { get; init; }
	[JsonPropertyName("cdn")]
	public string? StaticURL { get; init; } = "";
	[JsonPropertyName("pwrups")]
	public TrackPowerupData? Powerups { get; init; }

	[JsonPropertyName("img")]
	public string? ThumbnailURL { get; init; }

	[JsonInclude]
	[JsonPropertyName("thmb")]
	private string Thmb
	{
		init => ThumbnailURL = value;
	}

	[JsonPropertyName("ft_ts")]
	public long? FeaturedTimestamp { get; init; }
	[JsonPropertyName("p_ts")]
	public long? PublishTimestamp { get; init; }

	// Unknown/unused prop
	// [JsonPropertyName("admin")]
	// public int? Admin { get; init; }

	//#region Track list data
	[JsonPropertyName("vote_percent")]
	public ushort? VotePercentage { get; init; }

	[JsonPropertyName("votes")]
	[JsonConverter(typeof(StringOrNumberConverter))]
	public string? VotesText { get; init; }

	[JsonPropertyName("best_time")]
	[JsonConverter(typeof(StringOrFalseConverter))]
	public string? BestTime { get; init; }

	[JsonPropertyName("img_url_small")]
	public string? AuthorAvatarURL { get; init; }

	private void InferID(string? value)
	{
		if (value == null || ID != -1)
			return;

		var index = value.IndexOf('-');

		if (index > 0)
			ID = int.Parse(value[..index]);
	}
}

public sealed class TrackPowerupData
{
	[JsonPropertyName("gls")]
	public uint Goals { get; init; }
	[JsonPropertyName("chkpts")]
	public uint Checkpoints { get; init; }
	[JsonPropertyName("bsts")]
	public uint Boosts { get; init; }
	[JsonPropertyName("grvty")]
	public uint Gravitys { get; init; }
	[JsonPropertyName("slwmtn")]
	public uint Slowmos { get; init; }
	[JsonPropertyName("bmbs")]
	public uint Bombs { get; init; }
}