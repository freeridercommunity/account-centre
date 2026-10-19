using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

public class NotificationsPageResponse : BasePageResponse
{
	[JsonPropertyName("notification_days")]
	public NotificationDayData[] NotificationDays { get; init; } = [];
}