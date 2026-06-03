using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MovieController(IMovieService movieService) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    [Authorize(AuthConstants.TrustedMemberPolicy)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMovieRequest request,
        CancellationToken token
    )
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, token);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);

        return movie is null ? NotFound() : Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var userId = HttpContext.GetUserId();

        var movies = await _movieService.GetAllAsync(userId, token);

        return Ok(movies.MapToResponse());
    }

    [Authorize(AuthConstants.TrustedMemberPolicy)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken token
    )
    {
        var userId = HttpContext.GetUserId();

        var movie = request.MapToMovie(id);

        var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);

        return updatedMovie is not null ? Ok(updatedMovie.MapToResponse()) : NotFound();
    }

    [Authorize(AuthConstants.AdminPolicy)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
    {
        bool isDeleted = await _movieService.DeleteAsync(id, token);

        return isDeleted ? Ok() : NotFound();
    }
}
