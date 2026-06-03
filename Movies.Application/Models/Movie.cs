using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public string Slug => GenerateSlug();

    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }

    public float? Rating { get; set; }
    public int? UserRating { get; set; }

    // List<string> is mutable, we just don't allow reassign, hence the init here.
    public required List<string> Genres { get; init; } = [];

    private string GenerateSlug()
    {
        var sluggedTitle = SlugRegex().Replace(Title, string.Empty).ToLower().Replace(" ", "-");
        return $"{sluggedTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]")]
    private static partial Regex SlugRegex();
}
