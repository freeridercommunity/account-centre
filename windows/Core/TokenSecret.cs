using System.Security.Cryptography;
using System.Text;

namespace AccountCentre.Core;

public static class TokenSecret
{
	private static readonly string SecretPath = AppData.TokenSecretPath;

	public static void Set(string value)
	{
		Directory.CreateDirectory(AppData.Directory);

		var data = Encoding.UTF8.GetBytes(value);

		var encrypted = ProtectedData.Protect(
			data,
			null,
			DataProtectionScope.CurrentUser
		);

		File.WriteAllBytes(SecretPath, encrypted);
	}

	public static string? Get()
	{
		if (!File.Exists(SecretPath))
			return null;

		var encrypted = File.ReadAllBytes(SecretPath);

		var data = ProtectedData.Unprotect(
			encrypted,
			null,
			DataProtectionScope.CurrentUser
		);

		return Encoding.UTF8.GetString(data);
	}

	public static void Delete()
	{
		if (File.Exists(SecretPath))
			File.Delete(SecretPath);
	}
}