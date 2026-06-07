namespace Movies.Contracts.Requests;

public class GetAllMoviesRequest : IPagedRequest
{
    public required string? Title { get; init; }
    public required int? Year { get; init; }
    public required string? SortBy { get; init; }
    public required int? Page { get; init; }
    public required int? PageSize { get; init; }
}
