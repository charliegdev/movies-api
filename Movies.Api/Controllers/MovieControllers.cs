using Microsoft.AspNetCore.Mvc;
using Movies.Application.Repositories;

namespace Movies.Api.Controllers;

[ApiController]
public class MovieController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MovieController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }
}
