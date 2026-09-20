using Microsoft.UI.Xaml;
using System.Globalization;

namespace AccountCentre.Models;

using Core;
using Networking;
using Networking.Models;
using Networking.Responses;
using Networking.ExtendedAPI;

public sealed record Race : Base
{
	public string BestTime { get; set; }
	public RaceData? Data { get; set; }
	public Visibility SearchVisibility { get; set; } = Visibility.Visible;
	public Track? Track { get; set; }
	public User? User { get; set; }
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
	public bool IsFeatured { get; set; }
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

		BestTime = TicksToTime(1_000 * race.Data.Ticks / 30);
		Data = race.Data;
	}

	public async Task<UserTrackStats> FetchStats()
	{
		if (Track == null)
			throw new Exception("Track is not defined");

		var response = await REST.GetAsync<TrackRaceResponse>(Networking.Endpoints.TrackRace(Track.ID, App.User!.Name)) ?? throw new Exception("Response body is empty");

		if (response.UserTrackStats is not UserTrackStats stats)
			throw new Exception("Response body is empty");

		var date = DateTime.ParseExact(
			stats.BestDate,
			"dd/MM/yyyy",
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
		);
		CreatedTimestamp = new DateTimeOffset(date).ToUnixTimeSeconds();

		return stats;
	}

	public async Task Delete()
	{
		await RESTExtended.DeleteAsync(Networking.ExtendedAPI.Endpoints.Race(ID));
	}

	private string TicksToTime(uint ticks)
	{
		var minutes = ticks / 60_000;
		var seconds = ticks % 60_000 / 1_000.0;

		return $"{minutes}:{seconds:00.00}";
	}
}