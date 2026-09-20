using System.Reflection;
using System.Text.Json.Serialization;

namespace AccountCentre.Models;

using Core;
using Networking;
using Networking.Models;
using Networking.Responses;

public record Track : Base
{
	public string Title { get; internal set; } = "";
	public string Description { get; internal set; } = "";
	public string? Code { get; internal set; }
	public bool IsFeatured { get; internal set; }
	public bool IsHidden { get; internal set; }
	public long? PublishTimestamp { get; internal set; }
	public bool PublishedTimestampLoaded =>
		PublishTimestamp > 0;
	public string PublishedTimeAgo
	{
		get
		{
			if (PublishTimestamp is not long timestamp)
				return "";

			return Utils.FormatTimeAgo(timestamp);
		}
	}
	public virtual string? ThumbnailURL { get; set; }
	public string? DisplayThumbnailURL => (ThumbnailURL != null &&
		CacheManager.Has(ThumbnailURL))
		? CacheManager.GetPath(ThumbnailURL)
		: ThumbnailURL;
	// Test cache
	// public string? DisplayThumbnailURL {
	// 	get
	// 	{
	// 		if (ThumbnailURL is null)
	// 			return null;

	// 		if (!CacheManager.Has(ThumbnailURL))
	// 		{
	// 			_ = CacheThumbnailAsync();
	// 			return null;
	// 		}

	// 		return CacheManager.GetPath(ThumbnailURL);
	// 	}
	// }
	public Track(TrackData data) : base(data!.ID)
	{
		Patch(data);
	}

	public void Patch(TrackData data)
	{
		Title = data.Title;
		Description = data.Description;
		IsFeatured = data.IsFeatured;
		IsHidden = data.IsHidden;
		PublishTimestamp = data.PublishTimestamp;
		ThumbnailURL = data.ThumbnailURL;
	}

	public virtual async Task Fetch()
	{
		var response = await REST.GetAsync<TrackAPIResponse>($"track_api/load_track?id={ID}");

		if (response is null ||
			response.Data is null ||
			response.Data.Track is not TrackData trackData)
			return;

		Patch(trackData);
	}

	// private object? this[string field]
	// {
	// 	get
	// 	{
	// 		var property = GetType().GetProperty(field);
	// 		return property?.GetValue(this);
	// 	}
	// 	set
	// 	{
	// 		var property = GetType().GetProperty(field);
	// 		property?.SetValue(this, value);
	// 		OnPropertyChanged(nameof(field));
	// 	}
	// }

	private object? this[string field]
	{
		get
		{
			if (!FieldMap.TryGetValue(field, out var propertyName))
				return null;

			var property = GetType().GetProperty(propertyName);
			return property?.GetValue(this);
		}
		set
		{
			if (!FieldMap.TryGetValue(field, out var propertyName))
				return;

			var property = GetType().GetProperty(propertyName);
			property?.SetValue(this, value);
			OnPropertyChanged(propertyName);
		}
	}

	private readonly Dictionary<string, Task<object?>> _fetching = [];
	public async Task<object?> FetchField(string field)
	{
		if (this[field] is not null)
			return this[field];

		if (_fetching.TryGetValue(field, out var existing))
			return await existing;

		var task = FetchFieldInternal(field);
		_fetching[field] = task;

		try
		{
			return await task;
		}
		finally
		{
			_fetching.Remove(field);
		}
	}

	private async Task<object?> FetchFieldInternal(string field)
	{
		if (TrackData.ResolveJsonPropertyName(field) is not string prop)
			return null;

		var response = await REST.GetAsync<TrackAPIResponse>(Endpoints.TrackData(ID, prop));

		if (response?.Data?.Track is not TrackData trackData)
			return null;

		var value = trackData[field];

		if (value is not null)
			this[field] = value;

		return value;
	}

	private static readonly SemaphoreSlim ThumbnailSemaphore = new(4);
	public async Task CacheThumbnailAsync()
	{
		if (string.IsNullOrEmpty(ThumbnailURL) ||
			CacheManager.Has(ThumbnailURL))
			return;

		await ThumbnailSemaphore.WaitAsync();

		try
		{
			if (CacheManager.Has(ThumbnailURL))
				return;

			await using var stream = await REST.GetStreamAsync(ThumbnailURL);
			await CacheManager.SetAsync(
				ThumbnailURL,
				stream);

			OnPropertyChanged(nameof(DisplayThumbnailURL));
		}
		finally
		{
			ThumbnailSemaphore.Release();
		}
	}

	private static readonly Dictionary<string, string> FieldMap =
		CreateFieldMap();

	private static Dictionary<string, string> CreateFieldMap()
	{
		var map = new Dictionary<string, string>(
			StringComparer.OrdinalIgnoreCase);

		foreach (var property in typeof(TrackData).GetProperties())
		{
			map[property.Name] = property.Name;

			var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

			if (jsonName is not null)
				map[jsonName] = property.Name;
		}

		return map;
	}
}