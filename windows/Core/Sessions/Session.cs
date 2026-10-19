namespace AccountCentre.Core.Sessions;

using Core.Storage;
using Models;
using Networking;
using Networking.ExtendedAPI;
using Networking.Responses;

public sealed partial record Session : IDisposable
{
	public event EventHandler? Ended;
	public event EventHandler? Invalidated;
	internal readonly Guid ID = Guid.NewGuid();
	public REST.Session Rest { get; init; } = new();
	public RESTExtended.Session RestExtended { get; init; } = new();
	public ClientUser? User { get; internal set; }
	public required ulong UserID { get; init; }
	public string Username { get; internal set; } = "";
	// private string Token { get; init; } = null!;
	internal Session(string token)
		=> UpdateToken(token);

	internal void UpdateToken(string token)
	{
		Rest.SetToken(token);
		RestExtended.SetToken(token);

		if (this.User != null)
		{
			var secretKey = this.User.Name;
			if (SecretStore.Get(secretKey, "Sessions")?.Length > 0)
				SecretStore.Set(secretKey, token, "Sessions");
		}
	}

	public async Task<ClientUser> FetchUserAsync(bool force = false)
	{
		if (!force && User != null)
			return User;

		var response = await Rest.GetAsync<AccountPageResponse>(Routes.AccountSettings());
		if (response?.User == null)
			throw new Exception("User not found");

		var user = new ClientUser(response.User);
		User = user;
		return User;
	}

	public async Task RefreshToken(string password)
	{
		var response = await Rest.PostAsync<AuthAPIResponse>(
			Routes.AuthTokenRefresh(),
			new
			{
				old_password = password,
				new_password = password
			}) ?? throw new Exception("Response returned no data");
		if (response?.Token is not string token)
			throw new Exception(response?.Message ?? "Failed to refresh token");

		UpdateToken(token);
	}

	public void End()
	{
		SecretStore.ClearValue(Username, "Sessions");
		Dispose();
	}

	public async Task InvalidateAsync()
	{
		await Rest.DeleteAsync(Routes.AuthLogout());
		Rest.ClearToken();
		RestExtended.ClearToken();
		// Token = null;
		SecretStore.ClearValue(Username, "Sessions");
		Invalidated?.Invoke(this, EventArgs.Empty);
	}

	public void Dispose()
	{
		Rest.ClearToken();
		RestExtended.ClearToken();
		User = null;
		Ended?.Invoke(null, EventArgs.Empty);
	}
}