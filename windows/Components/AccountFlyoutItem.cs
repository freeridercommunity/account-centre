using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AccountCentre.Components;

using Models;

public partial class AccountFlyoutItem : MenuFlyoutItem
{
	public PersonPicture? Picture { get; private set; }
	public TextBlock? NameText { get; private set; }
	public TextBlock? EmailText { get; private set; }
	public ClientUser? User
	{
		get => (ClientUser?)GetValue(UserProperty);
		set => SetValue(UserProperty, value);
	}
	public static readonly DependencyProperty UserProperty =
		DependencyProperty.Register(
			nameof(User),
			typeof(ClientUser),
			typeof(AccountFlyoutItem),
			new PropertyMetadata(null, OnUserChanged));
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		Picture = GetTemplateChild("FlyoutPersonPicture") as PersonPicture;
		NameText = GetTemplateChild("FlyoutName") as TextBlock;
		EmailText = GetTemplateChild("FlyoutEmail") as TextBlock;

		if (User != null)
			SetUser(User);
	}

	public void SetUser(ClientUser user)
	{
		if (NameText != null && user.DisplayName != null)
			NameText.Text = user.DisplayName;
		if (EmailText != null && user.Email != null)
			EmailText.Text = user.Email;
		if (Picture != null && user.AvatarURL != null)
			Picture.ProfilePicture = new BitmapImage(new Uri(user.DisplayAvatarURL!));
	}

	private static void OnUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is AccountFlyoutItem item && e.NewValue is ClientUser user)
			item.SetUser(user);
	}
}