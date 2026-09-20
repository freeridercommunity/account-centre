using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.System;

namespace AccountCentre.Windows;

using Core;
using Networking;
using Views;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		this.ExtendsContentIntoTitleBar = true;
		this.SetTitleBar(this.TitleBar);

		if (AppWindow.Presenter is OverlappedPresenter presenter)
		{
			presenter.PreferredMinimumWidth = 750;
			presenter.PreferredMinimumHeight = 500;
		}

		AppWindow.Resize(new SizeInt32(1000, 650));

		var displayArea = DisplayArea.GetFromWindowId(
			AppWindow.Id,
			DisplayAreaFallback.Nearest);

		var workArea = displayArea.WorkArea;

		var x = workArea.X + (workArea.Width - AppWindow.Size.Width) / 2;
		var y = workArea.Y + (workArea.Height - AppWindow.Size.Height) / 2;

		AppWindow.Move(new PointInt32(x, y));

		AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
		// AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

		RenderView("Home");

		InitializeUser();
	}

	private async void InitializeUser()
	{
		if (App.User!.AvatarURL is not string avatarURL)
			return;

		var profilePicture = await LoadImageAsync(avatarURL);
		this.PersonPicture.ProfilePicture = profilePicture;
	}

	private async void AccountFlyout_Opened(object sender, object e)
	{
		if (AccountHeader.NameText is null ||
			AccountHeader.EmailText is null ||
			AccountHeader.Picture is null ||
			App.User is null)
			return;

		AccountHeader.NameText.Text = App.User?.DisplayName;
		AccountHeader.EmailText.Text = App.User?.Email;
		AccountHeader.Picture.ProfilePicture = this.PersonPicture.ProfilePicture;

		// AccountHeader.UpdateUser(App.User, this.PersonPicture.ProfilePicture);

		// Once
		// AccountFlyout.Opened -= AccountFlyout_Opened;
	}

	private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
	{
		this.NavigationView.IsPaneOpen = !this.NavigationView.IsPaneOpen;
	}

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
		{
			searchable.ClearSearch();
		}

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
		if (ContentFrame.Content is not ISearchableView searchable)
			return;

		searchable.SearchSuggestionChosen(args.SelectedItem.ToString());

	}

	private async void OpenProfile_Click(object sender, RoutedEventArgs e)
	{
		await Launcher.LaunchUriAsync(new Uri($"http://frhd.co/u/{App.User!.Name}"));
	}

	private async void OpenSettings_Click(object sender, RoutedEventArgs e)
	{
		RenderView("Settings");
	}

	private void Logout_Click(object sender, RoutedEventArgs e)
	{
		AuthManager.Logout();
	}

	private async void NavigationView_ItemInvoked(
		NavigationView sender,
		NavigationViewItemInvokedEventArgs args)
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

		RenderView(key);
	}

	private void RenderView(string key)
	{
		this.RefreshButton.IsEnabled = false;
		this.RefreshIcon.ForceStop();
		this.SearchBar.Text = null;
		this.SearchBar.IsEnabled = false;

		var view = GetView(key);

		this.RefreshButton.Visibility = view is IRefreshableView
			? Visibility.Visible
			: Visibility.Collapsed;
		this.RefreshButton.IsEnabled = true;

		this.SearchBar.Visibility = view is ISearchableView
			? Visibility.Visible
			: Visibility.Collapsed;
		this.SearchBar.IsEnabled = true;

		this.ContentFrame.Content = view;
	}

	private readonly Dictionary<string, FrameworkElement> _viewCache = new();
	private FrameworkElement GetView(string key)
	{
		if (_viewCache.TryGetValue(key, out var view))
			return view;

		view = key switch
		{
			"Home" => new HomePage(),
			"Created" => new CreatedPage(),
			"Friends" => new FriendsPage(),
			"Races" => new RacesPage(),
			"Settings" => new SettingsPage(),
			_ => throw new ArgumentException($"Unknown view: {key}")
		};

		_viewCache[key] = view;

		return view;
	}

	private static async Task<BitmapImage> LoadImageAsync(string url)
	{
		var bytes = await REST.GetBytesAsync(url);

		using var stream = new MemoryStream(bytes);

		var image = new BitmapImage();
		await image.SetSourceAsync(stream.AsRandomAccessStream());

		return image;
	}
}