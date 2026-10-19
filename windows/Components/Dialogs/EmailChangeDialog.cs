// using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// using Microsoft.UI.Xaml.Media;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking.ExtendedAPI;

public sealed partial class EmailChangeDialog : FormDialog
{
	// Suggest common domains after @: gmail.com, yahoo.com, etc.
	private readonly AutoSuggestBox NewEmail = new()
	{
		PlaceholderText = "Enter new email"
	};
	private readonly TextBox NewEmail2 = new()
	{
		PlaceholderText = "Re-enter new email"
	};
	public string Password { private get; init; } = null!;
	private readonly Session Session;
	public EmailChangeDialog(Session session)
	{
		this.Session = session;

		// this.ErrorBar.Title = "Email change failed";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Change Email";
		// this.Children.Add(new TextBlock
		// {
		// 	Text = session.User?.Email,
		// 	Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
		// });

		this.NewEmail.TextChanged += (_, _) => TextChanged();
		this.NewEmail2.TextChanged += (_, _) => TextChanged();

		this.Children.Add(this.NewEmail);
		this.Children.Add(this.NewEmail2);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonClick += PrimaryButton_Click;
		this.PrimaryButtonText = "Save";
	}

	private void TextChanged()
	{
		this.IsPrimaryButtonEnabled = this.NewEmail.Text.Contains('@') && this.NewEmail2.Text.Contains('@');
		this.ClearError();
	}

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var newEmail = this.NewEmail.Text.Trim();
		if (newEmail != this.NewEmail2.Text.Trim())
		{
			args.Cancel = true;

			var teachingTip = new TeachingTip
			{
				Title = "Email mismatch",
				Subtitle = "Emails don't match. Please try again",
				Target = this.NewEmail2,
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
		else if (newEmail == Session.User!.Email)
		{
			args.Cancel = true;

			var teachingTip = new TeachingTip
			{
				Title = "New email is the same as current",
				Subtitle = "Your new email must be different from your current email",
				Target = this.NewEmail,
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

			await Session.RestExtended.PatchAsync(
				RESTExtended.Routes.Email(),
				new
				{
					email = newEmail,
					password = this.Password
				});

			Session.User!.Email = newEmail;
			Session.User.OnPropertyChanged(nameof(Session.User.Email));
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