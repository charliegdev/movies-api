using System.ComponentModel;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;

namespace Movies.Application.Services;

public class MovieService(
    IMovieRepository movieRepository,
    IValidator<Movie> movieValidator,
    IRatingRepository ratingRepository,
    IValidator<GetAllMoviesOptions> getAllMoviesOptionsValidator
) : IMovieService
{
    private readonly IMovieRepository _movieRepository = movieRepository;
    private readonly IValidator<Movie> _movieValidator = movieValidator;
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IValidator<GetAllMoviesOptions> _getAllMoviesOptionsValidator =
        getAllMoviesOptionsValidator;

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);
        return await _movieRepository.CreateAsync(movie, token);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        return _movieRepository.DeleteAsync(id, token);
    }

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        return _movieRepository.ExistsByIdAsync(id, token);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(
        GetAllMoviesOptions options,
        CancellationToken token = default
    )
    {
        await _getAllMoviesOptionsValidator.ValidateAndThrowAsync(options, token);
        return await _movieRepository.GetAllAsync(options, token);
    }

    public Task<Movie?> GetByIdAsync(
        Guid id,
        Guid? userId = default,
        CancellationToken token = default
    )
    {
        return _movieRepository.GetByIdAsync(id, userId, token);
    }

    public Task<Movie?> GetBySlugAsync(
        string slug,
        Guid? userId = default,
        CancellationToken token = default
    )
    {
        return _movieRepository.GetBySlugAsync(slug, userId, token);
    }

    public async Task<Movie?> UpdateAsync(
        Movie movie,
        Guid? userId = default,
        CancellationToken token = default
    )
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken: token);

        var movieExists = await _movieRepository.ExistsByIdAsync(movie.Id, token);

        if (!movieExists)
        {
            return null;
        }
        await _movieRepository.UpdateAsync(movie, token);

        if (!userId.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
            movie.Rating = rating;
        }
        else
        {
            var (rating, userRating) = await _ratingRepository.GetRatingAsync(
                movie.Id,
                userId.Value,
                token
            );
            movie.Rating = rating;
            movie.UserRating = userRating;
        }

        return movie;
    }

    public async Task<int> GetCountAsync(
        string? title,
        int? yearOfRelease,
        CancellationToken token = default
    )
    {
        return await _movieRepository.GetCountAsync(title, yearOfRelease, token);
    }
}
