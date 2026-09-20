using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace AccountCentre.Core;

public static class CacheManager
{
	public static long Size
	{
		get
		{
			if (!Directory.Exists(CacheDirectory))
				return 0;

			var directory = new DirectoryInfo(CacheDirectory);
			return directory
				.EnumerateFiles("*", SearchOption.AllDirectories)
				.Sum(file => file.Length);
		}
	}
	public static string SizeText =>
		Utils.FormatBytes(Size);
	private static readonly string CacheDirectory = AppData.CacheDirectory;
	private static readonly ConcurrentDictionary<string, CancellationTokenSource> SetTokens = [];
	public static void Clear()
	{
		if (Directory.Exists(CacheDirectory))
			Directory.Delete(CacheDirectory, true);
	}

	public static void Set(string key, byte[] data)
	{
		Directory.CreateDirectory(CacheDirectory);

		var path = ResolvePath(key);
		File.WriteAllBytes(path, data);
	}

	public static async Task SetAsync(string key, Stream source)
	{
		Directory.CreateDirectory(CacheDirectory);

		var newTokenSource = new CancellationTokenSource();

		if (SetTokens.TryGetValue(key, out var previousTokenSource))
		{
			previousTokenSource.Cancel();
			previousTokenSource.Dispose();
		}

		SetTokens[key] = newTokenSource;

		try
		{
			var path = ResolvePath(key);

			await using var destination = new FileStream(
				path,
				FileMode.Create,
				FileAccess.Write,
				FileShare.None,
				4096,
				FileOptions.Asynchronous);

			await source.CopyToAsync(
				destination,
				newTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			// A newer SetAsync replaced this operation.
		}
		finally
		{
			if (SetTokens.TryGetValue(key, out var current) &&
				ReferenceEquals(current, newTokenSource))
			{
				SetTokens.TryRemove(key, out _);
			}

			newTokenSource.Dispose();
		}
	}

	public static byte[]? Get(string key)
	{
		var path = ResolvePath(key);

		if (!File.Exists(path))
			return null;

		return File.ReadAllBytes(path);
	}

	public static string GetPath(string key)
	{
		if (key.StartsWith(CacheDirectory))
			return key;

		return ResolvePath(key);
	}

	public static bool Has(string key)
	{
		var path = ResolvePath(key);
		return File.Exists(path);
	}

	public static void Delete(string key)
	{
		var path = ResolvePath(key);

		if (File.Exists(path))
			File.Delete(path);
	}

	private static string ResolvePath(string key)
	{
		var hash = SHA1.HashData(
			Encoding.UTF8.GetBytes(key));

		return Path.Combine(
			CacheDirectory,
			Convert.ToHexString(hash));
	}
}