using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
// using Microsoft.Windows.BadgeNotifications;
using System.Diagnostics;

namespace AccountCentre.Views;

using Components.Dialogs;
using Core;
using Core.Sessions;
using Core.Settings;
using Core.Storage;
using Interfaces;
using Models;

public sealed partial class SettingsPage : Page, ISessionAware
{
	public AppSettings Settings => SettingsManager.Current;
	public Session Session { get; set; } = null!;
	private int StartupAccountBehaviorIndex
	{
		get => Settings.StartupAccountBehavior;
		set
		{
			if (value >= 0)
				Settings.StartupAccountBehavior = (ushort)value;
		}
	}
	private int UpdateChannelIndex
	{
		get => Settings.UpdateChannel;
		set
		{
			if (value >= 0)
				Settings.UpdateChannel = (ushort)value;
		}
	}
	public SettingsPage()
	{
		this.NavigationCacheMode = NavigationCacheMode.Enabled;

		InitializeComponent();

		this.Version.Text = UpdateManager.CurrentVersion.ToString();
		this.CheckForUpdatesCard.Description = $"Last checked: {UpdateManager.LastCheckedText}";

		this.CacheDirectorySize.Text = Utils.FormatBytes(CacheStore.Default.Size);

		if (Directory.Exists(AppData.TempDirectory))
		{
			var directory = new DirectoryInfo(AppData.TempDirectory);
			var size = directory
				.EnumerateFiles("*", SearchOption.AllDirectories)
				.Sum(file => file.Length);

			if (size > 0)
			{
				this.TempDirectorySize.Text = Utils.FormatBytes(size);
			}
		}

		// Check if installer downloaded
		if (UpdateManager.IsInstallerDownloaded())
		{
			this.UpdateStatusBar.Severity = InfoBarSeverity.Warning;
			this.UpdateStatusBar.Title = "Update Ready";
			this.UpdateStatusBar.Message = Strings.Get("Update_Ready_Message");
			this.DownloadUpdateButton.Content = "Install";
			this.DownloadUpdateButton.Tag = "InstallWithValidation";
			this.DownloadUpdateButton.Visibility = Visibility.Visible;
		}
		else
			_ = CheckForUpdates();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		if (Session?.User == null)
			return;

		InitializeUserSettings(Session.User);
	}

	private async void InitializeUserSettings(ClientUser user)
	{
		var config = await user.FetchConfig(Session.Rest);
		if (config == null)
			return;

		this.NameChangesRemaining.Text = config.NameChangesRemaining.ToString();
		this.NameChangeButton.IsEnabled = config.NameChangesRemaining > 0;
	}

	private void EditRow_Entered(object sender, PointerRoutedEventArgs e)
	{
		if (sender is Grid grid && grid.Children.LastOrDefault() is UIElement element)
			element.Opacity = 1;
	}

	private void EditRow_Exited(object sender, PointerRoutedEventArgs e)
	{
		if (sender is Grid grid && grid.Children.LastOrDefault() is UIElement element)
			element.Opacity = 0;
	}

