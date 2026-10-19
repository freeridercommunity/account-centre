using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace AccountCentre.Core.Storage;

internal sealed class CacheStore
{
	internal static CacheStore Default { get; } = new(AppData.CacheDirectory);
	public bool SerializeKeys { get; init; } = true;
	public long Size
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
	private readonly string CacheDirectory = null!;
	private readonly ConcurrentDictionary<string, CancellationTokenSource> SetTokens = [];
	internal CacheStore(string directory)
		=> CacheDirectory = directory;

	public void Clear()
	{
		if (Directory.Exists(CacheDirectory))
			Directory.Delete(CacheDirectory, true);
	}

	public void Set(string key, byte[] data)
	{
		Directory.CreateDirectory(CacheDirectory);
		File.WriteAllBytes(ResolvePath(key), data);
	}

	public async Task SetAsync(string key, Stream source)
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
			await using var destination = new FileStream(
				ResolvePath(key),
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
				SetTokens.TryRemove(key, out _);

			newTokenSource.Dispose();
		}
	}

	public byte[]? Get(string key)
	{
		var path = ResolvePath(key);
		return File.Exists(path)
			? File.ReadAllBytes(path)
			: null;
	}

	public string GetPath(string key)
		=> ResolvePath(key);

	public bool Has(string key)
		=> File.Exists(ResolvePath(key));

	public void Delete(string key)
	{
		var path = ResolvePath(key);
		if (File.Exists(path))
			File.Delete(path);
	}

	private string ResolvePath(string key)
	{
		if (SerializeKeys)
		{
			var hash = SHA1.HashData(Encoding.UTF8.GetBytes(key));
			return Path.Combine(CacheDirectory, Convert.ToHexString(hash));
		}

		return Path.Combine(CacheDirectory, key);
	}
}