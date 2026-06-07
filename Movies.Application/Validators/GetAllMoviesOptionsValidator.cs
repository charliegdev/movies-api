using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidators : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptableSortField = ["title", "yearofrelease"];

    public GetAllMoviesOptionsValidators()
    {
        RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.SortByField)
            .Must(x =>
                x is null || AcceptableSortField.Contains(x, StringComparer.OrdinalIgnoreCase)
            )
            .WithMessage($"You can only sort by {string.Join(", ", AcceptableSortField)}");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage($"page must be a postiive number.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage($"page size must be a number between 1 and 25.");
    }
}
