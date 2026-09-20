using AccountCentre.Networking.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AccountCentre.Components;

public class AccountFlyoutItem : MenuFlyoutItem
{
	public PersonPicture? Picture { get; private set; }
	public TextBlock? NameText { get; private set; }
	public TextBlock? EmailText { get; private set; }

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		Picture = GetTemplateChild("FlyoutPersonPicture") as PersonPicture;
		NameText = GetTemplateChild("FlyoutName") as TextBlock;
		EmailText = GetTemplateChild("FlyoutEmail") as TextBlock;
	}

	public void UpdateUser(ClientUserData user, BitmapImage profilePicture)
	{
		NameText?.Text = user.DisplayName;
		EmailText?.Text = user.Email;

		if (Picture is not null)
			Picture.ProfilePicture = profilePicture;
	}
}