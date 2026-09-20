using System.Net;
using System.Net.Http.Json;

namespace AccountCentre.Networking;

using Responses;

public static class REST
{
	private const string Host = "www.freeriderhd.com";
	private const string BaseURL = $"https://{Host}/";
	private static readonly CookieContainer Cookies = new();
	private static readonly HttpClient Client = new(
		new HttpClientHandler
		{
			CookieContainer = Cookies,
			UseCookies = true
		})
	{
		BaseAddress = new Uri(BaseURL),
		DefaultRequestHeaders =
		{
			{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0) Chrome/120" }
		}
	};

	public static void SetToken(string token)
	{
		ClearToken();

		Cookies.Add(
			Client.BaseAddress!,
			new Cookie("frhd_app_sr", token, "/", $".{Host}")
		);
	}

	public static void ClearToken()
	{
		var cookies = Cookies.GetCookies(Client.BaseAddress!);
		foreach (Cookie cookie in cookies)
		{
			if (cookie.Name == "frhd_app_sr")
			{
				cookie.Expired = true;
			}
		}
	}

	public static async Task<T?> GetAsync<T>(string endpoint)
	{
		endpoint = ResolveEndpoint<T>(endpoint);

		var separator = endpoint.Contains('?') ? '&' : '?';
		endpoint = $"{endpoint}{separator}ajax";

		using var response = await Client.GetAsync(endpoint);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task<byte[]> GetBytesAsync(string url)
	{
		return await Client.GetByteArrayAsync(new Uri(url));
	}

	public static async Task<Stream> GetStreamAsync(string url)
	{
		return await Client.GetStreamAsync(new Uri(url));
	}

	public static async Task PostAsync(string endpoint, object? values = null)
	{
		using var response = await Client.PostAsync(endpoint, values == null ? null : FormContent(values));
		response.EnsureSuccessStatusCode();
	}

	public static async Task<T?> PostAsync<T>(string endpoint, object? values = null)
	{
		using var response = await Client.PostAsync(endpoint, values == null ? null : FormContent(values));

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task DeleteAsync(string endpoint)
	{
		using var response = await Client.DeleteAsync(endpoint);

		response.EnsureSuccessStatusCode();
	}

	private static HttpContent FormContent(object values)
	{
		return new FormUrlEncodedContent(
			values.GetType()
				.GetProperties()
				.Select(property =>
					new KeyValuePair<string, string>(
						property.Name,
						property.GetValue(values)?.ToString() ?? ""
					)
				)
		);
	}

	private static string ResolveEndpoint<T>(string endpoint)
	{
		if (typeof(T) == typeof(AccountPageResponse) && endpoint.Length == 0)
			return "account/settings";
		// if (typeof(T) == typeof(TrackPageResponse))
		// 	return $"t/{endpoint}";
		if (typeof(T) == typeof(UserPageResponse) && !endpoint.StartsWith("u/"))
			return $"u/{endpoint}";

		return endpoint;
	}
}