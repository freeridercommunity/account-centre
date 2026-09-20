using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace AccountCentre.Views;

using AccountCentre.Networking.ExtendedAPI;
using Components;
using Core;
using Core.Settings;
using Models;

public sealed partial class SettingsPage : Page
{
	// Bugged: Can't use x:Bind Settings.Property
	private AppSettings Settings => SettingsManager.Current;
	private bool CheckForUpdatesAtStartup
	{
		get => Settings.CheckForUpdatesAtStartup;
		set => Settings.CheckForUpdatesAtStartup = value;
	}

	private bool AutoDownloadUpdates
	{
		get => Settings.AutoDownloadUpdates;
		set => Settings.AutoDownloadUpdates = value;
	}

	private bool DeveloperMode
	{
		get => Settings.DeveloperMode;
		set => Settings.DeveloperMode = value;
	}

	private string UpdateChannel
	{
		get => Settings.UpdateChannel;
		set => Settings.UpdateChannel = value;
	}

	private ClientUser CurrentUser => App.User!;
	private sealed class BuildChannel
	{
		public string Name { get; set; } = "";
		public string Value { get; set; } = "";
		public bool IsEnabled { get; set; } = true;
	}
	private readonly BuildChannel[] UpdateChannels =
	[
		new()
		{
			Name = "Stable",
			Value = "stable"
		},
		new()
		{
			Name = "Beta",
			Value = "beta",
			IsEnabled = false
		}
	];

	public SettingsPage()
	{
		InitializeComponent();

		this.Version.Text = UpdateManager.CurrentVersion.ToString();
		this.CheckForUpdatesCard.Description = $"Last checked: {UpdateManager.LastCheckedText}";

		this.CacheDirectorySize.Text = CacheManager.SizeText;

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

	private async void ChangeUsername_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button)
			return;

		button.IsEnabled = false;

		try
		{
			throw new Exception("Not implemented");

			// var authDialog = new ReauthenticateDialog
			// {
			// 	XamlRoot = this.Content.XamlRoot
			// };

			// var authResult = await authDialog.ShowAsync();
			// if (authResult != ReauthenticateDialogResult.Authenticated)
			// 	return;

			// var newUsername = new TextBox
			// {
			// 	PlaceholderText = "Enter new username"
			// };

			// var confirmNewUsername = new TextBox
			// {
			// 	PlaceholderText = "Re-enter new username"
			// };

			// var stackPanel = new StackPanel
			// {
			// 	Spacing = 8,
			// 	Children =
			// 	{
			// 		newUsername,
			// 		confirmNewUsername
			// 	}
			// };

			// var dialog = new ContentDialog
			// {
			// 	XamlRoot = this.XamlRoot,
			// 	Title = "Change Username",
			// 	Content = stackPanel,
			// 	PrimaryButtonText = "Save",
			// 	IsPrimaryButtonEnabled = false,
			// 	CloseButtonText = "Cancel"
			// };

			// var result = await dialog.ShowAsync();
			// if (result == ContentDialogResult.Primary)
			// {
			// 	// await RESTExtended.PatchAsync(Endpoints.Username());
			// }
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
			var authDialog = new ReauthenticateDialog
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			var password = authDialog.Password;
			var newEmail = new TextBox
			{
				PlaceholderText = "Enter new email"
			};

			var confirmNewEmail = new TextBox
			{
				PlaceholderText = "Re-enter new email"
			};

			var stackPanel = new StackPanel
			{
				Spacing = 8,
				Children =
				{
					newEmail,
					confirmNewEmail
				}
			};

			var dialog = new ContentDialog
			{
				XamlRoot = this.XamlRoot,
				Title = "Change Email",
				Content = stackPanel,
				PrimaryButtonText = "Save",
				IsPrimaryButtonEnabled = false,
				CloseButtonText = "Cancel"
			};

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				await RESTExtended.PatchAsync(
					Endpoints.Email(),
					new
					{
						email = newEmail.Text,
						password
					}
				);
			}
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
			var authDialog = new ReauthenticateDialog
			{
				XamlRoot = this.Content.XamlRoot
			};

			var authResult = await authDialog.ShowAsync();
			if (authResult != ReauthenticateDialogResult.Authenticated)
				return;

			// var password = authDialog.Password;

			var newPassword = new PasswordBox
			{
				PlaceholderText = "Enter new password"
			};

			var confirmNewPassword = new PasswordBox
			{
				PlaceholderText = "Re-enter new password"
			};

			var stackPanel = new StackPanel
			{
				Spacing = 8,
				Children =
				{
					newPassword,
					confirmNewPassword
				}
			};

			var dialog = new ContentDialog
			{
				XamlRoot = this.XamlRoot,
				Title = "Change Password",
				Content = stackPanel,
				PrimaryButtonText = "Save",
				IsPrimaryButtonEnabled = false,
				CloseButtonText = "Cancel"
			};

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				try
				{
					// await change password
				}
				catch // (Exception ex)
				{
					
				}
			}
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
			CacheManager.Clear();
		}
		finally
		{
			button.IsEnabled = true;
		}
	}

	private async void ToggleDeveloperMode_Toggle(object sender, RoutedEventArgs e)
	{
		if (sender is not ToggleSwitch toggle)
			return;

		toggle.IsEnabled = false;

		try
		{
			SettingsManager.Current.DeveloperMode = toggle.IsOn;
			SettingsManager.Save();
		}
		finally
		{
			toggle.IsEnabled = true;
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

	private void UpdateChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not ComboBox comboBox)
			return;

		comboBox.IsEnabled = false;

		try
		{
			SettingsManager.Save();
		}
		finally
		{
			comboBox.IsEnabled = true;
		}
	}

	private void AutoScanUpdates_Toggle(object sender, RoutedEventArgs e)
	{
		if (sender is not ToggleSwitch toggle)
			return;

		toggle.IsEnabled = false;

		try
		{
			SettingsManager.Current.CheckForUpdatesAtStartup = toggle.IsOn;
			SettingsManager.Save();
		}
		finally
		{
			toggle.IsEnabled = true;
		}
	}

	private void AutoDownloadUpdates_Toggle(object sender, RoutedEventArgs e)
	{
		if (sender is not ToggleSwitch toggle)
			return;

		toggle.IsEnabled = false;

		try
		{
			SettingsManager.Current.AutoDownloadUpdates = toggle.IsOn;
			SettingsManager.Save();
		}
		finally
		{
			toggle.IsEnabled = true;
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
		}
		finally
		{
			this.UpdateProgressBar.Visibility = Visibility.Collapsed;
			this.DownloadUpdateButton.IsEnabled = true;
		}
	}
}