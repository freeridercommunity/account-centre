using Microsoft.Windows.ApplicationModel.Resources;

namespace AccountCentre.Core;

internal static class Strings
{
	private static readonly ResourceManager Manager = new();
	public static string Get(string key)
	{
		return Manager.MainResourceMap
			.GetValue($"Resources/{key}")
			.ValueAsString;
	}

	public static string Format(string key, params object?[] args)
	{
		return string.Format(Get(key), args);
	}
}