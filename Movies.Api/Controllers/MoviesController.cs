using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[ApiController]
public class MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    : ControllerBase
{
    private readonly IMovieService _movieService = movieService;
    private readonly IOutputCacheStore _outputCacheStore = outputCacheStore;

    // [Authorize(AuthConstants.TrustedMemberPolicy)]
    [ServiceFilter(typeof(ApiKeyAuthFilters))]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMovieRequest request,
        CancellationToken token
    )
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, token);
        await _outputCacheStore.EvictByTagAsync("movies", token);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    /*
    [ResponseCache(
        Duration = 30,
        VaryByHeader = "Accept, Accept-Encoding",
        Location = ResponseCacheLocation.Any
    )]
    */
    [OutputCache(PolicyName = "MovieCache")]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken token)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);

        return movie is null ? NotFound() : Ok(movie.MapToResponse());
    }

    /*
    [ResponseCache(
        Duration = 30,
        VaryByHeader = "Accept, Accept-Encoding",
        VaryByQueryKeys = ["title", "year", "sortby", "page", "pagesize"],
        Location = ResponseCacheLocation.Any
    )]
    */
    [OutputCache(PolicyName = "MovieCache")]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllMoviesRequest request,
        CancellationToken token
    )
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions().WithUserId(userId);

        var movies = await _movieService.GetAllAsync(options, token);
        var total = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);

        return Ok(movies.MapToResponse(options.Page, options.PageSize, total));
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

        await _outputCacheStore.EvictByTagAsync("movies", token);
        return updatedMovie is not null ? Ok(updatedMovie.MapToResponse()) : NotFound();
    }

    [Authorize(AuthConstants.AdminPolicy)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
    {
        bool isDeleted = await _movieService.DeleteAsync(id, token);

        await _outputCacheStore.EvictByTagAsync("movies", token);
        return isDeleted ? Ok() : NotFound();
    }
}
