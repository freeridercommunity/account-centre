using Microsoft.UI.Xaml.Controls;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;

public sealed partial class TrackCommentDialog : ContentDialog
{
	// Thumbnail backdrop?
	// Comment author bottom right?
	public ulong CommentId;
	private readonly Session Session;
	public ulong TrackId;
	public TrackCommentDialog(Session session, ulong trackId, ulong commentId)
	{
		this.Session = session;
		this.TrackId = trackId;
		this.CommentId = commentId;

		// this.ErrorBar.Title = "Track comment not found";
		// this.ErrorBar.Content = "Please try again later";

		this.Title = "Track Comment";

		this.Content = new ProgressRing
		{
			Height = 32,
			Width = 32
		};

		this.CloseButtonText = "Close";
		this.Opened += (_, _) => LoadComment(trackId, commentId);
	}

	public async void LoadComment(ulong trackId, ulong commentId)
	{
		// Search all comments on track until it is found

		throw new NotImplementedException();
	}
}