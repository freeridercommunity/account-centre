using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AccountCentre.Components.Dialogs;

using Core.Sessions;
using Networking;
using Networking.Responses;

public sealed partial class FriendRequestsDialog : ContentDialog
{
	private readonly ListView ListView = new()
	{
		IsItemClickEnabled = false
	};
	private readonly Session Session;
	public FriendRequestsDialog(Session session)
	{
		this.Session = session;

		this.Title = "Pending Friend Requests";
		this.Content = this.ListView;

		this.CloseButtonText = "Close";
		this.Opened += (_, _) => LoadRequests();
	}

	private async void LoadRequests()
	{
		var response = await Session.Rest.GetAsync<UserPageFriendsResponse>(Routes.UserFriends(Session.User!.Name))
			?? throw new Exception("Response returned no data");

		this.ListView.Items.Clear();

		var friendRequestData = response.FriendRequests?.Data;
		if (friendRequestData == null ||
			friendRequestData.Length == 0)
			return;

		foreach (var request in friendRequestData)
		{
			var grid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition
					{
						Width = new GridLength(1, GridUnitType.Star)
					},
					new ColumnDefinition
					{
						Width = GridLength.Auto
					}
				},
				Padding = new Thickness(8)
			};

			var item = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = 8,
				Children =
				{
					new PersonPicture
					{
						ProfilePicture = new BitmapImage(new Uri(request.User.AvatarURL!)),
						Height = 32,
						Width = 32
					},
					new StackPanel
					{
						Children =
						{
							new TextBlock
							{
								Text = request.User.DisplayName
							},
							new TextBlock
							{
								Text = request.Request.TimeAgo,
								Opacity = 0.8
							}
						}
					}
				}
			};

			var actionRow = new StackPanel
			{
				Margin = new Thickness(16, 0, 0, 0),
				Orientation = Orientation.Horizontal,
				Spacing = 4,
				Children =
				{
					new Button
					{
						Content = "Accept",
						IsEnabled = false
					},
					new Button
					{
						Content = "Reject",
						IsEnabled = false
					}
				}
			};

			Grid.SetColumn(actionRow, 1);

			grid.Children.Add(item);
			grid.Children.Add(actionRow);
			this.ListView.Items.Add(grid);
		}
	}
}