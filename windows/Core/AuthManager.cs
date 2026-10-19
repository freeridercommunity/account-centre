namespace AccountCentre.Core;

using Models;
using Networking;
using Networking.Responses;
using Sessions;
using Storage;

public static class AuthManager
{
	public static ulong LastUserID { get; private set; }
	public static event EventHandler? LoggedOut;
	public static async Task<Session> Login(string login, string password, bool save = false)
	{
		var response = await AuthenticateAsync(login, password);
		// Check if session exists
		// if (SessionManager.Get(response.Data.User.ID))
		// 	return SessionManager.Get(response.Data.User.ID);

		SecretStore.Set(response.Data!.User.Name, save ? response.Token! : "", "Sessions");

		var authUser = new ClientUser(response.Data!.User);

		// // Cache client user data
		// // CacheStore.Sessions?.Set($"u{authUser.ID}", authUser);
		// // CacheStore.For("Sessions")?.Set($"u{authUser.ID}", authUser);
		// // CacheStore.Default.Set($"u{authUser.ID}", authUser);

		try
		{
			LastUserID = (ulong)authUser.ID;
		}
		catch
		{
		}

		return SessionManager.Create(response.Token!, authUser);
	}

	public static async Task<Session> Login(string token)
	{
		var response = await AuthenticateAsync(token);

		// SecretStore.Set(response.Data!.User.Name, save ? token : "", "Sessions");

		var authUser = new ClientUser(response.Data!.User);
		return SessionManager.Create(token, authUser);
	}

	public static async Task<bool> VerifyPasswordAsync(string username, string password)
	{
		var response = await AuthenticateAsync(username, password);
		return response.Result;
	}

	public static async Task Logout(Session session)
	{
		await session.InvalidateAsync();
		SecretStore.ClearValue(session.User!.Name, "Sessions");
		LoggedOut?.Invoke(session, EventArgs.Empty);
	}

	private static async Task<AuthAPIResponse> AuthenticateAsync(string login, string password)
	{
		var response = await REST.PostAsync<AuthAPIResponse>(
			"auth/standard_login",
			new { login, password }
		) ?? throw new Exception("The response contained no data.");
		if (response.Result == false)
			throw new Exception(response.Message);
		if (response.Token == null)
			throw new Exception("Invalid token");

		return response;
	}

	private static async Task<AuthAPIResponse> AuthenticateAsync(string token)
	{
		var rest = new REST.Session(token);
		// var response = await rest.GetAsync<AccountPageResponse>(Routes.AccountSettings())
		// 	?? throw new Exception("The response contained no data.");
		var response = await rest.GetAsync<AccountPageResponse>(Routes.AccountSettings())
			?? throw new AuthFailedException("The response contained no data.");

		return new AuthAPIResponse
		{
			Data = new AuthResponseData
			{
				User = response.User!
			},
			Result = response.Result,
			Token = token
		};
	}
}

public class AuthFailedException : Exception
{
	public AuthFailedException(string? message = null) : base(message)
	{}
}