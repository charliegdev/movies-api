namespace Movies.Application.Models;

public record GetAllMoviesOptions
{
    public string? Title { get; set; }
    public int? YearOfRelease { get; set; }
    public Guid? UserId { get; set; }
    public string? SortByField { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Ascending,
    Descending,
}
