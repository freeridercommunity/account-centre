using AccountCentre.Core.Sessions;

namespace AccountCentre.Views.Interfaces;

public interface ISessionAware
{
	Session Session { get; set; }
}