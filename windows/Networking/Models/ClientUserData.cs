using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record ClientUserData : UserData
{
	[JsonPropertyName("email")]
	public string? Email { get; init; }
	[JsonPropertyName("verified")]
	public ushort? EmailVerified { get; init; }
	[JsonPropertyName("sex")]
	public string? Sex { get; init; }
	[JsonPropertyName("locale")]
	public string? Locale { get; init; }
	[JsonPropertyName("i_ts")]
	public long? CreatedTimestamp { get; init; }
	// Unknown
	[JsonPropertyName("l_ts")]
	public long? LTimestamp { get; init; }
	[JsonPropertyName("a_ts")]
	// Unknown
	public long? AccessedTimestamp { get; init; }
	[JsonPropertyName("f_ts")]
	public long? FTimestamp { get; init; }
	[JsonPropertyName("img_url_small")]
	public string? ImageUrlSmall { get; init; }
	[JsonPropertyName("fb_id")]
	public int? FacebookID { get; init; }
	[JsonPropertyName("fbcnvs_id")]
	public int? FacebookCanvasID { get; init; }
	// Gravatar? Unknown
	[JsonPropertyName("gg_id")]
	public string? GGID { get; init; }
	[JsonPropertyName("kg_id")]
	public int? KGID { get; init; }
	[JsonPropertyName("frm_id")]
	public int? ForumID { get; init; }
}