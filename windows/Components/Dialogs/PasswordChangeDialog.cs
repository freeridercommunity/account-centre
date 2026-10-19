using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class PasswordChangeDialog : FormDialog
{
	private readonly PasswordBox NewPassword = new()
	{
		PlaceholderText = "Enter new password"
	};
	private readonly PasswordBox NewPassword2 = new()
	{
		PlaceholderText = "Re-enter new password"
	};
	public string Password { private get; init; } = null!;
	private readonly Session Session;
	public PasswordChangeDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Password change failed";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Change Password";

		this.NewPassword.PasswordChanged += (_, _) => UpdatePrimaryButtonState();
		this.NewPassword2.PasswordChanged += (_, _) => UpdatePrimaryButtonState();

		this.Children.Add(this.NewPassword);
		this.Children.Add(this.NewPassword2);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Save";
	}

	private void UpdatePrimaryButtonState()
		=> this.IsPrimaryButtonEnabled = NewPassword.Password.Length > 0 && NewPassword2.Password.Length > 0;

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		if (this.NewPassword.Password != this.NewPassword2.Password)
		{
			args.Cancel = true;

			var teachingTip = new TeachingTip
			{
				Title = "Password mismatch",
				Subtitle = "Passwords don't match. Please try again",
				Target = this.NewPassword2,
				IsLightDismissEnabled = true,
				IsOpen = true
			};
			teachingTip.Closed += (_, _) =>
			{
				if (this.Children.Contains(teachingTip))
					this.Children.Remove(teachingTip);
			};
			this.Children.Add(teachingTip);
			return;
		}
		else if (this.NewPassword.Password == this.Password)
		{
			args.Cancel = true;

			var teachingTip = new TeachingTip
			{
				Title = "New password is the same as current",
				Subtitle = "Your new password must be different from your current password",
				Target = this.NewPassword,
				IsLightDismissEnabled = true,
				IsOpen = true
			};
			teachingTip.Closed += (_, _) =>
			{
				if (this.Children.Contains(teachingTip))
					this.Children.Remove(teachingTip);
			};
			this.Children.Add(teachingTip);
			return;
		}

		var deferral = args.GetDeferral();
		try
		{
			this.ToggleLoading(true);

			var response = await Session.Rest.PostAsync<AuthAPIResponse>(
				Routes.AccountPasswordChange(),
				new
				{
					old_password = Password,
					new_password = NewPassword.Password
				}) ?? throw new Exception("Response returned no data");
			if (response?.Result == false)
				throw new Exception(response?.Message ?? "Failed to change password");
			if (response?.Token is string token)
				Session.UpdateToken(token);
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