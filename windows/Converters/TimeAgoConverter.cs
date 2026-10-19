using Microsoft.UI.Xaml.Data;

namespace AccountCentre.Converters;

public sealed class TimeAgoConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		DateTimeOffset date = value switch
		{
			DateTimeOffset dto => dto,
			DateTime dt => new DateTimeOffset(dt.ToUniversalTime()),
			long timestamp => DateTimeOffset.FromUnixTimeSeconds(timestamp),
			_ => default
		};

		if (date == default)
			return "";

		var elapsed = DateTimeOffset.UtcNow - date;

		if (elapsed.TotalSeconds < 0)
			return "just now";

		return elapsed.TotalSeconds switch
		{
			< 60 => "just now",
			< 120 => "1 minute ago",
			< 3600 => $"{(int)elapsed.TotalMinutes} minutes ago",
			< 7200 => "1 hour ago",
			< 86400 => $"{(int)elapsed.TotalHours} hours ago",
			< 172800 => "yesterday",
			< 604800 => $"{(int)elapsed.TotalDays} days ago",
			_ => $"{(int)elapsed.TotalDays / 7} weeks ago"
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();
}