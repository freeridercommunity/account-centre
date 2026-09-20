using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public sealed class UMentionLookupAPIResponse : APIResponse<UMentionData[]>
{
	[JsonPropertyName("code")]
	public override bool Result { get; init; } = true;
}