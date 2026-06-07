namespace Movies.Contracts.Requests;

public interface IPagedRequest
{
    public int? Page { get; init; }
    int? PageSize { get; init; }
}
