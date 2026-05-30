namespace Movies.Application.Models;

public class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }

    // List<string> is mutable, we just don't allow reassign, hence the init here.
    public required List<string> Genres { get; init; } = [];
}
