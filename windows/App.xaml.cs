using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace AccountCentre;

using Core;
using Core.Settings;
using Models;
using Networking;
using Networking.Models;
using Networking.Responses;
using Windows;

public partial class App : Application
{
	public static readonly string IconPath = Path.Combine(
		AppContext.BaseDirectory,
		"Assets",
		"app.ico"
	);

	public static ClientUser? User { get; internal set; }

	private Window? authWindow;
	private Window? window;

	public App()
	{
		InitializeComponent();

		SettingsManager.Load();

		// ProtocolRegistration.Register();

		AuthManager.LogOut += (_, _) => HandleLogout();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		if (SettingsManager.Current.CheckForUpdatesAtStartup)
		{
			_ = UpdateManager.CheckAsync();
		}

		await TriggerLoginFlow();
	}

	private async Task TriggerLoginFlow()
	{
		if (User is null)
		{
			var token = TokenSecret.Get();

			// token = null;

			if (token == null)
			{
				var authWindow = new AuthWindow();
				authWindow.AppWindow.SetIcon(IconPath);
				authWindow.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
				authWindow.Activate();

				var authenticated = await authWindow.WaitForAuthenticationAsync();

				if (!authenticated)
					return;
 
				token = TokenSecret.Get();
				if (token == null)
					return;

				this.authWindow = authWindow;
			}

			await AuthManager.Initialize();
			// AuthManager.Init replace REST.SetToken
			// REST.SetToken(token);
			await InitializeUser();
		}

		window = new MainWindow();
		window.AppWindow.SetIcon(IconPath);
		window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
		window.Activate();

		if (authWindow != null)
		{
			authWindow.Close();
			authWindow = null;
		}
	}

	private async Task InitializeUser()
	{
		var response = await REST.GetAsync<AccountPageResponse>(Endpoints.Account());
		if (response?.User is not ClientUserData user)
			return;

		User = new ClientUser(user);
	}

	private void HandleLogout()
	{
		User = null;

		_ = TriggerLoginFlow();

		if (window != null)
		{
			window?.Close();
			window = null;
		}
	}
}