using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AccountCentre.Converters;

public sealed class FriendActivityToStatusStyleConverter : IValueConverter
{
	public object Convert(
		object value,
		Type targetType,
		object parameter,
		string language)
	{
		if (value is not long activityTimestamp)
			return Application.Current.Resources["InformationalDotInfoBadgeStyle"];

		var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var age = now - activityTimestamp;

		var key = age switch
		{
			<= 600 => "SuccessDotInfoBadgeStyle",
			<= 1800 => "CautionDotInfoBadgeStyle",
			> 31_536_000 => "AttentionDotInfoBadgeStyle",
			_ => "InformationalDotInfoBadgeStyle"
		};

		return Application.Current.Resources[key];
	}

	public object ConvertBack(
		object value,
		Type targetType,
		object parameter,
		string language)
	{
		throw new NotSupportedException();
	}
}