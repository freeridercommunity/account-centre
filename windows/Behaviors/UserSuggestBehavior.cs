using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Behaviors;

using Core;
using Networking.Models;

public sealed class UserSuggestBehavior : BaseBehavior<UserSuggestBehavior, AutoSuggestBox>, IBehavior<AutoSuggestBox>
{
	public static void OnAttached(AutoSuggestBox box)
	{
		box.TextChanged += OnTextChanged;
		box.SuggestionChosen += OnSuggestionChosen;
	}

	public static void OnDetached(AutoSuggestBox box)
	{
		box.TextChanged -= OnTextChanged;
		box.SuggestionChosen -= OnSuggestionChosen;
	}

	private static async void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
			return;

		var query = sender.Text.Trim();
		if (query.Length < 3)
		{
			sender.ItemsSource = null;
			return;
		}

		var users = await Search.User(query);
		sender.ItemsSource = users;
	}

	private static void OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		if (args.SelectedItem is UMentionData user)
			sender.Text = user.DisplayName;
	}
}