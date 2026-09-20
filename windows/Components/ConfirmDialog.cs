using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components;

public sealed partial class ConfirmDialog : ContentDialog
{
	public ConfirmDialog(string? message = null)
	{
		CloseButtonText = "Cancel";
		Content = message;
		DefaultButton = ContentDialogButton.Primary;
		PrimaryButtonText = "OK";
		Title = "Confirm";
	}
}