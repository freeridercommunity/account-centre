namespace AccountCentre.Core;

public static class Utils
{
	public static string FormatBytes(long bytes)
	{
		const double KB = 1024;
		const double MB = KB * 1024;
		const double GB = MB * 1024;
		return bytes switch
		{
			>= (long)GB => $"{bytes / GB:F2} GB",
			>= (long)MB => $"{bytes / MB:F2} MB",
			>= (long)KB => $"{bytes / KB:F2} KB",
			_ => $"{bytes} B"
		};
	}

	public static string FormatTimeAgo(long timestamp)
	{
		var published = DateTimeOffset.FromUnixTimeSeconds(timestamp);
		var elapsed = DateTimeOffset.UtcNow - published;

		if (elapsed.TotalSeconds < 60)
			return "just now";

		if (elapsed.TotalMinutes < 60)
			return $"{(ushort)elapsed.TotalMinutes} min ago";

		if (elapsed.TotalHours < 24)
			return $"{(ushort)elapsed.TotalHours} hr ago";

		if (elapsed.TotalDays < 30)
			return $"{(ushort)elapsed.TotalDays} day{(elapsed.TotalDays >= 2 ? "s" : "")} ago";

		if (elapsed.TotalDays < 365)
		{
			var months = (ushort)(elapsed.TotalDays / 30);
			return $"{months} month{(months >= 2 ? "s" : "")} ago";
		}

		var years = (uint)(elapsed.TotalDays / 365);
		if (years < 10)
			return $"{years} year{(years >= 2 ? "s" : "")} ago";

		return $"{Math.Floor((double)years / 10)} decade{(years >= 20 ? "s" : "")} ago";
	}

	public static string SanitizeFileName(string name, char substitute = '_')
	{
		foreach (var c in Path.GetInvalidFileNameChars())
			name = name.Replace(c, substitute);

		return name;
	}
}