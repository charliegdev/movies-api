using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;

    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Genres).NotEmpty();

        RuleFor(x => x.Title).NotEmpty();

        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This movie already exists in the system");
    }

    // * Validate: no 2 movies can share the same slug
    private async Task<bool> ValidateSlug(
        Movie movie,
        string slug,
        CancellationToken token = default
    )
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);

        if (existingMovie is not null)
        {
            // * Slug is in use, but if it's from this exact movie, we're good.
            // * Otherwise, this slug belongs to another movie. Already used => bad
            return existingMovie.Id == movie.Id;
        }

        // * Movie doesn't exist, can use.
        return true;
    }
}
