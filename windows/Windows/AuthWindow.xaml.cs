using AccountCentre.Core;
using AccountCentre.Core.Sessions;
using AccountCentre.Core.Storage;
using AccountCentre.Networking;
using AccountCentre.Networking.Responses;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Graphics;
using Windows.System;

namespace AccountCentre.Windows;

using Models;

public sealed partial class AuthWindow : Window
{
	// public List<User> UserSessions { get; } = [];
	public ObservableCollection<User> UserSessions { get; } = [];
	private readonly TaskCompletionSource<Session?> _authentication =
		new(TaskCreationOptions.RunContinuationsAsynchronously);
	public AuthWindow(string? username = null)
	{
		InitializeComponent();

		this.ExtendsContentIntoTitleBar = true;
		this.SetTitleBar(this.TitleBar);

		if (AppWindow.Presenter is OverlappedPresenter presenter)
		{
			presenter.IsMaximizable = false;
			presenter.IsMinimizable = false;
			presenter.IsResizable = false;
			// presenter.IsModal = true;
			presenter.PreferredMinimumWidth = 350;
			presenter.PreferredMinimumHeight = 200;
		}

		AppWindow.Resize(new SizeInt32(500, 350));

		var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
		var workArea = displayArea.WorkArea;

		var x = workArea.X + (workArea.Width - AppWindow.Size.Width) / 2;
		var y = workArea.Y + (workArea.Height - AppWindow.Size.Height) / 2;

		AppWindow.Move(new PointInt32(x, y));
		// AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

		this.PasswordBox.KeyDown += PasswordBox_KeyDown;

		if (username != null)
		{
			this.UsernameBox.Text = username;
			this.AuthForm.Visibility = Visibility.Visible;
			this.ProgressRing.Visibility = Visibility.Collapsed;
		}
		else
			PopulateUserSessions();

		Closed += (_, _) => _authentication.TrySetResult(null);
	}

	public Task<Session?> WaitForAuthenticationAsync()
		=> _authentication.Task;

	// public new Task<Session?> Activate()
	// {
	// 	base.Activate();
	// 	return WaitForAuthenticationAsync();
	// }

	private async void PopulateUserSessions()
	{
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

				var user = new User(response.User);
				var bytes = SecretStore.Get(userId, "Sessions");
				if (bytes?.Length > 0)
					user.IsAuthenticated = true;

				UserSessions.Add(user);
			}
			catch
			{
				continue;
			}
		}

		this.AuthForm.Visibility = UserSessions.Count > 0
			? Visibility.Collapsed
			: Visibility.Visible;
		this.ContinueAs.Visibility = UserSessions.Count > 0
			? Visibility.Visible
			: Visibility.Collapsed;
		this.ProgressRing.Visibility = Visibility.Collapsed;
	}

	private void UsernameBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		ErrorInfoBar!.IsOpen = false;
		UpdateLoginButtonState();
	}

	private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		this.ErrorInfoBar.IsOpen = false;
		UpdateLoginButtonState();
	}

	private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		if (e.Key != VirtualKey.Enter ||
			!this.LoginButton.IsEnabled)
			return;

		await LoginAsync();
		e.Handled = true;
	}

	private void UpdateLoginButtonState()
		=> this.LoginButton.IsEnabled = this.PasswordBox.Password.Length > 0 && this.UsernameBox.Text.Length >= 3;

	private async void Login_Click(object sender, RoutedEventArgs e)
		=> await LoginAsync();

	private async Task LoginAsync()
	{
		var originalContent = this.LoginButton.Content;

		this.LoginButton.IsEnabled = false;
		this.LoginButton.Content = new ProgressRing
		{
			Height = 14,
			Width = 14
		};

		try
		{
			var session = await AuthManager.Login(this.UsernameBox.Text, this.PasswordBox.Password, (bool)this.RememberMe.IsChecked!);
			_authentication.TrySetResult(session);
			AppWindow.Hide();
		}
		catch (Exception ex)
		{
			this.ErrorInfoBar.Message = ex.Message;
			this.ErrorInfoBar.IsOpen = true;
		}
		finally
		{
			this.LoginButton.Content = originalContent;
		}
	}

	private async void ViewProfile_Click(object sender, RoutedEventArgs e)
	{
		var flyout = (MenuFlyout)SessionsGrid.Resources["UserSessionMenu"];
		if (flyout.Target is not GridViewItem item ||
			item.Content is not User user)
			return;

		await Launcher.LaunchUriAsync(user.ProfileURL);
	}

	private void ForgetSession_Click(object sender, RoutedEventArgs e)
	{
		var flyout = (MenuFlyout)SessionsGrid.Resources["UserSessionMenu"];
		if (flyout.Target is not GridViewItem item ||
			item.Content is not User user)
			return;

		// throw if false?
		if (SecretStore.Delete(user.Name, "Sessions"))
			UserSessions.Remove(user);
	}

	private async void SessionsGrid_ItemClick(object sender, ItemClickEventArgs e)
	{
		if (e.ClickedItem is not User user)
			return;

		var tokenData = user.Name != null ? SecretStore.Get(user.Name, "Sessions") : null;
		var token = tokenData != null ? Encoding.UTF8.GetString(tokenData) : null;
		if (token?.Length > 0)
		{
			// Test token first, if invalid, prompt login!
			// Loading animation?
			try
			{
				var session = await AuthManager.Login(token);
				_authentication.TrySetResult(session);
				AppWindow.Hide();
				return;
			}
			catch (AuthFailedException)
			{
				SecretStore.ClearValue(user.Name!, "Sessions");
			}
		}

		this.UsernameBox.Text = user.DisplayName;
		this.ContinueAs.Visibility = Visibility.Collapsed;
		this.AuthForm.Visibility = Visibility.Visible;
		this.PasswordBox.Focus(FocusState.Programmatic);
	}

	private void SessionsGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
	{
		if (args.InRecycleQueue ||
			args.Item is not User user)
			return;

		_ = user.CacheAvatarAsync();
	}

	private void NotYou_Click(object sender, RoutedEventArgs e)
	{
		this.ContinueAs.Visibility = Visibility.Collapsed;
		this.AuthForm.Visibility = Visibility.Visible;
	}
}