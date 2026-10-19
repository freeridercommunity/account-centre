using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Core.Services;
using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class NotificationSettingsDialog : ContentDialog
{
	private readonly StackPanel StackPanel = new()
	{
		Spacing = 4
	};
	private readonly Session Session;
	public NotificationSettingsDialog(Session session)
	{
		this.Session = session;

		this.Title = "Notification Settings";
		this.Content = new ScrollView
		{
			Content = this.StackPanel
		};

		this.CloseButtonText = "Close";
		this.Opened += (_, _) => LoadSettings();
	}

	private async void LoadSettings()
	{
		var response = await Session.Rest.GetAsync<NotificationSettingsPageResponse>(Routes.NotificationSettings())
			?? throw new Exception("Response returned no data");
		if (response == null)
			throw new Exception("No settings found");

		this.StackPanel.Children.Clear();
		// foreach (var setting in response.AppSettings)
		// {
		// 	var appSetting = setting;
		// 	var emailSetting = response.EmailSettings[response.AppSettings.IndexOf(setting)];
		// 	var comboBox = new ComboBox
		// 	{
		// 		SelectedIndex = appSetting.Enabled ? emailSetting.Enabled ? 3 : 1 : emailSetting.Enabled ? 2 : 0,
		// 		Items =
		// 		{
		// 			new ComboBoxItem
		// 			{
		// 				Content = "Off"
		// 			},
		// 			new ComboBoxItem
		// 			{
		// 				Content = "App only"
		// 			},
		// 			new ComboBoxItem
		// 			{
		// 				Content = "Email only"
		// 			},
		// 			new ComboBoxItem
		// 			{
		// 				Content = "App and email"
		// 			}
		// 		}
		// 	};

		// 	var settingCard = new SettingsCard
		// 	{
		// 		Header = setting.Description,
		// 		Content = comboBox
		// 	};

		// 	this.StackPanel.Children.Add(settingCard);
		// }

		if (response.AppSettings.Length > 0)
		{
			var settingsExpander = new SettingsExpander
			{
				Header = "App Notifications",
				Tag = "ingame"
			};

			foreach (var setting in response.AppSettings)
			{
				var toggleSwitch = new ToggleSwitch
				{
					Tag = setting.Type,
					IsOn = setting.Enabled
				};

				toggleSwitch.Toggled += ToggleSwitch_Toggled;

				var settingsCard = new SettingsCard
				{
					Header = setting.Description,
					Content = toggleSwitch
				};

				settingsExpander.Items.Add(settingsCard);
			}

			this.StackPanel.Children.Add(settingsExpander);
		}

		if (response.EmailSettings.Length > 0)
		{
			var settingsExpander = new SettingsExpander
			{
				Header = "Email Notifications",
				Tag = "email"
			};

			foreach (var setting in response.EmailSettings)
			{
				var toggleSwitch = new ToggleSwitch
				{
					Tag = setting.Type,
					IsOn = setting.Enabled
				};

				toggleSwitch.Toggled += ToggleSwitch_Toggled;

				var settingsCard = new SettingsCard
				{
					Header = setting.Description,
					Content = toggleSwitch
				};

				settingsExpander.Items.Add(settingsCard);
			}

			this.StackPanel.Children.Add(settingsExpander);
		}
	}

	private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs args)
	{
		if (sender is not ToggleSwitch toggleSwitch)
			return;

		toggleSwitch.IsEnabled = false;
		toggleSwitch.Toggled -= ToggleSwitch_Toggled;

		try
		{
			var settingsExpander = toggleSwitch.FindAscendant<SettingsExpander>();
			if (settingsExpander?.Tag is not string)
				throw new Exception("Failed to parse settings category");

			await Session.Rest.PostAsync(Routes.NotificationSettingsUpdate((string)settingsExpander.Tag, (string)toggleSwitch.Tag, toggleSwitch.IsOn));
		}
		catch (Exception ex)
		{
			toggleSwitch.IsOn = !toggleSwitch.IsOn;
			ErrorService.Tip(toggleSwitch, ex.Message);
		}
		finally
		{
			toggleSwitch.Toggled += ToggleSwitch_Toggled;
			toggleSwitch.IsEnabled = true;
		}
	}
}