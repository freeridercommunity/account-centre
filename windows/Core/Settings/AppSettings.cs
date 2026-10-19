using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccountCentre.Core.Settings;

public sealed partial class AppSettings : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private bool _allowConcurrentSessions = false;
	private bool _autoDownloadUpdates = false;
	private bool _checkForUpdatesAtStartup = true;
	private bool _developerMode = false;
	private bool _minimizeToTray = false;
	private bool _saveLoginInfo = true;
	private ushort _startupAccountBehavior = 0;
	private ushort _updateChannel = 0;

	public bool AllowConcurrentSessions
	{
		get => _allowConcurrentSessions;
		set => SetProperty(ref _allowConcurrentSessions, value);
	}

	public bool AutoDownloadUpdates
	{
		get => _autoDownloadUpdates;
		set => SetProperty(ref _autoDownloadUpdates, value);
	}

	public bool CheckForUpdatesAtStartup
	{
		get => _checkForUpdatesAtStartup;
		set => SetProperty(ref _checkForUpdatesAtStartup, value);
	}

	public bool DeveloperMode
	{
		get => _developerMode;
		set => SetProperty(ref _developerMode, value);
	}

	public bool SaveLoginInfo
	{
		get => _saveLoginInfo;
		set => SetProperty(ref _saveLoginInfo, value);
	}

	public ushort StartupAccountBehavior
	{
		get => _startupAccountBehavior;
		set => SetProperty(ref _startupAccountBehavior, value);
	}

	public bool MinimizeToTray
	{
		get => _minimizeToTray;
		set => SetProperty(ref _minimizeToTray, value);
	}

	public ushort UpdateChannel
	{
		get => _updateChannel;
		set => SetProperty(ref _updateChannel, value);
	}

	private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
			return;

		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}