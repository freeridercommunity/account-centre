using System.Net.Http.Json;
using System.Text.Json;

namespace AccountCentre.Networking.ExtendedAPI;

using Responses;

public static class RESTExtended
{
	private const string Host = "api.freeridercommunity.workers.dev";
	private const string BaseURL = $"https://{Host}/";
	private static readonly HttpClient Client = new()
	{
		BaseAddress = new Uri(BaseURL),
		DefaultRequestHeaders =
		{
			{ "User-Agent", "FreeRiderCommunityAccountCentre" }
		}
	};

	public static void SetToken(string token)
	{
		ClearToken();

		Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
	}

	public static void ClearToken()
	{
		Client.DefaultRequestHeaders.Remove("Authorization");
	}

	public static async Task GetAsync(string endpoint)
	{
		using var response = await Client.GetAsync(endpoint);
		await EnsureSuccessStatusCode(response);
	}

	public static async Task<T?> GetAsync<T>(string endpoint)
	{
		using var response = await Client.GetAsync(endpoint);

		await EnsureSuccessStatusCode(response);

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task PatchAsync(string endpoint, object? values = null)
	{
		using var response = await Client.PatchAsJsonAsync(endpoint, values);
		await EnsureSuccessStatusCode(response);
	}

	public static async Task<T?> PatchAsync<T>(string endpoint, object? values = null)
	{
		using var response = await Client.PatchAsJsonAsync(endpoint, values);

		await EnsureSuccessStatusCode(response);

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task PostAsync(string endpoint, object? values = null)
	{
		using var response = await Client.PostAsJsonAsync(endpoint, values);
		await EnsureSuccessStatusCode(response);
	}

	public static async Task<T?> PostAsync<T>(string endpoint, object? values = null)
	{
		using var response = await Client.PostAsJsonAsync(endpoint, values);

		await EnsureSuccessStatusCode(response);

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task DeleteAsync(string endpoint)
	{
		using var response = await Client.DeleteAsync(endpoint);
		await EnsureSuccessStatusCode(response);
	}

	private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
			return;

		BaseResponse? body = null;

		try
		{
			body = await response.Content.ReadFromJsonAsync<BaseResponse>();
		}
		catch (JsonException)
		{
		}

		throw new HttpRequestException(
			body?.Message ?? response.ReasonPhrase ?? "The request failed.",
			null,
			response.StatusCode
		);
	}
}