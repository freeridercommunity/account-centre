using AccountCentre.Networking;
using AccountCentre.Networking.ExtendedAPI;
using AccountCentre.Networking.Responses;

namespace AccountCentre.Core;

public static class AuthManager
{
	// public static ClientUser? CurrentUser;
	// public static bool IsAuthenticated { get; private set; }
	public static event EventHandler? LogOut;
	public static async Task Initialize()
	{
		var token = TokenSecret.Get();
		if (token == null)
			return;

		REST.SetToken(token);
		RESTExtended.SetToken(token);

		// Cache user data
		// CacheManager.Set(response.User.AvatarURL, );
	}

	public static async Task Login(string login, string password)
	{
		var response = await LoginAsync(login, password);
		if (response.Token is not string token)
			throw new Exception("Invalid token");

		TokenSecret.Set(token);

		await Initialize();
	}

	public static async Task<bool> VerifyPasswordAsync(string password)
	{
		var response = await LoginAsync(App.User!.Name, password);
		return response.Result;
	}

	public static void Logout()
	{
		TokenSecret.Delete();
		REST.ClearToken();
		RESTExtended.ClearToken();

		LogOut?.Invoke(null, EventArgs.Empty);
	}

	private static async Task<AuthAPIResponse> LoginAsync(string login, string password)
	{
		var response = await REST.PostAsync<AuthAPIResponse>(
			"auth/standard_login",
			new { login, password }
		) ?? throw new Exception("The response contained no data.");
		if (response.Result == false)
			throw new Exception(response.Message);

		return response;
	}
}