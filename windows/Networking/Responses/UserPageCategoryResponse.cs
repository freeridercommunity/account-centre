namespace AccountCentre.Networking.Responses;

public abstract class UserPageCategoryResponse<T> : BasePageResponse
{
	public T[] Data { get; init; } = [];
}