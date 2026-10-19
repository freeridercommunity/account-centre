using System.Net;
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
		},
		DefaultRequestVersion = HttpVersion.Version30,
		DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
	};

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

	private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
			return;

		BaseResponse? body = null;
		try
		{
			body = await response.Content.ReadFromJsonAsync<BaseResponse>();
			if (body?.Error != null)
				throw new Exception(body.Error.Message);
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

	public static class Routes
	{
		public static string Email()
			=> "user/email";

		public static string OfficialAuthor()
			=> $"user/oa";

		public static string Race(ulong trackId)
			=> $"{Track(trackId)}/races";

		public static string Track(ulong trackId)
			=> $"tracks/{trackId}";
	}

	public sealed class Session
	{
		private string? _token;
		internal Session(string? token = null)
		{
			if (token != null)
				_token = token;
		}

		public Session SetToken(string token)
		{
			_token = token;
			return this;
		}

		public void ClearToken()
			=> _token = null;

		public async Task<T?> GetAsync<T>(string endpoint)
		{
			using var response = await SendAsync(HttpMethod.Get, endpoint);
			if (response.Content.Headers.ContentLength == 0)
				return default;

			return await response.Content.ReadFromJsonAsync<T>();
		}

		public async Task PatchAsync(string endpoint, object? values = null)
		{
			using var _ = await SendAsync(HttpMethod.Patch, endpoint, values);
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
				request.Content = JsonContent.Create(content);

			// return (await Client.SendAsync(request))
			// 	.EnsureSuccessStatusCode();

			var response = await Client.SendAsync(request);
			await EnsureSuccessStatusCode(response);
			return response;
		}
	}

	private static HttpRequestMessage BuildRequest(HttpMethod method, string endpoint, string? token = null)
	{
		var request = new HttpRequestMessage(method, endpoint);
		if (token != null)
			request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");

		return request;
	}
}