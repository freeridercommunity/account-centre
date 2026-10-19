using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public class NotificationSettingsPageResponse : BasePageResponse
{
	[JsonPropertyName("ingame_settings")]
	public SettingData[] AppSettings { get; init; } = [];

	[JsonPropertyName("email_settings")]
	public SettingData[] EmailSettings { get; init; } = [];
}

public class SettingData
{
	[JsonPropertyName("text")]
	public string Description { get; init; } = "Unknown";

	[JsonPropertyName("enabled")]
	public bool Enabled { get; init; }

	[JsonPropertyName("type")]
	// Convert to enum
	public string Type { get; init; } = "Unknown";
}