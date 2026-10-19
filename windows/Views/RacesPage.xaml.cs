using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace AccountCentre.Views;

using Core;
using Core.Sessions;
using Core.Settings;
using Components.Dialogs;
using Interfaces;
using Models;
using Networking;
using Networking.Models;
using Networking.Responses;

public sealed partial class RacesPage : Page, IRefreshableView, ISearchableView, ISessionAware
{
	public Session Session { get; set; } = null!;
	public bool IsCurrentUser { get; private set; } = false;
	public bool IsDeveloperModeEnabled => SettingsManager.Current.DeveloperMode;
	private readonly Dictionary<ulong, ObservableCollection<Race>> _raceCache = [];
	public ObservableCollection<Race> Races { get; private set; } = [];
	public User? User;
	public RacesPage()
	{
		this.NavigationCacheMode = NavigationCacheMode.Enabled;

		InitializeComponent();

		_ = RefreshRacesAsync();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		if (e.Parameter is not ClientUser user)
		{
			this.User = null;
			return;
		}

		this.User = user;
		this.IsCurrentUser = user.ID == Session.User!.ID;
		// Bindings.Update();

		if (_raceCache.TryGetValue(user.ID, out var cached))
			Races = cached;
		else
		{
			Races = [];
			Bindings.Update();

			_raceCache[user.ID] = Races;

			_ = RefreshRacesAsync();
		}

		_ = UpdateRaceCount((uint)Races.Count);
	}

	private async Task UpdateRaceCount(uint count = 0)
	{
		if (this.User == null)
			return;

		var userStats = await User.FetchStats();
		if (userStats == null)
			return;

		this.RaceCount.Text = $"{count} of {userStats.Completed} races";
	}

	private TaskCompletionSource? _refreshCompletion;
	public async Task RefreshAsync(CancellationToken? cancellationToken)
	{
		_refreshCompletion = new TaskCompletionSource(
			TaskCreationOptions.RunContinuationsAsynchronously);

		this.RefreshContainer.RequestRefresh();

		await _refreshCompletion.Task;
	}

	public void ClearSearch()
	{
		RacesGrid.ItemsSource = Races;
		_ = UpdateRaceCount((uint)Races.Count);
	}

	public async Task<List<string>?> Search(string query)
	{
		if (query.Length > 0)
		{
			var searchItems = new ObservableCollection<Race>();
			var hasID = ulong.TryParse(query, out var id);
			if (query.Length > 3 &&
				hasID &&
				!Races.Any(race => race.Track?.ID == id))
			{
				try
				{
					var trackRace = await REST.GetAsync<TrackRaceResponse>(Routes.TrackRace(id, this.User!.Name)) ?? throw new Exception("Track not found");
					var raceData = trackRace?.RaceData?.FirstOrDefault() ?? throw new Exception("Race not found");
					if (raceData.User?.ID != this.User!.ID)
						throw new Exception("Racer is not current user");

					raceData.Track = trackRace.Track;
					searchItems.Add(new Race(raceData));
				}
				catch
				{
				}
			}

			foreach (var race in Races)
			{
				if (string.IsNullOrWhiteSpace(query) ||
					race.Track?.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
					(hasID && race.Track?.ID == id))
					searchItems.Add(race);
			}

			RacesGrid.ItemsSource = searchItems;
			_ = UpdateRaceCount((uint)searchItems.Count);
		}

		return null;
	}

