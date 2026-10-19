using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Core.Services;

using Components.Dialogs;

public static class ErrorService
{
	public static async Task<ErrorDialog> Dialog(XamlRoot xamlRoot, string errorMessage)
	{
		var dialog = new ErrorDialog(errorMessage)
		{
			XamlRoot = xamlRoot
		};
		await dialog.ShowAsync();
		return dialog;
	}

	public static TeachingTip Tip(FrameworkElement target, string errorMessage)
	{
		// string key = $"ErrorTip_{Guid.NewGuid():N}";
		string key = "ErrorTip"; // Some get stuck, de-ref

		// Reuse an existing tip if present
		if (target.Resources.TryGetValue(key, out object existing)
			&& existing is TeachingTip existingTip)
		{
			existingTip.Subtitle = errorMessage;
			existingTip.IsOpen = true;
			return existingTip;
		}

		TeachingTip tip = new()
		{
			Target = target,
			Title = "Error",
			Subtitle = errorMessage,
			IsLightDismissEnabled = true,
			PreferredPlacement = TeachingTipPlacementMode.Bottom

			// Content = new StackPanel
			// {
			// 	Orientation = Orientation.Horizontal,
			// 	Spacing = 8,
			// 	Children =
			// 	{
			// 		new FontIcon
			// 		{
			// 			Glyph = "\uEA39"
			// 		},
			// 		new TextBlock
			// 		{
			// 			Text = $"Error: {ex.Message}"
			// 		}
			// 	}
			// }
		};

		target.Resources.Add(key, tip);

		tip.Closed += OnClosed;
		tip.IsOpen = true;

		return tip;

		void OnClosed(TeachingTip sender, TeachingTipClosedEventArgs args)
		{
			tip.Closed -= OnClosed;
			if (target.Resources.TryGetValue(key, out object current)
				&& ReferenceEquals(current, tip))
				target.Resources.Remove(key);
		}
	}
}
