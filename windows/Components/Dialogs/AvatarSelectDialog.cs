using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class AvatarSelectDialog : FormDialog
{
	private readonly GridView GridView = new()
	{
		HorizontalAlignment = HorizontalAlignment.Center,
		IsItemClickEnabled = true,
		SelectionMode = ListViewSelectionMode.Single
	};
	private readonly Session Session;
	public AvatarSelectDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Avatar selection failed";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Change Avatar";
		this.Children.Add(GridView);

		this.GridView.SelectionChanged += (_, _) => UpdatePrimaryButtonState();

		this.IsPrimaryButtonEnabled = false;
		this.Opened += (_, _) => LoadAvatars();
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Select";
	}

	private async void LoadAvatars()
	{
		var response = await Session.Rest.GetAsync<AccountPageResponse>(Routes.AccountSettings())
			?? throw new Exception("Response returned no data");
		if (response.UserAvatars.Length == 0)
			throw new Exception("No avatars found");

		GridView.Items.Clear();
		foreach (var avatarInfo in response.UserAvatars)
		{
			var profilePicture = new PersonPicture
			{
				ProfilePicture = new BitmapImage(new Uri(avatarInfo.URL)),
				Tag = avatarInfo,
				Margin = new Thickness(12),
				Height = 64,
				Width = 64
			};
			ToolTipService.SetToolTip(profilePicture, avatarInfo.Name);
			GridView.Items.Add(profilePicture);
			if (avatarInfo.Selected)
			{
				GridView.SelectedItem = profilePicture;
				profilePicture.BadgeGlyph = "\uE73E";
			}
		}
	}

	private void UpdatePrimaryButtonState()
		=> this.IsPrimaryButtonEnabled = // ((PersonPicture)this.GridView.SelectedItem).BadgeGlyph == "";
			!((UserPhoto)((PersonPicture)this.GridView.SelectedItem).Tag).Selected;

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();
		try
		{
			if (this.GridView.SelectedItem is not PersonPicture personPicture ||
				personPicture.Tag is not UserPhoto avatarInfo)
				throw new Exception("No avatar selected");

			this.ToggleLoading(true);

			var response = await Session.Rest.PostAsync<UserUpdatedAPIResponse>(
				Routes.AccountSelectAvatar(),
				new
				{
					img_type = avatarInfo.Type
				});

			foreach (var item in GridView.Items
				.Cast<PersonPicture>()
				.Where(x => !string.IsNullOrEmpty(x.BadgeGlyph)))
				item.BadgeGlyph = "";

			personPicture.BadgeGlyph = "\uE73E";

			Session.User!.AvatarURL = avatarInfo.URL;
			Session.User.OnPropertyChanged(nameof(Session.User.AvatarURL));
			await Session.User.CacheAvatarAsync(true);
		}
		catch (Exception ex)
		{
			args.Cancel = true;
			ShowError(ex.Message);
		}
		finally
		{
			this.ToggleLoading(false);
			deferral.Complete();
		}
	}
}