using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AccountCentre.Core.Settings;

public static class SettingsManager
{
	public static AppSettings Current { get; private set; } = new();
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		IndentCharacter = '\t',
		IndentSize = 1,
		WriteIndented = true,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers = { IgnoreDefaultSettings }
		}
	};
	public static void Load()
	{
		Current.PropertyChanged -= OnPropertyChanged;

		try
		{
			var json = File.ReadAllText(AppData.SettingsPath);
			Current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
				?? new AppSettings();
		}
		catch (DirectoryNotFoundException)
		{
			Current = new AppSettings();
		}
		catch (FileNotFoundException)
		{
			Current = new AppSettings();
		}
		catch (JsonException)
		{
			Current = new AppSettings();
		}

		Current.PropertyChanged += OnPropertyChanged;
	}

	public static void Save()
	{
		var path = AppData.SettingsPath;
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var json = JsonSerializer.Serialize(Current, JsonOptions);
		if (json == "{}")
		{
			File.Delete(path);
			return;
		}

		File.WriteAllText(path, json);
	}

	private static readonly SemaphoreSlim SaveLock = new(1, 1);
	public static async Task SaveAsync()
	{
		await SaveLock.WaitAsync();

		try
		{
			var path = AppData.SettingsPath;
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);

			var json = JsonSerializer.Serialize(Current, JsonOptions);
			if (json == "{}")
			{
				File.Delete(path);
				return;
			}

			var temporaryPath = path + ".tmp";
			await File.WriteAllTextAsync(temporaryPath, json);
			File.Move(temporaryPath, path, true);
		}
		finally
		{
			SaveLock.Release();
		}
	}

	private static CancellationTokenSource? _pendingSave;
	private static async Task SaveDebouncedAsync(CancellationTokenSource cts)
	{
		try
		{
			await Task.Delay(500, cts.Token);
			if (!cts.IsCancellationRequested)
				await SaveAsync();
		}
		catch (OperationCanceledException) when (cts.IsCancellationRequested)
		{
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Settings save failed: {ex}");
		}
		finally
		{
			Interlocked.CompareExchange(ref _pendingSave, null, cts);
			cts.Dispose();
		}
	}

	private static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var cts = new CancellationTokenSource();
		var previous = Interlocked.Exchange(ref _pendingSave, cts);
		if (previous is not null)
		{
			try
			{
				previous.Cancel();
			}
			catch (ObjectDisposedException)
			{
				// Previous save already completed.
			}
		}

		_ = SaveDebouncedAsync(cts);
	}

	private static void IgnoreDefaultSettings(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type != typeof(AppSettings))
			return;

		var defaults = new AppSettings();
		foreach (var property in typeInfo.Properties)
		{
			var info = typeof(AppSettings).GetProperty(property.Name);
			if (info is null)
				continue;

			var defaultValue = info.GetValue(defaults);
			property.ShouldSerialize = (_, value) =>
				!Equals(value, defaultValue);
		}
	}
}