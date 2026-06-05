namespace Movies.Contracts.Responses;

public class RatingsResponse
{
    public required IEnumerable<RatingResponse> Ratings { get; init; }
}
