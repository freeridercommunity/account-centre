using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace AccountCentre.Components;

using Core;

public enum ReauthenticateDialogResult
{
	Authenticated,
	Cancelled
}

public sealed partial class ReauthenticateDialog : ContentDialog
{
	public string Password => this.PasswordBox.Password;
	private readonly InfoBar ErrorBar = new()
	{
		// Title = "Authentication failed",
		// Message = "Password mismatch",
		Severity = InfoBarSeverity.Error,
		IsOpen = true
	};
	private readonly PasswordBox PasswordBox = new()
	{
		PlaceholderText = "Enter your password"
	};
	private ReauthenticateDialogResult Result = ReauthenticateDialogResult.Cancelled;
	private readonly StackPanel StackPanel = new()
	{
		Spacing = 8
	};
	public ReauthenticateDialog()
	{
		Title = "Verify account ownership";
		this.PasswordBox.KeyDown += PasswordBox_KeyDown;
		this.PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

		StackPanel.Children.Add(this.PasswordBox);
		Content = StackPanel;

		IsPrimaryButtonEnabled = false;
		PrimaryButtonText = "Authenticate";
		PrimaryButtonClick += PrimaryButton_Click;
		CloseButtonText = "Cancel";
		DefaultButton = ContentDialogButton.Primary;
	}

	public new async Task<ReauthenticateDialogResult> ShowAsync()
	{
		await base.ShowAsync();

		var result = Result;
		Result = ReauthenticateDialogResult.Cancelled;
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
			var authenticated = await AuthManager.VerifyPasswordAsync(this.PasswordBox.Password);
			if (!authenticated)
				return;

			Result = ReauthenticateDialogResult.Authenticated;
			Hide();
		}
		catch (Exception ex)
		{
			ShowError(ex.Message);
		}
	}

	private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
	{
		if (this.ErrorBar.IsOpen)
			ClearError();
		UpdatePrimaryButtonState();
	}

	private void UpdatePrimaryButtonState()
	{
		this.IsPrimaryButtonEnabled = this.PasswordBox.Password.Length > 0;
	}

	private async void PrimaryButton_Click(object sender, ContentDialogButtonClickEventArgs args)
	{
		this.IsPrimaryButtonEnabled = false;

		var deferral = args.GetDeferral();

		try
		{
			var authenticated = await AuthManager.VerifyPasswordAsync(this.PasswordBox.Password);
			if (!authenticated)
				return;

			Result = ReauthenticateDialogResult.Authenticated;
		}
		catch (Exception ex)
		{
			args.Cancel = true;

			ShowError(ex.Message);
		}
		finally
		{
			deferral.Complete();
		}
	}

	private void ShowError(string message)
	{
		this.ErrorBar.Message = message;
		this.ErrorBar.IsOpen = true;

		StackPanel.Children.Add(this.ErrorBar);
	}

	private void ClearError()
	{
		StackPanel.Children.Remove(this.ErrorBar);

		this.ErrorBar.IsOpen = false;
	}
}