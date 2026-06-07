namespace Movies.Contracts.Requests;

public class GetAllMoviesRequest : IPagedRequest
{
    public string? Title { get; init; }
    public int? Year { get; init; }
    public string? SortBy { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
