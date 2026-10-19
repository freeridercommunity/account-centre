using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
// using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
// using Microsoft.Windows.BadgeNotifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.System;

namespace AccountCentre.Windows;

using Components.Dialogs;
using Core;
using Core.Sessions;
using Core.Settings;
using Core.Storage;
using Models;
using Networking;
using Networking.Responses;
using Views;
using Views.Interfaces;

public sealed partial class MainWindow : Window
{
	internal readonly Session Session;
	private ushort Notifications;
	public bool IsDeveloperModeEnabled => SettingsManager.Current.DeveloperMode;
	public MainWindow(Session session)
	{
		this.Session = session;

		InitializeComponent();

		this.ExtendsContentIntoTitleBar = true;
		this.SetTitleBar(this.TitleBar);

		if (AppWindow.Presenter is OverlappedPresenter presenter)
		{
			presenter.PreferredMinimumWidth = 750;
			presenter.PreferredMinimumHeight = 500;
		}

		AppWindow.Resize(new SizeInt32(1000, 650));

		var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
		var workArea = displayArea.WorkArea;

		var x = workArea.X + (workArea.Width - AppWindow.Size.Width) / 2;
		var y = workArea.Y + (workArea.Height - AppWindow.Size.Height) / 2;

		AppWindow.Move(new PointInt32(x, y));

		AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
		// AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

		ContentFrame.Navigated += (_, e) =>
		{
			if (e.Content is ISessionAware sview)
				sview.Session = Session;
		};
		ContentFrame.Navigate(typeof(HomePage), Session.User);

		this.ProfileButton.RegisterPropertyChangedCallback(Button.IsPressedProperty, UpdateProfileButtonPictureScale);
		this.ProfileButton.RegisterPropertyChangedCallback(Button.IsPointerOverProperty, UpdateProfileButtonPictureScale);

		// Call session.FetchUserAsync in InitializeUser
		InitializeUser(session.User!);
	}

	private async void InitializeUser(ClientUser user)
	{
		if (user.AvatarURL != null)
		{
			await user.CacheAvatarAsync();
			// this.PersonPicture.ProfilePicture = new BitmapImage(new Uri(user.DisplayAvatarURL!));
			this.PersonPicture.SetBinding(
				PersonPicture.ProfilePictureProperty,
				new Binding
				{
					Source = user,
					Path = new PropertyPath(nameof(user.DisplayAvatarURL)),
					Mode = BindingMode.OneWay
				});
		}

		try
		{
			this.Notifications = (ushort)await user.FetchNotificationCount(Session.Rest);

			if (this.Notifications > 0)
			{
				this.NotificationInfoBadge.Value = this.Notifications;
				this.NotificationInfoBadge.Visibility = this.Notifications > 0
					? Visibility.Visible
					: Visibility.Collapsed;
			}
		}
		catch
		{
		}
		// this.PersonPicture.BadgeNumber = this.Notifications;
		// BadgeNotificationManager.Current.SetBadgeAsCount(response.Notifications);

		foreach (var userId in SecretStore.EnumerateSecrets("Sessions"))
		{
			try
			{
				// _ = ulong.TryParse(userId, out var userIdLong);

				// var response = await REST.PostAsync<RemoveFriendInferredIDResponse>(
				// 	Endpoints.UserFriendRemove(),
				// 	new
				// 	{
				// 		u_id = userIdLong
				// 	}
				// );
				// var username = response.Username;
				// if (username == null)
				// 	continue;

				var response = await REST.GetAsync<UserPageResponse>(
					Routes.User(userId));
				if (response == null ||
					response.User == null)
					continue;

				var menuItem = new MenuFlyoutItem
				{
					Text = response.User.DisplayName
				};

				if (response.User.ID == user.ID)
					menuItem.Icon = new FontIcon
					{
						Glyph = "\uE930"
					};
				else
					menuItem.Click += async (_, _) =>
					{
						var allowConcurrentSessions = SettingsManager.Current.AllowConcurrentSessions;
						if (!allowConcurrentSessions)
							AppWindow.Hide();

						try
						{
							await App.SwitchAccounts(response.User.DisplayName);
							if (!allowConcurrentSessions)
								this.Close();
						}
						catch
						{
							AppWindow.Show();
						}
					};

				this.SwitchAccounts.Items.Add(menuItem);
			}
			catch
			{
				continue;
			}
		}
	}

	private void UpdateProfileButtonPictureScale(object sender, object e)
		=> SetPictureScale(this.ProfileButton.IsPointerOver ? this.ProfileButton.IsPressed ? 0.825 : 0.925 : 1);

	private void SetPictureScale(double scale)
	{
		Storyboard storyboard = new();
		DoubleAnimation scaleX = new()
		{
			To = scale,
			Duration = TimeSpan.FromMilliseconds(80)
		};
		DoubleAnimation scaleY = new()
		{
			To = scale,
			Duration = TimeSpan.FromMilliseconds(80)
		};

		Storyboard.SetTarget(scaleX, PersonPictureTransform);
		Storyboard.SetTargetProperty(scaleX, "ScaleX");

		Storyboard.SetTarget(scaleY, PersonPictureTransform);
		Storyboard.SetTargetProperty(scaleY, "ScaleY");

		storyboard.Children.Add(scaleX);
		storyboard.Children.Add(scaleY);
		storyboard.Begin();
	}

