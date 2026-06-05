namespace Movies.Contracts.Responses;

public class RatingResponse
{
    public required Guid MovieId { get; init; }
    public required string Slug { get; init; }
    public required float Rating { get; init; }
}
