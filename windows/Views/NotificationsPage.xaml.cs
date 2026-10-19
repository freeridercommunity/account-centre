using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Text;
using System.Collections.ObjectModel;
using Windows.System;

namespace AccountCentre.Views;

using Components.Dialogs;
using Core.Services;
using Core.Sessions;
using Interfaces;
using Models;
using Networking;
using Networking.Models;
using Networking.Responses;

public sealed partial class NotificationsPage : Page, IRefreshableView, ISessionAware
{
	public Session Session { get; set; } = null!;
	public ObservableCollection<Notification> Notifications { get; private set; } = [];
	public NotificationsPage()
	{
		this.NavigationCacheMode = NavigationCacheMode.Enabled;

		InitializeComponent();

		_ = RefreshNotificationsAsync();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		if (Session?.User == null)
			return;

		InitializeUser(Session.User);
	}

	private async void InitializeUser(ClientUser user)
	{
		var notificationCount = await user.FetchNotificationCount(Session.Rest);
		this.NotificationCount.Text = notificationCount.ToString();

		_ = RefreshNotificationsAsync();
	}

	private TaskCompletionSource? _refreshCompletion;
	public async Task RefreshAsync(CancellationToken? cancellationToken)
	{
		_refreshCompletion = new TaskCompletionSource(
			TaskCreationOptions.RunContinuationsAsynchronously);

		this.RefreshContainer.RequestRefresh();

		await _refreshCompletion.Task;
	}

	private void NotificationText_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is not TextBlock textBlock ||
			textBlock.DataContext is not Notification notification)
			return;

		textBlock.Loaded -= NotificationText_Loaded;
		textBlock.Inlines.Clear();

		if (notification.User is not null)
			textBlock.Inlines.Add(new Hyperlink
			{
				FontWeight = FontWeights.SemiBold,
				NavigateUri = new Uri($"https://frhd.co/u/{notification.User.Name}"),
				UnderlineStyle = UnderlineStyle.None,
				Inlines =
				{
					new Run
					{
						Text = notification.User.DisplayName
					}
				}
			});

		textBlock.Inlines.Add(new Run
		{
			Text = $"{(notification.User is not null ? ' ' : "")}{notification.Text}{(notification.Track is not null ? ' ' : "")}"
		});

		if (notification.Track is not null)
			textBlock.Inlines.Add(new Hyperlink
			{
				FontWeight = FontWeights.SemiBold,
				NavigateUri = new Uri($"https://frhd.co/t/{notification.Track.ID}{(
					notification.Type == NotificationType.FriendLeaderboardPassed ||
					notification.Type == NotificationType.FriendRaceChallenge ||
					notification.Type == NotificationType.TrackLeaderboardPassed
					? $"/r/{notification.User!.Name}"
					: ""
				)}"), // If race, add /r/
				UnderlineStyle = UnderlineStyle.None,
				Inlines =
				{
					new Run
					{
						Text = notification.Track.Title
					}
				}
			});
	}

	private void NotificationActionRow_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is not StackPanel stackPanel ||
			stackPanel.DataContext is not Notification notification)
			return;

		stackPanel.Loaded -= NotificationActionRow_Loaded;
		stackPanel.Children.Clear();

		switch (notification.Type)
		{
			case NotificationType.FriendLeaderboardPassed:
			case NotificationType.FriendRaceChallenge:
			case NotificationType.TrackLeaderboardPassed:
				stackPanel.Children.Add(new HyperlinkButton
				{
					Content = "Race",
					NavigateUri = new Uri($"https://frhd.co/t/{notification.Track!.ID}/r/{notification.User!.Name}")
				});
				break;

			case NotificationType.FriendRequestReceived:
				// Add check icon
				var accept = new Button
				{
					Content = "Accept",
					IsEnabled = false
				};

				accept.Click += async (_, _) =>
				{
					throw new NotImplementedException();
					// await Session.Rest.PostAsync(Routes.FriendRequestAccept());
				};

				var reject = new Button
				{
					Content = "Reject",
					Style = (Style)Application.Current.Resources["SubtleButtonStyle"],
					IsEnabled = false
				};

				reject.Click += (_, _) =>
				{
					throw new NotImplementedException();
					// await Session.Rest.PostAsync(Routes.FriendRequestReject()));
				};

				stackPanel.Children.Add(accept);
				stackPanel.Children.Add(reject);
				break;

			case NotificationType.TrackCommentMention:
				// Open dialog instead?
				stackPanel.Children.Add(new HyperlinkButton
				{
					Content = "View comment",
					NavigateUri = new Uri($"https://frhd.co/t/{notification.Track!.ID}?c_id={notification.TrackComment!.ID}")
				});
				break;

			case NotificationType.SubscribedTrackPublish:
				stackPanel.Children.Add(new HyperlinkButton
				{
					Content = "View track",
					NavigateUri = new Uri($"https://frhd.co/t/{notification.Track!.ID}")
				});
				break;
		}
	}

	private async void OpenSettingsDialog_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var settingsDialog = new NotificationSettingsDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await settingsDialog.ShowAsync();
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

	private async void DisableNotificationType_Click(object sender, RoutedEventArgs e)
	{
		var flyout = (MenuFlyout)this.NotificationView.Resources["NotificationMenu"];
		if (flyout.Target is not ListViewItem item ||
			item.Content is not Notification notification)
			return;

		item.IsEnabled = false;

		try
		{
			string notificationType = notification.Type switch
			{
				NotificationType.FriendLeaderboardPassed => "friend_lb_passed",
				NotificationType.FriendRaceChallenge => "friend_t_challenge",
				NotificationType.FriendRequestAccepted => "friend_req_accptd",
				NotificationType.FriendRequestReceived => "friend_req_rcvd",
				NotificationType.SubscribedTrackPublish => "subscribed_t_publish",
				NotificationType.TrackCommentMention => "t_uname_mention",
				NotificationType.TrackLeaderboardPassed => "track_lb_passed",
				_ => throw new NotImplementedException()
			};

			await Session.Rest.PostAsync(Routes.NotificationSettingsUpdate("ingame", notificationType, false));
			foreach (var n in this.Notifications
				.Where(n => n.Type == notification.Type)
				.ToList())
				Notifications.Remove(n);
		}
		catch (Exception ex)
		{
			ErrorService.Tip(item, ex.Message);
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async void ViewProfile_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Notification notification)
			return;

		await Launcher.LaunchUriAsync(new Uri($"https://frhd.co/u/{notification.User!.Name}"));
	}

	private async void RemoveFriend_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item ||
			item.DataContext is not Notification notification)
			return;

		item.IsEnabled = false;

		try
		{
			await Session.Rest.PostAsync(
				Routes.UserFriendRemove(),
				new
				{
					u_id = notification.User!.ID
				}
			);
		}
		catch (Exception ex)
		{
			var errorDialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.XamlRoot
			};
			await errorDialog.ShowAsync();
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async Task RefreshNotificationsAsync()
	{
		var response = await Session.Rest.GetAsync<NotificationsPageResponse>(Routes.Notifications());

		Notifications.Clear();

		var notificationDays = response?.NotificationDays;
		if (notificationDays is null)
			return;

		// Exclude disabled notification types
		foreach (var day in notificationDays)
			foreach (var notification in day.Notifications)
				Notifications.Add(new Notification(notification));

		this.NotificationCount.Text = $"{Notifications.Count} notifications";
	}

	private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
	{
		using var deferral = args.GetDeferral();

		try
		{
			await RefreshNotificationsAsync();
		}
		finally
		{
			_refreshCompletion?.TrySetResult();
			_refreshCompletion = null;
		}
	}
}