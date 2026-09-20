namespace AccountCentre.Models;

using Networking.Models;

public record User : Base
{
	public string Name { get; set; } = "";
	public string DisplayName { get; set; } = "";
	public string? AvatarURL { get; set; }
	// public string? DisplayAvatarURL => (AvatarURL != null &&
	// 	CacheManager.Has(AvatarURL))
	// 	? CacheManager.GetPath(AvatarURL)
	// 	: AvatarURL;
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

	// private static readonly SemaphoreSlim AvatarSemaphore = new(4);
	// public async Task CacheThumbnailAsync()
	// {
	// 	if (string.IsNullOrEmpty(AvatarURL) ||
	// 		CacheManager.Has(AvatarURL))
	// 		return;

	// 	await AvatarSemaphore.WaitAsync();

	// 	try
	// 	{
	// 		if (CacheManager.Has(AvatarURL))
	// 			return;

	// 		await using var stream = await REST.GetStreamAsync(AvatarURL);
	// 		await CacheManager.SetAsync(
	// 			AvatarURL,
	// 			stream);

	// 		OnPropertyChanged(nameof(DisplayAvatarURL));
	// 	}
	// 	finally
	// 	{
	// 		AvatarSemaphore.Release();
	// 	}
	// }
}