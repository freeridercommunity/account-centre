using Microsoft.Win32;

namespace AccountCentre.Core;

public static class ProtocolRegistration
{
	private const string Protocol = "frhdaccount";

	public static void Register()
	{
		var executablePath = Environment.ProcessPath
			?? throw new InvalidOperationException("Unable to determine executable path");

		using var protocolKey = Registry.CurrentUser.CreateSubKey(
			$@"Software\Classes\{Protocol}");

		protocolKey.SetValue(
			"",
			"URL: Free Rider Community Account Centre Protocol");

		protocolKey.SetValue(
			"URL Protocol",
			"");

		using var shellKey = protocolKey.CreateSubKey(
			@"shell\open\command");

		shellKey.SetValue(
			"",
			$"\"{executablePath}\" \"%1\"");
	}

	public static void Unregister()
	{
		Registry.CurrentUser.DeleteSubKeyTree(
			$@"Software\Classes\{Protocol}",
			false);
	}
}