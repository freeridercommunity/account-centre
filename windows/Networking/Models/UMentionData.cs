using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed class UMentionData
{
	[JsonPropertyName("d_name")]
	public string DisplayName { get; init; } = "";

	[JsonPropertyName("u_name")]
	public string Username { get; init; } = "";

	[JsonPropertyName("u_id")]
	public int ID { get; init; }

	[JsonPropertyName("image")]
	public string AvatarURL { get; init; } = "";
}