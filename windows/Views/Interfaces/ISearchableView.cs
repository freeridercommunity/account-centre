namespace AccountCentre.Views.Interfaces;

public interface ISearchableView
{
	void ClearSearch() {}

	Task<List<string>?> Search(string query);

	void SearchSuggestionChosen(string selected) {}
}