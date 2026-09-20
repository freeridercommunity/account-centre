using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace AccountCentre.Components;

public sealed partial class WarningDialog : ContentDialog
{
	public WarningDialog(string message)
	{
		Background = new SolidColorBrush(ColorHelper.FromArgb(255, 67, 53, 25));
		CloseButtonText = "Cancel";
		Content = message;
		Title = new StackPanel
		{
			Children =
			{
				new FontIcon
				{
					Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 252, 225, 0)),
					Glyph = "\uF736",
					VerticalAlignment = VerticalAlignment.Center
				},
				new TextBlock
				{
					Text = "Warning",
					VerticalAlignment = VerticalAlignment.Center
				}
			},
			Orientation = Orientation.Horizontal,
			Spacing = 16
		};
	}
}