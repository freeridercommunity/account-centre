using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Models;

public sealed record ClientUserConfig
{
	[JsonPropertyName("about_max_length")]
	public uint AboutMaxLength { get; init; }

	[JsonPropertyName("num_username_changes_left")]
	public ushort NameChangesRemaining { get; init; }

	[JsonPropertyName("username_can_change")]
	public bool HasNameChange { get; init; }

	[JsonPropertyName("username_max_length")]
	public uint UsernameMaxLength { get; init; }
}