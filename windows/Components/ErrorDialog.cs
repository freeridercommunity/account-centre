using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AccountCentre.Components;

public sealed partial class ErrorDialog : ContentDialog
{
	public ErrorDialog(string message)
	{
		Background = new SolidColorBrush(ColorHelper.FromArgb(255, 68, 39, 38));
		CloseButtonText = "Close";
		Content = message;
		Title = new StackPanel
		{
			Children =
			{
				new FontIcon
				{
					Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 153, 164)),
					Glyph = "\uEB90", // "\uEA39",
					VerticalAlignment = VerticalAlignment.Center
				},
				new TextBlock
				{
					Text = "An error occurred",
					VerticalAlignment = VerticalAlignment.Center
				}
			},
			Orientation = Orientation.Horizontal,
			Spacing = 16
		};
	}
}