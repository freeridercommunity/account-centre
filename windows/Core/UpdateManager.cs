using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace AccountCentre.Core;

public static class UpdateManager
{
	public static Version CurrentVersion =>
		typeof(App).Assembly.GetName().Version ?? new Version();
	public static string? InstallerPath { get; private set; }
	public static DateTimeOffset? LastChecked;
	public static string LastCheckedText =>
		LastChecked is null
			? "Never"
			: LastChecked.Value.LocalDateTime.ToString("MMM d, yyyy h:mm tt");
	private const string Owner = "freeridercommunity";
	private const string Repository = "account-control.win";
	private const string Installer = "Setup-FreeRiderCommunityAccountCentre-x64.exe";
	private static readonly HttpClient Client = new()
	{
		BaseAddress = new Uri("https://api.github.com/"),
		DefaultRequestHeaders =
		{
			{ "User-Agent", "FreeRiderCommunityAccountCentre" }
		}
	};
	private static string InstallerDirectory =>
		Path.Combine(
			AppData.TempDirectory,
			"Installers"
		);
	private static GitHubRelease? LatestRelease;

	public static async Task<GitHubRelease?> CheckAsync(bool force = false)
	{
		if (LatestRelease != null && force != true)
			return LatestRelease;

		LastChecked = DateTimeOffset.UtcNow;

		var release = await GetLatestReleaseAsync();
		if (release == null)
			return null;

		var isNewer = IsNewer(release.TagName);
		// if (!isNewer)
		// 	return null;

		LatestRelease = release;
		return LatestRelease;
	}

	public static GitHubReleaseAsset? GetInstaller(GitHubRelease release)
	{
		return release.Assets.FirstOrDefault(
			asset => asset.Name.Equals(
				Installer,
				StringComparison.OrdinalIgnoreCase
			)
		);
	}

	public static async Task<string?> GetCachedInstallerAsync(GitHubReleaseAsset installer)
	{
		var path = Path.Combine(
			InstallerDirectory,
			installer.Name
		);

		if (!File.Exists(path))
			return null;

		if (await VerifyAssetHashAsync(path, installer.Digest))
		{
			InstallerPath = path;
			return path;
		}

		File.Delete(path);
		return null;
	}

	public static async Task<string> DownloadAssetAsync(GitHubReleaseAsset asset, bool validateChecksum = true)
	{
		Directory.CreateDirectory(InstallerDirectory);

		var path = Path.Combine(InstallerDirectory, asset.Name);
		var tempPath = path + ".tmp";

		try
		{
			await using var output = new FileStream(
				tempPath,
				FileMode.Create,
				FileAccess.Write,
				FileShare.None,
				4096,
				FileOptions.Asynchronous
			);

			using var response = await Client.GetAsync(
				asset.DownloadUrl,
				HttpCompletionOption.ResponseHeadersRead
			);

			response.EnsureSuccessStatusCode();

			await using var input = await response.Content.ReadAsStreamAsync();

			if (validateChecksum)
			{
				using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

				var buffer = new byte[81920];
				int read;

				while ((read = await input.ReadAsync(buffer)) > 0)
				{
					await output.WriteAsync(buffer.AsMemory(0, read));
					hasher.AppendData(buffer, 0, read);
				}

				var hash = Convert.ToHexString(hasher.GetHashAndReset());
				if (!GitHubReleaseAsset.ValidateChecksum(hash, asset.Digest))
					throw new InvalidDataException(
						"The downloaded asset failed hash validation. " +
						"Its integrity may be compromised."
					);
			}
			else
			{
				await input.CopyToAsync(output);
			}

			output.Close();

			File.Move(tempPath, path, true);

			InstallerPath = path;
			return path;
		}
		catch
		{
			if (File.Exists(tempPath))
				File.Delete(tempPath);

			throw;
		}
	}

	public static bool DownloadStarted(GitHubReleaseAsset asset)
	{
		return File.Exists(
			Path.Combine(
				InstallerDirectory,
				asset.Name + ".tmp"
			)
		);
	}

