using System.Text.Json.Serialization;

namespace AccountCentre.Networking.Responses;

using Models;

public class NotificationsAPIResponse : APIResponse<NotificationsAPIResponseData>
{
}

public class NotificationsAPIResponseData
{
	[JsonPropertyName("notification_data")]
	public required NotificationsData NotificationData { get; init; }
}

public class NotificationsData
{
	[JsonPropertyName("notification_days")]
	public NotificationDayData[] NotificationDays { get; init; } = [];
}

public class NotificationDayData
{
	[JsonPropertyName("date")]
	public string Date { get; init; } = "Unknown";

	[JsonPropertyName("notifications")]
	public NotificationData[] Notifications { get; init; } = [];
}