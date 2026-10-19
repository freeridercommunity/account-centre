using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class UsernameChangeDialog : FormDialog
{
	private readonly TextBox NewUsername = new()
	{
		PlaceholderText = "Enter new username",
		MaxLength = 20
	};
	private readonly TextBox NewUsername2 = new()
	{
		PlaceholderText = "Re-enter new username",
		MaxLength = 20
	};
	private readonly Session Session;
	public UsernameChangeDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Username change failed";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Change Username";

		this.NewUsername.TextChanged += (_, _) => TextChanged();
		this.NewUsername2.TextChanged += (_, _) => TextChanged();

		this.Children.Add(this.NewUsername);
		this.Children.Add(this.NewUsername2);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Save";
	}

	private void TextChanged()
		=> this.IsPrimaryButtonEnabled = NewUsername.Text.Length >= 3 && NewUsername2.Text.Length >= 3;

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();
		try
		{
			this.ToggleLoading(true);

			var response = await Session.Rest.PostAsync<UserUpdatedAPIResponse>(
				Routes.AccountUpdateProfile(),
				new
				{
					name = "u_name",
					value = this.NewUsername.Text
				});
			if (response?.Data != null &&
				response.Data.UpdateUser)
			{
				Session.User!.Name = response.Data.User!.Name;
				Session.User.OnPropertyChanged(nameof(Session.User.Name));
				Session.User.DisplayName = response.Data.User.DisplayName;
				Session.User.OnPropertyChanged(nameof(Session.User.DisplayName));
			}
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