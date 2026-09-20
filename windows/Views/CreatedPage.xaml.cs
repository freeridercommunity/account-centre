using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Windows.Storage.Pickers;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace AccountCentre.Views;

using AccountCentre.Core;
using Components;
using Models;
using Networking;
using Networking.Responses;

public sealed partial class CreatedPage : Page, IRefreshableView, ISearchableView
{
	public bool CanFilter => HasFeature || HasHidden;
	public bool HasFeature =>
		Tracks.Any(track => track.IsFeatured);
	public bool HasHidden =>
		Tracks.Any(track => track.IsHidden);
	public ObservableCollection<CreatedTrack> Tracks { get; } = [];
	public CreatedPage()
	{
		InitializeComponent();

		_ = RefreshTracksAsync();
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
		TracksGrid.ItemsSource = Tracks;
	}

	public async Task<List<string>?> Search(string query)
	{
		if (query.Length > 0)
		{
			var searchItems = new ObservableCollection<Track>();
			// If query IsFinite, fetch track by ID (could be hidden):
			var hasID = int.TryParse(query, out var id);
			if (query.Length > 3 &&
				hasID &&
				!Tracks.Any(track => track.ID == id))
			{
				try
				{
					var top = await REST.GetAsync<TrackAPIResponse>(Endpoints.TrackData(id));
					if (top != null &&
						top.Result &&
						top?.Data?.Track is not null &&
						top.Data.Track.AuthorID == App.User.ID)
					{
						searchItems.Add(new CreatedTrack(top?.Data?.Track));
					}
				}
				catch
				{
				}
			}

			foreach (var track in Tracks)
			{
				if (string.IsNullOrWhiteSpace(query) ||
					track.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
					(hasID && track.ID == id))
					searchItems.Add(track);
			}

			TracksGrid.ItemsSource = searchItems;
		}

		return null;
	}

