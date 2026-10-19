using Microsoft.UI.Xaml.Data;

namespace AccountCentre.Converters;

public sealed class InvertBoolConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
		=> value is not true;

	public object ConvertBack(object value, Type targetType, object parameter, string language)
		=> throw new NotSupportedException();
}