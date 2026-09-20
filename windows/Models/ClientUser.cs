namespace AccountCentre.Models;

using Networking.Models;
// using Networking.Responses;

public sealed record ClientUser : User
{
	public string? Email { get; set; } = "";
	public ClientUser(ClientUserData data) : base(data as UserData)
	{
		Email = data.Email;
	}

	// public Task<bool> ChangeEmail(string email)
	// {
	// 	return true;
	// }

	// public Task<bool> ChangeUsername(string username)
	// {
	// 	return true;
	// }

	private void NotifyStatusChanged()
	{
		// OnPropertyChanged(nameof(Status));
		// OnPropertyChanged(nameof(StatusText));
		// OnPropertyChanged(nameof(PrimaryAction));
		// OnPropertyChanged(nameof(PrimaryActionIcon));
		// OnPropertyChanged(nameof(PrimaryActionText));
		// OnPropertyChanged(nameof(CanAct));
		// OnPropertyChanged(nameof(IsRunning));
	}
}