	private async void BulkAction_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item)
			return;

		var selected = RacesGrid.SelectedItems;
		if (selected.Count == 0)
			return;

		var originalContent = BulkAction.Content;

		BulkAction.IsEnabled = false;
		BulkAction.Content = new ProgressRing
		{
			Height = 14,
			Width = 14
		};

		try
		{
			var races = await GetSelectedData();
			if (races is null)
				return;

			switch (item.Tag)
			{
				case "Copy":
				{
					var files = new List<StorageFile>(races.Count);

					foreach (var (name, data) in races)
					{
						var directory = Path.Combine(
							AppData.TempDirectory,
							"Clipboard");

						Directory.CreateDirectory(directory);

						var path = Path.Combine(directory, $"{name}.json");

						await File.WriteAllTextAsync(
							path,
							data,
							new UTF8Encoding(false));

						files.Add(
							await StorageFile.GetFileFromPathAsync(path));
					}

					var package = new DataPackage();
					package.SetStorageItems(files);

					Clipboard.SetContent(package);

					BulkActionStatusBar.Severity = InfoBarSeverity.Success;
					BulkActionStatusBar.Message = $"{races.Count} races copied to clipboard";
					break;
				}

				case "CopyAsZip":
				{
					var directory = Path.Combine(
						AppData.TempDirectory,
						"Clipboard");

					Directory.CreateDirectory(directory);

					var path = Path.Combine(directory, "races.zip");

					await using (var stream = new FileStream(
						path,
						FileMode.Create,
						FileAccess.Write,
						FileShare.None))
					{
						using var archive = new ZipArchive(
							stream,
							ZipArchiveMode.Create);

						foreach (var (name, code) in races)
						{
							var entry = archive.CreateEntry(
								$"{name}.txt",
								CompressionLevel.Optimal);

							await using var entryStream = entry.Open();
							await using var writer = new StreamWriter(
								entryStream,
								new UTF8Encoding(false));

							await writer.WriteAsync(code);
						}
					}

					var file = await StorageFile.GetFileFromPathAsync(path);

					var package = new DataPackage();
					package.SetStorageItems([file]);

					Clipboard.SetContent(package);

					BulkActionStatusBar.Severity = InfoBarSeverity.Success;
					BulkActionStatusBar.Message = $"{races.Count} races copied to clipboard";
					break;
				}

				case "Download":
				{
					var picker = new FileSavePicker(
						item.XamlRoot.ContentIslandEnvironment.AppWindowId)
					{
						DefaultFileExtension = ".zip",
						SuggestedFileName = "races"
					};

					picker.FileTypeChoices.Add(
						"ZIP archive",
						new List<string> { ".zip" });

					var result = await picker.PickSaveFileAsync();

					if (result is null)
						return;

					await using var stream = new FileStream(
						result.Path,
						FileMode.Create,
						FileAccess.Write,
						FileShare.None);

					using var archive = new ZipArchive(
						stream,
						ZipArchiveMode.Create);

					foreach (var (name, code) in races)
					{
						var entry = archive.CreateEntry(
							$"{name}.txt",
							CompressionLevel.Optimal);

						await using var entryStream = entry.Open();
						await using var writer = new StreamWriter(
							entryStream,
							new UTF8Encoding(false));

						await writer.WriteAsync(code);
					}

					BulkActionStatusBar.Severity = InfoBarSeverity.Success;
					BulkActionStatusBar.Message = $"{races.Count} races saved";
					break;
				}
			}

			RacesGrid.SelectedItems.Clear();
		}
		catch (Exception ex)
		{
			BulkActionStatusBar.Severity = InfoBarSeverity.Error;
			BulkActionStatusBar.Message = ex.Message;
		}
		finally
		{
			BulkActionStatusBar.IsOpen = true;

			_ = Task.Delay(3000).ContinueWith(_ =>
				{
					DispatcherQueue.TryEnqueue(() =>
						BulkActionStatusBar.IsOpen = false
					);
				});

			BulkAction.Content = originalContent;
		}
	}

	private async Task<List<(string Name, string Data)>?> GetSelectedData()
	{
		var selected = RacesGrid.SelectedItems;
		if (selected.Count == 0)
			return null;

		var tasks = selected
			.OfType<Race>()
			.Select<Race, Task<(string Name, string Data)?>>(async race =>
			{
				if (race?.Data == null)
					return null;

				var data = JsonSerializer.Serialize(race.Data);

				return data is string text
					? (Name: Utils.SanitizeFileName($"{race.Track?.ID}-{race.User.ID /* ?? this.User.ID */}-0"), Data: text)
					: null;
			});

		var results = await Task.WhenAll(tasks);

		var races = results
			.Where(x => x is not null)
			.Select(x => x!.Value)
			.ToList();

		if (races.Count == 0)
			return null;

		return races;
	}

	private void SelectAll_Checked(object sender, RoutedEventArgs e)
		=> RacesGrid.SelectAll();
	private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
		=> RacesGrid.SelectedItems.Clear();

	private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
	{
		var count = RacesGrid.Items.Count;
		var selected = RacesGrid.SelectedItems.Count;

		if (count != selected)
			return;

		RacesGrid.SelectedItems.Clear();
	}

	private void RacesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		=> UpdateSelectAllState();
	private void UpdateSelectAllState()
	{
		var count = RacesGrid.Items.Count;
		var selected = RacesGrid.SelectedItems.Count;

		BulkAction.Content = $"{selected} selected";
		BulkAction.IsEnabled = selected > 0;
		BulkAction.Visibility = selected > 0
			? Visibility.Visible
			: Visibility.Collapsed;
		OptionsAllCheckBox.IsChecked = selected switch
		{
			0 => false,
			_ when selected == count => true,
			_ => null
		};
	}

	private async void ViewRace_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Race race ||
			race.Track == null)
			return;

		await Launcher.LaunchUriAsync(new Uri($"https://frhd.co/t/{race.Track.ID}/r/{race.User.Name}"));
	}

	private async void CopyRace_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Race race)
			return;

		try
		{
			if (race.Data is not RaceData data)
				return;

			var text = JsonSerializer.Serialize(data);
			var package = new DataPackage();

			switch (item.Tag)
			{
				case "File":
					if (race.Track == null)
						throw new Exception("Track not loaded");

					var path = Path.Combine(Path.GetTempPath(), $"{race.Track.ID}-{race.User.ID}-0.json");

					await File.WriteAllTextAsync(path, text);

					var file = await StorageFile.GetFileFromPathAsync(path);
					package.SetStorageItems([file]);
					break;

				case "Plain":
					package.SetText(text);
					break;
			}

			Clipboard.SetContent(package);
		}
		catch (Exception ex)
		{
			var errorDialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.XamlRoot
			};
			await errorDialog.ShowAsync();
		}
	}

	private async void DownloadRace_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Race race)
			return;

		try
		{
			if (race.Data is not RaceData data ||
				race.Track == null)
				return;

			var text = JsonSerializer.Serialize(data);
			var picker = new FileSavePicker(item.XamlRoot.ContentIslandEnvironment.AppWindowId)
			{
				CommitButtonText = "Save Race",
				DefaultFileExtension = ".json",
				SuggestedFileName = $"{race.Track.ID}-{race.User.ID}-0",
				SuggestedFolder = "",
				SuggestedStartLocation = PickerLocationId.Downloads
			};

			picker.FileTypeChoices.Add("Race Files", new List<string>() { ".json" });

			var result = await picker.PickSaveFileAsync();
			if (result == null)
				return;

			string savePath = result.Path;
			await File.WriteAllTextAsync(savePath, text);
			// SavedFileTextBlock.Text = "File saved to: " + savePath;
		}
		catch (Exception ex)
		{
			var errorDialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.XamlRoot
			};
			await errorDialog.ShowAsync();
		}
	}

	private async void DeleteRace_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Race race)
			return;

		item.IsEnabled = false;
		try
		{
			if (race.Track == null)
				throw new Exception("Track not loaded");

			var confirmDialog = new ConfirmDialog
			{
				Content = new StackPanel
				{
					Children =
					{
						new TextBlock
						{
							Inlines =
							{
								new Run { Text = "Are you sure you wish to permanently delete your race and any future races on " },
								new Hyperlink
								{
									Inlines =
									{
										new Run {
											FontWeight = FontWeights.Bold,
											Text = race.Track.Title
										}
									},
									NavigateUri = new Uri($"http://frhd.co/t/{race.Track.ID}/r/{race.User.Name}")
								},
								new Run { Text = "?" }
							}
						},
						new InfoBar
						{
							IsClosable = false,
							IsOpen = true,
							Message = "This action cannot be undone, and you will not be able to save another race on this track.",
							Severity = InfoBarSeverity.Warning
						}
					},
					Spacing = 12
				},
				XamlRoot = this.XamlRoot
			};

			var result = await confirmDialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				await race.Delete(Session.RestExtended);
				Races.Remove(race);
			}
		}
		catch (Exception ex)
		{
			var dialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.XamlRoot
			};
			await dialog.ShowAsync();
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async void CopyRaceID_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Race race)
			return;

		item.IsEnabled = false;
		try
		{
			if (race.Track == null)
				throw new Exception("Track not loaded");

			var package = new DataPackage();
			package.SetText($"{race.Track.ID}/{race.User.ID}-0");
			Clipboard.SetContent(package);
		}
		catch (Exception ex)
		{
			var dialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.XamlRoot
			};
			await dialog.ShowAsync();
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async void RacesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (args.InRecycleQueue ||
			args.Item is not Race race)
			return;

		try
		{
			if (race.CreatedTimestamp is null)
			{
				await race.FetchStats(Session.Rest);
				race.OnPropertyChanged("CreatedTimeAgo");
				race.OnPropertyChanged("CreatedTimestampLoaded");
			}

			if (!race.IsFeaturedLoaded)
			{
				try
				{
					await race.FetchFeatured();
					race.OnPropertyChanged("IsFeatured");
				}
				catch
				{
					// Show error on it "(X) Failed to load"
				}
			}

			if (race.Track != null)
				_ = race.Track.CacheThumbnailAsync();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex.Message);
			// Show error inline
			// (x) Error: ex.Message
		}
	}

	private async Task RefreshRacesAsync()
	{
		var response = await REST.GetAsync<UserPageRacesResponse>(Routes.UserRaces(this.User!.Name));

		Races.Clear();

		var recentlyCompleted = response?.Data;
		if (recentlyCompleted is null)
			return;

		var races = (await Task.WhenAll(
			recentlyCompleted.Select(async track =>
			{
				var response = await REST.GetAsync<APIResponse<RaceEntryData[]>>(
					Routes.RaceData((ulong)track.ID!, this.User!.ID)
				);

				var race = response?.Data?.FirstOrDefault();

				if (race != null)
					race.Track = track;

				return race;
			})
		))
		.OfType<RaceEntryData>()
		.ToArray();

		foreach (var data in races)
			Races.Add(new Race(data));

		_ = UpdateRaceCount((uint)Races.Count);
	}

	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
	{
		using var deferral = args.GetDeferral();
		try
		{
			await RefreshRacesAsync();
		}
		finally
		{
			_refreshCompletion?.TrySetResult();
			_refreshCompletion = null;
		}
	}
}