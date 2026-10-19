using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record CommentData
{
	[JsonPropertyName("id")]
	public long ID { get; init; }
}