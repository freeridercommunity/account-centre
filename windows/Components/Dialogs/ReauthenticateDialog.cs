using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace AccountCentre.Components.Dialogs;

using Core;

public enum ReauthenticateDialogResult
{
	Authenticated,
	Cancelled
}

public sealed partial class ReauthenticateDialog : FormDialog
{
	public string Password => this.PasswordBox.Password;
	private readonly PasswordBox PasswordBox = new()
	{
		PlaceholderText = "Enter your password"
	};
	private ReauthenticateDialogResult Result = ReauthenticateDialogResult.Cancelled;
	private readonly string Username;
	public ReauthenticateDialog(string username)
	{
		this.Username = username;

		this.Title = "Verify account ownership";
		this.PasswordBox.KeyDown += PasswordBox_KeyDown;
		this.PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

		this.Children.Add(this.PasswordBox);

		this.IsPrimaryButtonEnabled = false;
		this.PrimaryButtonText = "Authenticate";
		this.PrimaryButtonClick += PrimaryButton_Click;
	}

	public new async Task<ReauthenticateDialogResult> ShowAsync()
	{
		await base.ShowAsync();

		var result = Result;
		this.Result = ReauthenticateDialogResult.Cancelled;
		return result;
	}

	private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
	{
		if (e.Key != VirtualKey.Enter)
			return;

		e.Handled = true;
		this.IsPrimaryButtonEnabled = false;

		try
		{
			var authenticated = await AuthManager.VerifyPasswordAsync(this.Username, this.PasswordBox.Password);
			if (!authenticated)
				throw new Exception("Password mismatch");

			this.Result = ReauthenticateDialogResult.Authenticated;
			this.Hide();
		}
		catch (Exception ex)
		{
			this.ShowError(ex.Message);
		}
	}

	private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		if (this.ErrorBar.IsOpen)
			this.ClearError();
		this.UpdatePrimaryButtonState();
	}

	private void UpdatePrimaryButtonState()
		=> this.IsPrimaryButtonEnabled = this.PasswordBox.Password.Length > 0;

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();
		try
		{
			var authenticated = await AuthManager.VerifyPasswordAsync(this.Username, this.PasswordBox.Password);
			if (!authenticated)
				throw new Exception("Password mismatch");

			this.Result = ReauthenticateDialogResult.Authenticated;
		}
		catch (Exception ex)
		{
			args.Cancel = true;
			this.ShowError(ex.Message);
		}
		finally
		{
			deferral.Complete();
		}
	}
}