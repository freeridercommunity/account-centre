namespace AccountCentre.Core.Settings;

public sealed class AppSettings
{
	public bool AutoDownloadUpdates { get; set; } = false;

	public bool CheckForUpdatesAtStartup { get; set; } = true;

	public bool DeveloperMode { get; set; } = false;

	public bool MinimizeToTray { get; set; } = false;

	public string UpdateChannel { get; set; } = "stable";
}