	public static bool IsDownloaded(GitHubReleaseAsset asset)
	{
		return File.Exists(
			Path.Combine(
				InstallerDirectory,
				asset.Name
			)
		);
	}

	public static bool InstallerDownloadStarted()
	{
		return File.Exists(
			Path.Combine(
				InstallerDirectory,
				Installer + ".tmp"
			)
		);
	}

	public static bool IsInstallerDownloaded()
	{
		var path = Path.Combine(
			InstallerDirectory,
			Installer
		);
		var exists = File.Exists(path);
		if (exists)
			InstallerPath = path;

		return exists;
	}

	public static async Task<bool> IsDownloadedAsync(GitHubReleaseAsset asset)
	{
		if (!IsDownloaded(asset))
			return false;

		return await VerifyAssetHashAsync(
			Path.Combine(
				InstallerDirectory,
				asset.Name
			),
			asset.Digest
		);
	}

	public static async Task AssertAssetHashAsync(string path, string expectedDigest)
	{
		if (!await VerifyAssetHashAsync(path, expectedDigest))
			throw new InvalidDataException(
				"The downloaded asset failed hash validation. " +
				"Its integrity may be compromised."
			);
	}

	public static async Task<bool> VerifyAssetHashAsync(string path, string expectedDigest)
	{
		if (!File.Exists(path))
			return false;

		await using var input = new FileStream(
			path,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read,
			81920,
			FileOptions.Asynchronous | FileOptions.SequentialScan
		);

		var hash = await ComputeHashAsync(input);
		return GitHubReleaseAsset.ValidateChecksum(hash, expectedDigest);
	}

	private static async Task<GitHubRelease?> GetLatestReleaseAsync()
	{
		try
		{
			return await Client.GetFromJsonAsync<GitHubRelease>(
				$"repos/{Owner}/{Repository}/releases/latest"
			) ?? null;
		}
		catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	private static async Task<GitHubRelease[]> GetReleasesAsync()
	{
		try
		{
			return await Client.GetFromJsonAsync<GitHubRelease[]>(
				$"repos/{Owner}/{Repository}/releases"
			) ?? [];
		}
		catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			return [];
		}
	}

	private static async Task<string> ComputeHashAsync(Stream input)
	{
		var hash = await SHA256.HashDataAsync(input);
		return Convert.ToHexString(hash);
	}

	private static bool IsNewer(string tag)
	{
		if (tag.StartsWith('v'))
			tag = tag[1..];

		return Version.TryParse(tag, out var version)
			&& version > CurrentVersion;
	}
}

// https://docs.github.com/en/rest/releases/releases?apiVersion=latest#get-the-latest-release
public sealed class GitHubRelease
{
	[JsonPropertyName("tag_name")]
	public string TagName { get; init; } = "";

	[JsonPropertyName("name")]
	public string Name { get; init; } = "";

	[JsonPropertyName("html_url")]
	public string HtmlUrl { get; init; } = "";

	[JsonPropertyName("assets")]
	public List<GitHubReleaseAsset> Assets { get; init; } = [];

	public Version? Version =>
		Version.TryParse(TagName.TrimStart('v'), out var version)
			? version
			: null;

	public bool IsNewer =>
		this.Version > UpdateManager.CurrentVersion;
}

public sealed class GitHubReleaseAsset
{
	[JsonPropertyName("name")]
	public string Name { get; init; } = "";

	// [JsonPropertyName("content_type")]
	// public string ContentType { get; init; } = "";

	[JsonPropertyName("size")]
	public long Size { get; init; }

	[JsonPropertyName("digest")]
	public string Digest { get; init; } = "";

	[JsonPropertyName("browser_download_url")]
	public string DownloadUrl { get; init; } = "";

	public static bool ValidateChecksum(string hash, string digest)
	{
		var expectedHash = digest;
		if (expectedHash.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
			expectedHash = expectedHash[7..];

		return hash.Equals(
			expectedHash,
			StringComparison.OrdinalIgnoreCase
		);
	}
}