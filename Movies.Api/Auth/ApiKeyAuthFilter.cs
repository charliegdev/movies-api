using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth;

public class ApiKeyAuthFilters(IConfiguration configuration) : IAuthorizationFilter
{
    private readonly IConfiguration _configuration = configuration;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (
            !context.HttpContext.Request.Headers.TryGetValue(
                AuthConstants.ApiKeyHeaderName,
                out var extractedApiKey
            )
        )
        {
            context.Result = new UnauthorizedObjectResult("API Key missing");
            return;
        }

        var apiKey = _configuration["ApiKey"]!;
        if (apiKey != extractedApiKey)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
        }
    }
}
