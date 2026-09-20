using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed class TLookupData
{
	[JsonPropertyName("title")]
	public string Title { get; init; } = "";

	[JsonPropertyName("slug")]
	public string Slug { get; init; } = "";

	[JsonPropertyName("author")]
	public string AuthorDisplayName { get; init; } = "";

	[JsonPropertyName("author_slug")]
	public string AuthorSlug { get; init; } = "";

	[JsonPropertyName("thmb")]
	public string ThumbnailURL { get; init; } = "";

	[JsonPropertyName("img_url_small")]
	public string AuthorAvatarURL { get; init; } = "";

	[JsonPropertyName("vote_percent")]
	public ushort VotePercent { get; init; }

	[JsonPropertyName("votes")]
	public uint Votes { get; init; }
}