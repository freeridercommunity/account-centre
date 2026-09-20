namespace AccountCentre.Views;

public interface IRefreshableView
{
	Task RefreshAsync(CancellationToken? cancellationToken = null);
}