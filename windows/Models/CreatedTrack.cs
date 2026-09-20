using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace AccountCentre.Models;

using Networking.Models;
using Networking.ExtendedAPI;

public sealed record CreatedTrack : Track
{
	public string BestTime { get; set; } = "No record";
	public bool CanDelete => !IsHidden && !IsFeatured;
	public Brush DeleteActionForeground =>
		IsHidden
			? (Brush)Application.Current.Resources["AccentFillColorSecondaryBrush"]
			: (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
	public string DeleteActionIcon =>
		IsHidden ? "\uE890" : "\uE74D";
	public string DeleteActionText =>
		IsHidden ? "Restore" : "Delete";
	public string Rating =>
		$"{VotePercentage}% of {VotesText} votes";
	public ushort VotePercentage { get; set; }
	// public uint Votes { get; set; }
	public string VotesText { get; set; } = "0";
	public CreatedTrack(TrackData data) : base(data)
	{
		if (data.BestTime != null)
			BestTime = data.BestTime;

		if (data.VotePercentage != null)
			VotePercentage = (ushort)data.VotePercentage;
		if (data.VotesText != null)
			VotesText = data.VotesText;
	}

	private bool IsPartial = true;
	public override async Task Fetch()
	{
		if (IsPartial is not true)
			return;

		await base.Fetch();
		IsPartial = false;
	}

	public async Task Delete()
	{
		await RESTExtended.DeleteAsync(Endpoints.Track(ID));
	}
}