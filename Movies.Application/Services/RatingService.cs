using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository)
    : IRatingService
{
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IMovieRepository _movieRepository = movieRepository;

    public async Task<bool> RateMovieAsync(
        Guid movieId,
        int rating,
        Guid userId,
        CancellationToken token = default
    )
    {
        if (rating <= 0 || rating > 5)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 1 to 5",
                },
            ]);
        }

        var movieExists = await _movieRepository.ExistsByIdAsync(movieId, token);

        return movieExists
            && await _ratingRepository.RateMovieAsync(movieId, rating, userId, token);
    }

    public async Task<bool> DeleteRatingAsync(
        Guid movieId,
        Guid userId,
        CancellationToken token = default
    )
    {
        return await _ratingRepository.DeleteRatingAsync(movieId, userId, token);
    }
}