	private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
		=> this.NavigationView.IsPaneOpen = !this.NavigationView.IsPaneOpen;

	private async void TitleBar_RefreshRequested(object sender, object e)
	{
		if (ContentFrame.Content is not IRefreshableView refreshable)
			return;

		if (sender is not Button button)
			return;

		RefreshIcon.StartSpinning();

		button.IsEnabled = false;

		try
		{
			await refreshable.RefreshAsync(null);
		}
		finally
		{
			button.IsEnabled = true;

			RefreshIcon.ForceStop();
		}
	}

	private async void TitleBar_SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		sender.ItemsSource = null;
		if (ContentFrame.Content is not ISearchableView searchable)
			return;

		if (sender.Text.Length == 0)
			searchable.ClearSearch();

		// if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
		// 	return;

		var items = await searchable.Search(sender.Text);
		if (items is null ||
			items.Count == 0)
			return;

		sender.ItemsSource = items;
	}

	private void TitleBar_SearchSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		if (ContentFrame.Content is not ISearchableView searchable ||
			args.SelectedItem.ToString() is not string selected)
			return;

		searchable.SearchSuggestionChosen(selected);
	}

	private async void OpenProfile_Click(object sender, RoutedEventArgs e)
		=> await Launcher.LaunchUriAsync(new Uri($"http://frhd.co/u/{Session.User!.Name}"));

	private async void OpenSettings_Click(object sender, RoutedEventArgs e)
		=> ContentFrame.Navigate(typeof(SettingsPage), Session.User);

	private async void SwitchAccounts_Click(object sender, RoutedEventArgs e)
	{
		var allowConcurrentSessions = SettingsManager.Current.AllowConcurrentSessions;
		if (!allowConcurrentSessions)
			AppWindow.Hide();

		try
		{
			await App.SwitchAccounts();
			if (!allowConcurrentSessions)
				this.Close();
		}
		catch
		{
			AppWindow.Show();
		}
	}

	private async void Logout_Click(object sender, RoutedEventArgs e)
	{
		await AuthManager.Logout(Session);
		AppWindow?.Hide();
	}

	private async void CopyUserID_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem item)
			return;

		item.IsEnabled = false;

		try
		{
			var package = new DataPackage();
			package.SetText(Session.User!.ID.ToString());
			Clipboard.SetContent(package);
		}
		catch (Exception ex)
		{
			var dialog = new ErrorDialog(ex.Message)
			{
				XamlRoot = this.Content.XamlRoot
			};
			await dialog.ShowAsync();
		}
		finally
		{
			item.IsEnabled = true;
		}
	}

	private async void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
	{
		if (args.InvokedItemContainer is not NavigationViewItem item)
			return;

		if (item.Tag is string url &&
			Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
			!string.IsNullOrEmpty(uri.Host))
		{
			await Launcher.LaunchUriAsync(uri);
			return;
		}

		if (item.Tag is not string key)
			return;

		var viewType = key switch
		{
			"Home" => typeof(HomePage),
			"Created" => typeof(CreatedPage),
			"Friends" => typeof(FriendsPage),
			"Notifications" => typeof(NotificationsPage),
			"Races" => typeof(RacesPage),
			"Settings" => typeof(SettingsPage),
			_ => throw new ArgumentException($"Unknown view: {key}")
		};

		ContentFrame.Navigate(viewType, Session.User);
	}

	private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
	{
		this.RefreshButton.IsEnabled = false;
		this.RefreshIcon.ForceStop();
		this.SearchBar.Text = null;
		this.SearchBar.IsEnabled = false;
	}

	private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
	{
		var view = this.ContentFrame.Content;
		this.RefreshButton.Visibility = view is IRefreshableView
			? Visibility.Visible
			: Visibility.Collapsed;
		this.RefreshButton.IsEnabled = true;
		this.SearchBar.Visibility = view is ISearchableView
			? Visibility.Visible
			: Visibility.Collapsed;
		this.SearchBar.IsEnabled = true;

		if (e.SourcePageType == typeof(NotificationsPage))
		{
			this.NotificationInfoBadge.Value = 0;
			this.NotificationInfoBadge.Visibility = Visibility.Collapsed;
		}

		if (e.SourcePageType == typeof(SettingsPage) &&
			NavigationView.SelectedItem != NavigationView.SettingsItem)
		{
			NavigationView.SelectedItem = NavigationView.SettingsItem;
			return;
		}

		// Select active tab
		string? tag = e.SourcePageType.Name switch
		{
			nameof(HomePage) => "Home",
			nameof(CreatedPage) => "Created",
			nameof(FriendsPage) => "Friends",
			nameof(NotificationsPage) => "Notifications",
			nameof(RacesPage) => "Races",
			_ => null
		};
		if (tag is null)
			return;

		foreach (var item in NavigationView.MenuItems)
		{
			if (item is NavigationViewItem navigationItem &&
				navigationItem.Tag is string itemTag &&
				itemTag == tag)
			{
				if (NavigationView.SelectedItem != item)
					NavigationView.SelectedItem = item;
				break;
			}
		}
	}
}