namespace AccountCentre.Models;

using Core;
using Core.Storage;
using Networking;
using Networking.Models;
using Networking.Responses;

public record User : Base
{
	internal static readonly CacheStore AvatarCache = new(Path.Combine(AppData.CacheDirectory, "Avatars"))
	{
		// SerializeKeys = false
	};
	internal bool IsAuthenticated;
	public string? AvatarURL { get; set; }
	public string? DisplayAvatarURL => (AvatarURL != null &&
		AvatarCache.Has(ID.ToString()))
		? AvatarCache.GetPath(ID.ToString())
		: AvatarURL;
	public string DisplayName { get; set; } = "";
	public string Name { get; set; } = "";
	public Uri ProfileURL =>
		new($"http://frhd.co/u/{Name}");
	public UserStatsData? Stats;
	public User(UserData data) : base(data.ID)
	{
		Name = data.Name;
		DisplayName = data.DisplayName;
		AvatarURL = data.AvatarURL;
	}

	// public void Patch(UserData data)
	// {
	// 	base.Patch(data.ID);

	// 	Name = data.Name;
	// 	DisplayName = data.DisplayName;
	// }

	public async Task<UserStatsData?> FetchStats(bool force = false)
	{
		if (!force && Stats != null)
			return Stats;

		var response = await REST.GetAsync<UserPageResponse>(Routes.User(Name));
		if (response?.UserStats == null)
			return null;

		Stats = response.UserStats;
		return Stats;
	}

	private static readonly SemaphoreSlim AvatarSemaphore = new(4);
	public async Task CacheAvatarAsync(bool force = false)
	{
		if (string.IsNullOrEmpty(AvatarURL) ||
			(!force && AvatarCache.Has(ID.ToString())))
			return;

		await AvatarSemaphore.WaitAsync();
		try
		{
			if (!force && AvatarCache.Has(AvatarURL))
				return;

			await using var stream = await REST.GetStreamAsync(AvatarURL);
			await AvatarCache.SetAsync(ID.ToString(), stream);

			OnPropertyChanged(nameof(DisplayAvatarURL));
		}
		finally
		{
			AvatarSemaphore.Release();
		}
	}

	public static async Task<User?> FetchAsync(string username)
	{
		var response = await REST.GetAsync<UserPageResponse>(Routes.User(username));
		if (response?.User == null)
			return null;

		var user = new User(response.User);
		if (response.UserStats != null)
			user.Stats = response.UserStats;

		return user;
	}
}