	private async void BulkAction_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item)
			return;

		var selected = TracksGrid.SelectedItems;
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
			var tracks = await GetSelectedTrackCodes();
			if (tracks is null)
				return;

			switch (item.Tag)
			{
				case "Copy":
				{
					var files = new List<StorageFile>(tracks.Count);

					foreach (var (name, code) in tracks)
					{
						var directory = Path.Combine(
							AppData.TempDirectory,
							"Clipboard");

						Directory.CreateDirectory(directory);

						var path = Path.Combine(directory, $"{name}.json");

						await File.WriteAllTextAsync(
							path,
							code,
							new UTF8Encoding(false));

						files.Add(
							await StorageFile.GetFileFromPathAsync(path));
					}

					var package = new DataPackage();
					package.SetStorageItems(files);

					Clipboard.SetContent(package);

					BulkActionStatusBar.Severity = InfoBarSeverity.Success;
					BulkActionStatusBar.Message = $"{tracks.Count} tracks copied to clipboard";
					break;
				}

				case "CopyAsZip":
				{
					var directory = Path.Combine(
						AppData.TempDirectory,
						"Clipboard");

					Directory.CreateDirectory(directory);

					var path = Path.Combine(
						directory,
						"tracks.zip");

					await using (var stream = new FileStream(
						path,
						FileMode.Create,
						FileAccess.Write,
						FileShare.None))
					{
						using var archive = new ZipArchive(
							stream,
							ZipArchiveMode.Create);

						foreach (var (name, code) in tracks)
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
					BulkActionStatusBar.Message = $"{tracks.Count} tracks copied to clipboard";
					break;
				}

				case "Download":
				{
					var picker = new FileSavePicker(
						item.XamlRoot.ContentIslandEnvironment.AppWindowId)
					{
						DefaultFileExtension = ".zip",
						SuggestedFileName = "tracks"
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

					foreach (var (name, code) in tracks)
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
					BulkActionStatusBar.Message = $"{tracks.Count} tracks saved";
					break;
				}
			}

			TracksGrid.SelectedItems.Clear();
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

	private async Task<List<(string Name, string Code)>?> GetSelectedTrackCodes()
	{
		var selected = TracksGrid.SelectedItems;
		if (selected.Count == 0)
			return null;

		var tasks = selected
			.OfType<CreatedTrack>()
			.Select<CreatedTrack, Task<(string Name, string Code)?>>(async track =>
			{
				var code = await track.FetchField("Code");

				return code is string text
					? (Name: Utils.SanitizeFileName(track.Title), Code: text)
					: null;
			});

		var results = await Task.WhenAll(tasks);

		var tracks = results
			.Where(x => x is not null)
			.Select(x => x!.Value)
			.ToList();

		if (tracks.Count == 0)
			return null;

		return tracks;
	}

	private void SelectAll_Checked(object sender, RoutedEventArgs e) =>
		TracksGrid.SelectAll();

	private void SelectAll_Unchecked(object sender, RoutedEventArgs e) =>
		TracksGrid.SelectedItems.Clear();

	private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
	{
		var count = TracksGrid.Items.Count;
		var selected = TracksGrid.SelectedItems.Count;

		if (count != selected)
			return;

		TracksGrid.SelectedItems.Clear();
	}

	private void TracksGrid_SelectionChanged(
		object sender,
		SelectionChangedEventArgs e)
	{
		UpdateSelectAllState();
	}

	private void UpdateSelectAllState()
	{
		var count = TracksGrid.Items.Count;
		var selected = TracksGrid.SelectedItems.Count;

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

	private async void ViewTrack_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not CreatedTrack track)
			return;

		await Launcher.LaunchUriAsync(new Uri($"https://frhd.co/t/{track.ID}"));
	}

	private async void CopyTrack_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not CreatedTrack track)
			return;

		try
		{
			var code = await track.FetchField("Code");

			if (code is not string text)
				return;

			var package = new DataPackage();

			switch (item.Tag)
			{
				case "File":
					var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

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
	
	private async void DownloadTrack_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not CreatedTrack track)
			return;

		try
		{
			var code = await track.FetchField("Code");

			if (code is not string text)
				return;

			var picker = new FileSavePicker(item.XamlRoot.ContentIslandEnvironment.AppWindowId)
			{
				CommitButtonText = "Save Track",
				DefaultFileExtension = ".txt",
				SuggestedFileName = track.Title,
				SuggestedFolder = "",
				SuggestedStartLocation = PickerLocationId.Downloads
			};

			picker.FileTypeChoices.Add("Track Files", new List<string>() { ".txt" });

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

	private async void DeleteTrack_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not CreatedTrack track)
			return;

		item.IsEnabled = false;

		try
		{
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
								new Run { Text = "Are you sure you wish to delete " },
								new Hyperlink
								{
									Inlines =
									{
										new Run {
											FontWeight = FontWeights.Bold,
											Text = track.Title
										}
									},
									NavigateUri = new Uri($"http://frhd.co/t/{track.ID}")
								},
								new Run { Text = "?" }
							}
						},
						new InfoBar
						{
							IsClosable = false,
							IsOpen = true,
							Message = "This action can only be reversed by a moderator.",
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
				await track.Delete();
				Tracks.Remove(track);
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

	private async void CopyTrackID_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not CreatedTrack track)
			return;

		item.IsEnabled = false;

		try
		{
			var package = new DataPackage();
			package.SetText(track.ID.ToString());
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

	private async void TracksGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (args.InRecycleQueue ||
			args.Item is not CreatedTrack track)
			return;

		if (track.PublishTimestamp is null)
		{
			await track.FetchField("p_ts");
			track.OnPropertyChanged("PublishedTimeAgo");
			track.OnPropertyChanged("PublishedTimestampLoaded");
		}

		await track.CacheThumbnailAsync();
	}

	private async Task RefreshTracksAsync()
	{
		var response = await REST.GetAsync<UserPageResponse>(App.User!.Name);

		Tracks.Clear();

		var tracks = response?.CreatedTracks?.Data;
		if (tracks is null)
			return;

		foreach (var trackData in tracks)
			Tracks.Add(new CreatedTrack(trackData));

		this.TrackCount.Text = $"{Tracks.Count} tracks";
	}

	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
	{
		using var deferral = args.GetDeferral();

		try
		{
			await RefreshTracksAsync();
		}
		finally
		{
			_refreshCompletion?.TrySetResult();
			_refreshCompletion = null;
		}
	}
}