	private async void ChangeAvatar_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var avatarDialog = new AvatarSelectDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await avatarDialog.ShowAsync();
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
			button.IsEnabled = true;
		}
	}

	private async void ChangeUsername_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var authDialog = new ReauthenticateDialog(this.Session.Username)
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			var usernameDialog = new UsernameChangeDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await usernameDialog.ShowAsync();
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
			button.IsEnabled = true;
		}
	}

	private async void ChangeEmail_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var authDialog = new ReauthenticateDialog(this.Session.Username)
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			var emailDialog = new EmailChangeDialog(Session)
			{
				Password = authDialog.Password,
				XamlRoot = this.XamlRoot
			};
			await emailDialog.ShowAsync();
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
			button.IsEnabled = true;
		}
	}

	private async void ChangePassword_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var authDialog = new ReauthenticateDialog(this.Session.Username)
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			var passwordDialog = new PasswordChangeDialog(Session)
			{
				Password = authDialog.Password,
				XamlRoot = this.XamlRoot
			};
			await passwordDialog.ShowAsync();
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
			button.IsEnabled = true;
		}
	}

	private async void UpdateForumPassword_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var authDialog = new ReauthenticateDialog(this.Session.Username)
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			var passwordDialog = new ForumPasswordUpdateDialog(Session)
			{
				XamlRoot = this.XamlRoot
			};
			await passwordDialog.ShowAsync();
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
			button.IsEnabled = true;
		}
	}

	private async void ClearSessions_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			var authDialog = new ReauthenticateDialog(this.Session.Username)
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			await Session.RefreshToken(authDialog.Password);
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
			button.IsEnabled = true;
		}
	}

	private async void ClearCache_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			CacheStore.Default.Clear();
		}
		finally
		{
			button.IsEnabled = true;
		}
	}

	private async void ClearTempFiles_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			if (Directory.Exists(AppData.TempDirectory))
				Directory.Delete(AppData.TempDirectory, true);
		}
		finally
		{
			button.IsEnabled = true;
		}
	}

	private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
	{
		await CheckForUpdates();
	}

	private async void DownloadUpdate_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		var originalContent = button.Content;

		button.IsEnabled = false;
		button.Content = new ProgressRing
		{
			Height = 14,
			Width = 14
		};

		switch (button.Tag)
		{
			case "Download":
				await DownloadUpdate();
				break;

			case "Install":
			case "InstallWithValidation":
				var requiresValidation = button.Tag is "InstallWithValidation";
				try
				{
					if (UpdateManager.InstallerPath is not string installerPath)
						return;

					if (requiresValidation)
					{
						var release = await UpdateManager.CheckAsync() ?? throw new Exception("Couldn't find a newer release");
						var installer = UpdateManager.GetInstaller(release) ?? throw new Exception($"Couldn't find the installer for {release.Name} ({release.TagName})");

						this.UpdateStatusBar.Severity = InfoBarSeverity.Warning;
						this.UpdateStatusBar.Title = Strings.Format("Update_Validating_Title", release.Version);
						this.UpdateStatusBar.Message = Strings.Get("Update_Validating_Message");

						await UpdateManager.AssertAssetHashAsync(installerPath, installer.Digest);
					}

					this.UpdateStatusBar.Severity = InfoBarSeverity.Informational;
					this.UpdateStatusBar.Title = Strings.Format("Update_Installing_Title", "Unknown");
					this.UpdateStatusBar.Message = Strings.Get("Update_Installing_Message");

					Process.Start(new ProcessStartInfo
					{
						FileName = installerPath,
						Arguments = $"/SILENT /NOCANCEL /NORESTART /DIR=\"{AppContext.BaseDirectory}\"",
						UseShellExecute = true
					});
					Application.Current.Exit();
				}
				catch (Exception ex)
				{
					this.UpdateStatusBar.Severity = InfoBarSeverity.Error;
					this.UpdateStatusBar.Title = "Installation Failed";
					this.UpdateStatusBar.Message = ex.Message;

					if (requiresValidation)
					{
						button.Content = "Redownload";
						button.Tag = "Download";
					}
					else
						button.Content = "Retry";
				}
				finally
				{
					button.IsEnabled = true;
				}
				break;
		}
	}

	private async Task CheckForUpdates()
	{
		this.CheckForUpdatesButton.IsEnabled = false;
		// this.UpdateProgressBar.Visibility = Visibility.Visible;

		try
		{
			var release = await UpdateManager.CheckAsync();

			this.CheckForUpdatesCard.Description = $"Last checked: {UpdateManager.LastCheckedText}";

			if (release == null)
			{
				this.CheckForUpdatesButton.IsEnabled = true;
				return;
			}

			this.UpdateStatusBar.Severity = InfoBarSeverity.Warning;
			this.UpdateStatusBar.Title = Strings.Format("Update_Available_Title", release.Version);
			this.UpdateStatusBar.Message = Strings.Get("Update_Available_Message");
			this.DownloadUpdateButton.Content = "Download";
			this.DownloadUpdateButton.Tag = "Download";
			this.DownloadUpdateButton.Visibility = Visibility.Visible;

			if (SettingsManager.Current.AutoDownloadUpdates)
			{
				var installer = UpdateManager.GetInstaller(release);
				await DownloadUpdate();
			}
		}
		catch
		{
			// Show error in dialog
			this.CheckForUpdatesButton.IsEnabled = true;
		}
		finally
		{
			// this.UpdateProgressBar.Visibility = Visibility.Collapsed;
		}
	}

	private async Task DownloadUpdate()
	{
		this.DownloadUpdateButton.IsEnabled = false;
		this.DownloadUpdateButton.Content = new ProgressRing
		{
			Height = 14,
			Width = 14
		};
		// BadgeNotificationManager.Current.SetBadgeAsGlyph(BadgeNotificationGlyph.Activity);

		try
		{
			var release = await UpdateManager.CheckAsync() ?? throw new Exception("Couldn't find a newer release");
			var installer = UpdateManager.GetInstaller(release) ?? throw new Exception($"Couldn't find the installer for {release.Name} ({release.TagName})");

			var cached = await UpdateManager.GetCachedInstallerAsync(installer);
			if (cached == null)
			{
				this.UpdateStatusBar.Severity = InfoBarSeverity.Informational;
				this.UpdateStatusBar.Title = null; // Strings.Format("Update_Downloading_Title", release.Version);
				this.UpdateStatusBar.Message = Strings.Format("Update_Downloading_Message", Utils.FormatBytes(installer.Size));
				this.UpdateProgressBar.Visibility = Visibility.Visible;

				var path = await UpdateManager.DownloadAssetAsync(installer);

				// Single-pass validation in UpdateManager.DownloadAssetAsync
				// this.UpdateStatusBar.Severity = InfoBarSeverity.Warning;
				// this.UpdateStatusBar.Message = $"Validating hash…";

				// await UpdateManager.VerifyAssetHashAsync(path, installer.Digest);
			}

			this.DownloadUpdateButton.Content = "Install";
			this.DownloadUpdateButton.Tag = "Install";

			this.UpdateStatusBar.Severity = InfoBarSeverity.Warning;
			this.UpdateStatusBar.Title = Strings.Get("Update_Ready_Title");
			this.UpdateStatusBar.Message = Strings.Format("Update_Ready_Message", release.Version);
		}
		catch (Exception ex)
		{
			this.UpdateStatusBar.Severity = InfoBarSeverity.Error;
			this.UpdateStatusBar.Title = "Download Failed";
			this.UpdateStatusBar.Message = ex.Message;

			this.DownloadUpdateButton.Content = "Retry";
			// BadgeNotificationManager.Current.SetBadgeAsGlyph(BadgeNotificationGlyph.Error);
		}
		finally
		{
			this.UpdateProgressBar.Visibility = Visibility.Collapsed;
			this.DownloadUpdateButton.IsEnabled = true;
			// BadgeNotificationManager.Current.ClearBadge();
		}
	}
}