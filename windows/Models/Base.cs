using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountCentre.Models;

using Core.Settings;

public abstract record Base : INotifyPropertyChanged
{
	public int ID { get; set; }
	public bool IsDeveloperModeEnabled =>
		SettingsManager.Current.DeveloperMode;
	public Base(int id) =>
		ID = id;

	public event PropertyChangedEventHandler? PropertyChanged;
	internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(
			this,
			new PropertyChangedEventArgs(propertyName));
	}
}