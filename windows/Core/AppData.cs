namespace AccountCentre.Core;

public static class AppData
{
	public static readonly string Directory = Path.Combine(
		Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData
		),
		"Free Rider Community",
		"Account Centre"
	);

	public static readonly string CacheDirectory =
		Path.Combine(Directory, "Cache");

	public static readonly string TempDirectory = Path.Combine(
		Path.GetTempPath(),
		"FreeRiderCommunityAccountCentre"
	);

	public static readonly string SettingsPath =
		Path.Combine(Directory, "settings.json");

	public static readonly string TokenSecretPath =
		Path.Combine(Directory, "secret");
}