using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace AccountCentre.Windows;

using Core;
using Networking;
using Networking.Responses;

public sealed partial class AuthWindow : Window
{
	private readonly TaskCompletionSource<bool> _authentication =
		new(TaskCreationOptions.RunContinuationsAsynchronously);
	public AuthWindow()
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

		var displayArea = DisplayArea.GetFromWindowId(
			AppWindow.Id,
			DisplayAreaFallback.Nearest);

		var workArea = displayArea.WorkArea;

		var x = workArea.X + (workArea.Width - AppWindow.Size.Width) / 2;
		var y = workArea.Y + (workArea.Height - AppWindow.Size.Height) / 2;

		AppWindow.Move(new PointInt32(x, y));
		// AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

		this.PasswordBox.KeyDown += PasswordBox_KeyDown;

		Closed += (_, _) =>
			_authentication.TrySetResult(false);
	}

	public Task<bool> WaitForAuthenticationAsync()
	{
		return _authentication.Task;
	}

	private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		this.ErrorInfoBar!.IsOpen = false;

		UpdateLoginButtonState();

		if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
			return;

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
	}

	private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		this.ErrorInfoBar!.IsOpen = false;
		UpdateLoginButtonState();
	}

	private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		if (e.Key != VirtualKey.Enter ||
			!this.LoginButton!.IsEnabled)
			return;

		await LoginAsync();
		e.Handled = true;
	}

	private void UpdateLoginButtonState()
	{
		this.LoginButton!.IsEnabled = this.PasswordBox!.Password.Length > 0 && this.UsernameBox!.Text.Length >= 3;
	}

	private async void Login_Click(object sender, RoutedEventArgs e)
	{
		await LoginAsync();
	}

	private async Task LoginAsync()
	{
		var originalContent = this.LoginButton!.Content;

		this.LoginButton.IsEnabled = false;
		this.LoginButton.Content = new ProgressRing
		{
			Height = 14,
			Width = 14
		};

		try
		{
			await AuthManager.Login(this.UsernameBox!.Text, this.PasswordBox!.Password);

			_authentication.TrySetResult(true);
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
}