using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.System;

namespace AccountCentre.Views;

using AccountCentre.Core.Services;
using Behaviors;
using Components.Dialogs;
using Core.Sessions;
using Interfaces;
using Models;
using Networking;
using Networking.Responses;

public sealed partial class FriendsPage : Page, IRefreshableView, ISessionAware
{
	public Session Session { get; set; } = null!;
	public bool IsCurrentUser { get; private set; } = false;
	public ObservableCollection<Friend> Friends { get; private set; } = [];
	private readonly Dictionary<ulong, ObservableCollection<Friend>> _friendCache = [];
	private User User = null!;
	public FriendsPage()
	{
		this.NavigationCacheMode = NavigationCacheMode.Enabled;
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		if (e.Parameter is not User user)
		{
			// this.User = null;
			return;
		}

		// Check friend requests
		this.User = user;
		this.IsCurrentUser = user.ID == Session.User!.ID;
		// Bindings.Update();

		// Bandaid solution
		// if (Friends.Count == 0)
		// 	_ = RefreshFriendsAsync();

		if (_friendCache.TryGetValue(user.ID, out var cached))
			Friends = cached;
		else
		{
			Friends = [];
			Bindings.Update();

			_friendCache[user.ID] = Friends;

			_ = RefreshFriendsAsync();
		}
	}

	private TaskCompletionSource? _refreshCompletion;
	public async Task RefreshAsync(CancellationToken? cancellationToken)
	{
		_refreshCompletion = new TaskCompletionSource(
			TaskCreationOptions.RunContinuationsAsynchronously);

		this.RefreshContainer.RequestRefresh();

		await _refreshCompletion.Task;
	}

	private async void OpenFriendRequestsDialog_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var dialog = new FriendRequestsDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await dialog.ShowAsync();
		}
		catch (Exception ex)
		{
			ErrorService.Tip(button, ex.Message);
		}
		finally
		{
			button.IsEnabled = true;
		}
	}

	private async void OpenAddFriendDialog_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var dialog = new AddFriendDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await dialog.ShowAsync();
		}
		catch (Exception ex)
		{
			ErrorService.Tip(button, ex.Message);
		}
		finally
		{
			button.IsEnabled = true;
		}
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
			await friend.Remove(Session.Rest);
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
		var response = await REST.GetAsync<UserPageFriendsResponse>(Routes.UserFriends(this.User!.Name));

		Friends.Clear();

		var friends = response?.Data;
		if (friends is null)
			return;

		foreach (var friend in friends)
			Friends.Add(new Friend(friend));

		this.FriendCount.Text = $"{Friends.Count} friends";

		// Friend requests
		// Update nav tab badge count with
		// var friendRequestCount = response.FriendRequests.Data.Length;
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