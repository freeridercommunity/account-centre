using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Converters;

public sealed class DataPollResponse : BaseResponse
{
	[JsonPropertyName("notification_count")]
	[JsonConverter(typeof(NotificationCountConverter))]
	public ushort? Notifications { get; init; }
}