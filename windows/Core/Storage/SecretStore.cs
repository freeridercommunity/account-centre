using System.Security.Cryptography;
using System.Text;

namespace AccountCentre.Core.Storage;

internal static class SecretStore
{
	private static readonly string SecretDirectory =
		Path.Combine(AppData.Directory, "Secrets");
	public static void Set(string key, string value, string? group = null)
	{
		Directory.CreateDirectory(group != null
			? Path.Combine(SecretDirectory, group)
			: SecretDirectory);

		var path = ResolvePath(key, group);
		var data = Encoding.UTF8.GetBytes(value);

		var encrypted = data.Length > 0
			? ProtectedData.Protect(
				data,
				optionalEntropy: null,
				scope: DataProtectionScope.CurrentUser)
			: [];

		File.WriteAllBytes(path, encrypted);
	}

	public static byte[]? Get(string key, string? group = null)
	{
		var path = ResolvePath(key, group);
		if (!File.Exists(path))
			return null;

		var encrypted = File.ReadAllBytes(path);
		if (encrypted.Length == 0)
			return null;

		return ProtectedData.Unprotect(
			encrypted,
			optionalEntropy: null,
			scope: DataProtectionScope.CurrentUser);
	}

	public static IEnumerable<string> EnumerateSecrets(string? group = null)
	{
		var path = group != null
			? Path.Combine(SecretDirectory, group)
			: SecretDirectory;

		if (!Directory.Exists(path))
			return [];

		return Directory.EnumerateFiles(path)
			.OrderByDescending(File.GetLastAccessTimeUtc)
			.Select(Path.GetFileName)!;
	}

	public static void ClearValue(string key, string? group = null)
		=> Set(key, "", group);

	public static bool Delete(string key, string? group = null)
	{
		var path = ResolvePath(key, group);
		if (!File.Exists(path))
			return false;

		File.Delete(path);
		return true;
	}

	private static string ResolvePath(string key, string? group = null)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentException("Secret key cannot be empty.", nameof(key));

		if (key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			throw new ArgumentException("Invalid secret key.", nameof(key));

		if (group != null)
			key = Path.Combine(group, key);

		return Path.Combine(SecretDirectory, key);
	}
}