using System.Text.Json;

namespace AccountCentre.Core.Settings;

public static class SettingsManager
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		IndentCharacter = '\t',
		IndentSize = 1,
		WriteIndented = true
	};

	public static AppSettings Current { get; private set; } = new();

	public static void Load()
	{
		if (!File.Exists(AppData.SettingsPath))
		{
			Current = new AppSettings();
			return;
		}

		try
		{
			var json = File.ReadAllText(AppData.SettingsPath);

			Current = JsonSerializer.Deserialize<AppSettings>(
				json,
				JsonOptions
			) ?? new AppSettings();
		}
		catch
		{
			Current = new AppSettings();
		}
	}

	public static void Save()
	{
		Directory.CreateDirectory(AppData.Directory);

		var json = JsonSerializer.Serialize(
			Current,
			JsonOptions
		);

		File.WriteAllText(
			AppData.SettingsPath,
			json
		);
	}
}