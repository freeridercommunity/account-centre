using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Text;

namespace AccountCentre;

using Core;
using Core.Sessions;
using Core.Settings;
using Core.Storage;
using Windows;

public partial class App : Application
{
	public static readonly string IconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
	public App()
	{
		InitializeComponent();

		SettingsManager.Load();
		// ProtocolRegistration.Register();
		AuthManager.LoggedOut += (_, _) => HandleLogout();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		if (SettingsManager.Current.CheckForUpdatesAtStartup)
			_ = UpdateManager.CheckAsync();

		if (SettingsManager.Current.StartupAccountBehavior == 0)
		{
			// Get most recently modified file
			var key = SecretStore.EnumerateSecrets("Sessions")?.FirstOrDefault();
			var tokenData = key != null ? SecretStore.Get(key, "Sessions") : null;
			var token = tokenData != null ? Encoding.UTF8.GetString(tokenData) : null;
			if (token != null)
			{
				// Add try/catch block
				try
				{
					var session = await AuthManager.Login(token);
					CreateMainWindow(session);
					return;
				}
				catch (AuthFailedException)
				{
					SecretStore.ClearValue(key!, "Sessions");
				}
			}
		}

		await TriggerLoginFlow();
	}

	public static async Task SwitchAccounts(string? username = null)
	{
		if (username != null)
		{
			var tokenData = SecretStore.Get(username, "Sessions");
			if (tokenData != null &&
				Encoding.UTF8.GetString(tokenData) is string token)
			{
				var session = await AuthManager.Login(token);
				if (session != null)
				{
					CreateMainWindow(session);
					return;
				}
			}
		}

		var authWindow = CreateAuthWindow(username);
		var cancelled = await TriggerLoginFlow(authWindow);
		if (cancelled)
			throw new Exception("Operation cancelled");
	}

	private static void HandleLogout()
		=> _ = TriggerLoginFlow();

	private static async Task<bool> TriggerLoginFlow(AuthWindow? authWindow = null)
	{
		authWindow ??= CreateAuthWindow();

		var session = await authWindow.WaitForAuthenticationAsync();
		if (session == null)
			return true;

		CreateMainWindow(session);
		if (authWindow != null)
		{
			authWindow.Close();
			authWindow = null;
		}

		return false;
	}

	private static AuthWindow? authWindow;
	private static AuthWindow CreateAuthWindow(string? username = null)
	{
		if (authWindow == null)
		{
			authWindow = new(username);
			authWindow.AppWindow.SetIcon(IconPath);
			authWindow.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
			authWindow.Closed += (_, _) => authWindow = null;
		}

		authWindow.Activate();
		return authWindow;
	}

	private static MainWindow CreateMainWindow(Session session)
	{
		MainWindow window = new(session);
		window.AppWindow.SetIcon(IconPath);
		window.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

		session.Invalidated += (_, _) =>
		{
			CreateAuthWindow();
			window.Close();
			// _ = TriggerLoginFlow(authWindow);
		};

		window.Activate();
		return window;
	}
}