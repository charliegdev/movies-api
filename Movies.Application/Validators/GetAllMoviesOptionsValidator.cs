using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidators : AbstractValidator<GetAllMoviesOptions>
{
    public GetAllMoviesOptionsValidators()
    {
        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);
    }
}
