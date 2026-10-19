using System.Net;
using System.Net.Http.Json;

namespace AccountCentre.Networking;

public static class REST
{
	private const string Host = "www.freeriderhd.com";
	private const string BaseURL = $"https://{Host}/";
	private static readonly HttpClient Client = new()
	{
		BaseAddress = new Uri(BaseURL),
		DefaultRequestHeaders =
		{
			{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0) Chrome/120" }
		},
		DefaultRequestVersion = HttpVersion.Version30,
		DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
	};
	public static async Task<T?> GetAsync<T>(string endpoint)
	{
		using var response = (await Client.GetAsync(AjaxEndpoint(endpoint)))
			.EnsureSuccessStatusCode();
		if (response.Content.Headers.ContentLength == 0)
			return default;

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public static async Task<byte[]> GetBytesAsync(string url)
		=> await Client.GetByteArrayAsync(new Uri(url));
	public static async Task<Stream> GetStreamAsync(string url)
		=> await Client.GetStreamAsync(new Uri(url));
	public static async Task PostAsync(string endpoint, object? values = null)
	{
		using var _ = (await Client.PostAsync(endpoint, values == null ? null : FormContent(values)))
			.EnsureSuccessStatusCode();
	}

	public static async Task<T?> PostAsync<T>(string endpoint, object? values = null)
	{
		using var response = (await Client.PostAsync(endpoint, values == null ? null : FormContent(values)))
			.EnsureSuccessStatusCode();
		if (response.Content.Headers.ContentLength == 0)
			return default;

		return await response.Content.ReadFromJsonAsync<T>();
	}

	public sealed class Session
	{
		private string? _token;
		internal Session(string? token = null)
		{
			if (token != null)
				_token = token;
		}

		// public ExtendedAPI.Session

		// public Task<T?> GetAsync<T>(string endpoint)
		// 	=> SendAsync<T>(HttpMethod.Get, endpoint, _token);

		public Session SetToken(string token)
		{
			_token = token;
			return this;
		}

		public void ClearToken()
			=> _token = null;

		public async Task<T?> GetAsync<T>(string endpoint)
		{
			using var response = await SendAsync(HttpMethod.Get, AjaxEndpoint(endpoint));
			if (response.Content.Headers.ContentLength == 0)
				return default;

			return await response.Content.ReadFromJsonAsync<T>();
		}

		public async Task PostAsync(string endpoint, object? values = null)
		{
			using var _ = await SendAsync(HttpMethod.Post, endpoint, values);
		}

		public async Task<T?> PostAsync<T>(string endpoint, object? values = null)
		{
			using var response = await SendAsync(HttpMethod.Post, endpoint, values);
			if (response.Content.Headers.ContentLength == 0)
				return default;

			return await response.Content.ReadFromJsonAsync<T>();
		}

		public async Task DeleteAsync(string endpoint)
		{
			using var _ = await SendAsync(HttpMethod.Delete, endpoint);
		}

		private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? content = null)
		{
			using var request = BuildRequest(method, endpoint, _token);
			if (content != null)
				request.Content = FormContent(content);

			return (await Client.SendAsync(request))
				.EnsureSuccessStatusCode();
		}
	}

	private static string AjaxEndpoint(string endpoint)
	{
		var separator = endpoint.Contains('?') ? '&' : '?';
		return $"{endpoint}{separator}ajax";
	}

	private static HttpRequestMessage BuildRequest(HttpMethod method, string endpoint, string? token = null)
	{
		if (method == HttpMethod.Get)
			endpoint = AjaxEndpoint(endpoint);

		var request = new HttpRequestMessage(method, endpoint);
		if (token != null)
			request.Headers.TryAddWithoutValidation(
				"Cookie",
				$"frhd_app_sr={Uri.EscapeDataString(token)}");

		return request;
	}

	private static FormUrlEncodedContent FormContent(object values)
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
}