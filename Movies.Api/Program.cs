using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Health;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder
    .Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
        };
    });

builder.Services.AddAuthorization(x =>
{
    x.AddPolicy(AuthConstants.AdminPolicy, p => p.RequireClaim(AuthConstants.AdminClaim, "true"));

    x.AddPolicy(
        AuthConstants.TrustedMemberPolicy,
        p =>
            p.RequireAssertion(c =>
                c.User.HasClaim(m => m is { Type: AuthConstants.AdminClaim, Value: "true" })
                || c.User.HasClaim(m =>
                    m is { Type: AuthConstants.TrustedMemberClaim, Value: "true" }
                )
            )
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();

builder
    .Services.AddApiVersioning(x =>
    {
        x.DefaultApiVersion = new ApiVersion(1.0);
        x.AssumeDefaultVersionWhenUnspecified = true;
        x.ReportApiVersions = true;
        x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
    })
    .AddMvc();

// builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x =>
{
    x.AddBasePolicy(c => c.Cache());
    x.AddPolicy(
        "MovieCache",
        c =>
        {
            c.Cache()
                .Expire(TimeSpan.FromMinutes(1))
                .SetVaryByQuery(["title", "year", "sortby", "page", "pagesize"])
                .Tag("movies");
        }
    );
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthChecks>(DatabaseHealthChecks.Name);
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("_health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// app.UseResponseCaching();
app.UseOutputCache();
app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
