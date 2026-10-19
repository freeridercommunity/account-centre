using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class ForumPasswordUpdateDialog : FormDialog
{
	private readonly PasswordBox NewPassword = new()
	{
		PlaceholderText = "Enter new password"
	};
	private readonly Session Session;
	public ForumPasswordUpdateDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Forum password update failed";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Update Forum Password";

		this.NewPassword.PasswordChanged += (_, _) => UpdatePrimaryButtonState();
		this.Children.Add(this.NewPassword);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Save";
	}

	private void UpdatePrimaryButtonState()
		=> this.IsPrimaryButtonEnabled = this.NewPassword.Password.Length > 0;

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();
		try
		{
			this.ToggleLoading(true);

			var response = await Session.Rest.PostAsync<AuthAPIResponse>(
				Routes.ForumAccountUpdate(),
				new
				{
					password = this.NewPassword.Password
				}) ?? throw new Exception("Response returned no data");
			if (response?.Result == false)
				throw new Exception(response?.Message ?? "Failed to change password");
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