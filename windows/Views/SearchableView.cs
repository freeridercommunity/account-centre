namespace AccountCentre.Views;

public interface ISearchableView
{
	void ClearSearch() {}

	Task<List<string>?> Search(string query);

	void SearchSuggestionChosen(string selected) {}
}