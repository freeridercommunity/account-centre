namespace AccountCentre.Core;

using Networking;
using Networking.Models;
using Networking.Responses;

public static class Search
{
	// public static async Task<List<string>> Track(string query)
	// {
	// 	var items = new List<string>();
	// 	try
	// 	{
	// 		var response = await REST.PostAsync<TrackLookupAPIResponse>($"search/t/{query}");

	// 		if (response?.Data is not TrackData[] results ||
	// 			results.Length == 0)
	// 			return;

	// 		foreach (var data in results)
	// 			items.Add(data.Title);
	// 	}
	// 	finally
	// 	{
	// 		return items;
	// 	}
	// }

	public static async Task<UMentionData[]> User(string query)
	{
		var response = await REST.PostAsync<UMentionLookupAPIResponse>($"search/u_mention_lookup/{query}");

		if (response == null)
			throw new Exception("Response returned no data");

		return response.Data;
	}
}