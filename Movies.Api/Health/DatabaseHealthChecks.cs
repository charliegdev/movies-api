using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthChecks(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<DatabaseHealthChecks> logger
) : IHealthCheck
{
    public const string Name = "Database";
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
    private readonly ILogger _logger = logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(
                cancellationToken
            );
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Database is unhealthy");
            return HealthCheckResult.Unhealthy("Database is unhealthy", e);
        }
    }
}
