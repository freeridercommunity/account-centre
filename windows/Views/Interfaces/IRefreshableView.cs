namespace AccountCentre.Views.Interfaces;

public interface IRefreshableView
{
	Task RefreshAsync(CancellationToken? cancellationToken = null);
}