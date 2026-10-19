using Microsoft.UI.Xaml;
using System.Globalization;
using System.Text.Json;

namespace AccountCentre.Models;

using Core;
using Core.Storage;
using Networking;
using Networking.Models;
using Networking.Responses;
using Networking.ExtendedAPI;

public sealed record Race : Base
{
	public string BestTime { get; set; } = "";
	public RaceData? Data { get; set; }
	public Visibility SearchVisibility { get; set; } = Visibility.Visible;
	public Track? Track { get; set; }
	public User User { get; set; }
	public long? CreatedTimestamp { get; internal set; }
	public bool CreatedTimestampLoaded =>
		CreatedTimestamp > 0;
	public string CreatedTimeAgo
	{
		get
		{
			if (CreatedTimestamp is not long timestamp)
				return "";

			return Utils.FormatTimeAgo(timestamp);
		}
	}
	public bool IsFeatured { get; internal set; } = false;
	public bool IsFeaturedLoaded { get; private set; }
	public Race(RaceEntryData data) : base(data!.User!.ID)
	{
		if (data.Track != null)
			Track = new(data.Track);

		User = new(data.User);

		Patch(data);
	}

	private void Patch(RaceEntryData race)
	{
		if (Track != null && race.Track != null)
			Track.Patch(race.Track);

		// if (race.User != null)
		// 	User.Patch(race.User);

		if (race.Data != null)
		{
			BestTime = TicksToTime(1_000 * race.Data.Ticks / 30);
			Data = race.Data;
		}
	}

	public async Task<bool> FetchFeatured()
	{
		if (IsFeaturedLoaded)
			return IsFeatured;

		var endpoint = "https://raw.githubusercontent.com/freeridercommunity/featured-ghosts/master/data.json";
		try
		{
			if (CacheStore.Default.Get(endpoint) is not byte[] data)
			{
				using HttpClient Client = new()
				{
					DefaultRequestHeaders =
					{
						{ "User-Agent", "FreeRiderCommunityAccountCentre" }
					}
				};

				var response = await Client.GetAsync(endpoint);
				if (!response.IsSuccessStatusCode)
					throw new Exception(response.ReasonPhrase ?? "Failed to fetch");

				data = await response.Content.ReadAsByteArrayAsync();
				CacheStore.Default.Set(endpoint, data);
			}

			JsonDocument document = JsonDocument.Parse(data);

			if (document.RootElement.TryGetProperty(this.User!.DisplayName, out JsonElement races))
			{
				var url = $"frhd.co/t/{Track!.ID}/r/";
				if (races.TryGetProperty($"{url}{this.User!.Name}", out JsonElement featureType))
				{
					IsFeatured = true;
				}
				else
				{
					foreach (JsonProperty property in races.EnumerateObject())
					{
						if (property.Name.StartsWith(url, StringComparison.OrdinalIgnoreCase))
						{
							// string featureType = property.Value.GetString() ?? "";

							IsFeatured = true;
						}
					}
				}
			}

			IsFeaturedLoaded = true;
		}
		catch
		{
		}

		return IsFeatured;
	}

	public async Task<UserTrackStats> FetchStats(REST.Session rest)
	{
		if (Track == null)
			throw new Exception("Track is not defined");

		var response = await rest.GetAsync<TrackRaceResponse>(Routes.TrackRace(Track.ID, this.User!.Name)) ?? throw new Exception("Response body is empty");
		if (response.UserTrackStats is not UserTrackStats stats)
			throw new Exception("Response body is empty");

		if (stats.BestDate != null)
		{
			var date = DateTime.ParseExact(
				stats.BestDate,
				"dd/MM/yyyy",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
			);
			CreatedTimestamp = new DateTimeOffset(date).ToUnixTimeSeconds();
		}

		return stats;
	}

	public async Task Delete(RESTExtended.Session rest)
		=> await rest.DeleteAsync(RESTExtended.Routes.Race(ID));

	private static string TicksToTime(uint ticks)
	{
		var minutes = ticks / 60_000;
		var seconds = ticks % 60_000 / 1_000.0;

		return $"{minutes}:{seconds:00.00}";
	}
}