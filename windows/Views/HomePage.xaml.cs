using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AccountCentre.Views;

using Core.Sessions;
using Interfaces;

public sealed partial class HomePage : Page, ISessionAware
{
	public Session Session { get; set; } = null!;
	private string WelcomeText => $"Welcome, {Session.User!.DisplayName}";
	public HomePage()
	{
		this.NavigationCacheMode = NavigationCacheMode.Enabled;

		InitializeComponent();
	}

	private void GridView_ItemClick(object sender, ItemClickEventArgs e)
	{
		if (e.ClickedItem is not FrameworkElement item)
			return;

		switch (item.Tag)
		{
			case "CreatedPage":
				Frame?.Navigate(typeof(CreatedPage), Session.User);
				break;

			case "RacesPage":
				Frame?.Navigate(typeof(RacesPage), Session.User);
				break;

			case "SettingsPage":
				Frame?.Navigate(typeof(SettingsPage), Session.User);
				break;
		}

		// _navigate(item.Tag);
	}

	private void ChangeEmail_Click(object sender, RoutedEventArgs e)
	{
		// Open dialog?
	}

	private void ChangeUsername_Click(object sender, RoutedEventArgs e)
	{
	}

	private void RemoveRace_Click(object sender, RoutedEventArgs e)
	{
	}

	private void RemoveTrack_Click(object sender, RoutedEventArgs e)
	{
	}
}