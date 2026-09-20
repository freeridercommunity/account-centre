using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.System;

namespace AccountCentre.Views;

using AccountCentre.Core;
using Components;
using Models;
using Networking;
using Networking.Responses;

public sealed partial class FriendsPage : Page, IRefreshableView
{
	public ObservableCollection<Friend> Friends { get; } = [];
	public FriendsPage()
	{
		InitializeComponent();

		_ = RefreshFriendsAsync();
	}

	private TaskCompletionSource? _refreshCompletion;
	public async Task RefreshAsync(CancellationToken? cancellationToken)
	{
		_refreshCompletion = new TaskCompletionSource(
			TaskCreationOptions.RunContinuationsAsynchronously);

		this.RefreshContainer.RequestRefresh();

		await _refreshCompletion.Task;
	}

	private async void OpenAddFriendDialog_Click(object sender, RoutedEventArgs e)
	{
		ContentDialog dialog = new ContentDialog
		{
			XamlRoot = this.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "Add Friend",
			PrimaryButtonText = "Send",
			IsPrimaryButtonEnabled = false,
			CloseButtonText = "Cancel",
			DefaultButton = ContentDialogButton.Primary
		};

		// var input = new TextBox
		var input = new AutoSuggestBox
		{
			PlaceholderText = "Username"
		};

		var infoBar = new InfoBar
		{
			IsOpen = false,
			Severity = InfoBarSeverity.Error,
			Title = "Unable to send friend request"
		};

		input.SuggestionChosen += (_, _) =>
		{
			dialog.IsPrimaryButtonEnabled = true;
		};

		input.TextChanged += async (AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
		{
			infoBar.IsOpen = false;
			dialog.IsPrimaryButtonEnabled = input.Text.Length >= 3;

			if (sender.Text.Length < 3)
			{
				sender.ItemsSource = null;
				return;
			}

			var suggestions = new List<string>();
			try
			{
				var results = await Search.User(sender.Text);

				foreach (var data in results)
					suggestions.Add(data.DisplayName);
			}
			finally
			{
				sender.ItemsSource = suggestions;
			}
		};

		dialog.Content = new StackPanel
		{
			Spacing = 8,
			Children =
			{
				input,
				infoBar
			}
		};

		dialog.PrimaryButtonClick += async (_, args) =>
		{
			dialog.IsPrimaryButtonEnabled = false;

			var username = input.Text.Trim();
			if (username.Length < 3)
			{
				args.Cancel = true;

				infoBar.Message = "Username must be 3 characters or greater";
				infoBar.IsOpen = true;
				return;
			}

			var deferral = args.GetDeferral();

			try
			{
				var response = await REST.PostAsync<APIResponse>(
					"friends/send_friend_request",
					new
					{
						u_name = username
					}
				);

				if (response?.Result is false)
					throw new Exception(response.Message);
			}
			catch (Exception ex)
			{
				args.Cancel = true;

				infoBar.Message = ex.Message;
				infoBar.IsOpen = true;
			}
			finally
			{
				deferral.Complete();
			}
		};

		await dialog.ShowAsync();
		// var result = await dialog.ShowAsync();

		// if (result == ContentDialogResult.Primary)
		// {
		// 	FriendRequestSentInfoBar.IsOpen = true;

		// 	_ = Task.Delay(3000).ContinueWith(_ =>
		// 	{
		// 		DispatcherQueue.TryEnqueue(() =>
		// 			FriendRequestSentInfoBar.IsOpen = false
		// 		);
		// 	});
		// }
	}

	private async void ViewProfile_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Friend friend)
			return;

		await Launcher.LaunchUriAsync(new Uri($"https://frhd.co/u/{friend.Name}"));
	}

	private async void RemoveFriend_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Friend friend)
			return;

		item.IsEnabled = false;

		try
		{
			await friend.Remove();
			Friends.Remove(friend);
			// Clear friend-related cache entries
		}
		catch (Exception ex)
		{
			var errorDialog = new ErrorDialog(ex.Message);
			await errorDialog.ShowAsync();
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async Task RefreshFriendsAsync()
	{
		var response = await REST.GetAsync<UserPageResponse>(App.User!.Name);

		Friends.Clear();

		var friends = response?.Friends?.Data;
		if (friends is null)
			return;

		foreach (var friend in friends)
			Friends.Add(new Friend(friend));

		this.FriendCount.Text = $"{Friends.Count} friends";
	}

	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
	{
		using var deferral = args.GetDeferral();

		try
		{
			await RefreshFriendsAsync();
		}
		finally
		{
			_refreshCompletion?.TrySetResult();
			_refreshCompletion = null;
		}
	}
}