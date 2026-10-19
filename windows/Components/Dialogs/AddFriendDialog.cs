using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Behaviors;
using Core.Sessions;
using Networking.Responses;

public sealed partial class AddFriendDialog : FormDialog
{
	private readonly AutoSuggestBox Username = new()
	{
		PlaceholderText = "Username"
	};
	private readonly Session Session;
	public AddFriendDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Failed to add friend";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Add Friend";

		UserSuggestBehavior.SetEnabled(this.Username, true);

		this.Username.TextChanged += (_, _) => TextChanged();
		this.Children.Add(this.Username);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Send";
	}

	private void TextChanged()
	{
		this.ClearError();
		this.IsPrimaryButtonEnabled = Username.Text.Length >= 3;
	}

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();
		try
		{
			this.ToggleLoading(true);

			var response = await Session.Rest.PostAsync<APIResponse>(
				"friends/send_friend_request",
				new
				{
					u_name = Username.Text
				}
			);

			if (response?.Result is false)
				throw new Exception(response.Message);
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