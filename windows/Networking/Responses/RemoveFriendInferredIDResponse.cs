using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AccountCentre.Networking.Responses;

public class RemoveFriendInferredIDResponse : APIResponse
{
	[JsonInclude]
	[JsonPropertyName("msg")]
	private string? Msg {
		init
		{
			if (value == null)
				return;

			// Server response template
			// "You are not friends with {username}, you cannot remove friendship."
			var match = Regex.Match(
				value,
				@"You are not friends with (.*?), you cannot remove friendship\."
			);
			if (match.Length == 0)
				return;

			Username = match.Groups[1].Value;
		}
	}

	public string? Username { get; init; }
}