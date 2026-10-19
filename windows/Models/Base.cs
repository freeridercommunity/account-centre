using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountCentre.Models;

public abstract record Base : INotifyPropertyChanged
{
	public ulong ID { get; set; }
	public Base(ulong id)
		=> ID = id;

	public event PropertyChangedEventHandler? PropertyChanged;
	internal void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(
			this,
			new PropertyChangedEventArgs(propertyName));
}